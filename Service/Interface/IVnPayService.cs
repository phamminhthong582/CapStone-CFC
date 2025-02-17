using BusinessObject.Helper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IVnPayService
    {
        Task<String> CreatePaymentUrlAsync(Guid PaymentId);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
