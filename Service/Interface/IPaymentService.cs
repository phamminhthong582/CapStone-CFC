using BusinessObject.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponse> GetPyamentById(Guid paymentId);
        Task UpdateStatusPayment(Guid paymentId, string status);
        Task CreatePayment(Guid  OrderId);
    }
}
