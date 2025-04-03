using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessObject.Entities;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Interface;

namespace Service.Implement;

public class JWTService : JWTKEY
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Iterations = 10000;
    private const char Delimiter = ';';
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private readonly IConfiguration _configuration;

    public JWTService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool VerifyPassword(string HashPassword, string InputPassword)
    {
        var elments = HashPassword.Split(Delimiter);
        var salt = Convert.FromBase64String(elments[0]);
        var hash = Convert.FromBase64String(elments[1]);

        var hashInput = Rfc2898DeriveBytes.Pbkdf2(InputPassword, salt, Iterations, _hashAlgorithmName, KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, hashInput);
    }

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", user.UserId.ToString()),
            new Claim("RoleId", user.RoleId.ToString()),
            new Claim(ClaimTypes.Role, "User")

        };
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["AppSettings:SerectKey"]));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_configuration["Tokens:Issuer"],
            _configuration["Tokens:Issuer"],
            claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
}