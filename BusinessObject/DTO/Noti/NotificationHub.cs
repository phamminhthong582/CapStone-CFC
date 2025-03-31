using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Noti
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinNotificationGroup(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId is required");
                    return;
                }

                _logger.LogInformation($"Adding user {userId} to group (Connection: {Context.ConnectionId})");
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} added to notification group");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining notification group");
                throw;
            }
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation($"New connection: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation(exception == null
                ? $"Connection {Context.ConnectionId} disconnected"
                : $"Connection {Context.ConnectionId} disconnected with error: {exception.Message}");

            return base.OnDisconnectedAsync(exception);
        }
    }
}
