using API.DTO;
using API.Interfaces;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _employeeSkillLevelService.GetUserByIdAsync(id);

        return Ok(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NewUserReqDto? newUserReq)
    {
        if (newUserReq == null)
        {
            return BadRequest();
        }
        
        var newUser = new User
        {
            Username = newUserReq.Username,
            PasswordHash = newUserReq.PasswordHash
        };
            
        await _employeeSkillLevelService.AddUserAsync(newUser);

        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }
}