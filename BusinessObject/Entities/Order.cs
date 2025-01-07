using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Order
{
    public Guid OrderId { get; set; }

    public double? OrderPrice { get; set; }

    public Guid? UserId { get; set; }

    public Guid? PromotionId { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? Note { get; set; }

    public string? Banner { get; set; }

    public DateTime? DeliveryDateTime { get; set; }

    public string? Phone { get; set; }

    public bool? Transfer { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
