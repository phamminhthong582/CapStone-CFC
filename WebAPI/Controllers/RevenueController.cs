using BusinessObject.DTO.Revenue;
using BusinessObject.DTO.Role;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenueController : Controller
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }
        [HttpGet("GetRevenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var result = await _revenueService.GetGeneralRevenue();
            return Ok(new BaseResponseModel<RevenueResponse>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: result));
        }
        [HttpGet("GetRevenueByStoreId")]
        public async Task<IActionResult> GetRevenueByStoreId(Guid storeId)
        {
            var result = await _revenueService.GetRevenueByStoreId(storeId);
            return Ok(new BaseResponseModel<RevenueResponse>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: result));
        }
    }
}
