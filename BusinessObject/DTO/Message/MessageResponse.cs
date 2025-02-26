namespace BusinessObject.DTO.Message;

public class MessageResponse
{
    public Guid? MessageId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public Guid? SenderId {  get; set; }
    public Guid? ReceiveId { get; set; }
    public string? MessageType {  get; set; }
    public string? Content { get; set; }
    public string? Status { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}