namespace BusinessObject.DTO.Chat;

public class ChatRoomResponse
{
    public Guid? ChatRoomId {  get; set; }
    public Guid CustomerId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? AiAssistantId { get; set; }
    public string Status { get; set; } 
    public DateTime CreateAt { get; set; }
}