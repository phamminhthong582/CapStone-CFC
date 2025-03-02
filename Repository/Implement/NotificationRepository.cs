using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class NotificationRepository : INotificationRepository
{
    private readonly CustomFlowerChainContext _context;

    public NotificationRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<Notification> CreateNotification(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<List<Notification>> GetNotificationsByUserId(Guid userId)
    {
        return await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
    }

    public async Task<Notification> GetNotificationById(Guid notificationId)
    {
        return await _context.Notifications.FindAsync(notificationId);
    }

    public async Task<Notification> UpdateNotification(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }
}