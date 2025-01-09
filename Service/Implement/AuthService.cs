using System.Security.Claims;
using AutoMapper;
using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Response;
using BusinessObject.Entities;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, IMapper mapper,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _mapper = mapper;
        _configuration = configuration;
    }
    public async Task<Result<LoginResponse>> Login(string email, string password)
    {
        var user = await _userRepository.GetUserByEmail(email);
        var admin = _userRepository.GetAdminAccount(email, password);
        if (user is null && admin is null)
        {
            return new Result<LoginResponse>()
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = ["Account is Not Found"]
            };
        }
        else if (admin != null)
        {
            var userAdmin = new User
            {
                FullName = admin,
                RoleName = RoleName.Admin.ToString() // Điều chỉnh nếu cần
            };
            var accessTokenadmin = _tokenService.GenerateToken(userAdmin);

            var dataadmin = new LoginResponse()
            {
                AccessToken = accessTokenadmin,
                Email = admin,
                RoleName = RoleName.Admin.ToString()
            };
            return new Result<LoginResponse>()
            {
                Data = dataadmin,
                Messages = ["Login successfully. Welcome Admin"],
                ResultStatus = ResultStatus.Success.ToString()
            };
            
        }
        return new Result<LoginResponse>()
        {
            ResultStatus = ResultStatus.NotFound.ToString(),
            Messages = ["Login failed"]
        };
    }

    public async Task<Result<UserResponse>> Register(RegisterRequest request)
    {
        throw new NotImplementedException();
    }
}