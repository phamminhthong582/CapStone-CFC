using BusinessObject.DTO.Product;
using BusinessObject.DTO.WithdrawMoney;
using BusinessObject.Entities;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WithdrawMoneyController : Controller
    {
        private readonly IWithdrawMoneyService withdrawMoneyService;

        public WithdrawMoneyController(IWithdrawMoneyService withdrawMoneyService)
        {
            this.withdrawMoneyService = withdrawMoneyService;
        }
        [HttpGet("GetWithDrawMoneyWithWalletId")]
        public async Task<IActionResult> GetWithDrawMoneyWithWalletId(Guid WalletId)
        {
            var result = await withdrawMoneyService.GetWithDrawMoneyByWalletId(WalletId);
            return Ok(new BaseResponseModel<IEnumerable<WithdrawMoneyResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpGet("GetWithDrawMoney")]
        public async Task<IActionResult> GetWithDrawMoney()
        {
            var result = await withdrawMoneyService.GetWithDrawMoney();
            return Ok(new BaseResponseModel<IEnumerable<WithdrawMoneyResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpPost("CreateWithDrawMoney")]
        public async Task<IActionResult> CreateWithDrawMoney(Guid WalletId, WithdrawMoneyRequest withdrawMoneyRequest)
        {
            var withdrawMoneyId = await withdrawMoneyService.CreateWithdrawMoney(WalletId, withdrawMoneyRequest);
            return Ok(new BaseResponseModel<Guid>(
                       statusCode: StatusCodes.Status200OK,
                       code: ResponseCodeConstants.SUCCESS,
                       data: withdrawMoneyId));
        }

        [HttpPut("UpdateWithdrawMoneyId")]
        public async Task<IActionResult> UpdateStatusWithdrawMoney(Guid WithdrawMoneyId, string status)
        {
            await withdrawMoneyService.UpdateStatusWithdrawMoney(WithdrawMoneyId, status);
            return Ok(new BaseResponseModel<string>(
                         statusCode: StatusCodes.Status200OK,
                         code: ResponseCodeConstants.SUCCESS,
                         data: "cập nhật sẩn phẩm thành công"));
        }
      
        [HttpGet("GetWithDrawMoneyWitWithdrawMoneyId")]
        public async Task<IActionResult> GetWithDrawMoneyByWithdrawMoneyId(Guid WithdrawMoneyId)
        {
            var result = await withdrawMoneyService.GetWithDrawMoneyByWithdrawMoneyId(WithdrawMoneyId);
            return Ok(new BaseResponseModel<WithdrawMoneyResponse>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpDelete("DeleteWithdrawMoney")]
        public async Task<IActionResult> DeleteWithdrawMoney(Guid WithdrawMoneyId)
        {
            await withdrawMoneyService.DeleteWithdrawMoney(WithdrawMoneyId);

            return Ok(new BaseResponseModel<string>(
                             statusCode: StatusCodes.Status200OK,
                             code: ResponseCodeConstants.SUCCESS,
                             data: "xóa sản phẩm thành công"));
        }
        [HttpPost("ConfirmOPT")]
        public async Task<IActionResult> ConfirmOPT(Guid WithdrawMoneyId, string otp)
        {
            await withdrawMoneyService.ConfirmOPT(WithdrawMoneyId, otp);
            return Ok(new BaseResponseModel<string>(
                       statusCode: StatusCodes.Status200OK,
                       code: ResponseCodeConstants.SUCCESS,
                       data: "Thêm yêu cau rut tiền thành công"));
        }
    }
}
