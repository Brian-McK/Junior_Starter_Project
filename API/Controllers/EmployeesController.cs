using API.DTO;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController: ControllerBase
{
    private readonly IEmployeeSkillLevelService _employeeSkillLevelService;

    public EmployeesController(IEmployeeSkillLevelService employeeSkillLevelService)
    {
        _employeeSkillLevelService = employeeSkillLevelService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        var employees = await _employeeSkillLevelService.GetAllEmployeesAsync();

        return Ok(employees);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeById(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var employee = await _employeeSkillLevelService.GetEmployeeByIdAsync(id);

        return Ok(employee);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddNewEmployee([FromBody] EmployeeCreateDto? newEmpReq)
    {
        if (newEmpReq == null)
        {
            return BadRequest();
        }

        var newEmployee = new Employee
        {
            FirstName = newEmpReq.FirstName,
            LastName = newEmpReq.LastName,
            Dob = newEmpReq.Dob,
            Email = newEmpReq.Email,
            SkillLevels = new List<SkillLevel>(),
            IsActive = newEmpReq.IsActive,
            Age = newEmpReq.Age
        };

        var newSkillLevel = new SkillLevel
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = newEmpReq.SkillLevel.Name,
            Description = newEmpReq.SkillLevel.Description
        };
        
        newEmployee.SkillLevels.Add(newSkillLevel);
        
        await _employeeSkillLevelService.AddEmployeeAsync(newEmployee);
        
        await _employeeSkillLevelService.AddSkillLevelAsync(newSkillLevel);
        
        return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, newEmployee.Id);
    }
}