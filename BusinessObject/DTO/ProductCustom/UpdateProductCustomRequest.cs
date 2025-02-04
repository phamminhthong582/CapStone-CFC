namespace BusinessObject.DTO.ProductCustom;

public class UpdateProductCustomRequest
{
    public string? ProductName { get; set; }
    public Guid? FlowerBasketId { get; set; }
    public int? Quantity { get; set; }
    public double? TotalPrice { get; set; }
    public string? Description { get; set; }
    public bool? Status { get; set; }
}