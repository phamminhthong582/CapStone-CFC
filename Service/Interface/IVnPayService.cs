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
        Task<String> CreatePaymentUrlAsync(Guid orderId);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);

        VnPaymentWalletResponseModel PaymentDepositExecute(IQueryCollection collections);

        Task<String> CreateDepositWallet(Guid walletId, double price );


    }
}
