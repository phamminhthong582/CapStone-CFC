using BusinessObject.Entities;

namespace Repository.Interface;

public interface INotificationRepository
{
    Task<Notification> CreateNotification(Notification notification);
    Task<List<Notification>> GetNotificationsByUserId(Guid userId);
    Task<Notification> GetNotificationById(Guid notificationId);
    Task<Notification> UpdateNotification(Notification notification);
}