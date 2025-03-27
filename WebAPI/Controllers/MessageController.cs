using System.Net;
using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Message;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/messages")]
[ApiController]
public class MessageController : Controller
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessageController(IMessageService messageService, IHubContext<ChatHub> hubContext)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    [HttpGet("chatrooms/{chatRoomId}/messages")]
    public async Task<IActionResult> GetMessagesByChatRoomId(Guid chatRoomId)
    {
        var result = await _messageService.GetMessageByChatRoomId(chatRoomId);
        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data);
        }
        return NotFound(result.Messages);  
    }
    [HttpGet("messages/{orderId}/{customerId}/{employeeId}")]
    public async Task<IActionResult> GetMessagesByChatRoom(Guid orderId, Guid customerId, Guid employeeId)
    {
        var result = await _messageService.GetMessagesByChatRoom(orderId, customerId, employeeId);

        if (result.ResultStatus == ResultStatus.NotFound.ToString())
        {
            return NotFound(new { Message = result.Messages });
        }
        return Ok(new { Messages = result.Messages, Data = result.Data });
    }

    [HttpPost("create-message")]
    public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequest request)
    {
        var result = await _messageService.SendMessage(request);


        if (result == null)
        {
            return StatusCode(500, "Internal Server Error: result is null.");
        }

        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            if (result.Data == null)
            {
                return StatusCode(500, "Internal Server Error: result.Data is null.");
            }

            await _hubContext.Clients.Group(request.ChatRoomId.ToString())
                .SendAsync("ReceiveMessage", result.Data);
            return Ok(result.Data);
        }

        return BadRequest(result.Messages);

    }

    [HttpPut("messages/{messageId}/status")]
    public async Task<IActionResult> UpdateMessageStatus(Guid messageId, [FromBody] string newStatus)
    {
        var result = await _messageService.UpdateMessageStatus(messageId, newStatus);

        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data);
        }
        return BadRequest(result.Messages);
    }
    [HttpDelete("delete-messages")]
    public async Task<ActionResult<Result<Message>>> DeleteMessage(Guid id)
    {
        var result = await _messageService.DeleteMessage(id);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
}