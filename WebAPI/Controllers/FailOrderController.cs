using BusinessObject.DTO.Employee;
using BusinessObject.DTO.FailOrder;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/failOrder")]
    [ApiController]
    public class FailOrderController : Controller
    {
         private readonly IFailOrderService failOrderService;

        public FailOrderController(IFailOrderService failOrderService)
        {
            this.failOrderService = failOrderService;
        }
        [HttpGet("GetFailOrderByOrderId")]
        public async Task<IActionResult> GetFailOrderByOrderId(Guid orderID)
        {
            var result = await failOrderService.GetFailOrderByOrderId(orderID);
            return Ok(new BaseResponseModel<FailOrderResponse>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               data: result));
        }

    }
}
