using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class NewUserReqDto
{
    public string? Username { get; set; }
    
    public string? PasswordHash { get; set; }
}