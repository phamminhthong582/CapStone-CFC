using System.Security.Cryptography;
using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Customer;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using Service.Interface;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.Implement;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;
    private readonly ILogger<CustomerService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    private readonly string tempdata = "tempdatakey"; // Không cần truyền vào constructor nữa

    public CustomerService(
        ICustomerRepository customerRepository,
        IMapper mapper,
        IRoleRepository roleRepository,
        IMemoryCache cache,
        IEmailService emailService,
        ILogger<CustomerService> logger,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _roleRepository = roleRepository;
        _cache = cache;
        _emailService = emailService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerResponse>> RegisterCustomer(CreateCustomerRequest request)
    {
        try
        {
            var response = new Result<CustomerResponse>();

            // Log the incoming request
            _logger.LogInformation($"Starting registration for email: {request.Email}");

            var isMailUsed = await _customerRepository.FindCustomerByEmail(request.Email);
            if (isMailUsed != null)
            {
                _logger.LogWarning($"Email {request.Email} is already in use");
                response.Messages = new[] { "This mail is already used" };
                response.ResultStatus = ResultStatus.Duplicated.ToString();
                return response;
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var roleToAssign = RoleName.Customer.ToString();
            var roleId = await _roleRepository.GetRoleIdByName(roleToAssign);
            if (roleId == null)
            {
                response.Messages = new[] { $"Role '{roleToAssign}' not found. Please contact admin." };
                response.ResultStatus = ResultStatus.Failed.ToString();
                return response;
            }
            var User = new User
            {
                Email = request.Email,
                Password = Convert.ToBase64String(passwordHash),
                Status = false,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                RoleId = roleId,
            };
            await _unitOfWork.GetRepo<User>().AddAsync(User);
            await _unitOfWork.CompleteAsync();  
            var customer = new Customer
            {
                UserId  = User.UserId,
                Email = request.Email,
                Status = CustomerStatus.NotVerified.ToString(),
            };
            
            _logger.LogInformation("Attempting to register customer in database");
            var user = await _customerRepository.RegisterCustomer(customer);
            User.CustomerId = customer.CustomerId;

             _unitOfWork.GetRepo<User>().Update(User);
            await _unitOfWork.CompleteAsync();

            var token = _customerRepository.CreateRandomToken();
            var cacheEntryOption = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetPriority(CacheItemPriority.Normal);

            _logger.LogInformation("Setting cache entry for verification token");
            _cache.Set(tempdata, token, cacheEntryOption);

            _logger.LogInformation("Attempting to send verification email");
            var mail = await _emailService.SendMailRegister(customer.Email, token);

            if (mail.ResultStatus != ResultStatus.Success.ToString())
            {
                _logger.LogError("Failed to send verification email");
                response.Messages = new[] { "Failed to send confirmation email." };
                response.ResultStatus = ResultStatus.Failed.ToString();
                return response;
            }

            response.RoleName = RoleName.Customer.ToString();
            response.ResultStatus = ResultStatus.Success.ToString();
            response.Messages = new[] { "Register successfully!" };
            response.Data = _mapper.Map<CustomerResponse>(user);

            _logger.LogInformation("Registration completed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer registration");
            throw; // Let the middleware handle the exception
        }
    }
    public async Task<List<CustomerResponse>> GetAllCustomer()
    {
        var list = await _customerRepository.GetAllCustomer();
        return _mapper.Map<List<CustomerResponse>>(list);
    }

    public async Task<Result<CustomerResponse>> GetCustomerById(Guid id)
    {
        var response = new Result<CustomerResponse>();
        var customer = await _customerRepository.GetCustomerById(id);
        if (customer == null)
        {
            response.Messages = ["Customer not found!"];
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        else
        {
            response.Data = _mapper.Map<CustomerResponse>(customer);
            response.Messages = ["Successfully!"];
            response.ResultStatus = ResultStatus.Success.ToString();
            return response;
        }
    }

   public async Task<Result<CustomerResponse>> UpdateCustomer(Guid id, UpdateCustomerRequest request)
{
    var response = new Result<CustomerResponse>();
    var customer = await _customerRepository.GetCustomerById(id);
    if (customer == null)
    {
        response.Messages = new[] { "Customer is not found!" };
        response.ResultStatus = ResultStatus.NotFound.ToString();
        return response;
    }
    if (request.Phone == customer.Phone &&
        request.City == customer.City &&
        request.District == customer.District &&
        request.Address == customer.Address &&
        request.Birthday == customer.Birthday &&
        request.Status == customer.Status &&
        request.Avatar == customer.Avatar)
    {
        response.Data = _mapper.Map<CustomerResponse>(customer);
        response.Messages = new[] { "Nothing changed!" };
        response.ResultStatus = ResultStatus.Error.ToString();
        return response;
    }
    if (!string.IsNullOrEmpty(request.Phone) && request.Phone != customer.Phone)
    {
        var isPhoneExisted = await _customerRepository.FindCustomerByPhone(request.Phone);
        if (isPhoneExisted != null)
        {
            response.Messages = new[] { "This phone number is already existed!" };
            response.ResultStatus = ResultStatus.Error.ToString();
            return response;
        }
    }
    customer.City = request.City ?? customer.City;
    customer.District = request.District ?? customer.District;
    customer.Address = request.Address ?? customer.Address;
    customer.Phone = request.Phone ?? customer.Phone;
    customer.Birthday = request.Birthday ?? customer.Birthday;
    customer.Status = request.Status ?? customer.Status;
    customer.Avatar = request.Avatar ?? customer.Avatar;
    await _customerRepository.UpdateCustomer(customer);
    response.Data = _mapper.Map<CustomerResponse>(customer);
    response.Messages = new[] { "Update successfully!" };
    response.ResultStatus = ResultStatus.Success.ToString();
    return response;
}


    public async Task<Result<Customer>> DeleteCustomer(Guid id)
    {
        var customer = await _customerRepository.GetCustomerById(id);
    
        if (customer == null)
        {
            return new Result<Customer>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"Customer not found."}
            };
        }

        await _customerRepository.DeleteMember(id);

        return new Result<Customer>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"Customer deleted successfully."}
        };
    }
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}