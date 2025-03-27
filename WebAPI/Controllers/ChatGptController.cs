using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Service.Implement; 

namespace YourApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatGptController : ControllerBase
    {
        private readonly ChatGptService _chatGptService;
        public ChatGptController(ChatGptService chatGptService)
        {
            _chatGptService = chatGptService;
        }
        [HttpPost("chat-boxgpt")]
        public async Task<IActionResult> GenerateResponse([FromBody] UserMessageDto userMessageDto)
        {
            if (userMessageDto == null || string.IsNullOrWhiteSpace(userMessageDto.Message))
            {
                return BadRequest("User message cannot be empty.");
            }

            try
            {
                var response = await _chatGptService.GenerateTextAsync(userMessageDto.Message);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
    public class UserMessageDto
    {
        public string Message { get; set; }
    }
}