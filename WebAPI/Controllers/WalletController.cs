using BusinessObject.DTO.Product;
using BusinessObject.DTO.Wallet;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : Controller
    {
        private readonly IWalletService walletService;

        public WalletController(IWalletService walletService)
        {
            this.walletService = walletService;
        }

        [HttpGet("GetWalletByCustomerId")]
        public async Task<IActionResult> GetWalletByCustomerId(Guid CustomerId)
        {
            var result = await walletService.GetWallet(CustomerId);
            return Ok(new BaseResponseModel<WalletResponse>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: result));

        }
        [HttpGet("GetWalletByAdmin")]
        public async Task<IActionResult> GetWalletByAdmin(Guid id)
        {
            var result = await walletService.GetWalletByAdmin(id);
            return Ok(new BaseResponseModel<WalletResponse>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: result));

        }
        [HttpGet("CheckWallet")]
        public async Task<IActionResult> CheckWallet(Guid CustomerId)
        {
            var result = await walletService.CheckWallet(CustomerId);
            return Ok(new BaseResponseModel<bool>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: result));

        }
        [HttpPost("CreateWallet")]
        public async Task<IActionResult> CreateWallet(Guid CusomterId, string PasswordWallet)
        {
            await walletService.CreateWallet(CusomterId, PasswordWallet);
            return Ok(new BaseResponseModel<string>(
                      statusCode: StatusCodes.Status200OK,
                      code: ResponseCodeConstants.SUCCESS,
                      data: "Thêm wallet mới thành công"));
        }

        [HttpPost("PaymentByWallet")]
        public async Task<IActionResult> PaymentByWallet(Guid OrderId, string PasswordWallet)
        {
            await walletService.PaymentByWallet(OrderId, PasswordWallet);
            return Ok(new BaseResponseModel<string>(
                      statusCode: StatusCodes.Status200OK,
                      code: ResponseCodeConstants.SUCCESS,
                      data: "Thêm wallet mới thành công"));
        }

    }
}
