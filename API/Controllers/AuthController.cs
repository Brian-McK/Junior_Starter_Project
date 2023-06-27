using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

        var jwtToken = GenerateJwtToken(user.Username);

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

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string username)
    {
        if (username == null)
        {
            return BadRequest();
        }
        
        // get refresh token from http only cookie
        var cookieRefreshToken = Request.Cookies.ToDictionary(cookie => cookie.Key, cookie => cookie.Value).FirstOrDefault();

        if (!cookieRefreshToken.Key.Contains("refreshToken") && !cookieRefreshToken.Value.IsNullOrEmpty())
        {
            // Access the cookie key-value pairs
            return Unauthorized();
        }
        
        // validate and decrypt token
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Refresh_Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew= TimeSpan.FromMinutes(0)
        };

        var principal = new ClaimsPrincipal();
        var isValidDate = false;

        try
        {
            // Validate and decrypt the JWT token
            principal = tokenHandler.ValidateToken(cookieRefreshToken.Value, validationParameters, out var validatedToken);
            isValidDate = validatedToken.ValidTo > DateTime.Now;
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine(ex);
        }
        
        var isAdmin = principal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            
        var isUser = principal.HasClaim(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == username);

        if (!(isAdmin && isUser && isValidDate))
        {
            return Forbid();
        }
        
        var newJwtToken = GenerateJwtToken(username);

        return Ok(newJwtToken);
    }
    
    private RefreshToken GenerateRefreshToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Refresh_Token").Value!));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, username!),
            new Claim(ClaimTypes.Role, "Admin"),
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

    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:JWT_Token").Value!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, username!),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(10),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}