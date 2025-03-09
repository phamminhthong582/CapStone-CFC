using BusinessObject.DTO.Check;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckController : Controller
    {
        private readonly ICheckService checkService;

        public CheckController(ICheckService checkService)
        {
            this.checkService = checkService;
        }
        [HttpPost("CheckDelivery")]
        public async Task<IActionResult> CheckDeliveryByCheckOutProduct([FromQuery] Guid storeId, [FromBody] CheckProductRequest checkProductRequest)
        {
            try
            {
                var result = await checkService.CheckDeliveryByCheckOutProduct(storeId, checkProductRequest);
                if (result.isSuccess)
                {
                    return Ok(new { success = true, distance = result.distance });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error calculating delivery cost." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpPost("CheckDeliveryByProductCustom")]
        public async Task<IActionResult> CheckDeliveryByProductCustom([FromQuery] Guid storeId, [FromBody] CheckProductFlowerRequest checkProductFlowerRequest)
        {
            try
            {
                var result = await checkService.CheckDeliveryByProductCustom(storeId, checkProductFlowerRequest);
                if (result.isSuccess)
                {
                    return Ok(new { success = true, distance = result.distance });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error calculating delivery cost." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
