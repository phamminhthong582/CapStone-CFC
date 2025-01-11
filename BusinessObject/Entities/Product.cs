using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Product
{
    public Guid ProductId { get; set; }

    public string? ProductName { get; set; }

    public Guid? StoreId { get; set; }

    public int? Quantity { get; set; }

    public double? Price { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? Size { get; set; }

    public double? Discount { get; set; }

    public string? Description { get; set; }

    public bool? Featured { get; set; }

    public Guid? CategoryId { get; set; }

    public int? Sold { get; set; }

    public bool? Status { get; set; }
}
