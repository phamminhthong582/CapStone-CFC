namespace BusinessObject.DTO.ProductCustom;

public class ProductCustomResponse
{
    public Guid ProductCustomId { get; set; }
    public string ProductName { get; set; }
    public Guid? FlowerBasketId { get; set; }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
    public string? Description { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public bool Status { get; set; }

}