using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BusinessObject.DTO.Response;

public class UserResponse
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Gender { get; set; }
    
    public bool? Status { get; set; }

    public string? Avatar { get; set; }

    public int? Point { get; set; }


}