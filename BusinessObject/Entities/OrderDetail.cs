using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class OrderDetail
{
    public Guid OrderDetailId { get; set; }

    public Guid? ProductId { get; set; }

    public double? ProductTotalPrice { get; set; }

    public int? Quantity { get; set; }

    public Guid? ProductCustomId { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? StoreId { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ProductCustom? ProductCustom { get; set; }
}
