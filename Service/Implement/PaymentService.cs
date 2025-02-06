using BusinessObject.DTO.Payment;
using BusinessObject.Entities;
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

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
