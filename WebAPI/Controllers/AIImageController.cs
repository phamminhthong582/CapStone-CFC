using Microsoft.AspNetCore.Mvc;
using Service.Implement;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIImageController : Controller
    {

        /*    private readonly OpenAIService _openAIService;

            public AIImageController(OpenAIService openAIService)
            {
                _openAIService = openAIService;
            }*/
        /*
                  [HttpPost("generate")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageRequest request)
        {
            try
            {
                var imageUrl = await _openAIService.GenerateImageAsync(request.Prompt);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Lỗi khi gọi OpenAI: {ex.Message}" });
            }
        }
                }

                public class ImageRequest
                {
                    public string Prompt { get; set; }
                }*/
        private readonly GeminiService _geminiService;
        private readonly ImageService _imageService;

        public AIImageController(GeminiService geminiService, ImageService imageService)
        {
            _geminiService = geminiService;
            _imageService = imageService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageRequest request)
        {
            var description = await _geminiService.GenerateTextAsync(request.Prompt);
            var imageUrl = await _imageService.GenerateImageAsync(request.Prompt);

            return Ok(new { imageUrl });
        }
    }

    public class ImageRequest
    {
        public string Prompt { get; set; }
    }

}
