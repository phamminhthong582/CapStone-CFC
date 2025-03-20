using Microsoft.AspNetCore.Mvc;
using Service.Implement;

namespace WebAPI.Controllers;

public class ChatBoxController : ControllerBase
{
    private readonly GeminiService _geminiService;

    public ChatBoxController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }
    [HttpPost("chat-box")]
    public async Task<IActionResult> GenerateImage([FromBody] ImageRequest request)
    {
        var description = await _geminiService.GenerateTextAsync(request.Prompt);
        return Ok(new { description });
    }
    public class ImageRequest
    {
        public string Prompt { get; set; }
    }
}