using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Order
{
    public class OrderCartRequest
    {
        public Guid? PromotionId { get; set; }
        public string? DeliveryDistrict { get; set; }
        public string? DeliveryCity { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Note { get; set; }
        public Guid? StoreId { get; set; }
        public string? RecipientName { get; set; }
        public DateTime? RecipientTime { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public bool? Transfer { get; set; }
        public bool? Delivery { get; set; }
    }
}
