using BusinessObject.DTO.Accessory;
using BusinessObject.DTO.Style;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/accessory")]
    [ApiController]
    public class AccessoryController : Controller
    {
        private readonly IAccessoryService _accessoryService;

        public AccessoryController(IAccessoryService accessoryService)
        {
            _accessoryService = accessoryService;
        }
        [HttpGet("GetAllAccessory")]
        public async Task<IActionResult> GetAllAccessory()
        {
            var result = await _accessoryService.GetAllAccessory();
            return Ok(new BaseResponseModel<IEnumerable<AccessoryResponse>>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               data: result));
        }
        [HttpGet("GetAccessoryById")]
        public async Task<IActionResult> GetAccessoryById(Guid id)
        {
            var result = await _accessoryService.GetAccessoryById(id);
            return Ok(new BaseResponseModel<AccessoryResponse>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               data: result));
        }
        [HttpPost("CreateAccessory")]
        public async Task<IActionResult> CreateAccessory([FromForm] AccessoryRequest accessoryRequest)
        {
            await _accessoryService.CreateAccessory(accessoryRequest);
            return Ok(new BaseResponseModel<string>(
                       statusCode: StatusCodes.Status200OK,
                       code: ResponseCodeConstants.SUCCESS,
                       data: "Thêm sản phẩm mới thành công"));
        }
        [HttpPut("UpdateAccessory/{AccessoryId}")]
        public async Task<IActionResult> UpdateStyle(Guid AccessoryId, [FromForm] AccessoryRequest accessoryRequest)
        {
            await _accessoryService.UpdateAccessory(AccessoryId, accessoryRequest);
            return Ok(new BaseResponseModel<string>(
                         statusCode: StatusCodes.Status200OK,
                         code: ResponseCodeConstants.SUCCESS,
                         data: "cập nhật sẩn phẩm thành công"));
        }
        [HttpDelete("DeleteAccessory")]
        public async Task<IActionResult> DeleteAccessory(Guid id)
        {
            await _accessoryService.DeleteAccessory(id);

            return Ok(new BaseResponseModel<string>(
                             statusCode: StatusCodes.Status200OK,
                             code: ResponseCodeConstants.SUCCESS,
                             data: "xóa sản phẩm thành công"));


        }
    }
}
