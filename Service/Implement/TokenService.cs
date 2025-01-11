using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessObject.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.CompilerServices;
using Service.Interface;

namespace Service.Implement;

public class TokenService : ITokenService
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Iterations = 10000;
    private const char Delimiter = ';';
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GenerateToken(Employee employee)
    {
        var secretKey = _configuration["Jwt:Key"];
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var secretKryByte = Encoding.UTF8.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.FullName!),
            new("Id", employee.EmployeeId.ToString()),
             new(ClaimTypes.Role, employee.RoleId.ToString()),
            new("FullName", employee.FullName!)
        };

        if (!string.IsNullOrEmpty(employee.Avatar))
            claims.Add(new Claim("Avatar", employee.Avatar));
        else
            claims.Add(new Claim("Avatar", ""));

        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(secretKryByte), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescription);
        return jwtTokenHandler.WriteToken(token);
    }
}