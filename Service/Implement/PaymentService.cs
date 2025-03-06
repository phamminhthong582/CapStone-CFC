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

        public PaymentService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task CreatePayment(Guid OrderId)
        {
            var order = await _unitOfWork.Repository<Order>().Entities.Include(m => m.Customer).FirstOrDefaultAsync(n => n.OrderId == OrderId);
          

            if (order.Transfer == true && order.Status == "Pending Payment")
            {
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Method = "Tiền tổng",
                    StoreId = order.StoreId,
                    TotalPrice = order.OrderPrice,
                    CreateAt = DateTime.Now,
                    Status = "Payment Successfully",
                };
                order.Status = "Order Successfully";
                await _unitOfWork.Repository<Payment>().AddAsync(payment);
                _unitOfWork.Repository<Order>().Update(order);
                await SendPaymentConfirmationEmail(order);

                await _unitOfWork.CompleteAsync();

            }
            else if (order.Transfer == false && order.Status == "Pending Payment")
            {
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Method = "Tiền cọc",
                    StoreId = order.StoreId,
                    TotalPrice = order.OrderPrice * 50 / 100,
                    CreateAt = DateTime.Now,
                    Status = "Payment Confirmed",
                };
                order.Status = "Order Successfully";
                await _unitOfWork.Repository<Payment>().AddAsync(payment);
                _unitOfWork.Repository<Order>().Update(order);
                await SendPaymentConfirmationEmail(order);

                await _unitOfWork.CompleteAsync();

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
