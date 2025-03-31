using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implement
{
    public class NotiRepository : INotiRepository
    {
        private readonly CustomFlowerChainContext _context;

        public NotiRepository(CustomFlowerChainContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Noti>> GetAllNotificationsAsync()
        {
            return await _context.Notis.ToListAsync();
        }

        public async Task<IEnumerable<Noti>> GetUserNotificationsAsync(Guid userId)
        {
            return await _context.Notis
                .Where(n => n.ToUserId == userId)
                .OrderByDescending(n => n.CreateAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notis
                .Where(n => n.ToUserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<Noti?> GetNotificationByIdAsync(Guid notificationId)
        {
            return await _context.Notis.FindAsync(notificationId);
        }

        public async Task<Noti> CreateNotificationAsync(Noti notification)
        {
            notification.NotiId = Guid.NewGuid();
            notification.CreateAt = DateTime.UtcNow;

            _context.Notis.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notis.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            var notification = await _context.Notis.FindAsync(notificationId);
            if (notification == null) return false;

            _context.Notis.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
