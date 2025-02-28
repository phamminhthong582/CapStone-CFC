namespace BusinessObject.DTO.FlowerBasket;

public class FlowerBasketResponse
{
    public Guid? FlowerBasketId { get; set; }
    public string? FlowerBasketName { get; set; }
    public int? MaxQuantity { get; set; }
    public int? MinQuantity { get; set; }
    public int? Quantity { get; set; }
    public string? Image { get; set; }
    public string? CategoryName { get; set; } 
    public double? Price { get; set; }
    public string? Decription { get; set; }
    public bool? Feature { get; set; }
    public bool? Status { get; set; }
    public int? Sold { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}