namespace BusinessObject.DTO.Flower;

public class CreateFlowerRequest
{
    public string? FlowerName { get; set; }

    public double? Price { get; set; }

    public Guid? StoreId { get; set; }

    public string? Image { get; set; }

    public int? Quantity { get; set; }

    public Guid? CategoryId { get; set; }

    public string? Description { get; set; }

}