using API.Models;

namespace API.Interfaces;

public interface ITokenService
{ 
    string GenerateToken(string username, TimeSpan expiration, string? secret, string role);

    string GenerateJwtToken(string username, string role);

    RefreshToken GenerateRefreshToken(string username, string role);

    KeyValuePair<string, string>? GetTokenFromCookies(HttpRequest request, string tokenName);

    void AssignRefreshTokenToCookie(HttpResponse response, string cookieName, RefreshToken refreshToken);

    void RemoveCookie(HttpResponse response, string cookieName);
}