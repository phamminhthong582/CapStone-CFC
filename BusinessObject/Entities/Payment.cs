using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? OrderId { get; set; }

    public string? Method { get; set; }

    public Guid? StoreId { get; set; }

    public string? TotalPrice { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? User { get; set; }
}
