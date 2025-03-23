using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Employee;
using BusinessObject.DTO.Response;

namespace Service.Interface;

public interface IAuthService
{
    //Task<Result<LoginResponse>> Login(string email, string password);
    Task ForgotPasswordForCustomer(string email);
    Task ForgotPasswordForEmployee(string email);

    Task<Result<EmployeeResponse>> RegisterFlorist(Guid StoreId, RegisterRequest request);
    Task<Result<EmployeeResponse>> CreateCourierAccount(Guid StoreId, CreateCourierRequest request);
    Task<Result<string>> VerifyEmail(Guid id, string token);
    //Task SetPasswordForCustomer(string email, string NewPassword, string token);
    Task SetPasswordForEmployee(string email, string NewPassword, string token);
    //Task ChangedPaswordForCustomer(Guid customerId, string newPassword);
    //Task ChangedPaswordForEmployee(Guid employeeid, string newPassword);


}