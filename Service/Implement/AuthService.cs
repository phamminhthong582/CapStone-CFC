using System.Net.Mail;
using System.Net;
using System.Security.Claims;

using System.Security.Cryptography;

using AutoMapper;
using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Employee;
using BusinessObject.DTO.Response;
using BusinessObject.Entities;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IRoleRepository _roleRepository;
    private readonly string tempdata = "tempdatakey";
    private readonly string newpass = "newpasskey";
    private readonly IMemoryCache _cache;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudinaryService _cloudinaryService;

    public AuthService(IEmployeeRepository employeeRepository, ITokenService tokenService, IMapper mapper, IConfiguration configuration, IRoleRepository roleRepository, IMemoryCache cache, ICustomerRepository customerRepository, IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
    {
        _employeeRepository = employeeRepository;
        _tokenService = tokenService;
        _mapper = mapper;
        _configuration = configuration;
        _roleRepository = roleRepository;
        _cloudinaryService = cloudinaryService;
        _cache = cache;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Login(string email, string password)
{
    var customer = await _customerRepository.FindCustomerByEmail(email);
    if (customer != null)
    {
        if (customer.Status == ResultStatus.NotVerified.ToString()) 
        {
            return new Result<LoginResponse>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = new[] { "Account is Not Verified! Please verify your account" }
            };
        }

        if (customer.Password == password)
        {
            var accessToken = _tokenService.GenerateToken(customer);
            var dataCustomer = new LoginResponse
            {
                AccessToken = accessToken,
                Email = customer.Email,
                RoleName = RoleName.Customer.ToString()
            };

            return new Result<LoginResponse>
            {
                RoleName = RoleName.Customer.ToString(),
                Data = dataCustomer,
                Messages = new[] { "Login successfully. Welcome Customer" },
                ResultStatus = ResultStatus.Success.ToString()
            };
        }
    }

    var employee = await _employeeRepository.GetEmployeeByEmail(email);
    var admin = _employeeRepository.GetAdminAccount(email, password);

    if (employee is null && admin is null)
    {
        return new Result<LoginResponse>
        {
            ResultStatus = ResultStatus.NotFound.ToString(),
            Messages = ["Account is not found"]
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
            Messages = ["Login successfully. Welcome Admin"],
            ResultStatus = ResultStatus.Success.ToString()
        };
    }
    else if (employee != null)
    {
        if (employee.Role == null || string.IsNullOrEmpty(employee.Role.RoleName))
        {
            return new Result<LoginResponse>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = ["Employee role is not defined."]
            };
        }

        var roleName = employee.Role.RoleName;
        var accessToken = _tokenService.GenerateToken(employee);
        string welcomeMessage = roleName switch
        {
            nameof(RoleName.StoreManager) => "Login successfully. Welcome Store Manager",
            nameof(RoleName.Florist) => "Login successfully. Welcome Florist",
            nameof(RoleName.Courier) => "Login successfully. Welcome Courier",
            nameof(RoleName.Customer) => "Login successfully. Welcome Customer",
            _ => "Login successfully. Welcome"
        };

        var dataUser = new LoginResponse
        {
            AccessToken = accessToken,
            Email = employee.Email,
            RoleName = roleName
        };

        return new Result<LoginResponse>
        {
            Data = dataUser,
            Messages = [welcomeMessage],
            ResultStatus = ResultStatus.Success.ToString()
        };
    }

    return new Result<LoginResponse>
    {
        ResultStatus = ResultStatus.NotFound.ToString(),
        Messages = ["Login failed"]
    };
}

    public async Task<Result<EmployeeResponse>> RegisterFlorist(Guid StoreId, RegisterRequest request)
    {
        var response = new Result<EmployeeResponse>();
        var isMailUsed = await _employeeRepository.FindEmployeeByEmail(request.Email);
        if (isMailUsed != null)
        {
            response.Messages = new[] { "This email is already used" };
            response.ResultStatus = ResultStatus.Duplicated.ToString();
            return response;
        }

        var isPhoneUsed = await _employeeRepository.FindEmployeeByPhone(request.Phone);
        if (isPhoneUsed != null)
        {
            response.Messages = new[] { "This phone number is already used" };
            response.ResultStatus = ResultStatus.Duplicated.ToString();
            return response;
        }

        var roleToAssign = RoleName.Florist.ToString();
        var roleId = await _roleRepository.GetRoleIdByName(roleToAssign);
        if (roleId == null)
        {
            response.Messages = new[] { $"Role '{roleToAssign}' not found. Please contact admin." };
            response.ResultStatus = ResultStatus.Failed.ToString();
            return response;
        }
        var folderName = $"Employee/{request.Email}";

        var avatarUrl = request.Avatar != null
            ? await _cloudinaryService.UploadImageAsync(request.Avatar.OpenReadStream(), $"{folderName}/avatar")
            : null;

        var idFrontUrl = request.IdentificationFontOfPhoto != null
            ? await _cloudinaryService.UploadImageAsync(request.IdentificationFontOfPhoto.OpenReadStream(), $"{folderName}/id_front")
            : null;

        var idBackUrl = request.IdentificationBackOfPhoto != null
            ? await _cloudinaryService.UploadImageAsync(request.IdentificationBackOfPhoto.OpenReadStream(), $"{folderName}/id_back")
            : null;


        /*        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
        */
        var employee = new Employee
        {
            Email = request.Email,
            FullName = request.FullName,
            Address = request.Address,
            Gender = request.Gender,
            Birthday = request.Birthday,
            StoreId = StoreId,
            IdentificationNumber = request.IdentificationNumber,
            Avatar = avatarUrl, // Lưu URL ảnh vào database
            IdentificationFontOfPhoto = idFrontUrl,
            IdentificationBackOfPhoto = idBackUrl,
            Phone = request.Phone,
            Status = false,
            CreateAt = DateTime.UtcNow,
            RoleId = roleId
        };
        var registeredEmployee = await _employeeRepository.Register(employee);
        response.RoleName = roleToAssign;
        response.ResultStatus = ResultStatus.Success.ToString();
        response.Messages = new[] { $"Register successfully as a {roleToAssign}!" };
        response.Data = _mapper.Map<EmployeeResponse>(registeredEmployee);
        return response;
    }

   public async Task<Result<EmployeeResponse>> CreateCourierAccount(Guid StoreId,CreateCourierRequest request)
{
    var response = new Result<EmployeeResponse>();
    var isEmailUsed = await _employeeRepository.FindEmployeeByEmail(request.Email);
    if (isEmailUsed != null)
    {
        response.Messages = new[] { "This email is already used" };
        response.ResultStatus = ResultStatus.Duplicated.ToString();
        return response;
    }
    var roleId = await _roleRepository.GetRoleIdByName(RoleName.Courier.ToString());
    if (roleId == null)
    {
        response.Messages = new[] { "Role 'Courier' not found. Please contact admin." };
        response.ResultStatus = ResultStatus.Failed.ToString();
        return response;
    }
        var folderName = $"Employee/{request.Email}";

        var avatarUrl = request.Avatar != null
            ? await _cloudinaryService.UploadImageAsync(request.Avatar.OpenReadStream(), $"{folderName}/avatar")
            : null;

        var idFrontUrl = request.IdentificationFontOfPhoto != null
            ? await _cloudinaryService.UploadImageAsync(request.IdentificationFontOfPhoto.OpenReadStream(), $"{folderName}/id_front")
            : null;

        var idBackUrl = request.IdentificationBackOfPhoto != null
            ? await _cloudinaryService.UploadImageAsync(request.IdentificationBackOfPhoto.OpenReadStream(), $"{folderName}/id_back")
            : null;
        var employee = new Employee
    {
        Email = request.Email,
        FullName = request.FullName,
        Phone = request.Phone,
        RoleId = roleId.Value, 
        MotoType = request.MotoType,
        StoreId = StoreId,
        NumberMoto = request.NumberMoto,
        Gender = request.Gender,    
        Birthday = request.Birthday,    
        ColorMoto = request.ColorMoto,
        Avatar = avatarUrl, // Lưu URL ảnh vào database
        IdentificationFontOfPhoto = idFrontUrl,
        IdentificationBackOfPhoto = idBackUrl,
        IdentificationNumber = request.IdentificationNumber,
        Status = false, 
        CreateAt = DateTime.UtcNow
    };
    
    var registeredEmployee = await _employeeRepository.Register(employee);
    response.ResultStatus = ResultStatus.Success.ToString();
    response.Messages = new[] { "Create Courier successfully" };
    response.Data = _mapper.Map<EmployeeResponse>(registeredEmployee);
    return response;
}

public async Task<Result<string>> VerifyEmail(Guid id, string token)
{
    var response = new Result<string>();
    if (string.IsNullOrEmpty(token))
    {
        response.Messages = ["Token is null"];
        response.ResultStatus = ResultStatus.Error.ToString();
        return response;
    }

    if (token.Equals(_cache.Get<string>(tempdata)))
    {
        var user = await _customerRepository.FindOne(c => c.CustomerId == id);
        user.Status = CustomerStatus.Active.ToString();
        await _customerRepository.UpdateCustomer(user);
        response.Messages = ["Verify successfully at" + DateTime.UtcNow.ToString()];
        response.ResultStatus = ResultStatus.Success.ToString();
        return response;
    }
    response.Messages = ["Token is not correct or expired"];
    response.ResultStatus = ResultStatus.Error.ToString();
    return response;
}


// public async Task<Result<EmployeeResponse>> CreateStoreManagerAccount(CreateCourierRequest request)
//     {
//         var isused = await _employeeRepository.FindEmployeeByEmail(request.Email);
//         var response = new Result<EmployeeResponse>();
//         if (isused != null)
//         {
//             response.Messages = new[] { "This mail is already used" };
//             response.ResultStatus = ResultStatus.Duplicated.ToString();
//             return response;
//         }
//         else
//         {
//             CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
//             Employee? employee = new Employee();
//             employee.Email = request.Email;
//             *//*account.AccountId = new Guid();
//               employee.Password = Convert.ToBase64String(passwordHash);
//             employee.Password = Convert.ToBase64String(passwordSalt);
//             employee.FullName = request.FullName;
//             employee.Phone = request.Phone;
//             employee.Role = RoleName.StoreManager.ToString();
//             employee.Status = UserStatus.Active == UserStatus.Active;
//             employee.CreateAt = DateTime.UtcNow;
//
//             var usernew = await _userRepository.Register(user);
//             response.ResultStatus = ResultStatus.Success.ToString();
//             response.Messages = ["Create StoreManager successfully"];
//             response.Data = _mapper.Map<UserResponse>(user);
//             return response;
//
//         }
//     }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

    public async Task ForgotPasswordForCustomer(string email)
    {
        var customer = (await _unitOfWork.Repository<Customer>().GetAllAsync())
            .FirstOrDefault(n => n.Email == email);

        if (customer != null)
        {
            // 1️⃣ Tạo token reset mật khẩu
            var token = Guid.NewGuid().ToString(); // Có thể thay bằng JWT hoặc mã hash bảo mật hơn

            // 2️⃣ Lưu token vào database (tuỳ theo hệ thống của bạn)
            // (Ví dụ, có thể thêm thuộc tính ResetPasswordToken vào Customer/Employee và lưu vào DB)

            // 3️⃣ Tạo link reset mật khẩu
            string resetUrl = $"https://yourwebsite.com/reset-password?email={email}&token={token}";
            customer.Otp = token;
            _unitOfWork.Repository<Customer>().Update(customer);
            await _unitOfWork.CompleteAsync();
            // 4️⃣ Gửi email reset mật khẩu
            string subject = "Reset Your Password";
            string body = $"Click vào link sau để đặt lại mật khẩu: <a href='{resetUrl}'>Reset Password</a>";

            await SendEmailAsync(email, subject, body);
        }
    }
    public async Task ForgotPasswordForEmployee(string email)
    {
        var emplyee = (await _unitOfWork.Repository<Employee>().GetAllAsync())
            .FirstOrDefault(n => n.Email == email);

        if (emplyee != null)
        {
            // 1️⃣ Tạo token reset mật khẩu
            var token = Guid.NewGuid().ToString(); // Có thể thay bằng JWT hoặc mã hash bảo mật hơn

            // 2️⃣ Lưu token vào database (tuỳ theo hệ thống của bạn)
            // (Ví dụ, có thể thêm thuộc tính ResetPasswordToken vào Customer/Employee và lưu vào DB)

            // 3️⃣ Tạo link reset mật khẩu
            string resetUrl = $"https://yourwebsite.com/reset-password?email={email}&token={token}";
            emplyee.Otp = token;
            _unitOfWork.Repository<Employee>().Update(emplyee);
            await _unitOfWork.CompleteAsync();
            // 4️⃣ Gửi email reset mật khẩu
            string subject = "Reset Your Password";
            string body = $"Click vào link sau để đặt lại mật khẩu: <a href='{resetUrl}'>Reset Password</a>";

            await SendEmailAsync(email, subject, body);
        }
    }
    // Hàm gửi email (có thể dùng thư viện như MailKit hoặc SMTP client)
    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using (var smtpClient = new SmtpClient("smtp.gmail.com"))

        {
            smtpClient.Credentials = new NetworkCredential("minhthongpham9a2@gmail.com", "nmcf zksq weyr wphx");
            smtpClient.EnableSsl = true;
            smtpClient.Port = 587; // Cổng SMTP (thay đổi theo nhà cung cấp)

            var mailMessage = new MailMessage
            {
                From = new MailAddress("minhthongpham9a2@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
    public async Task SetPasswordForCustomer(string email, string NewPassword, string token)
    {
        var customer = (await _unitOfWork.Repository<Customer>().GetAllAsync())
            .FirstOrDefault(n => n.Email == email);

       
        if (customer != null && customer.Otp == token)
        {
            CreatePasswordHash(NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            customer.Password = Convert.ToBase64String(passwordHash);

            _unitOfWork.Repository<Customer>().Update(customer);
            await _unitOfWork.CompleteAsync();
        }
       
    }
    public async Task SetPasswordForEmployee(string email, string NewPassword, string token)
    {
        var employee = (await _unitOfWork.Repository<Employee>().GetAllAsync())
            .FirstOrDefault(n => n.Email == email);
        if (employee != null && employee.Otp == token)
        {
            CreatePasswordHash(NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            employee.Password = Convert.ToBase64String(passwordHash);
            _unitOfWork.Repository<Employee>().Update(employee);
            await _unitOfWork.CompleteAsync();
        }
    }
}