namespace BusinessObject.DTO.Message;

public class CreateMessageRequest
{
    public Guid? ChatRoomId { get; set; }
    public Guid? SenderId {  get; set; }
    public Guid? ReceiveId { get; set; }
    public string? MessageType {  get; set; }
    public string? Content { get; set; }      
}