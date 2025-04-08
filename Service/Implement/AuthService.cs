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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Identity;

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
    private readonly JWTKEY _jwtkey;

    public AuthService(IEmployeeRepository employeeRepository,JWTKEY jwtkey, ITokenService tokenService, IMapper mapper, IConfiguration configuration, IRoleRepository roleRepository, IMemoryCache cache, ICustomerRepository customerRepository, IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
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
        _jwtkey = jwtkey;
    }
    public class PasswordHasher
    {
        public static bool VerifyHashedPassword(string hashedPassword, string inputPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = Convert.FromBase64String(parts[1]);

            using (var hmac = new HMACSHA256(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                return storedHash.SequenceEqual(computedHash);
            }
        }
    }
    public async Task<Result<LoginResponse>> Login(string email, string password)
    {
        try
        {
            // Kiểm tra xem người dùng có phải là customer không
            var customer = await _customerRepository.FindCustomerByEmail(email);
            var user = await _unitOfWork.GetRepo<User>().Entities.FirstOrDefaultAsync(x => x.Email == email);

            if (customer != null)
            {
                // Kiểm tra trạng thái tài khoản customer
                if (user == null || user.Status == false)
                {
                    return new Result<LoginResponse>
                    {
                        ResultStatus = ResultStatus.Error.ToString(),
                        Messages = new[] { "Account is Not Verified! Please verify your account" }
                    };
                }
                var isValidPassword = PasswordHasher.VerifyHashedPassword(user.Password, password);

                // ✅ Kiểm tra mật khẩu đã hash
                if (isValidPassword)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, customer.Email),
                    new Claim(ClaimTypes.Role, RoleName.Customer.ToString()),
                    new Claim("Id", customer.CustomerId.ToString())
                };

                    var accessToken = _tokenService.GenerateAccessToken(claims);

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
                else
                {
                    return new Result<LoginResponse>
                    {
                        ResultStatus = ResultStatus.Error.ToString(),
                        Messages = new[] { "Invalid password for customer." }
                    };
                }
            }

            // Kiểm tra xem người dùng có phải là admin không
            var admin = _employeeRepository.GetAdminAccount(email, password);
            if (admin != null)
            {
                var claimsAdmin = new List<Claim>
            {
                new Claim(ClaimTypes.Role, RoleName.Admin.ToString())
            };

                var accessTokenAdmin = _tokenService.GenerateAccessToken(claimsAdmin);
                var dataAdmin = new LoginResponse
                {
                    AccessToken = accessTokenAdmin,
                    RoleName = RoleName.Admin.ToString()
                };

                return new Result<LoginResponse>
                {
                    Data = dataAdmin,
                    Messages = new[] { "Login successfully. Welcome Admin" },
                    ResultStatus = ResultStatus.Success.ToString()
                };
            }

            // Kiểm tra xem người dùng có phải là employee không
            var employee = await _employeeRepository.GetEmployeeByEmail(email);
            if (employee == null)
            {
                return new Result<LoginResponse>
                {
                    ResultStatus = ResultStatus.NotFound.ToString(),
                    Messages = new[] { "Account is not found" }
                };
            }

            if (employee.User.Role == null || string.IsNullOrEmpty(employee.User.Role.RoleName))
            {
                return new Result<LoginResponse>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { "Employee role is not defined." }
                };
            }

            // Lấy thông tin StoreId (nếu có)
            var storeId = employee.StoreId.HasValue ? employee.StoreId.ToString() : "";

            // Tạo token cho employee với StoreId
            var roleName = employee.User.Role.RoleName;
            var accessTokenEmployee = _tokenService.GenerateAccessToken(new List<Claim>
        {
            new Claim("Id", employee.EmployeeId.ToString()),
            new Claim(ClaimTypes.Name, employee.Email),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("StoreId", storeId)
        });

            // Tạo thông điệp chào mừng dựa trên vai trò của nhân viên
            string welcomeMessage = roleName switch
            {
                nameof(RoleName.StoreManager) => "Login successfully. Welcome Store Manager",
                nameof(RoleName.Florist) => "Login successfully. Welcome Florist",
                nameof(RoleName.Courier) => "Login successfully. Welcome Courier",
                _ => "Login successfully. Welcome"
            };

            var dataUser = new LoginResponse
            {
                AccessToken = accessTokenEmployee,
                Email = employee.Email,
                RoleName = roleName,
            };

            return new Result<LoginResponse>
            {
                Data = dataUser,
                Messages = new[] { welcomeMessage },
                ResultStatus = ResultStatus.Success.ToString()
            };
        }
        catch (Exception e)
        {
            return new Result<LoginResponse>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = new[] { e.Message }
            };
        }
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
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        var User = new User()
        {
            Email = request.Email,
            RoleId = roleId,
            CreateAt = vietnamTime,
            UpdateAt = vietnamTime,
            Status = false,
        };
        await _unitOfWork.GetRepo<User>().AddAsync(User);
        await _unitOfWork.CompleteAsync();
        var employee = new Employee
        {
            UserId = User.UserId,
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
            CreateAt = vietnamTime,
            RoleId = roleId
        };
        var registeredEmployee = await _employeeRepository.Register(employee);
        User.EmployeeId = employee.EmployeeId;
        _unitOfWork.GetRepo<User>().Update(User);
        await _unitOfWork.CompleteAsync();
        response.RoleName = roleToAssign;
        response.ResultStatus = ResultStatus.Success.ToString();
        response.Messages = new[] { $"Register successfully as a {roleToAssign}!" };
        response.Data = _mapper.Map<EmployeeResponse>(registeredEmployee);
        return response;
    }

    public async Task<Result<EmployeeResponse>> CreateCourierAccount(Guid StoreId, CreateCourierRequest request)
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
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        var User = new User()
        {
            Email = request.Email,
            RoleId = roleId,
            CreateAt = vietnamTime,
            UpdateAt = vietnamTime,
            Status = false,
        };
        await _unitOfWork.GetRepo<User>().AddAsync(User);
        await _unitOfWork.CompleteAsync();
        var employee = new Employee
        {
            UserId = User.UserId,
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
            CreateAt = vietnamTime
        };

        var registeredEmployee = await _employeeRepository.Register(employee);
        User.EmployeeId = employee.EmployeeId;
        _unitOfWork.GetRepo<User>().Update(User);
        await _unitOfWork.CompleteAsync();
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
        var a = await _unitOfWork.GetRepo<User>().Entities.FirstOrDefaultAsync(c => c.CustomerId == id);
        user.Status = CustomerStatus.Active.ToString();
        await _customerRepository.UpdateCustomer(user);
            a.Status = true;
            _unitOfWork.GetRepo<User>().Update(a);
            await _unitOfWork.CompleteAsync();
        response.Messages = ["Verify successfully at" + DateTime.UtcNow.ToString()];
        response.ResultStatus = ResultStatus.Success.ToString();
        return response;
    }
    response.Messages = ["Token is not correct or expired"];
    response.ResultStatus = ResultStatus.Error.ToString();
    return response;
}


    public void CreatePasswordHash(string password, out string hashedPassword)
    {
        using (var hmac = new HMACSHA256())
        {
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            hashedPassword = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
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
            string resetUrl = $"http://capstone-cfc-fe-user.vercel.app/reset-password?email={email}&token={token}";
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
            string resetUrl = $"http://capstone-cfc-fe-user.vercel.app/reset-password?email=${email}&token={token}";
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
        var user = await _unitOfWork.Repository<User>().Entities.FirstOrDefaultAsync(n => n.CustomerId == customer.CustomerId);


        if (customer != null && customer.Otp == token)
        {
            CreatePasswordHash(NewPassword, out string hashedPassword);
            user.Password = hashedPassword;// ✅ lưu cả salt

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
            CreatePasswordHash(NewPassword, out string hashedPassword);
            _unitOfWork.Repository<Employee>().Update(employee);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task<Customer> ProfileCustomer(Guid customerId)
    {
        var customer = await _unitOfWork.GetRepo<Customer>().GetByIdAsync(customerId);

        if (customer == null)
        {
            throw new Exception("Customer not found.");
        }

        return customer;
    }

    public async Task<Employee> ProfileEmployee(Guid employeeId)
    {
        var employee = await _unitOfWork.GetRepo<Employee>().GetByIdAsync(employeeId);

        if (employee == null)
        {
            throw new Exception("Customer not found.");
        }

        return employee;
    }

    public async Task ChangedPaswordForCustomer(Guid customerId,  string oldPassword,string newPassword)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
        var user = await _unitOfWork.Repository<User>().Entities.FirstOrDefaultAsync(n => n.CustomerId == customerId);
        CreatePasswordHash(newPassword, out string hashedPassword);
        var isValidPassword = PasswordHasher.VerifyHashedPassword(user.Password, oldPassword);

        if (isValidPassword)
        {
            user.Password = hashedPassword;// ✅ lưu cả salt
            _unitOfWork.Repository<Customer>().Update(customer);
            await _unitOfWork.CompleteAsync();
        }
        else {
            throw new Exception("Password is not correct.");
        }


    }

    //public async Task ChangedPaswordForEmployee(Guid employeeid , string newPassword)
    //{
    //    var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeid);
    //    employee.Password = newPassword;
    //    _unitOfWork.Repository<Employee>().Update(employee);
    //    await _unitOfWork.CompleteAsync();
    //}


}