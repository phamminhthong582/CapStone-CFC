using Microsoft.AspNetCore.Mvc;
using Service.Implement;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBoxController : Controller
    {


        private readonly GeminiService _geminiService;
        private readonly ImageService _imageService;

        public ChatBoxController(GeminiService geminiService, ImageService imageService)
        {
            _geminiService = geminiService;
            _imageService = imageService;
        }

        [HttpPost("generate-image")]
        public async Task<IActionResult> GenerateImages([FromBody] ImageRequest request)
        {
            var description = await _geminiService.GenerateTextAsync(request.Prompt);
            var imageUrl = await _imageService.GenerateImageAsync(request.Prompt);

            return Ok(new { imageUrl });
        }
        [HttpPost("chat-box")]
        public async Task<IActionResult> Chatbot([FromBody] ImageRequest request)
        {
            var description = await _geminiService.GenerateTextAsync(request.Prompt);
            return Ok(new { description });
        }
    }

    public class ImageRequest
    {
        public string Prompt { get; set; }
    }

}
