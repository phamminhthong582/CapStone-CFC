using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessObject.DTO.FeedBack;

public class CreateFeedbackRequest
{
    public string? FeedbackByCustomer { get; set; }
    [FromForm]
    public IFormFile? FeedBackVideoByCustomer { get; set; } // Chấp nhận file video
    public bool? RequestRefundByCustomer { get; set; }
    public int? Rating { get; set; }
}