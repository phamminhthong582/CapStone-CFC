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
    [HttpPost("register-staffaccount")]
    public async Task<ActionResult<Result<EmployeeResponse>>> Register([FromBody] RegisterRequest registerRequest)
    {
        try
        {
            var result = await _authService.Register(registerRequest);

            
            if (result.ResultStatus == ResultStatus.Duplicated.ToString())
            {
                return Conflict(result); 
            }
            if (result.ResultStatus == ResultStatus.Failed.ToString())
            {
                return BadRequest(result);
            }
            return Ok(result); 
        }
        catch (Exception ex)
        {
           
            return StatusCode((int)HttpStatusCode.InternalServerError, new
            {
                Message = "An unexpected error occurred. Please try again later.",
                Error = ex.Message
            });
        }
    }
    [HttpPost("create-courier-account")]
    public async Task<ActionResult<Result<EmployeeResponse>>> CreateCourierAccount(
        [FromBody] CreateCourierRequest registerRequest)
    {
        var result = await _authService.CreateCourierAccount(registerRequest);

        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return result;
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