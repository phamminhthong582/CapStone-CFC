using BusinessObject.DTO.Request;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : Controller
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpPost]
        public async Task<IActionResult> AddStore(StoreRequest storeRequest)
        {
            await _storeService.AddStore(storeRequest);
            return Ok(new BaseResponseModel<string>(
                          statusCode: StatusCodes.Status200OK,
                          code: ResponseCodeConstants.SUCCESS,
                          data: "Thêm cửa hàng mới thành công"));
        }
    }
}
