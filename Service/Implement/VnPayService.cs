using BusinessObject.Entities;
using BusinessObject.Helper;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Service.Interface;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;

        public VnPayService(IConfiguration config, IUnitOfWork unitOfWork)
        {
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreatePaymentUrlAsync(Guid ordeId)
        {
            /*            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId);
            */
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(ordeId);
            if (order.Transfer == true)
            {
                return CreatePaymentUrl(ordeId, (decimal)order.OrderPrice);
            }
            else if (order.Transfer == false)
            {
                return CreatePaymentUrl(ordeId, (decimal)order.OrderPrice * 30 / 100);

            }
            return null; 
        }

        private string CreatePaymentUrl(Guid ordeId, decimal amount)
        {
            var vnpay = new VnPayLibrary();
            var orderIdStr = ordeId.ToString("N"); // Sử dụng format không có dấu gạch
            var amountStr = ((int)(amount * 100)).ToString();

            vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", amountStr);
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", orderIdStr);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VnPay:PaymentBackReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", orderIdStr);

            return vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"], _config["VnPay:HashSecret"]);
        }
     
        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, value.ToString());
            }

            bool isValidSignature = vnpay.ValidateSignature(collections["vnp_SecureHash"], _config["VnPay:HashSecret"]);
            if (!isValidSignature)
                return new VnPaymentResponseModel { Success = false };

            return new VnPaymentResponseModel
            {
                Success = true,
                PaymentId = "VnPay",
                OrderDescription = vnpay.GetResponseData("vnp_OrderInfo"),
                OrderId = vnpay.GetResponseData("vnp_TxnRef"),
                TransactionId = vnpay.GetResponseData("vnp_TransactionNo"),
                Token = collections["vnp_SecureHash"],
                VnPayResponseCode = vnpay.GetResponseData("vnp_ResponseCode")
            };
        }
    }
}

