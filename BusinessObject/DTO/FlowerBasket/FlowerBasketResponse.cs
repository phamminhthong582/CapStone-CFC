namespace BusinessObject.DTO.FlowerBasket;

public class FlowerBasketResponse
{
    public Guid FlowerBasketId { get; set; }
    public string FlowerBasketName { get; set; }
    public double Price { get; set; }
    public Guid StoreId { get; set; }
    public string? Description { get; set; }
    public bool Feature { get; set; }
    public bool Status { get; set; }
    public int Sold { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
}