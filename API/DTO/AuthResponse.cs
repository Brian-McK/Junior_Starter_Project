namespace API.DTO;

public class AuthResponse
{
    public string JwtToken { get; set; } = string.Empty;
    
    public string RefreshToken { get; set; } = string.Empty;
}