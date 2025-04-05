using BusinessObject.Entities;
using CloudinaryDotNet;

namespace Service.Interface;

public interface JWTKEY
{
    public bool VerifyPassword(string HashPassword, string InputPassword);
    public string Hash(string password);
    public string GenerateToken(User user);
}