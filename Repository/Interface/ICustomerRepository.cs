using BusinessObject.Entities;

namespace Repository.Interface;

public interface ICustomerRepository
{
    Task<List<Customer?>> GetAllCustomer();
    Task<Customer?> GetCustomerById(Guid id);
    Task<Customer?> RegisterCustomer(Customer customer);
    Task<Customer?> UpdateCustomer(Customer customer);
    Task<Customer?> DeleteMember(Guid id);
    Task<Customer?> FindCustomerByPhone(string phone);
    Task<Customer?> FindCustomerByEmail(string email);
}