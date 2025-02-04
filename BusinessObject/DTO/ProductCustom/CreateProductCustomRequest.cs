namespace BusinessObject.DTO.ProductCustom;

public class CreateProductCustomRequest
{
    public string ProductName { get; set; }
    public Guid? FlowerBasketId { get; set; } 
    public Guid? CustomerId { get; set; } 
    public int Quantity { get; set; }
    public double? TotalPrice { get; set; }
    public string? Description { get; set; }
}