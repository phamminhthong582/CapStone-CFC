using System.Net;
using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Employee;
using BusinessObject.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[ApiController]
[Route("api/auth")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest loginRequest)
    {
        var result = await _authService.Login(loginRequest.Email, loginRequest.Password);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return Ok(result);
    }
    [HttpPost("register")]
    public async Task<ActionResult<Result<EmployeeResponse>>> Register([FromBody] RegisterRequest registerRequest, [FromQuery] string? roleName = null)
    {
        try
        {
            // Gọi service để xử lý logic đăng ký
            var result = await _authService.Register(registerRequest, roleName);

            // Kiểm tra kết quả trả về
            if (result.ResultStatus == ResultStatus.Duplicated.ToString())
            {
                return Conflict(result); // HTTP 409 Conflict
            }
            if (result.ResultStatus == ResultStatus.Failed.ToString())
            {
                return BadRequest(result); // HTTP 400 Bad Request
            }

            // Thành công
            return Ok(result); // HTTP 200 OK
        }
        catch (Exception ex)
        {
            // Xử lý lỗi hệ thống không mong muốn
            return StatusCode((int)HttpStatusCode.InternalServerError, new
            {
                Message = "An unexpected error occurred. Please try again later.",
                Error = ex.Message
            });
        }
    }
   /* [Authorize(Roles = "Admin")]
    [HttpPost("create-storemanager-account")]
    public async Task<ActionResult<UserResponse>> CreateStoreManagerAccount(
      [FromBody] CreateStoreManagerRequest registerRequest)
    {
        var result = await _authService.CreateStoreManagerAccount(registerRequest);

        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return Ok(result);
    }*/

}