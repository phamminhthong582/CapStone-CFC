namespace BusinessObject.DTO.Message;

public class UpdateMessageStatusRequest
{
    public Guid? MessageId { get; set; }
    public string? Status { get; set; }
}