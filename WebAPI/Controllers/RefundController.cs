using BusinessObject.DTO.Product;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RefundController : Controller
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }
        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(Guid OrderId)
        {
            await _refundService.CancelOrder(OrderId);
            return Ok(new BaseResponseModel<string>(
                       statusCode: StatusCodes.Status200OK,
                       code: ResponseCodeConstants.SUCCESS,
                       data: "Thêm sản phẩm mới thành công"));
        }

    }
}
