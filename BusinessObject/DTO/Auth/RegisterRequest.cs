using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // Thêm dòng này để nhận diện FromForm

namespace BusinessObject.DTO.Auth
{
    public class RegisterRequest
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string? IdentificationNumber { get; set; }

        [FromForm]
        public IFormFile? Avatar { get; set; }

        [FromForm]
        public IFormFile? IdentificationFontOfPhoto { get; set; }

        [FromForm]
        public IFormFile? IdentificationBackOfPhoto { get; set; }
    }
}
