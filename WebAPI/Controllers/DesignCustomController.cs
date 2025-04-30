using BusinessObject.DTO.DesignCustom;
using BusinessObject.DTO.Order;
using BusinessObject.Entities;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/DesignCustom")]
    [ApiController]
    public class DesignCustomController : Controller
    {
        private IDesignCustomService designCustomService;

        public DesignCustomController(IDesignCustomService designCustomService)
        {
            this.designCustomService = designCustomService;
        }
        [HttpPost("CreateDesignCustomByCustomer")]
        public async Task<IActionResult> CreateDesignCustomByCustomer([FromForm] DesignCustomByCustomerRequest designCustomByCustomerRequest, Guid customerId)
        {
            await designCustomService.CreateDesignCustomByCustomer(designCustomByCustomerRequest, customerId);
            return Ok(new BaseResponseModel<string>(
                     statusCode: StatusCodes.Status200OK,
                     code: ResponseCodeConstants.SUCCESS,
                     data: "cập nhật sẩn phẩm thành công"));
        }
        [HttpPut("UpdateDesignCustomByStaff")]
        public async Task<IActionResult> UpdateDesignCustomByStaff([FromForm] DesignCustomByStaffRequest designCustomByStaffRequest, Guid DesginCustom)
        {
            await designCustomService.UpdateDesignCustomByStaff(designCustomByStaffRequest, DesginCustom);
            return Ok(new BaseResponseModel<string>(
                     statusCode: StatusCodes.Status200OK,
                     code: ResponseCodeConstants.SUCCESS,
                     data: "cập nhật sẩn phẩm thành công"));
        }
        [HttpPut("UpdateDesignCustomByCustomer")]
        public async Task<IActionResult> UpdateDesignCustomByCustomer(UpdateOrderDesignCustomByCustomer updateOrderDesignCustomByCustomer, Guid DesginCustom)
        {
            var result = await designCustomService.UpdateDesignCustomByCustomer(updateOrderDesignCustomByCustomer, DesginCustom);

            if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Code = "NotFound",
                    Message = "Không tìm thấy thiết kế để cập nhật"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Code = "Success",
                Message = "Cập nhật thiết kế thành công",
                OrderId = result
            });
        }
        [HttpGet("GetDesignCustomByCustomer")]
        public async Task<IActionResult> GetDesignCustomByCustomer(Guid customer)
        {
            var result = await designCustomService.GetDesignCustomByCustomer(customer);
            return Ok(new BaseResponseModel<IEnumerable<DesignCustomBuCustomerResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpGet("GetDesignCustomByStore")]
        public async Task<IActionResult> GetDesignCustomByStore(Guid store)
        {
            var result = await designCustomService.GetDesignCustomByStore(store);
            return Ok(new BaseResponseModel<IEnumerable<DesignCustomBuCustomerResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpGet("GetDesignCustomByStaff")]
        public async Task<IActionResult> GetDesignCustomByStaff(Guid staff)
        {
            var result = await designCustomService.GetDesignCustomByStaff(staff);
            return Ok(new BaseResponseModel<IEnumerable<DesignCustomBuCustomerResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpGet("GetDesignCustomById")]
        public async Task<IActionResult> GetDesignCustomById(Guid id)
        {
            var result = await designCustomService.GetDesignCustomById(id);
            return Ok(new BaseResponseModel<DesignCustomBuCustomerResponse>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));

        }
        [HttpDelete("DeleteDesignCustom")]
        public async Task<IActionResult> DeleteDesignCustom(Guid id)
        {
            await designCustomService.DeleteDesignCustom(id);
            return Ok(new BaseResponseModel<string>(
                     statusCode: StatusCodes.Status200OK,
                     code: ResponseCodeConstants.SUCCESS,
                     data: "cập nhật sẩn phẩm thành công"));
        }
    }
}
