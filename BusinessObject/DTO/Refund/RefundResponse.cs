using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Refund
{
    public class RefundResponse
    {
        public Guid RefundId { get; set; }

        public Guid? OrderId { get; set; }

        public double? Price { get; set; }

        public Guid? WallerId { get; set; }
        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string? Status { get; set; }
    }
}
