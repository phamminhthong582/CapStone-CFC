using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VnPayController : Controller
    {
        private readonly IVnPayService _orderService;

        public VnPayController(IVnPayService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("proceed-vnpay-payment")]
        public async Task<IActionResult> ProceedVnPayPayment( Guid paymentId)
        {
         
            
                var paymentUrl = _orderService.CreatePaymentUrlAsync(paymentId);
                return Ok(new { paymentUrl });
           
        }
        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallBack()
        {
            try
            {
                var response = _orderService.PaymentExecute(HttpContext.Request.Query);

                if (response == null || !int.TryParse(response.OrderDescription?.ToString(), out int orderId))
                    return Redirect("http://localhost:3000/payment-failure");

                return Redirect("http://localhost:3000/payment-failure");
            }
            catch (Exception)
            {
                return Redirect("http://localhost:3000/payment-failure");
            }
            // return Json(new { response });
        }
    }
}
