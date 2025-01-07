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

    public int? Discount { get; set; }

    public bool? Featured { get; set; }

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public int? Sold { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Size> Sizes { get; set; } = new List<Size>();

    public virtual Store? Store { get; set; }
}
