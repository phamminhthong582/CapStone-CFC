using Microsoft.AspNetCore.Mvc;
using Service.Implement;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : Controller
    {
        private readonly CloudinaryService _cloudinaryService;

        public UploadController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File không hợp lệ!");
            }

            using var stream = file.OpenReadStream();
            var imageUrl = await _cloudinaryService.UploadImageAsync(stream, file.FileName);

            return Ok(new { Url = imageUrl });
        }
    }
}
