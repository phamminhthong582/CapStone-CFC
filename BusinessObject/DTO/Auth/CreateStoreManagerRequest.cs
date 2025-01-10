namespace BusinessObject.DTO.Auth;

public class CreateStoreManagerRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
}