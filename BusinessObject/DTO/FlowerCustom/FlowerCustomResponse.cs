namespace BusinessObject.DTO.FlowerCustom;

public class FlowerCustomResponse
{
    public Guid FlowerCustomId { get; set; }
    public string FlowerName { get; set; }  
    public string ProductCustomName { get; set; }  
    public int Quantity { get; set; }
    public double Price { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public bool Status { get; set; }
}