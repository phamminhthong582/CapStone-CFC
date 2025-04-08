using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface INotiRepository
    {
        Task<IEnumerable<Noti>> GetAllNotificationsAsync();
        Task<IEnumerable<Noti>> GetUserNotificationsAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<Noti?> GetNotificationByIdAsync(Guid notificationId);
        Task<Noti> CreateNotificationAsync(Noti notification);
        Task<Noti?> UpdateNotificationByRelatedIdAndToUserAsync(Guid relatedId, Guid toUserId, Noti updatedNoti);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> DeleteNotificationAsync(Guid notificationId);
    }
}
