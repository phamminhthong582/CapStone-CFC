using BusinessObject.DTO.Product;
using BusinessObject.DTO.Style;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/flowerBaskets")]
    [ApiController]

    public class StyleController : Controller
    {
        private readonly IStyleService _styleService;

        public StyleController(IStyleService styleService)
        {
            _styleService = styleService;
        }

        [HttpGet("GetAllStyle")]
        public async Task<IActionResult> GetAllStyle()
        {
            var result = await _styleService.GetAllStyle();
            return Ok(new BaseResponseModel<IEnumerable<StyleResponse>>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               data: result));
        }
        [HttpGet("GetStyleById")]
        public async Task<IActionResult> GetStyleById(Guid Id)
        {
            var result = await _styleService.GetStyleById(Id);
            return Ok(new BaseResponseModel<StyleResponse>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               data: result));
        }
        [HttpPost("CreateStyle")]
        public async Task<IActionResult> CreateStyle([FromForm] StyleRequest styleRequest)
        {
            await _styleService.CreateStyle(styleRequest);
            return Ok(new BaseResponseModel<string>(
                       statusCode: StatusCodes.Status200OK,
                       code: ResponseCodeConstants.SUCCESS,
                       data: "Thêm sản phẩm mới thành công"));
        }
        [HttpPut("UpdateStyle/{styleId}")]
        public async Task<IActionResult> UpdateStyle( Guid styleId, [FromForm] StyleRequest styleRequest)
        {
            await _styleService.UpdateStyle(styleId, styleRequest);
            return Ok(new BaseResponseModel<string>(
                         statusCode: StatusCodes.Status200OK,
                         code: ResponseCodeConstants.SUCCESS,
                         data: "cập nhật sẩn phẩm thành công"));
        }
        [HttpDelete("DeleteStyle")]
        public async Task<IActionResult> DeleteStyle(Guid id)
        {
            await _styleService.DeleteStyle(id);

            return Ok(new BaseResponseModel<string>(
                             statusCode: StatusCodes.Status200OK,
                             code: ResponseCodeConstants.SUCCESS,
                             data: "xóa sản phẩm thành công"));


        }
    }
}
