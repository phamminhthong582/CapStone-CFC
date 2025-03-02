using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Notification;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/Notifications")]
[ApiController]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    [HttpGet("user/{userId}/notifications")]
    public async Task<IActionResult> GetNotificationsByUserId(Guid userId)
    {
        var result = await _notificationService.GetNotificationsByUserId(userId);

        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data);
        }

        return NotFound(result.Messages);
    }
    [HttpGet("{notificationId}")]
    public async Task<IActionResult> GetNotificationById(Guid notificationId)
    {
        var result = await _notificationService.GetNotificationById(notificationId);

        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data);
        }

        return NotFound(result.Messages);
    }
    [HttpPost("create-notification")]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)
    {
        var result = await _notificationService.CreateNotification(request);

        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Messages);
    }
    [HttpPut("{notificationId}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var result = await _notificationService.MarkAsRead(notificationId);

        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Messages);
    }
    [HttpPut("{notificationId}/update-status")]
    public async Task<IActionResult> UpdateNotificationStatus(Guid notificationId, [FromBody] string status)
    {
        var result = await _notificationService.UpdateNotificationStatus(notificationId, status);

        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Messages);
    }
}