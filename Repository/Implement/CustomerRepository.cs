using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomFlowerChainContext _context;

    public CustomerRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<Customer?>> GetAllCustomer()
    {
        var cus = await _context.Customers.ToListAsync();
        return cus;
    }

    public async Task<Customer?> GetCustomerById(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.CustomerId == id);
        return customer;
    }

    public async Task<Customer?> RegisterCustomer(Customer customer)
    {
        var result = await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Customer?> UpdateCustomer(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> DeleteMember(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.CustomerId == id);
        if (customer == null)
        {
            return null;
        }
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> FindCustomerByPhone(string phone)
    {
        var cus = await _context.Customers.FirstOrDefaultAsync(x => x.Phone == phone);
        return cus;
    }

    public async Task<Customer?> FindCustomerByEmail(string email)
    {
        var cus = await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);
        return cus;
    }
}