using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Delivery
{
    public Guid DeliveryId { get; set; }

    public Guid? OrderId { get; set; }

    public double? Ship { get; set; }

    public bool? FreeShip { get; set; }

    public string? OrderCode { get; set; }

    public Guid? ShipperId { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? Shipper { get; set; }
}
