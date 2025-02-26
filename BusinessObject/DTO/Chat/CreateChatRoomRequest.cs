namespace BusinessObject.DTO.Chat;

public class CreateChatRoomRequest
{
    public Guid CustomerId { get; set; }
    public Guid? EmployeeId { get; set; } 
}