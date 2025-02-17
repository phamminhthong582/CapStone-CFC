using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Wallet
{
    public class WalletResponse
    {
        public Guid WalletId { get; set; }
        public Guid? CustomerId { get; set; }

        public double? TotalPrice { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public DateTime? Status { get; set; }
    }
}
