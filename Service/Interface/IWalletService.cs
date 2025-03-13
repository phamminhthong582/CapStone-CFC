using BusinessObject.DTO.Wallet;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IWalletService
    {
        Task<WalletResponse> GetWallet(Guid CustomerId);
        Task CreateWallet(Guid CustomerId, string PasswordWallet);
        Task DepositWallet(Guid WalletId, double price);
        Task<bool> CheckWallet(Guid CustomerId);
    }
}
