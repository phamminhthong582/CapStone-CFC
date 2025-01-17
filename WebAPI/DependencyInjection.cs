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
        services.AddScoped<IEmployeeService, EmployeeService>();
        //Store
        services.AddScoped<IStoreService, StoreService>();
        // Category
        services.AddScoped<ICategoryService, CategoryService>(); 
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        // Promotion
        services.AddScoped<IPromotionService, PromotionService>(); 
        services.AddScoped<IPromotionRepository, PromotionRepository>();
       
        // Role
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddAutoMapper(typeof(MapperConfig).Assembly);
        return services;
    }
}