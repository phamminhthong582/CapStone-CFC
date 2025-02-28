using BusinessObject.DTO.Flower;

namespace BusinessObject.DTO.FlowerCustom;

public class FlowerCustomResponse
{
    public Guid FlowerCustomId { get; set; }
    public Guid? FlowerId {  get; set; }
    public FlowerResponse? flowerResponse { get; set; }
    public int? Quantity { get; set; }
    public double? TotalPrice { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public bool? Status { get; set; }
}