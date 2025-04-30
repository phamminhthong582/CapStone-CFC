using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.FailOrder
{
    public class FailOrderResponse
    {
        public Guid FailOrderId { get; set; }
        public string? ReasonFail { get; set; }
        public string? ImageFail { get; set; }
        public DateTime? TimeDelay { get; set; }
        public double? RefundPrice { get; set; }
        public bool? Wallet { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? Status { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? StaffId { get; set; }
        public Guid? DeliveryId { get; set; }
        public Guid? ShipperId { get; set; }
    }
}
