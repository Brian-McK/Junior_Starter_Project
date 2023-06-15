using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string? Username { get; set; }
    
    [Required]
    public string? Password { get; set; }
}