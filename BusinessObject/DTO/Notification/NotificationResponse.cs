namespace BusinessObject.DTO.Notification;

public class NotificationResponse
{
    public Guid NotificationId { get; set; }
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}