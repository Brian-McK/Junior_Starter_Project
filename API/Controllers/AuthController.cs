using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

    public AuthController(IEmployeeSkillLevelService employeeSkillLevelService)
    {
        _employeeSkillLevelService = employeeSkillLevelService;
    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserReqDto? registerUserDetails)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDetails.PasswordHash);
    
        var user = new User
        {
            Username = registerUserDetails.Username,
            PasswordHash = passwordHash
        };
        
        // check if user with username exists first
        if (_employeeSkillLevelService.GetUserByUsernameAsync(registerUserDetails.Username).Result.Username == registerUserDetails.Username)
        {
            return BadRequest();
        }

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

        if (!(user.Username == loginDetails.Username && BCrypt.Net.BCrypt.Verify(user.PasswordHash, loginDetails.PasswordHash)))
        {
            return Unauthorized();
        }
        
        return Ok(GenerateJwtToken(user.Id));
    }
    
    private static string GenerateJwtToken(string userId)
    {
        const string secretKey = "this is my custom Secret key for authentication";
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }

}