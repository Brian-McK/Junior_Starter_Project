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

        var refreshToken = GenerateRefreshToken();
        
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
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = "eknfwnkergknrgwer";

        if (refreshToken == null)
        {
            return Unauthorized("Invalid refresh token");
        }
        
        // Create a JWT token handler
        var tokenHandler = new JwtSecurityTokenHandler();
        //
        // try
        // {
        //     // Validate and decrypt the JWT token
        //     ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
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
        
        // decrypt jwt here
        
        // get user id
        
        // get expiry
        
    
        var newRefreshToken = GenerateRefreshToken();
        SetRefreshTokenToCookie(newRefreshToken);
    
        return Ok(refr);
    }
    
    private RefreshToken GenerateRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.Now.AddDays(1),
        };

        return refreshToken;
    }
    
    private void SetRefreshTokenToCookie(RefreshToken refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // prevents client-side scripts accessing the data
            Expires = refreshToken.Expires
        };
        
        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        
        // below needs to be changed as tutorial is saving in memory - he recommended saving in db but 
        var user = new User();

        user.RefreshToken = refreshToken.Token;
        user.TokenCreated = refreshToken.CreatedDate;
        user.TokenExpires = refreshToken.Expires;
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