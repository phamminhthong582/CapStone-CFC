using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Response;

namespace Service.Interface;

public interface IAuthService
{
    Task<Result<LoginResponse>> Login(string email, string password);
    Task<Result<UserResponse>> Register(RegisterRequest request);
    /*    Task<Result<UserResponse>> CreateStoreManagerAccount(CreateStoreManagerRequest request);
    */
}