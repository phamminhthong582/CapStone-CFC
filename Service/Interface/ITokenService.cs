using System.Security.Claims;
using BusinessObject.Entities;

namespace Service.Interface;

public interface ITokenService
{
    public string GenerateAccessToken(List<Claim> claims);
}