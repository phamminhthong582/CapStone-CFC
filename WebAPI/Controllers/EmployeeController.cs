using System.Net;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Employee;
using BusinessObject.DTO.Product;
using BusinessObject.Entities;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/employees")]
[ApiController]

public class EmployeeController : ControllerBase
{
   private readonly IEmployeeService _employeeService;

   public EmployeeController(IEmployeeService employeeService)
   {
      _employeeService = employeeService;
   }
   [HttpGet]
   public async Task<IActionResult> GetEmployee()
   {
      var result = await _employeeService.GetAllEmployee();
      return Ok(result);
   }
    [HttpGet("AllEmployeeWithStoreId")]
    public async Task<IActionResult> GetAllEmployeeByStoreId(Guid StoreId)
    {
        var result = await _employeeService.GetAllEmployeeByStoreId(StoreId);
        return Ok(new BaseResponseModel<IEnumerable<EmployeeResponse>>(
           statusCode: StatusCodes.Status200OK,
           code: ResponseCodeConstants.SUCCESS,
           data: result));
    }
    [HttpGet("Id")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        var result = await _employeeService.GetEmployeeById(id);
        return Ok(new BaseResponseModel<EmployeeResponse>(
          statusCode: StatusCodes.Status200OK,
          code: ResponseCodeConstants.SUCCESS,
          data: result));


    }
    [HttpGet("storeId-florist-status-true")]
    public async Task<IActionResult> GetFloristWithStoreIdWithStatusTrue(Guid storeid)
    {
        var result = await _employeeService.GetFloristWithStoreIdWithStatusTrue(storeid);
        return Ok(new BaseResponseModel<IEnumerable<EmployeeResponse>>(
             statusCode: StatusCodes.Status200OK,
             code: ResponseCodeConstants.SUCCESS,
             data: result));

    }
    [HttpPost("ApproveEmployee")]
    public async Task<IActionResult> ApproveEmployee(Guid employeeId)
    {
        await _employeeService.ApproveEmployee(employeeId);
        return Ok(new BaseResponseModel<string>(
                   statusCode: StatusCodes.Status200OK,
                   code: ResponseCodeConstants.SUCCESS,
                   data: "Thêm sản phẩm mới thành công"));
    }
    [HttpGet("storeId-courier-status-true")]
    public async Task<IActionResult> GetCourierWithStoreIdWithStatusTrue(Guid storeid)
    {
        var result = await _employeeService.GetCourierWithStoreIdWithStatusTrue(storeid);

        return Ok(new BaseResponseModel<IEnumerable<EmployeeResponse>>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               data: result));
    }
    [HttpGet("storeId-florist-status-false")]
   public async Task<IActionResult> GetFloristWithStoreId(Guid storeid)
   {
      var result = await _employeeService.GetFloristWithStoreIdWithStatusFalse(storeid);
        return Ok(new BaseResponseModel<IEnumerable<EmployeeResponse>>(
             statusCode: StatusCodes.Status200OK,
             code: ResponseCodeConstants.SUCCESS,
             data: result));

    }
   [HttpGet("storeId-courier-status-false")]
   public async Task<IActionResult> GetCourierWithStoreId(Guid storeid)
   {
      var result = await _employeeService.GetCourierWithStoreIdWithStatusFalse(storeid);

        return Ok(new BaseResponseModel<IEnumerable<EmployeeResponse>>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               data: result));
    }
 
   [HttpPut("{employeeId}")]
   public async Task<IActionResult> UpdateEmployee([FromRoute] Guid employeeId,
      [FromBody] UpdateEmployeeRequest request)
   {
      var result = await _employeeService.UpdateEmployee(employeeId, request);

      if (result.ResultStatus != ResultStatus.Success.ToString())
         return StatusCode((int)HttpStatusCode.InternalServerError, result);

      return Ok(result);
   }
   [HttpDelete("delete-employee")]
   public async Task<ActionResult<Result<Employee>>> DeleteEmployee(Guid id)
   {
      var result = await _employeeService.DeleteEmployee(id);
      if (result.ResultStatus != ResultStatus.Success.ToString())
      {
         return StatusCode((int)HttpStatusCode.InternalServerError, result);
      }
      return Ok(result);
   }
}