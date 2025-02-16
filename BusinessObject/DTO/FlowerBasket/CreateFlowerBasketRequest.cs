namespace BusinessObject.DTO.FlowerBasket;

public class CreateFlowerBasketRequest
{
    public string FlowerBasketName { get; set; }
    public string? Description { get; set; } 
    public bool Feature { get; set; } 
    
    public double? Price { get; set; }
    
    public int? Quantity { get; set; }
}