using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessObject.DTO.Flower;

public class UpdateFlowerRequest
{
    public double? Price { get; set; }

    [FromForm]
    public IFormFile? Image { get; set; }

    public int? Quantity { get; set; }
    public string? FlowerName {  get; set; }
    public string? Color { get; set; }

    public Guid? CategoryId { get; set; }

    public string? Description { get; set; }

}