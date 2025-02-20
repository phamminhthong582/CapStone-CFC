using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessObject.DTO.Promotion;

public class UpdatePromotionRequest
{
    public string? PromotionName { get; set; }

    public string? PromotionCode { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    [FromForm]
    public IFormFile? Image { get; set; }
    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? Quantity { get; set; }

    public double? PromotionDiscount { get; set; }
}