using API.DTO;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController: ControllerBase
{
    private readonly IEmployeeSkillLevelService _employeeSkillLevelService;
    public UsersController(IEmployeeSkillLevelService employeeSkillLevelService)
    {
        _employeeSkillLevelService = employeeSkillLevelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _employeeSkillLevelService.GetAllUsersAsync();

        return Ok(users);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var user = await _employeeSkillLevelService.GetUserByIdAsync(id);

        return Ok(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddNewUser([FromBody] UserReqDto? newUserReq)
    {
        if (newUserReq == null)
        {
            return BadRequest();
        }
        
        var newUser = new User
        {
            Username = newUserReq.Username,
            PasswordHash = newUserReq.Password
        };
            
        await _employeeSkillLevelService.AddUserAsync(newUser);

        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> EditUserDetails(string? id, [FromBody] UserReqDto? newUserReq)
    {
        if (id == null || newUserReq == null)
        {
            return BadRequest();
        }

        var user = await _employeeSkillLevelService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var updatedUser = new User
        {
            Id = user.Id,
            Username = newUserReq.Username,
            PasswordHash = newUserReq.Password
        };

        var isUpdatedUser = await _employeeSkillLevelService.UpdateUserAsync(updatedUser);

        return isUpdatedUser ? Ok() : BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var requestResult = await _employeeSkillLevelService.DeleteUserAsync(id);

        return requestResult ? NoContent() : NotFound();
    }
}