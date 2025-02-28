using BusinessObject.DTO.FlowerCustom;

namespace BusinessObject.DTO.ProductCustom;

public class CreateProductCustomRequest
{
    public string? ProductName { get; set; }
    public Guid? FlowerBasketId { get; set; }
    public Guid? StyleId  { get; set; }
    public Guid? AccessoryId { get; set; }
    public int? Quantity { get; set; }
    public List<CreateFlowerCustomRequest>? createFlowerCustomRequests { get; set; }
    public string? Description { get; set; }
}