namespace BusinessObject.DTO.FlowerCustom;

public class UpdateFlowerCustomRequest
{
    public Guid FlowerCustomId { get; set; }  // ID của mẫu cần cập nhật
    public int? Quantity { get; set; }  // Cập nhật số lượng (nếu cần)
    public double? Price { get; set; }  // Cập nhật giá (nếu cần)
    public bool? Status { get; set; }
}