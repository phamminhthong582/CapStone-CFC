using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Size
{
    public Guid SizeId { get; set; }

    public double? SizrePrice { get; set; }

    public string? SizeText { get; set; }

    public Guid? ProductId { get; set; }

    public int? SizeQuantity { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual Product? Product { get; set; }
}
