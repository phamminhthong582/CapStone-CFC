using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.IncomeWallet
{
    public class IncomeWalletResponse
    {
        public Guid? IncomeWalletID { get; set; }
        public Guid? WalletID { get; set; }
        public double? IncomePrice { get; set; }
        public string? Method { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
