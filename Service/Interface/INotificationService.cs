using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Notification;
using BusinessObject.Entities;

namespace Service.Interface;

public interface INotificationService
{
    Task<Result<NotificationResponse>> CreateNotification(NotificationRequest request);
    Task<Result<List<NotificationResponse>>> GetNotificationsByUserId(Guid userId);
    Task<Result<NotificationResponse>> GetNotificationById(Guid notificationId);
    Task<Result<NotificationResponse>> MarkAsRead(Guid notificationId);
    Task<Result<NotificationResponse>> UpdateNotificationStatus(Guid notificationId, string status);
}