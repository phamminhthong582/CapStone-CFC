using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.DesignCustom
{
    public class UpdateOrderDesignCustomByCustomer
    {
        public string? DeliveryDistrict { get; set; }
        public string? DeliveryCity { get; set; }
        public string? DeliveryAddress { get; set; }
        public bool? Wallet { get; set; }
        public bool? Transfer { get; set; }
        public bool? Delivery { get; set; }
        public DateTime? RecipientTime { get; set; }


    }
}
