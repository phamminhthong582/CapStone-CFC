using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Request
{
    public class StoreRequest
    {
        public string? StoreName { get; set; }
        public string? Address { get; set; }

        public string? StorePhone { get; set; }

        public string? Password { get; set; }

        public string? StoreAvatar { get; set; }
        public string? StoreEmail { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }

        public string? BankNumber { get; set; }

        public string? MonoNumber { get; set; }
    }
}
