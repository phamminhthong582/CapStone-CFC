using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Notification;
using BusinessObject.Entities;
using Microsoft.AspNetCore.SignalR;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository )
    {
        _notificationRepository = notificationRepository;
       
    }
    public async Task<Result<NotificationResponse>> CreateNotification(NotificationRequest request)
    {
        var response = new Result<NotificationResponse>();
        var notification = new Notification
        {
            MessageId = request.MessageId,
            UserId = request.UserId,
            Status = NotificationStatus.Unread.ToString(),
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow
        };
        var createdNotification = await _notificationRepository.CreateNotification(notification);
        if (createdNotification == null)
        {
            response.Messages = new[] { "Failed to create notification." };
            response.ResultStatus = ResultStatus.Error.ToString();
            return response;
        }
        response.Data = new NotificationResponse()
        {
            NotificationId = createdNotification.NotificationId,
            MessageId = createdNotification.MessageId,
            UserId = createdNotification.UserId,
            Status = createdNotification.Status,
            CreateAt = createdNotification.CreateAt,
            UpdateAt = createdNotification.UpdateAt
        };
        response.Messages = new[] { "Notification created successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();
        return response;
    }

    public async Task<Result<List<NotificationResponse>>> GetNotificationsByUserId(Guid userId)
    {
        var response = new Result<List<NotificationResponse>>();
        var notifications = await _notificationRepository.GetNotificationsByUserId(userId);

        if (notifications == null || !notifications.Any())
        {
            response.Messages = new[] { "No notifications found for this user." };
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        var notificationResponses = notifications.Select(n => new NotificationResponse
        {
            NotificationId = n.NotificationId,
            MessageId = n.MessageId,
            UserId = n.UserId,
            Status = n.Status,
            CreateAt = n.CreateAt,
            UpdateAt = n.UpdateAt
        }).ToList();

        response.Data = notificationResponses;
        response.Messages = new[] { "Notifications retrieved successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();

        return response;
    }

    public async Task<Result<NotificationResponse>> GetNotificationById(Guid notificationId)
    {
        var response = new Result<NotificationResponse>();
        var notification = await _notificationRepository.GetNotificationById(notificationId);

        if (notification == null)
        {
            response.Messages = new[] { "Notification not found." };
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        response.Data = new NotificationResponse
        {
            NotificationId = notification.NotificationId,
            MessageId = notification.MessageId,
            UserId = notification.UserId,
            Status = notification.Status,
            CreateAt = notification.CreateAt,
            UpdateAt = notification.UpdateAt
        };

        response.Messages = new[] { "Notification retrieved successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();

        return response;
    }

    public async Task<Result<NotificationResponse>> MarkAsRead(Guid notificationId)
    {
        var response = new Result<NotificationResponse>();
        var notification = await _notificationRepository.GetNotificationById(notificationId);

        if (notification == null)
        {
            response.Messages = new[] { "Notification not found." };
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        notification.Status = NotificationStatus.Read.ToString();
        notification.UpdateAt = DateTime.UtcNow;
        await _notificationRepository.UpdateNotification(notification);
        response.Data = new NotificationResponse
        {
            NotificationId = notification.NotificationId,
            MessageId = notification.MessageId,
            UserId = notification.UserId,
            Status = notification.Status,
            CreateAt = notification.CreateAt,
            UpdateAt = notification.UpdateAt
        };
        response.Messages = new[] { "Notification marked as read." };
        response.ResultStatus = ResultStatus.Success.ToString();

        return response;
    }

    public async Task<Result<NotificationResponse>> UpdateNotificationStatus(Guid notificationId, string status)
    {
        var response = new Result<NotificationResponse>();
        var notification = await _notificationRepository.GetNotificationById(notificationId);

        if (notification == null)
        {
            response.Messages = new[] { "Notification not found." };
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        notification.Status = status;
        notification.UpdateAt = DateTime.UtcNow;
        await _notificationRepository.UpdateNotification(notification);

        response.Data = new NotificationResponse
        {
            NotificationId = notification.NotificationId,
            MessageId = notification.MessageId,
            UserId = notification.UserId,
            Status = notification.Status,
            CreateAt = notification.CreateAt,
            UpdateAt = notification.UpdateAt
        };

        response.Messages = new[] { "Notification status updated successfully." };
        response.ResultStatus = ResultStatus.Success.ToString();

        return response;
    }
}