using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Noti;
using BusinessObject.Entities;
using Microsoft.AspNetCore.SignalR;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class NotiService : INotiService
    {
        private readonly INotiRepository _notiRepository;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public NotiService(INotiRepository notiRepository, IHubContext<NotificationHub> notificationHub)
        {
            _notiRepository = notiRepository;
            _notificationHub = notificationHub;
        }

        public async Task<Result<IEnumerable<Noti>>> GetAllNotificationsAsync()
        {
            try
            {
                var notifications = await _notiRepository.GetAllNotificationsAsync();
                return new Result<IEnumerable<Noti>>
                {
                    Data = notifications,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Successfully retrieved all notifications" }
                };
            }
            catch (Exception ex)
            {
                return new Result<IEnumerable<Noti>>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<IEnumerable<Noti>>> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                var notifications = await _notiRepository.GetUserNotificationsAsync(userId);

                if (notifications == null || !notifications.Any())
                {
                    return new Result<IEnumerable<Noti>>
                    {
                        ResultStatus = ResultStatus.NotFound.ToString(),
                        Messages = new[] { "No notifications found for this user" }
                    };
                }

                return new Result<IEnumerable<Noti>>
                {
                    Data = notifications,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Successfully retrieved user notifications" }
                };
            }
            catch (Exception ex)
            {
                return new Result<IEnumerable<Noti>>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<int>> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var count = await _notiRepository.GetUnreadCountAsync(userId);
                return new Result<int>
                {
                    Data = count,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { $"Found {count} unread notifications" }
                };
            }
            catch (Exception ex)
            {
                return new Result<int>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<Noti>> GetNotificationByIdAsync(Guid notificationId)
        {
            try
            {
                var notification = await _notiRepository.GetNotificationByIdAsync(notificationId);

                if (notification == null)
                {
                    return new Result<Noti>
                    {
                        ResultStatus = ResultStatus.NotFound.ToString(),
                        Messages = new[] { "Notification not found" }
                    };
                }

                return new Result<Noti>
                {
                    Data = notification,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Successfully retrieved notification" }
                };
            }
            catch (Exception ex)
            {
                return new Result<Noti>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<Noti>> CreateNotificationAsync(Noti notification)
        {
            try
            {
                // Validate input
                if (notification.ToUserId == Guid.Empty)
                {
                    return new Result<Noti>
                    {
                        ResultStatus = ResultStatus.Invalid.ToString(),
                        Messages = new[] { "ToUserId is required" }
                    };
                }

                // Set default values
                notification.NotiId = Guid.NewGuid();
                notification.CreateAt = DateTime.UtcNow;
                notification.IsRead = false;

                // Save to database
                var createdNoti = await _notiRepository.CreateNotificationAsync(notification);

                // Send real-time notification
                var sendResult = await SendRealTimeNotificationAsync(createdNoti);
                if (sendResult.ResultStatus != ResultStatus.Success.ToString())
                {
                    // Log warning but still return success for creation
                    Console.WriteLine($"Warning: Notification created but real-time send failed: {string.Join(", ", sendResult.Messages)}");
                }

                return new Result<Noti>
                {
                    Data = createdNoti,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Notification created successfully" }
                };
            }
            catch (Exception ex)
            {
                return new Result<Noti>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<bool>> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var result = await _notiRepository.MarkAsReadAsync(notificationId);

                if (!result)
                {
                    return new Result<bool>
                    {
                        Data = false,
                        ResultStatus = ResultStatus.NotFound.ToString(),
                        Messages = new[] { "Notification not found" }
                    };
                }

                return new Result<bool>
                {
                    Data = true,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Notification marked as read" }
                };
            }
            catch (Exception ex)
            {
                return new Result<bool>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<bool>> DeleteNotificationAsync(Guid notificationId)
        {
            try
            {
                var result = await _notiRepository.DeleteNotificationAsync(notificationId);

                if (!result)
                {
                    return new Result<bool>
                    {
                        Data = false,
                        ResultStatus = ResultStatus.NotFound.ToString(),
                        Messages = new[] { "Notification not found" }
                    };
                }

                return new Result<bool>
                {
                    Data = true,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Notification deleted successfully" }
                };
            }
            catch (Exception ex)
            {
                return new Result<bool>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<bool>> SendRealTimeNotificationAsync(Noti notification)
        {
            try
            {
                Console.WriteLine($"Sending realtime notification to user: {notification.ToUserId}");

                var notificationData = new
                {
                    notificationId = notification.NotiId.ToString(),
                    message = notification.Message,
                    type = notification.Type,
                    relatedId = notification.RelatedId?.ToString(),
                    createdAt = notification.CreateAt?.ToString("o") ?? string.Empty, // ISO 8601 format
                    isRead = notification.IsRead,
                    status = notification.Status,
                    
                };
                Console.WriteLine($"Notification payload: {JsonSerializer.Serialize(notificationData)}");
                if (notification.ToUserId == null || notification.ToUserId == Guid.Empty)
                {
                    return new Result<bool>
                    {
                        Data = false,
                        ResultStatus = ResultStatus.Invalid.ToString(),
                        Messages = new[] { "Invalid recipient ID" }
                    };
                }

                Console.WriteLine($"Sending to group: user-{notification.ToUserId}");
                await _notificationHub.Clients
                    .Group($"user-{notification.ToUserId}")
                    .SendAsync("ReceiveNotification", notificationData);
                Console.WriteLine("Notification sent to SignalR");


                return new Result<bool>
                {
                    Data = true,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Real-time notification sent successfully" }
                };
            }
            catch (Exception ex)
            {
                return new Result<bool>
                {
                    Data = false,
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

        public async Task<Result<Noti?>> UpdateNotificationByRelatedIdAndToUserAsync(Guid relatedId, Guid toUserId, Noti updatedNoti)
        {
            try
            {
                
                updatedNoti.IsRead = false;

                
                var updated = await _notiRepository.UpdateNotificationByRelatedIdAndToUserAsync(relatedId, toUserId, updatedNoti);

                if (updated != null)
                {
                    await SendRealTimeNotificationAsync(updated);
                    return new Result<Noti?>
                    {
                        Data = updated,
                        ResultStatus = ResultStatus.Success.ToString(),
                        Messages = new[] { "Notification updated successfully." }
                    };
                }

                
                var newNoti = new Noti
                {
                    NotiId = Guid.NewGuid(),
                    RelatedId = relatedId,
                    ToUserId = toUserId,
                    FromUserId = updatedNoti.FromUserId,
                    Type = updatedNoti.Type,
                    Message = updatedNoti.Message,
                    Status = updatedNoti.Status,
                    IsRead = false,
                    CreateAt = DateTime.UtcNow
                };

                var created = await _notiRepository.CreateNotificationAsync(newNoti);
                await SendRealTimeNotificationAsync(created);

                return new Result<Noti?>
                {
                    Data = created,
                    ResultStatus = ResultStatus.Success.ToString(),
                    Messages = new[] { "Notification created successfully." }
                };
            }
            catch (Exception ex)
            {
                return new Result<Noti?>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { ex.Message }
                };
            }
        }

    }
}