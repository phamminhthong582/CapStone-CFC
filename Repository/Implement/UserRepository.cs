using BusinessObject.Entities;
using BusinessObject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Interface;

namespace Repository.Implement;

public class UserRepository  : IUserRepository
{
    private readonly CustomFlowerChainContext _context;

    public UserRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<User?>> GetAllUsers()
    {
        var userlist = await _context.Users.ToListAsync();
        return userlist;
    }

    public async Task<User?> Register(User? user)
    {
        var result = await _context.Users.AddAsync(user);
        return result.Entity;
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
            string passwordJson = config["AdminAccount:adminpassword"];

            // Check if both email and password match
            if (emailJson == email && passwordJson == password)
            {
                return emailJson;
            }
        }

        return null;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }
}