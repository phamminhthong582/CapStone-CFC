namespace BusinessObject.DTO.Notification;

public class NotificationResponse
{
    public Guid? NotificationId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? MessageId { get; set; }
    public string? Status { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}