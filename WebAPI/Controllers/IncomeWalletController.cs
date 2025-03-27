using BusinessObject.DTO.IncomeWallet;
using BusinessObject.DTO.WithdrawMoney;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncomeWalletController : Controller
    {
        private readonly IIncomWalletService incomWalletService;

        public IncomeWalletController(IIncomWalletService incomWalletService)
        {
            this.incomWalletService = incomWalletService;
        }
        [HttpGet("GetInComWalletByWalletId")]
        public async Task<IActionResult> GetInComWalletByWalletId(Guid WalletId)
        {
            var result = await incomWalletService.GetInComWalletByWalletId(WalletId);
            return Ok(new BaseResponseModel<IEnumerable<IncomeWalletResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
    }
}
