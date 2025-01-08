using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BusinessObject.DTO.Response;

public class UserResponse
{
    public int UserId { get; set; } 
    public string? Password { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; } 
    [EmailAddress]public string? Email { get; set; }
    public string? Phone { get; set; }
    public long Gender { get; set; } 
    public int RoleId { get; set; }
}