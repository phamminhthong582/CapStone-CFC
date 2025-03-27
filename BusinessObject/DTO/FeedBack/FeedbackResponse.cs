namespace BusinessObject.DTO.FeedBack;

public class FeedbackResponse
{
    public Guid FeedbackId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? StoreId { get; set; }
    public string? FeedbackByCustomer { get; set; }
    public string? FeedBackVideoByCustomer { get; set; }
    public string? ResponseFeedBackStore { get; set; }
    public bool? RequestRefundByCustomer { get; set; }
    public int? Rating { get; set; }
    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? Status { get; set; }
}