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
        
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDetails.PasswordHash);
    
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

        if (!(user.Username == loginDetails.Username && BCrypt.Net.BCrypt.Verify(user.PasswordHash, loginDetails.PasswordHash)))
        {
            return Unauthorized();
        }
        
        return Ok(GenerateJwtToken(user));
    }
    
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:JWT_Token").Value!));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username!)
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