using API.Interfaces;
using API.Services;
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
    public async Task<IActionResult> GetUserById(string id)
    {
        var guid = Guid.Parse(id);
        
        var user = await _employeeSkillLevelService.GetUserByIdAsync(guid);

        return Ok(user);
    }
}