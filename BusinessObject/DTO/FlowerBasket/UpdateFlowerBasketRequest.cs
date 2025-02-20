using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessObject.DTO.FlowerBasket;

public class UpdateFlowerBasketRequest
{
    public string? FlowerBasketName { get; set; }
    public string? Description { get; set; }
    public double? Price { get; set; }
    public int? MaxQuantity { get; set; }
    public int? MinQuantity { get; set; }
    public Guid? CategoryId { get; set; }
    public int? Quantity { get; set; }
    [FromForm]
    public IFormFile? Image { get; set; }
    public bool? Feature { get; set; }
    public bool? Status { get; set; } 
}