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
        //Authen
        services.AddScoped<IAuthService, AuthService>();
        //Token 
        services.AddScoped<ITokenService, TokenService>();
        services.AddAutoMapper(typeof(MapperConfig).Assembly);
        //User
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}