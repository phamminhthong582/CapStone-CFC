using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Payment
{
    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }

        public Guid? OrderId { get; set; }

        public string? Method { get; set; }

        public Guid? StoreId { get; set; }

        public Guid? CustomerId { get; set; }

        public double? TotalPrice { get; set; }
        public string? Status { get; set; }

    }
}
