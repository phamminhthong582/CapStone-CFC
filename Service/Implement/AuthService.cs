using System.Security.Claims;

using System.Security.Cryptography;

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
/*        var user = await _userRepository.GetUserByEmail(email);
*/        var admin = _userRepository.GetAdminAccount(email, password);
       /* if (user is null && admin is null)
        {
            return new Result<LoginResponse>()
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = ["Account is Not Found"]
            };
        }*/
       /* else if (admin != null)
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
            
        }*/
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

    /*public async Task<Result<UserResponse>> CreateStoreManagerAccount(CreateStoreManagerRequest request)
    {
        var isused = await _userRepository.GetUserByEmail(request.Email);
        var response = new Result<UserResponse>();
        if (isused != null)
        {
            response.Messages = new[] { "This mail is already used" };
            response.ResultStatus = ResultStatus.Duplicated.ToString();
            return response;
        }
        else
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            User? user = new User();
            user.Email = request.Email;
            *//*account.AccountId = new Guid();*//*
            user.Password = Convert.ToBase64String(passwordHash);
            user.Password = Convert.ToBase64String(passwordSalt);
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.RoleName = RoleName.StoreManager.ToString();
            user.Status = UserStatus.Active == UserStatus.Active;
            user.CreateAt = DateTime.UtcNow;

            var usernew = await _userRepository.Register(user);
            response.ResultStatus = ResultStatus.Success.ToString();
            response.Messages = ["Create StoreManager successfully"];
            response.Data = _mapper.Map<UserResponse>(user);
            return response;

        }
    }*/

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}