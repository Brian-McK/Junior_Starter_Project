using API.DTO;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly IEmployeeSkillLevelService _employeeSkillLevelService;
    private readonly ITokenService _tokenService;

    public AuthController(IEmployeeSkillLevelService employeeSkillLevelService, ITokenService tokenService)
    {
        _employeeSkillLevelService = employeeSkillLevelService;
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
        if (!ModelState.IsValid || loginDetails == null)
        {
            return BadRequest(ModelState);
        }

        var user = await _employeeSkillLevelService.GetUserByUsernameAsync(loginDetails.Username);

        if (user == null)
        {
            return NotFound("Incorrect Username or password");
        }
        
        if(loginDetails.Password != null && !BCrypt.Net.BCrypt.Verify(loginDetails.Password, user.PasswordHash))
        {
            return Unauthorized("Incorrect Username or Password");
        }
        
        const string role = "Admin";

        var jwtToken = _tokenService.GenerateJwtToken(user.Username, role);

        var refreshToken = _tokenService.GenerateRefreshToken(user.Username, role);
        
        _tokenService.AssignRefreshTokenToCookie(Response, "refreshToken", refreshToken);

        var refreshTokenStore = new RefreshTokenStore
        {
            UserId = user.Id,
            RefreshToken = refreshToken.Token,
            Created = refreshToken.CreatedDate,
        };

        await _employeeSkillLevelService.AddRefreshTokenAsync(refreshTokenStore);

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
            return BadRequest("Bad Request");
        }

        var cookieRefreshToken = _tokenService.GetTokenFromCookies(Request, "refreshToken");

        if (cookieRefreshToken == null)
        {
            return Unauthorized("Token does not exist");
        }

        var user = await _employeeSkillLevelService.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return Unauthorized("Unauthorized");
        }

        // check not equal
        var savedRefreshToken = await _employeeSkillLevelService.GetSavedRefreshToken(user.Id, cookieRefreshToken.Value.Value);

        if (savedRefreshToken?.RefreshToken != cookieRefreshToken.Value.Value)
        {
            return Unauthorized("Invalid Token, You have been logged out");
        }
        
        // extract role from claims?
        const string role = "Admin";

        if (!_tokenService.IsValidToken(cookieRefreshToken, username, role))
        {
            return Forbid();
        }

        var newJwtToken = _tokenService.GenerateJwtToken(username, role);

        var jsonResponse = new { Username = username, JwtToken = newJwtToken };

        return Ok(jsonResponse);
    }
}