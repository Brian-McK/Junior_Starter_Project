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
    private readonly ITokenService _tokenService;

    public AuthController(IEmployeeSkillLevelService employeeSkillLevelService, IConfiguration configuration, ITokenService tokenService)
    {
        _employeeSkillLevelService = employeeSkillLevelService;
        _configuration = configuration;
        _tokenService = tokenService;
    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserReqDto? registerUserDetails)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var findUser = await _employeeSkillLevelService.GetUserByUsernameAsync(registerUserDetails.Username);
        
        // check if user with username exists first
        if (findUser != null)
        {
            return BadRequest("User already exists");
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _employeeSkillLevelService.GetUserByUsernameAsync(loginDetails.Username);

        if (user == null)
        {
            return NotFound("User Not Found");
        }
        
        if(loginDetails.Password != null && !IsValidCredentials(user, loginDetails.Password))
        {
            return Unauthorized("Incorrect Password");
        }
        
        const string role = "Admin";

        var jwtToken = _tokenService.GenerateJwtToken(user.Username, role);

        var refreshToken = _tokenService.GenerateRefreshToken(user.Username, role);
        
        _tokenService.AssignRefreshTokenToCookie(Response, "refreshToken", refreshToken);

        var authResponse = new AuthResponse
        {
            Username = user.Username,
            JwtToken = jwtToken,
            RefreshToken = refreshToken.Token
        };

        return Ok(authResponse);
    }
    
    [HttpPost("logout")]
    public ActionResult Logout()
    {
        _tokenService.RemoveCookie(Response, "refreshToken");

        return NoContent();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string username)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var cookieRefreshToken = _tokenService.GetTokenFromCookies(Request, "refreshToken");

        if (cookieRefreshToken == null)
        {
            return Unauthorized("Token does not exist");
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
        const string role = "Admin";

        try
        {
            // Validate and decrypt the JWT token
            principal = tokenHandler.ValidateToken(cookieRefreshToken?.Value, validationParameters, out var validatedToken);
            isValidDate = validatedToken.ValidTo > DateTime.Now;
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine(ex);
        }
        
        var isAdmin = principal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role);
            
        var isUser = principal.HasClaim(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == username);

        if (!(isAdmin && isUser && isValidDate))
        {
            return Forbid();
        }
        
        var newJwtToken = _tokenService.GenerateJwtToken(username, role);

        var jsonResponse = new { Username = username, JwtToken = newJwtToken };

        return Ok(jsonResponse);
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
            expires: DateTime.UtcNow.AddMinutes(20),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
    
    private bool IsValidCredentials(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}