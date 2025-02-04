using BusinessObject.DTO.Wallet;
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
        Task CreateWallet(Guid CustomerId);
    }
}
