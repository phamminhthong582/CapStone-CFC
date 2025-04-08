using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Employee;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using Repository.Interface;
using Service.Interface;
using System.Net.Mail;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.Utilities.Net;
using Microsoft.AspNetCore.Identity;

namespace Service.Implement;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudinaryService _cloudinaryService;

    public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper, IRoleRepository roleRepository, IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<List<EmployeeResponse>> GetAllEmployee()
    {
        var list = await _employeeRepository.GetAllEmployees();
        return _mapper.Map<List<EmployeeResponse>>(list);
    }

    public async Task<EmployeeResponse> GetEmployeeById(Guid id)
    {
        if (id == Guid.Empty) // Kiểm tra StoreId hợp lệ
        {
            throw new Exception("Employee not found");
        }
        var employee = await _unitOfWork.Repository<Employee>()
                .Entities
                .Include(n => n.User).ThenInclude(a => a.Role)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
        var employeeResponse = new EmployeeResponse
        {
            EmployeeId = employee.EmployeeId,
            FullName = employee.FullName,
            Address = employee.Address,
            Email = employee.Email,
            Phone = employee.Phone,
            Gender = employee.Gender,
            Birthday = employee.Birthday,
            IdentificationNumber = employee.IdentificationNumber,
            IdentificationFontOfPhoto = employee.IdentificationFontOfPhoto,
            IdentificationBackOfPhoto = employee.IdentificationBackOfPhoto,
            RoleName = employee.User.Role.RoleName,
            StoreId = employee.StoreId,
            Status = employee.Status,
            Avatar = employee.Avatar,
        };
        return employeeResponse;
    }
    public async Task<IEnumerable<EmployeeResponse>> GetAllEmployeeByStoreId(Guid StoreId)
    {
        if (StoreId == Guid.Empty) // Kiểm tra StoreId hợp lệ
        {
            throw new Exception("Store not found");
        }

        var employees = await _unitOfWork.Repository<Employee>()
            .Entities
                .Include(n => n.User).ThenInclude(a => a.Role)
            .Where(m => m.StoreId == StoreId)
            .Where(a => a.Status == true)
            .ToListAsync();

        var rolePriority = new List<string> { "StoreManager", "Florist", "Courier" };

        var employeeResponse = employees
            .Select(employee => new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Gender = employee.Gender,
                Birthday = employee.Birthday,
                IdentificationNumber = employee.IdentificationNumber,
                IdentificationFontOfPhoto = employee.IdentificationFontOfPhoto,
                IdentificationBackOfPhoto = employee.IdentificationBackOfPhoto,
                RoleName = employee.User.Role.RoleName,
                StoreId = employee.StoreId,
                Status = employee.Status,
                Avatar = employee.Avatar,
            })
            .OrderBy(e => rolePriority.IndexOf(e.RoleName)) // Sắp xếp theo thứ tự ưu tiên
            .ToList();

        return employeeResponse;
    }
    public async Task<IEnumerable<EmployeeResponse>> GetFloristWithStoreIdWithStatusFalse(Guid storeId)
    {
        if (storeId == Guid.Empty) // Kiểm tra StoreId hợp lệ
        {
            throw new Exception("Store not found");
        }

        var employees = await _unitOfWork.Repository<Employee>()
            .Entities
              .Include(n => n.User).ThenInclude(a => a.Role)
            .Where(m => m.StoreId == storeId) // Chỉ lọc theo StoreId trước
            .ToListAsync(); // Lấy danh sách về trước

        // Lọc tiếp theo RoleName và Status
        var florists = employees
            .Where(e => e.User.Role != null
                     && e.User.Role.RoleName == RoleName.Florist.ToString()
                     && !(e.Status ?? false))
            .ToList();

        // Chuyển danh sách đã lọc sang EmployeeResponse
        var employeeResponse = florists
            .Select(employee => new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Gender = employee.Gender,
                Birthday = employee.Birthday,
                IdentificationNumber = employee.IdentificationNumber,
                IdentificationFontOfPhoto = employee.IdentificationFontOfPhoto,
                IdentificationBackOfPhoto = employee.IdentificationBackOfPhoto,
                RoleName = employee.User.Role.RoleName,
                StoreId = employee.StoreId,
                Status = employee.Status,
                Avatar = employee.Avatar,
            })
            .ToList();

        return employeeResponse;
    }


    public async Task<IEnumerable<CourierResponse>> GetCourierWithStoreIdWithStatusFalse(Guid storeid)
    {
        if (storeid == Guid.Empty) // Kiểm tra StoreId hợp lệ
        {
            throw new Exception("Store not found");
        }

        var employees = await _unitOfWork.Repository<Employee>()
            .Entities
              .Include(n => n.User).ThenInclude(a => a.Role)
            .Where(m => m.StoreId == storeid) // Chỉ lọc theo StoreId trước
            .ToListAsync(); // Lấy danh sách về trước

        // Lọc tiếp theo RoleName và Status
        var florists = employees
            .Where(e => e.User.Role != null
                     && e.User.Role.RoleName == RoleName.Courier.ToString()
                     && !(e.Status ?? false)) // Chỉ lấy Status == false hoặc null
            .ToList();

        // Chuyển danh sách đã lọc sang EmployeeResponse
        var employeeResponse = florists
            .Select(employee => new CourierResponse
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Gender = employee.Gender,
                Birthday = employee.Birthday,
                IdentificationNumber = employee.IdentificationNumber,
                IdentificationFontOfPhoto = employee.IdentificationFontOfPhoto,
                IdentificationBackOfPhoto = employee.IdentificationBackOfPhoto,
                RoleName = employee.User.Role.RoleName,
                StoreId = employee.StoreId,
                Status = employee.Status,
                Avatar = employee.Avatar,
                MotoType = employee.MotoType,
                NumberMoto = employee.NumberMoto,
                ColorMoto = employee.ColorMoto,

            })
            .ToList();

        return employeeResponse;
    }

    public async Task<IEnumerable<EmployeeResponse>> GetFloristWithStoreIdWithStatusTrue(Guid storeId)
    {
        if (storeId == Guid.Empty) // Kiểm tra StoreId hợp lệ
        {
            throw new Exception("Store not found");
        }

        var employees = await _unitOfWork.Repository<Employee>()
            .Entities
             .Include(n => n.User).ThenInclude(a => a.Role)
            .Where(m => m.StoreId == storeId) // Chỉ lọc theo StoreId trước
            .ToListAsync(); // Lấy danh sách về trước

        // Lọc tiếp theo RoleName và Status
        var florists = employees
            .Where(e => e.User.Role != null
                     && e.User.Role.RoleName == RoleName.Florist.ToString()
                     && (e.Status ?? true)) // Chỉ lấy Status == false hoặc null
            .ToList();

        // Chuyển danh sách đã lọc sang EmployeeResponse
        var employeeResponse = florists
            .Select(employee => new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Gender = employee.Gender,
                Birthday = employee.Birthday,
                IdentificationNumber = employee.IdentificationNumber,
                IdentificationFontOfPhoto = employee.IdentificationFontOfPhoto,
                IdentificationBackOfPhoto = employee.IdentificationBackOfPhoto,
                RoleName = employee.User.Role.RoleName,
                StoreId = employee.StoreId,
                Status = employee.Status,
                Avatar = employee.Avatar,
            })
            .ToList();

        return employeeResponse;
    }


    public async Task<IEnumerable<CourierResponse>> GetCourierWithStoreIdWithStatusTrue(Guid storeid)
    {
        if (storeid == Guid.Empty) // Kiểm tra StoreId hợp lệ
        {
            throw new Exception("Store not found");
        }

        var employees = await _unitOfWork.Repository<Employee>()
            .Entities
             .Include(n => n.User).ThenInclude(a => a.Role)
            .Where(m => m.StoreId == storeid) // Chỉ lọc theo StoreId trước
            .ToListAsync(); // Lấy danh sách về trước

        // Lọc tiếp theo RoleName và Status
        var florists = employees
            .Where(e => e.User.Role != null
                     && e.User.Role.RoleName == RoleName.Courier.ToString()
                     && (e.Status ?? true)) // Chỉ lấy Status == false hoặc null
            .ToList();

        // Chuyển danh sách đã lọc sang EmployeeResponse
        var employeeResponse = florists
            .Select(employee => new CourierResponse
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Gender = employee.Gender,
                Birthday = employee.Birthday,
                IdentificationNumber = employee.IdentificationNumber,
                IdentificationFontOfPhoto = employee.IdentificationFontOfPhoto,
                IdentificationBackOfPhoto = employee.IdentificationBackOfPhoto,
                RoleName = employee.User.Role.RoleName,
                StoreId = employee.StoreId,
                Status = employee.Status,
                Avatar = employee.Avatar,
                MotoType = employee.MotoType,
                NumberMoto = employee.NumberMoto,
                ColorMoto = employee.ColorMoto,
            })
            .ToList();

        return employeeResponse;
    }
    //public async Task<Result<EmployeeResponse>> UpdateEmployee(Guid id, UpdateEmployeeRequest request)
    //{
    //    var response = new Result<EmployeeResponse>();
    //    var employee = await _employeeRepository.GetEmployeesById(id);
    //    if (employee == null)
    //    {
    //        response.Messages = ["User is not found!"];
    //        response.ResultStatus = ResultStatus.NotFound.ToString();
    //        return response;
    //    }
    //    else if (request.Phone.Equals(employee.Phone) && request.FullName.Equals(employee.FullName))
    //    {
    //        response.Data = _mapper.Map<EmployeeResponse>(employee);
    //        response.Messages = ["Nothing change!"];
    //        response.ResultStatus = ResultStatus.Error.ToString();
    //        return response;
    //    }

    //    var isPhoneExisted = await _employeeRepository.FindEmployeeByPhone(request.Phone);
    //    if (isPhoneExisted != null)
    //    {
    //        response.Messages = new[] { "This is already existed" };
    //        response.ResultStatus = ResultStatus.Error.ToString();
    //        return response;
    //    }
    //    var newemployee = _mapper.Map(request, employee);
    //    response.Data = _mapper.Map<EmployeeResponse>(await _employeeRepository.UpdateEmployee(newemployee));
    //    response.Messages = ["Update successfully"];
    //    response.ResultStatus = ResultStatus.Success.ToString();
    //    return response;
    //}

    //public async Task<Result<Employee>> DeleteEmployee(Guid id)
    //{
    //    var employee = await _employeeRepository.GetEmployeesById(id);

    //    if (employee == null)
    //    {
    //        return new Result<Employee>
    //        {
    //            ResultStatus = ResultStatus.NotFound.ToString(),
    //            Messages = new []{"Employee not found."}
    //        };
    //    }

    //    await _employeeRepository.DeleteEmployee(id);

    //    return new Result<Employee>
    //    {
    //        ResultStatus = ResultStatus.Success.ToString(),
    //        Messages = new []{"Employee deleted successfully."}
    //    };
    //}
    public async Task ApproveEmployee(Guid employeeId)
    {
        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId);
        var user = await _unitOfWork.Repository<User>().Entities.FirstOrDefaultAsync(m => m.EmployeeId == employeeId);

        if (employee == null)
        {
            throw new Exception("Employee not found");
        }

        if (employee.Status == false && user.Status == false)
        {
            employee.Status = true;
            user.Status = true;

            // Tạo mật khẩu ngẫu nhiên
            string newPassword = PasswordGenerator.GenerateRandomPassword(12);

            // Hash mật khẩu
            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.Password = $"{Convert.ToBase64String(passwordSalt)}:{Convert.ToBase64String(passwordHash)}";

            _unitOfWork.Repository<User>().Update(user);
            _unitOfWork.Repository<Employee>().Update(employee);
            await _unitOfWork.CompleteAsync();

            // Gửi email chứa mật khẩu mới
            await SendEmailAsync(employee.Email, "Your New Password",
                $"Your account has been approved. Your new password is: <strong>{newPassword}</strong>");
        }
    }
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }


    public async Task Reject(Guid employeeId, string reason)
    {
        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId);
        var user = await _unitOfWork.Repository<User>().Entities.FirstOrDefaultAsync(m => m.EmployeeId == employeeId);

        if (employee == null)
        {
            throw new Exception("Employee not found");
        }

        if (employee.Status != false)
        {
            throw new Exception("Employee is not in a rejectable state.");
        }

        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            throw new Exception("Employee does not have a valid email address.");
        }

        // Gửi email thông báo từ chối
        await SendEmailAsync(employee.Email, "Your Account Rejection",
            $"Your account has been rejected. Reason: <strong>{reason}</strong>");

        // Xóa nhân viên sau khi gửi email thành công
        _unitOfWork.Repository<User>().Delete(user);

        _unitOfWork.Repository<Employee>().Delete(employee);
        await _unitOfWork.CompleteAsync();
    }
    //public async Task UpdateStatusEmloyee(Guid employeeid, bool status)
    //{
    //    var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeid);
    //    if (employee == null)
    //    {
    //        throw new Exception("Employee not found");
    //    }

    //    employee.Status = status;
    //    string newPassword = PasswordGenerator.GenerateRandomPassword(12);
    //    employee.Password = newPassword;

    //    _unitOfWork.Repository<Employee>().Update(employee);
    //    await _unitOfWork.CompleteAsync();



    //}
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using (var smtpClient = new SmtpClient("smtp.gmail.com"))
        {
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential("minhthongpham9a2@gmail.com", "nmcf zksq weyr wphx");
            smtpClient.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
    public class PasswordGenerator
    {
        public static string GenerateRandomPassword(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var crypto = new RNGCryptoServiceProvider();
            var data = new byte[length];

            crypto.GetBytes(data);

            var result = new StringBuilder(length);
            foreach (var b in data)
            {
                result.Append(chars[b % chars.Length]);
            }
            return result.ToString();
        }
    }
    public async Task CreateManagerStore(Guid storeid, CreateManagerStoreRequest createManagerStoreRequest)
    {

        var store = await _unitOfWork.Repository<Store>().GetByIdAsync(storeid);
        if (store == null)
        {
            throw new Exception("store not found");
        }
        var folderName = $"Employee/{createManagerStoreRequest.Email}";

        var avatarUrl = createManagerStoreRequest.Avatar != null
           ? await _cloudinaryService.UploadImageAsync(createManagerStoreRequest.Avatar.OpenReadStream(), $"{folderName}/avatar")
           : null;
        var User = new User
        {
            Email = createManagerStoreRequest.Email,
            Password = createManagerStoreRequest.Password,
            RoleId = Guid.Parse("a7ad79e3-a5e8-4e85-a672-41c95a2e37ac"),
            CreateAt = DateTime.Now,
            UpdateAt = DateTime.Now,
            Status = true,
        };
        await _unitOfWork.GetRepo<User>().AddAsync(User);
        await _unitOfWork.CompleteAsync();
        var employee = new Employee
        {
            UserId = User.UserId,
            StoreId = storeid,
            FullName = createManagerStoreRequest.FullName,
            Address = createManagerStoreRequest.Address,
            Email = createManagerStoreRequest.Email,
            Gender = createManagerStoreRequest.Gender,
            Phone = createManagerStoreRequest.Phone,
            Birthday = createManagerStoreRequest.Birthday,
            RoleId = Guid.Parse("a7ad79e3-a5e8-4e85-a672-41c95a2e37ac"),
            Status = true,
            Avatar = avatarUrl,
        };
        User.EmployeeId = employee.EmployeeId;
        _unitOfWork.GetRepo<User>().Update(User);
        await _unitOfWork.Repository<Employee>().AddAsync(employee);
        await _unitOfWork.CompleteAsync();
    }
}