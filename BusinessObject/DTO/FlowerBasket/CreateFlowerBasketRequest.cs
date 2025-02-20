using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessObject.DTO.FlowerBasket;

public class CreateFlowerBasketRequest
{
    public string FlowerBasketName { get; set; }
    public string? Decription { get; set; } 
    public Guid? CategoryId { get; set; }
    public int? MaxQuantity { get; set; }
    public int? MinQuantity { get; set; }
    public bool Feature { get; set; }
    [FromForm]
    public IFormFile? Image { get; set; }
    public double? Price { get; set; }
    
    public int? Quantity { get; set; }
}