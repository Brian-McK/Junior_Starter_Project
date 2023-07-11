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
        return GenerateToken(username, TimeSpan.FromMinutes(20), _jwtSecret, role);
    }
    
    public RefreshToken GenerateRefreshToken(string username, string role)
    {
        var refreshTokenString = GenerateToken(username, TimeSpan.FromDays(1), _refreshTokenSecret, role);

        var refreshToken = new RefreshToken
        {
            CreatedDate = DateTime.Now,
            Expires = DateTime.Now.AddDays(1),
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
    
    
    
    
    
    
    
    
   // ___________________________________________________________________________________________________
    
    
    
    //
    // public string GenerateJwtToken(string username)
    // {
    //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:JWT_Token").Value!));
    //
    //     var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    //
    //     var claims = new[]
    //     {
    //         new Claim(JwtRegisteredClaimNames.Name, username!),
    //         new Claim(ClaimTypes.Role, "Admin")
    //     };
    //
    //     var token = new JwtSecurityToken(
    //         claims: claims,
    //         expires: DateTime.UtcNow.AddMinutes(20),
    //         signingCredentials: credentials
    //     );
    //
    //     var tokenHandler = new JwtSecurityTokenHandler();
    //     
    //     var tokenString = tokenHandler.WriteToken(token);
    //
    //     return tokenString;
    // }
    //
    // public RefreshToken GenerateRefreshToken(string username)
    // {
    //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Refresh_Token").Value!));
    //     
    //     var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    //     
    //     var claims = new[]
    //     {
    //         new Claim(JwtRegisteredClaimNames.Name, username!),
    //         new Claim(ClaimTypes.Role, "Admin"),
    //     };
    //
    //     var token = new JwtSecurityToken(
    //         claims: claims,
    //         expires: DateTime.Now.AddDays(1),
    //         signingCredentials: credentials
    //     );
    //
    //     var tokenHandler = new JwtSecurityTokenHandler();
    //     
    //     var refreshTokenStr = tokenHandler.WriteToken(token);
    //
    //     var refreshToken = new RefreshToken
    //     {
    //         CreatedDate = DateTime.Now,
    //         Expires = token.ValidTo,
    //         Token = refreshTokenStr
    //     };
    //
    //     return refreshToken;
    // }
    //
    // private void SetRefreshTokenToCookie(RefreshToken refreshToken)
    // {
    //     var cookieOptions = new CookieOptions
    //     {
    //         HttpOnly = true, // prevents client-side scripts accessing the data
    //         Expires = refreshToken.Expires,
    //         SameSite = SameSiteMode.None,
    //         Secure = true
    //     };
    //     
    //     Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
    // }
}