using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int UserId { get; set; } 

    [Required]
    public string Password { get; set; }

    [Required]
    public string FullName { get; set; }

    public string Address { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public string Phone { get; set; }

    public string Gender { get; set; } 

    public int RoleId { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime UpdateAt { get; set; }

    public bool Status { get; set; }

    public string Avatar { get; set; }

    public string Point { get; set; }

    public string Otp { get; set; }
}