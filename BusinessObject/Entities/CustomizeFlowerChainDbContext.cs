using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessObject.Entities;

public class CustomizeFlowerChainDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public CustomizeFlowerChainDbContext(DbContextOptions<CustomizeFlowerChainDbContext> options) : base(options)
    {
    }

    public CustomizeFlowerChainDbContext()
    {
    }
    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("Server=localhost;uid=sa;pwd=123456;Database=CustomeFlowerChain;Trusted_Connection=true;TrustServerCertificate=True");
            optionsBuilder.UseSqlServer(connectionString);
        }

        optionsBuilder.EnableSensitiveDataLogging();
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        #region User

        builder.Entity<User>()
            .ToTable("User")
            .HasKey(x => x.UserId);
        

        #endregion

        /*builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());*/
    }
}