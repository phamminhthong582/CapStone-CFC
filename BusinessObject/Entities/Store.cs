using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Store
{
    public Guid StoreId { get; set; }

    public string? StoreName { get; set; }

    public string? Address { get; set; }

    public string? StorePhone { get; set; }

    public string? StoreAvatar { get; set; }

    public string? StoreEmail { get; set; }

    public string? Password { get; set; }

    public string? BankAccountName { get; set; }

    public string? BankName { get; set; }

    public string? BankNumber { get; set; }

    public string? MonoNumber { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<FlowerBasket> FlowerBaskets { get; set; } = new List<FlowerBasket>();

    public virtual ICollection<Flower> Flowers { get; set; } = new List<Flower>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
