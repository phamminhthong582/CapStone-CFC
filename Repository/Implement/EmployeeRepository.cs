using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Interface;

namespace Repository.Implement;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly CustomFlowerChainContext _context;

    public EmployeeRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }

    public string? GetAdminAccount(string email, string password)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        // Check if the configuration key exists
        if (config.GetSection("AdminAccount").Exists())
        {
            string? emailJson = config["AdminAccount:adminemail"];
            string? passwordJson = config["AdminAccount:adminpassword"];

            // Check if both email and password match
            if (emailJson == email && passwordJson == password)
            {
                return emailJson;
            }
        }

        return null;
    }

    public async Task<List<Employee?>> GetAllEmployees()
    {
        var emplist = await _context.Employees.ToListAsync();
        return emplist;
    }

    public async Task<Employee?> GetEmployeeByEmail(string email)
    {
        return await _context.Employees
            .Include(e => e.Role) 
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<Employee?> GetEmployeesById(Guid id)
    {
        var employ = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
        return employ;
    }

    public async Task<Employee?> UpdateEmployee(Employee? employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> Register(Employee? employee)
    {
        var result = await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Employee?> DeleteEmployee(Guid id)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
        if (employee == null)
        {
            return null;
        }
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> FindEmployeeByPhone(string phone)
    {
        var employ = await _context.Employees.FirstOrDefaultAsync(x => x.Phone == phone);
        return employ;
    }
}