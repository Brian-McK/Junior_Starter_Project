namespace API.Models;

public class RefreshToken
{
    public required string Token { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime Expires { get; set; }
}