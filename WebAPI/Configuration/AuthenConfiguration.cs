namespace WebAPI.Configuration;

using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
public static class DependencyInjection
{
    public static void AuthenConfiguration(this IServiceCollection services, WebApplicationBuilder builder,
        IConfiguration config)
    {
        var secretKeyBytes = Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:SecretKey"]);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                //auto generate token
                ValidateIssuer = false,
                ValidateAudience = false,

                //sign in token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                ClockSkew = TimeSpan.Zero
            };
        });
        //UserPolicy check id user có trùng với id yêu cầu hay không
        services.AddAuthorization(options =>
        {
            options.AddPolicy("UserPolicy", policy =>
                policy.RequireAssertion(context =>
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var requestedId = context.Resource as Guid?;

                    return Guid.TryParse(userIdClaim, out var userId) && userId == requestedId;
                }));
        });

       
    }
}