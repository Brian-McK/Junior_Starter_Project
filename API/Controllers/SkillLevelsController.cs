using API.DTO;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillLevelsController: ControllerBase
{
    private readonly IEmployeeSkillLevelService _employeeSkillLevelService;

    public SkillLevelsController(IEmployeeSkillLevelService employeeSkillLevelService)
    {
        _employeeSkillLevelService = employeeSkillLevelService;
    }
    
    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllSkillLevels()
    {
        var skillLevels = await _employeeSkillLevelService.GetAllSkillLevelsAsync();

        return Ok(skillLevels);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSkillLevelById(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var skillLevel = await _employeeSkillLevelService.GetSkillLevelByIdAsync(id);

        return Ok(skillLevel);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddNewSkillLevel([FromBody] SkillLevelCreateDto? skillLevelReq)
    {
        if (skillLevelReq == null)
        {
            return BadRequest();
        }
        
        var newSkillLevel = new SkillLevel
        {
            Name = skillLevelReq.Name,
            Description = skillLevelReq.Description
        };
            
        await _employeeSkillLevelService.AddSkillLevelAsync(newSkillLevel);

        return CreatedAtAction(nameof(GetSkillLevelById), new { id = newSkillLevel.Id }, newSkillLevel);
    }
}