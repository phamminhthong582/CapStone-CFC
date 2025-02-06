using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VnPayController : ControllerBase  // Đổi từ Controller -> ControllerBase cho đúng chuẩn API
    {
        private readonly IVnPayService _vnPayService;
        private readonly IPaymentService _paymentService;

        public VnPayController(IVnPayService vnPayService, IPaymentService paymentService)
        {
            _vnPayService = vnPayService;
            _paymentService = paymentService;
        }

        [HttpPost("proceed-vnpay-payment")]
        public async Task<IActionResult> ProceedVnPayPayment([FromBody] string paymentId)
        {
            if (string.IsNullOrEmpty(paymentId) || !Guid.TryParse(paymentId, out var parsedPaymentId))
            {
                return BadRequest(new { message = "Invalid paymentId format. It must be a valid GUID." });
            }

            var paymentUrl = await _vnPayService.CreatePaymentUrlAsync(parsedPaymentId);
            return Ok(new { paymentUrl });
        }

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallBack()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(HttpContext.Request.Query);

                if (response == null)
                {
                    Console.WriteLine("Response is null");
                    return Redirect("http://localhost:3000/payment-failure");
                }

                Console.WriteLine($"Order Description: {response.OrderDescription}");
                Console.WriteLine($"Response Code: {response.VnPayResponseCode}");

                if (!Guid.TryParse(response.OrderDescription, out Guid paymentId))
                {
                    Console.WriteLine("Invalid Payment ID");
                    return Redirect("http://localhost:3000/payment-failure");
                }

                if (response.VnPayResponseCode == "00")
                {
                    await _paymentService.UpdateStatusPayment(paymentId, "thành công");
                    return Redirect("http://localhost:3000/payment-success");
                }
                else
                {
                    await _paymentService.UpdateStatusPayment(paymentId, "thất bại");
                    return Redirect("http://localhost:3000/payment-failure");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Payment Callback: {ex.Message}");
                return Redirect("http://localhost:3000/payment-failure");
            }
        }
    }
    }

