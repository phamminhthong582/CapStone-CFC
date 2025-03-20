using BusinessObject.DTO.Message;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ChatGptController : ControllerBase
{
    private readonly IChatgptService _chatGptService;

    public ChatGptController(IChatgptService chatGptService)
    {
        _chatGptService = chatGptService;
    }

    [HttpPost("send")]  // Route này cần phải khớp với URL trong cURL
    public async Task<IActionResult> SendMessage([FromBody] string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return BadRequest("Content is required.");
        }

        var response = await _chatGptService.GetChatGptResponse(content);
        return Ok(new { response });
    }
}