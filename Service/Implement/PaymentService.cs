using BusinessObject.DTO.Email;
using BusinessObject.DTO.Payment;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly INotiService _notiService;

        public PaymentService(IUnitOfWork unitOfWork, IEmailService emailService, INotiService notiService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _notiService = notiService;
        }

        public async Task CreatePayment(Guid OrderId)
        {
            var order = await _unitOfWork.Repository<Order>().Entities.Include(m => m.Customer).Include(n =>n.ProductCustom).FirstOrDefaultAsync(n => n.OrderId == OrderId);
            var isPaymentCreated = false;
            var designcustom = await _unitOfWork.Repository<DesignCustom>().Entities.FirstOrDefaultAsync(n => n.OrderId == OrderId);
            var wallet = await _unitOfWork.Repository<Wallet>().Entities
                .FirstOrDefaultAsync(n => n.WalletId == Guid.Parse("55d9964b-8543-4b74-96d6-e0ab2ce86d3f"));
            if (order.Transfer == true && order.Status == "Pending Payment")
            {
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Method = "100% transfer",
                    StoreId = order.StoreId,
                    TotalPrice = order.OrderPrice,
                    CreateAt = vietnamTime,
                    Status = "Payment Successfully",
                };
                await _unitOfWork.Repository<Payment>().AddAsync(payment);
                await _unitOfWork.CompleteAsync();

                if (order.DesignCustomId == null) {
                    order.Status = "Order Successfully";
                    _unitOfWork.Repository<Order>().Update(order);
                    await _unitOfWork.CompleteAsync();
                }
                else if(order.DesignCustomId != null)
                {
                    order.Status = "Arranging & Packing";

                    _unitOfWork.Repository<Order>().Update(order);
                    designcustom.Status = "Design Successfully";
                    _unitOfWork.Repository<DesignCustom>().Update(designcustom);

                    await _unitOfWork.CompleteAsync();

                }
                await SendPaymentConfirmationEmail(order);
                //wallet.TotalPrice += payment.TotalPrice;
                //_unitOfWork.Repository<Wallet>().Update(wallet);
                //var incomeWallet = new IncomeWallet
                //{
                //    WalletID = wallet.WalletId,
                //    IncomePrice = payment.TotalPrice,
                //    Method = "Payment",
                //    Status = "Successfull",
                //    CreateAt = vietnamTime,
                //    UpdateAt = vietnamTime,
                //};
                //await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);

                await _unitOfWork.CompleteAsync();
                isPaymentCreated = true;
            }

            else if (order.Transfer == false && order.Status == "Pending Payment")
            {
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Method = "50% deposit",
                    StoreId = order.StoreId,
                    TotalPrice = order.OrderPrice * 50 / 100,
                    CreateAt = vietnamTime,
                    Status = "Payment Successfully",
                };
                await _unitOfWork.Repository<Payment>().AddAsync(payment);
                await _unitOfWork.CompleteAsync();

                if (order.DesignCustomId != null)
                {
                    order.Status = "Order Successfully";
                    _unitOfWork.Repository<Order>().Update(order);
                    await _unitOfWork.CompleteAsync();

                }
                else if (order.DesignCustomId != null)
                {
                    order.Status = "Arranging & Packing";

                    _unitOfWork.Repository<Order>().Update(order);
                    designcustom.Status = "Design Successfully";
                    _unitOfWork.Repository<DesignCustom>().Update(designcustom);

                    await _unitOfWork.CompleteAsync();

                }
                await SendPaymentConfirmationEmail(order);
                //wallet.TotalPrice += payment.TotalPrice;
                //_unitOfWork.Repository<Wallet>().Update(wallet);
                //var incomeWallet = new IncomeWallet
                //{
                //    WalletID = wallet.WalletId,
                //    IncomePrice = payment.TotalPrice,
                //    Method = "Payment",
                //    Status = "Successfull",
                //    CreateAt = vietnamTime,
                //    UpdateAt = vietnamTime,
                //};
                //await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                await _unitOfWork.CompleteAsync();
                isPaymentCreated = true;
            }

           if(isPaymentCreated)
            {
                try
                {
                    var notification = new Noti
                    {
                        ToUserId = order.StoreId,
                        Message = $"bạn có một đơn hàng cần xử lý",
                        Type = "Order",
                        RelatedId = order.OrderId
                    };

                    await _notiService.CreateNotificationAsync(notification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
            }



            // Gửi email xác nhận thanh toán


        }
        private async Task SendPaymentConfirmationEmail(Order order)
        {
            string subject = "Xác nhận thanh toán đơn hàng";
            string message = $@"
        Xin chào {order.Customer.FullName}, 

        Đơn hàng #{order.OrderId} của bạn đã được xác nhận thanh toán thành công.

        - Số tiền: {order.OrderPrice} VND
        - Cửa hàng: {order.StoreId}
        - Trạng thái: {order.Status}

        Cảm ơn bạn đã mua hàng!

        Trân trọng,
        Đội ngũ hỗ trợ";

            await _emailService.SendEmail(new SendEmailRequest
            {
                To = order.Customer.Email,
                Subject = subject,
                Body = message
            });

        }


        public async Task<PaymentResponse> GetPyamentById(Guid paymentId)
        {
            // Tìm payment theo ID
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId);

            if (payment == null)
            {
                throw new Exception($"Payment with ID {paymentId} not found.");
            }

            // Chuyển đổi entity thành DTO
            var paymentResponse = new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                Method = payment.Method,
                StoreId = payment.StoreId,
                CustomerId = payment.CustomerId,
                TotalPrice = payment.TotalPrice,
                Status = payment.Status
            };

            return paymentResponse;
        }

        public async Task UpdateStatusPayment(Guid paymentId, string status)
        {
            // Tìm payment theo ID
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId);

            if (payment == null)
            {
                throw new Exception($"Payment with ID {paymentId} not found.");
            }

            // Cập nhật trạng thái
            payment.Status = status;

            // Lưu thay đổi vào database
            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.CompleteAsync();
        }
    }
}
