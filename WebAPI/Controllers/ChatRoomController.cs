using System.Net;
using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Commons;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;

[Route("api/chatRooms")]
[ApiController]
public class ChatRoomController : Controller
{
    private readonly IChatRoomService _chatRoomService;

    public ChatRoomController(IChatRoomService chatRoomService)
    {
        _chatRoomService = chatRoomService;
    }
    [HttpGet]
    public async Task<IActionResult> GetChatRoom()
    {
        var result = await _chatRoomService.GetAllChatRoom();
        return Ok(result);
    }
    [HttpGet("Id")]
    public async Task<IActionResult> GetChatRoomById(Guid id)
    {
        var result = await _chatRoomService.GetChatRoomById(id);

        if (result.ResultStatus != ResultStatus.Success.ToString())
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        return Ok(result);
    }
    [HttpPost("create-chatroom")]
    public async Task<ActionResult<Result<ChatRoom>>> CreateChatRoom( [FromForm] CreateChatRoomRequest request)
    {
        var result = await _chatRoomService.CreateChatRoom(request);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
    [HttpPut("{chatRoomId}/status")]
    public async Task<IActionResult> UpdateStatusChatRoom(Guid chatRoomId, [FromBody] string newStatus)
    {
        var result = await _chatRoomService.UpdateStatusChatRoom(chatRoomId, newStatus);

        if (result.ResultStatus == ResultStatus.NotFound.ToString())
        {
            return NotFound(result.Messages); 
        }
        if (result.ResultStatus == ResultStatus.Success.ToString())
        {
            return Ok(result.Data); 
        }
        return BadRequest(result.Messages); 
    }
    [HttpDelete("delete-chatroom")]
    public async Task<ActionResult<Result<ChatRoom>>> DeleteChatRoom(Guid id)
    {
        var result = await _chatRoomService.DeleteChatRoom(id);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
}