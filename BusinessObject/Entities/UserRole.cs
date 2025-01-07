using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class UserRole
{
    public Guid RoleId { get; set; }

    public string? RoleName { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
