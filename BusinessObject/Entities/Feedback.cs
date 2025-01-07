using System;
using System.Collections.Generic;

namespace BusinessObject.Entities;

public partial class Feedback
{
    public Guid FeedbackId { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? UserId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Status { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? User { get; set; }
}
