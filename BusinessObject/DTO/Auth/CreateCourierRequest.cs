using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessObject.DTO.Auth;

public class CreateCourierRequest
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

    public string? NumberMoto { get; set; }
    
    public string? ColorMoto {  get; set; }
    
    public string? MotoType { get; set; }   
    

}