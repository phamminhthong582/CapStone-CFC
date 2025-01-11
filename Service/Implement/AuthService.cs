using System.Security.Claims;

using System.Security.Cryptography;

using AutoMapper;
using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Response;
using BusinessObject.Entities;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class AuthService : IAuthService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IEmployeeRepository employeeRepository, ITokenService tokenService, IMapper mapper,
        IConfiguration configuration)
    {
        _employeeRepository = employeeRepository;
        _tokenService = tokenService;
        _mapper = mapper;
        _configuration = configuration;
    }
   public async Task<Result<LoginResponse>> Login(string email, string password)
{
    var user = await _employeeRepository.GetEmployeeByEmail(email);
    var admin =  _employeeRepository.GetAdminAccount(email, password);
    if (user is null && admin is null)
    {
        return new Result<LoginResponse>
        {
            ResultStatus = ResultStatus.NotFound.ToString(),
            Messages =  ["Account is not found"]  
        };
    }
    else if (admin != null)
    {
        var userAdmin = new Employee
        {
            FullName = admin, 
        };
        var accessTokenAdmin = _tokenService.GenerateToken(userAdmin);
        var dataAdmin = new LoginResponse
        {
            AccessToken = accessTokenAdmin,
            Email = admin, 
            RoleName = RoleName.Admin.ToString() 
        };

        return new Result<LoginResponse>
        {
            Data = dataAdmin,
            Messages =  ["Login successfully. Welcome Admin" ],
            ResultStatus = ResultStatus.Success.ToString()
        };
    }
    else if (user != null)
    {
        var role = user.Role?.RoleName ?? "User";
        var accessTokenUser = _tokenService.GenerateToken(user);
        var dataUser = new LoginResponse
        {
            AccessToken = accessTokenUser,
            Email = user.Email,
            RoleName = role
        };

        return new Result<LoginResponse>
        {
            Data = dataUser,
            Messages = [ "Login successfully. Welcome User"],
            ResultStatus = ResultStatus.Success.ToString()
        };
    }
    return new Result<LoginResponse>
    {
        ResultStatus = ResultStatus.NotFound.ToString(),
        Messages =["Login failed"]
    };
}
   
    /*public async Task<Result<UserResponse>> CreateStoreManagerAccount(CreateStoreManagerRequest request)
    {
        var isused = await _userRepository.GetUserByEmail(request.Email);
        var response = new Result<UserResponse>();
        if (isused != null)
        {
            response.Messages = new[] { "This mail is already used" };
            response.ResultStatus = ResultStatus.Duplicated.ToString();
            return response;
        }
        else
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            User? user = new User();
            user.Email = request.Email;
            *//*account.AccountId = new Guid();*//*
            user.Password = Convert.ToBase64String(passwordHash);
            user.Password = Convert.ToBase64String(passwordSalt);
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.RoleName = RoleName.StoreManager.ToString();
            user.Status = UserStatus.Active == UserStatus.Active;
            user.CreateAt = DateTime.UtcNow;

            var usernew = await _userRepository.Register(user);
            response.ResultStatus = ResultStatus.Success.ToString();
            response.Messages = ["Create StoreManager successfully"];
            response.Data = _mapper.Map<UserResponse>(user);
            return response;

        }
    }*/

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}