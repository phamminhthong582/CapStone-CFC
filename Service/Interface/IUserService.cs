using BusinessObject.DTO.Response;

namespace Service.Interface;

public interface IUserService
{
    Task<List<UserResponse>> GetAllUsers();
    
    
}