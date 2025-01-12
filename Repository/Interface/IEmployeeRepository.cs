using BusinessObject.Entities;

namespace Repository.Interface;

public interface IEmployeeRepository
{
    string? GetAdminAccount(string email, string password);
    Task<List<Employee?>> GetAllEmployees();
    Task<Employee?> GetEmployeeByEmail(string email);
}