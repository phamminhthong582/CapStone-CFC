using BusinessObject.DTO.IncomeWallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IIncomWalletService
    {
        Task<IEnumerable<IncomeWalletResponse>> GetInComWalletByWalletId(Guid walletId);
    }
}
