using System.Net;
using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Customer;
using BusinessObject.DTO.Employee;
using BusinessObject.DTO.Response;
using Core.Infrastructures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[ApiController]
[Route("api/auth")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICustomerService _customerService;

    public AuthController(IAuthService authService , ICustomerService customerService)
    {
        _authService = authService;
        _customerService = customerService;
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
    [HttpPost("register-Florist-account")]
    public async Task<ActionResult<Result<EmployeeResponse>>> Register([FromForm] RegisterRequest registerRequest)
    {
        try
        {
            var result = await _authService.RegisterFlorist(registerRequest);

            
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
    [HttpPost("register-courier-account")]
    public async Task<ActionResult<Result<EmployeeResponse>>> CreateCourierAccount(
        [FromForm] CreateCourierRequest registerRequest)
    {
        var result = await _authService.CreateCourierAccount(registerRequest);

        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return result;
    }
    [HttpPost("register-customer-account")]
    public async Task<ActionResult<Result<CustomerResponse>>> CreateCustomerAccount(
        [FromBody] CreateCustomerRequest registerRequest)
    {
        var result = await _customerService.RegisterCustomer(registerRequest);

        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return result;
    }
    [HttpGet("confirm-email")]
    public async Task<IActionResult> VerifyEmail(Guid id, string token)
    {
        var result = await _authService.VerifyEmail(id, token);
        if (result.ResultStatus == ResultStatus.Success.ToString())
            return Redirect($"http://localhost:5243/swagger/index.html");

        return Redirect($"https://giveawayproject.jettonetto.org/verify-email?verificationstatus=failed");
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
    [HttpPost("forgot-password-by-customer")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
         await  _authService.ForgotPasswordForCustomer(email);
        return Ok(new BaseResponseModel<string>(
                        statusCode: StatusCodes.Status200OK,
                        code: ResponseCodeConstants.SUCCESS,
                        data: "Thêm sản phẩm mới thành công"));
    }
    [HttpPost("forgot-password-by-employee")]
    public async Task<IActionResult> ForgotPasswordByEmployee(string email)
    {
        await _authService.ForgotPasswordForEmployee(email);
        return Ok(new BaseResponseModel<string>(
                        statusCode: StatusCodes.Status200OK,
                        code: ResponseCodeConstants.SUCCESS,
                        data: "Thêm sản phẩm mới thành công"));
    }
    [HttpPost("set-password-by-customer")]
    public async Task<IActionResult> SetPasswordForCustomer(string email, string NewPassword, string token)
    {
        await _authService.SetPasswordForCustomer(email, NewPassword, token);
        return Ok(new BaseResponseModel<string>(
                        statusCode: StatusCodes.Status200OK,
                        code: ResponseCodeConstants.SUCCESS,
                        data: "Thêm sản phẩm mới thành công"));
    }
    [HttpPost("set-password-by-employee")]
    public async Task<IActionResult> SetPasswordForEmployee(string email, string NewPassword, string token)
    {
        await _authService.SetPasswordForEmployee(email, NewPassword, token);
        return Ok(new BaseResponseModel<string>(
                        statusCode: StatusCodes.Status200OK,
                        code: ResponseCodeConstants.SUCCESS,
                        data: "Thêm sản phẩm mới thành công"));
    }
}