using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Wallet
{
    public Guid WalletId { get; set; }

    public Guid? UserId { get; set; }

    public double? TotalPrice { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual User? User { get; set; }
}
