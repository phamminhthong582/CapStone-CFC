using BusinessObject.DTO.Wallet;
using BusinessObject.DTO.WithdrawMoney;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IWithdrawMoneyService
    {
        Task<IEnumerable<WithdrawMoneyResponse>> GetWithDrawMoneyByWalletId(Guid WalletId);
        Task CreateWithdrawMoney(Guid WalletId, WithdrawMoneyRequest withdrawMoneyRequest );
        Task UpdateStatusWithdrawMoney(Guid WithdrawMoneyId, string status);
        Task<WithdrawMoneyResponse> GetWithDrawMoneyByWithdrawMoneyId(Guid WithdrawMoneyId);

        Task ConfirmOPT(Guid WithdrawMoneyId, string otp);
        Task DeleteWithdrawMoney(Guid WithdrawMoneyId);

    }
}
