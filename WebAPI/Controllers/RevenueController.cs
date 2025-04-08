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
        [HttpGet("GetLossByStoreId")]
        public async Task<IActionResult> GetLossByStoreId(Guid storeId)
        {
            var result = await _revenueService.GetLossByStoreId(storeId);
            return Ok(new BaseResponseModel<RevenueResponse>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: result));
        }
        [HttpGet("GetLoss")]
        public async Task<IActionResult> GetLoss()
        {
            var result = await _revenueService.GetGeneralLoss();
            return Ok(new BaseResponseModel<RevenueResponse>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: result));
        }
        [HttpGet("GetTotalOrdersByStoreId")]
        public async Task<IActionResult> GetTotalOrdersByStoreId(Guid storeId)
        {
            var result = await _revenueService.GetTotalOrdersByStoreId(storeId);
            return Ok(new BaseResponseModel<TotalOrderResponse>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: result));
        }
        [HttpGet("GetTotalOrder")]
        public async Task<IActionResult> GetTotalOrder()
        {
            var result = await _revenueService.GetTotalOrder();
            return Ok(new BaseResponseModel<TotalOrderResponse>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: result));
        }

        [HttpGet("GetAllStoreRevenue")]
        public async Task<IActionResult> GetAllStoreRevenue() { 
        
            var result = await _revenueService.GetAllStoreRevenue();
            return Ok(new BaseResponseModel<IEnumerable<StoreRevenue>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: result));
        }
    }
}
