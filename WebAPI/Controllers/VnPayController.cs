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
        private readonly IWalletService _walletService;

        public VnPayController(IVnPayService vnPayService, IPaymentService paymentService, IWalletService walletService)
        {
            _vnPayService = vnPayService;
            _paymentService = paymentService;
            _walletService = walletService;
        }

        [HttpPost("proceed-vnpay-payment")]
        public async Task<IActionResult> ProceedVnPayPayment([FromBody] string orderId)
        {
            if (string.IsNullOrEmpty(orderId) || !Guid.TryParse(orderId, out var parsedPaymentId))
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

                if (!Guid.TryParse(response.OrderDescription, out Guid orderId))
                {
                    Console.WriteLine("Invalid Payment ID");
                    return Redirect("http://localhost:5173/payment-failure");
                }

                if (response.VnPayResponseCode == "00")
                {
                    await _paymentService.CreatePayment(orderId);
                    return Redirect("http://localhost:5173/payment-success");
                }
                else
                {
                    return Redirect("http://localhost:5173/payment-failure");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Payment Callback: {ex.Message}");
                return Redirect("http://localhost:5173/payment-failure");
            }
        }
        [HttpPost("Deposit-vnpay-payment")]
        public async Task<IActionResult> CreateDepositWallet([FromBody] DepositWalletRequest request)
        {
            if (string.IsNullOrEmpty(request.WalletId) || !Guid.TryParse(request.WalletId, out var parsedWalletId))
            {
                return BadRequest(new { message = "Invalid walletId format. It must be a valid GUID." });
            }

            var paymentUrl = await _vnPayService.CreateDepositWallet(parsedWalletId, request.Price);
            return Ok(new { paymentUrl });
        }

        [HttpGet("payment-deposit-callback")]
        public async Task<IActionResult> PaymentDepositCallBack()
        {
            try
            {
                var response = _vnPayService.PaymentDepositExecute(HttpContext.Request.Query);

                if (response == null)
                {
                    Console.WriteLine("Response is null");
                    return Redirect("http://localhost:5173/payment-failure");
                }

                Console.WriteLine($"Order Description: {response.OrderDescription}");
                Console.WriteLine($"Response Code: {response.VnPayResponseCode}");

                if (!Guid.TryParse(response.OrderDescription, out Guid walletId))
                {
                    Console.WriteLine("Invalid Payment ID");
                    return Redirect("http://localhost:5173/payment-failure");
                }

                if (!double.TryParse(response.TotalPrice, out double totalPrice))
                {
                    Console.WriteLine("Invalid Total Price");
                    return Redirect("http://localhost:5173/payment-failure");
                }

                if (response.VnPayResponseCode == "00")
                {
                    await _walletService.DepositWallet(walletId, totalPrice / 100);
                    return Redirect("http://localhost:5173/payment-success");
                }
                else
                {
                    return Redirect("http://localhost:5173/payment-failure");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Payment Callback: {ex.Message}");
                return Redirect("http://localhost:5173/payment-failure");
            }
        }

        // DTO request model
        public class DepositWalletRequest
        {
            public string WalletId { get; set; }
            public double Price { get; set; }
        }
      
    }
    }

