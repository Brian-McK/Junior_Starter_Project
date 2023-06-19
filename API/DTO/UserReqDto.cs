using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class UserReqDto
{
    public string? Username { get; set; }
    
    public string? PasswordHash { get; set; }
}