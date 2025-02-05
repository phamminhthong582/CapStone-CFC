using BusinessObject.Entities;
using BusinessObject.Helper;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<string>  CreatePaymentUrlAsync(Guid  PaymentId)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(PaymentId);
            if (payment == null)
            {
                throw new Exception("Order not found");
            }
            var amount = payment.TotalPrice;
            return CreatePaymentUrl(PaymentId, (decimal)amount);

        }
        public string CreatePaymentUrl(Guid PaymentId, decimal amount)
        {
            var tick = DateTime.Now.Ticks.ToString().Substring(10);
            var vnpay = new VnPayLibrary();

            var ipAddr = "127.0.0.1"; //Config dai
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var orderIdStr = PaymentId.ToString();
            var amountStr = amount;

            var vnp_TmnCode = _config["VnPay:TmnCode"];
            var vnp_HashSecret = _config["VnPay:HashSecret"];
            var vnp_Url = _config["VnPay:BaseUrl"];
            var returnUrl = _config["VnPay:PaymentBackReturnUrl"];


            vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

            vnpay.AddRequestData("vnp_Amount", ((int)amountStr * 100).ToString()); //Số tiền thanh toán. Số tiền không 
            //mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND
            //(một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần(khử phần thập phân), sau đó gửi sang VNPAY
            //là: 10000000

            vnpay.AddRequestData("vnp_CreateDate", createDate);
            vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", ipAddr);
            vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);

            vnpay.AddRequestData("vnp_OrderInfo", orderIdStr);
            vnpay.AddRequestData("vnp_OrderType", "orther"); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderIdStr + tick); // Mã tham chiếu của giao dịch tại hệ 

            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }


        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }
            var vnp_orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VnPay:HashSecret"]);
            if (!checkSignature)
            {
                return new VnPaymentResponseModel
                {
                    Success = false,
                };
            }
            return new VnPaymentResponseModel
            {
                Success = true,
                PaymentId = "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_orderId.ToString(),
                TransactionId = vnp_TransactionId.ToString(),
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode,
            };
        }
    }
}
