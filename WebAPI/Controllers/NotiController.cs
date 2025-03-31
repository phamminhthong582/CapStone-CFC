using BusinessObject.DTO.Commons;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotiController : ControllerBase
    {
        private readonly INotiService _notiService;

        public NotiController(INotiService notiService)
        {
            _notiService = notiService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var result = await _notiService.GetAllNotificationsAsync();
            return HandleResult(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(Guid userId)
        {
            var result = await _notiService.GetUserNotificationsAsync(userId);
            return HandleResult(result);
        }

        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadCount(Guid userId)
        {
            var result = await _notiService.GetUnreadCountAsync(userId);
            return HandleResult(result);
        }

        [HttpGet("{notificationId}")]
        public async Task<IActionResult> GetNotificationById(Guid notificationId)
        {
            var result = await _notiService.GetNotificationByIdAsync(notificationId);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] Noti notification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Result<Noti>
                {
                    ResultStatus = ResultStatus.Invalid.ToString(),
                    Messages = new[] { "Invalid notification data" }
                });
            }

            var result = await _notiService.CreateNotificationAsync(notification);

            if (result.ResultStatus == ResultStatus.Success.ToString())
            {
                return CreatedAtAction(
                    nameof(GetNotificationById),
                    new { notificationId = result.Data?.NotiId },
                    result);
            }

            return HandleResult(result);
        }

        [HttpPut("read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var result = await _notiService.MarkAsReadAsync(notificationId);
            return HandleResult(result);
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            var result = await _notiService.DeleteNotificationAsync(notificationId);
            return HandleResult(result);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendRealTimeNotification([FromBody] Noti notification)
        {
            var result = await _notiService.SendRealTimeNotificationAsync(notification);
            return HandleResult(result);
        }

        private IActionResult HandleResult<T>(Result<T> result)
        {
            switch (result.ResultStatus)
            {
                case nameof(ResultStatus.Success):
                    return Ok(result);

                case nameof(ResultStatus.NotFound):
                    return NotFound(result);

                case nameof(ResultStatus.Invalid):
                    return BadRequest(result);

                case nameof(ResultStatus.Duplicated):
                    return Conflict(result);

                default:
                    return StatusCode(500, result);
            }
        }
    }
}