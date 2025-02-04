namespace BusinessObject.DTO.FlowerBasket;

public class UpdateFlowerBasketRequest
{
    public string? FlowerBasketName { get; set; }
    public Guid? StoreId { get; set; }
    public string? Description { get; set; }
    public bool? Feature { get; set; }
    public bool? Status { get; set; } 
}