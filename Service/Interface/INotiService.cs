using BusinessObject.DTO.Commons;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface INotiService
    {
        Task<Result<IEnumerable<Noti>>> GetAllNotificationsAsync();
        Task<Result<IEnumerable<Noti>>> GetUserNotificationsAsync(Guid userId);
        Task<Result<int>> GetUnreadCountAsync(Guid userId);
        Task<Result<Noti>> GetNotificationByIdAsync(Guid notificationId);
        Task<Result<Noti>> CreateNotificationAsync(Noti notification);
        Task<Result<Noti?>> UpdateNotificationByRelatedIdAndToUserAsync(Guid relatedId, Guid toUserId, Noti updatedNoti);
        Task<Result<bool>> MarkAsReadAsync(Guid notificationId);
        Task<Result<bool>> DeleteNotificationAsync(Guid notificationId);
        Task<Result<bool>> SendRealTimeNotificationAsync(Noti notification);
    }
}
