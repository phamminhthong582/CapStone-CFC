using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.WithdrawMoney
{
    public class WithdrawMoneyResponse
    {
        public Guid WithdrawMoneyId { get; set; }

        public Guid? WalletId { get; set; }

        public double? Price { get; set; }

        public string? BankAccountName { get; set; }

        public string? BankName { get; set; }

        public string? BankNumber { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }
}
