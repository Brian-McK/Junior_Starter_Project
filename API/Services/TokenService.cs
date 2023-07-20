using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Interfaces;
using API.Models;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService: ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string? _jwtSecret;
    private readonly string? _refreshTokenSecret;
    
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _jwtSecret = _configuration["AppSettings:JWT_Token"];
        _refreshTokenSecret = _configuration["AppSettings:Refresh_Token"];
    }
    
    public string GenerateToken(string username, TimeSpan expiration, string? secret, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration),
            signingCredentials: credentials
        );

        return tokenHandler.WriteToken(token);
    }
    
    public string GenerateJwtToken(string username, string role)
    {
        return GenerateToken(username, TimeSpan.FromMinutes(15), _jwtSecret, role); // demo - change here
    }
    
    public RefreshToken GenerateRefreshToken(string username, string role)
    {
        var refreshTokenString = GenerateToken(username, TimeSpan.FromDays(1), _refreshTokenSecret, role); // demo - change here

        var refreshToken = new RefreshToken
        {
            CreatedDate = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(1), // demo - change here
            Token = refreshTokenString
        };

        return refreshToken;
    }

    public KeyValuePair<string, string>? GetTokenFromCookies(HttpRequest request ,string tokenName)
    {
        var cookieRefreshToken = request.Cookies.ToDictionary(cookie => cookie.Key, cookie => cookie.Value).FirstOrDefault();

        if (cookieRefreshToken.Key?.Equals(tokenName) == true && !string.IsNullOrEmpty(cookieRefreshToken.Value))
        {
            return cookieRefreshToken;
        }

        return null;
    }
    
    public void AssignRefreshTokenToCookie(HttpResponse response, string cookieName, RefreshToken refreshToken)
    {
        response.Cookies.Append(cookieName, refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshToken.Expires,
            SameSite = SameSiteMode.None,
            Secure = true
        });
    }
    
    public void RemoveCookie(HttpResponse response, string cookieName)
    {
        response.Cookies.Delete(cookieName, new CookieOptions()
        {
            SameSite = SameSiteMode.None,
            Secure = true,
        });
    }
    
    public bool IsValidToken(KeyValuePair<string, string>? token, string username, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_refreshTokenSecret!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        var principal = new ClaimsPrincipal();
        var isValidDate = false;

        try
        {
            principal = tokenHandler.ValidateToken(token.Value.Value, validationParameters, out var validatedToken);
            isValidDate = validatedToken.ValidTo > DateTime.UtcNow;
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine(ex);
        }

        var isAdmin = principal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role);
        var isUser = principal.HasClaim(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == username);

        return isAdmin && isUser && isValidDate;
    }
}