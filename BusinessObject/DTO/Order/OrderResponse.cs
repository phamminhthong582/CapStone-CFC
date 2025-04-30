using BusinessObject.DTO.DesignCustom;
using BusinessObject.DTO.OrderDetails;
using BusinessObject.DTO.ProductCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Order
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }

        public double? OrderPrice { get; set; }

        public Guid? ProductCustomId { get; set; }
        public ProductCustomResponse? ProductCustomResponse { get; set; }
        public DesignCustomBuCustomerResponse? DesignCustomBuCustomerResponse {  get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? StaffId { get; set; }
        public string? StaffFullName { get; set; }
        public string? StaffEmail { get; set; }
        public string? StaffPhone {  get; set; }
        public Guid? PromotionId { get; set; }
        public string? PromotionName { get; set; }

        public double? PromotionDiscount { get; set; }

        public string? DeliveryDistrict { get; set; }
        public string? DeliveryCity { get; set; }

        public string? DeliveryAddress { get; set; }

        public Guid? StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? StoreAddress { get; set; }


        public string? Note { get; set; }

        public DateTime? DeliveryDateTime { get; set; }

        public string? Phone { get; set; }

        public bool? Transfer { get; set; }
        public bool? Delivery { get; set; }

        public Guid? DeliveryId { get; set; }
        public Guid? ShipperId { get; set; }
        public string? ShipperName { get; set; }
        public string? ShipperEmail { get; set; }
        public string? ShipperPhone { get; set; }

        public string? NumberMoto { get; set; }

        public string? ColorMoto { get; set; }

        public string? MotoType { get; set; }
        public Guid? PaymentId { get; set; }
        public double? PaymentPrice { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? PaymentCreateAt { get; set; }
        public string? PaymentMethod {  get; set; }

        public bool? Refund { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string? Status { get; set; }
        public List<OrderDetailsResponse>? OrderDetails { get; set; }
    }
}
