namespace BusinessObject.DTO.FlowerCustom;

public class UpdateFlowerCustomRequest
{
    public Guid FlowerCustomId { get; set; }
    public int? Quantity { get; set; }  
    public double? Price { get; set; }  
    public bool? Status { get; set; }
}