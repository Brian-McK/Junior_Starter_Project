using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class UserReqDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 50 characters")]
    public string? Username { get; set; }
    
    // [Required(ErrorMessage = "Password is required")]
    // [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@#$%^&+=!])(?!.*\s).{8,}$", 
    //     ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string? Password { get; set; }
}