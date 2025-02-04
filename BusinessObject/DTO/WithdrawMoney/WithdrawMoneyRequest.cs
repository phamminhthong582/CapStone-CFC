using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.WithdrawMoney
{
    public class WithdrawMoneyRequest
    {
        public double? Price { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? BankNumber { get; set; }
        public string? Reason { get; set; }
    }
}
