using BusinessObject.Entities;

namespace Repository.Interface;

public interface IUserRepository
{
   /* Task<List<User?>> GetAllUsers();
    Task<User?> Register(User? user);*/
    string? GetAdminAccount(string email, string password);
/*    Task<User?> GetUserByEmail(string email);
*/
}