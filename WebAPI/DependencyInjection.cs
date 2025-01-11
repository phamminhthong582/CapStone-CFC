using Repository.Implement;
using Repository.Interface;
using Service.AutoMapper;
using Service.Implement;
using Service.Interface;

namespace WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        //Unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        //Authen
        services.AddScoped<IAuthService, AuthService>();
        //Token 
        services.AddScoped<ITokenService, TokenService>();
        //Employee
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        //Store
        services.AddScoped<IStoreService, StoreService>();
        // Category
        services.AddScoped<ICategoryService, CategoryService>(); 
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        services.AddAutoMapper(typeof(MapperConfig).Assembly);
        return services;
    }
}