using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.DTO;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly IEmployeeSkillLevelService _employeeSkillLevelService;
    private readonly IConfiguration _configuration;

    public AuthController(IEmployeeSkillLevelService employeeSkillLevelService, IConfiguration configuration)
    {
        _employeeSkillLevelService = employeeSkillLevelService;
        _configuration = configuration;
    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserReqDto? registerUserDetails)
    {

        var findUser = await _employeeSkillLevelService.GetUserByUsernameAsync(registerUserDetails.Username);
        
        // check if user with username exists first
        if (findUser != null)
        {
            return BadRequest();
        }
        
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDetails.Password);
    
        var user = new User
        {
            Username = registerUserDetails.Username,
            PasswordHash = passwordHash
        };
        
        await _employeeSkillLevelService.AddUserAsync(user);
    
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] UserReqDto? loginDetails)
    {
        if (loginDetails == null)
        {
            return BadRequest();
        }

        var user = await _employeeSkillLevelService.GetUserByUsernameAsync(loginDetails.Username);

        if (!(user.Username == loginDetails.Username && BCrypt.Net.BCrypt.Verify(loginDetails.Password, user.PasswordHash)))
        {
            return Unauthorized();
        }

        var jwtToken = GenerateJwtToken(user);

        var refreshToken = GenerateRefreshToken(user.Username);
        
        SetRefreshTokenToCookie(refreshToken);

        var authResponse = new AuthResponse
        {
            Username = user.Username,
            JwtToken = jwtToken,
            RefreshToken = refreshToken.Token
        };

        return Ok(authResponse);
    }

    [HttpGet("refresh-token")]
    public async Task<IActionResult> RefreshToken() // refresh token from redux state???
    {
        // get refresh token from http only cookie
        
        var cookieValues = Request.Cookies.ToDictionary(cookie => cookie.Key, cookie => cookie.Value);

        if (cookieValues.Count > 0)
        {
            // Access the cookie key-value pairs
            return Ok(cookieValues);
        }
        return NotFound();

        // var refreshTokenFromCookie = Request.Cookies.
        //
        // // check if it exists
        // if (refreshToken == null)
        // {
        //     return Unauthorized("Invalid refresh token");
        // }
        //
        // // decrypt the refresh token
        //
        // // Create a JWT token handler
        // var tokenHandler = new JwtSecurityTokenHandler();
        //
        // var validationParameters = new TokenValidationParameters
        // {
        //     ValidateIssuerSigningKey = true,
        //     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Refresh_Token").Value!)),
        //     ValidateIssuer = false,
        //     ValidateAudience = false,
        //     ClockSkew= TimeSpan.FromMinutes(0)
        // };
        //
        // try
        // {
        //     // Validate and decrypt the JWT token
        //     var claimsPrincipal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);
        //
        //     // Access the claims from the decrypted token
        //     var claims = claimsPrincipal.Claims;
        //     
        //     // Process the claims as needed
        //     foreach (Claim claim in claims)
        //     {
        //         string claimType = claim.Type;
        //         string claimValue = claim.Value;
        //         // Do something with the claims
        //     }
        // }
        // catch (SecurityTokenException ex)
        // {
        //     // Token validation failed
        //     // Handle the exception or error message accordingly
        // }
        //
        //
        // // ensure user etc...
        //
        //
        //
        //
        // // decrypt jwt here
        //
        // // get user id
        //
        // // get expiry
        //
        //
        // var newRefreshToken = GenerateRefreshToken();
        // SetRefreshTokenToCookie(newRefreshToken);
        //
        // return Ok(refreshToken);
    }
    
    private RefreshToken GenerateRefreshToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Refresh_Token").Value!));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, username!),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var refreshTokenStr = tokenHandler.WriteToken(token);

        var refreshToken = new RefreshToken
        {
            CreatedDate = DateTime.Now,
            Expires = token.ValidTo,
            Token = refreshTokenStr
        };

        return refreshToken;
    }
    
    private void SetRefreshTokenToCookie(RefreshToken refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // prevents client-side scripts accessing the data
            Expires = refreshToken.Expires,
            SameSite = SameSiteMode.None,
            Secure = true
        };
        
        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:JWT_Token").Value!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, user.Username!),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}