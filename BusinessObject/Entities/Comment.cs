using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Comment
{
    public Guid CommentId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? UserId { get; set; }

    public int? Rating { get; set; }

    public string? Comment1 { get; set; }

    public DateTime? CreateAt { get; set; }

    public bool? Status { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
