namespace BusinessObject.DTO.FlowerCustom;

public class CreateFlowerCustomRequest
{
    public Guid FlowerId { get; set; }  
    public Guid ProductCustomId { get; set; }  
    public int Quantity { get; set; }
    public double Price { get; set; }
    public bool Status { get; set; }  
}