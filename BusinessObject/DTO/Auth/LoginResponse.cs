namespace BusinessObject.DTO.Auth;

public class LoginResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
}