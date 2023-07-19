using API.DTO;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;

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
    
    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllEmployees()
    {
        var employees = await _employeeSkillLevelService.GetAllEmployeesAsync();

        return employees.Any() ? Ok(employees) : NoContent();
    }
    
    [HttpGet("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmployeeById(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var employee = await _employeeSkillLevelService.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            return NotFound("Employee Not Found!");
        }

        return Ok(employee);
    }
    
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddNewEmployee([FromBody] AddEmployeeRequestDto newEmpReq)
    {
        if (newEmpReq == null)
        {
            return BadRequest();
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var employee = await _employeeSkillLevelService.GetEmployeeByEmailAsync(newEmpReq.Email);

        if (employee != null)
        {
            return Conflict("User Already Exists");
        }

        var newEmployee = new Employee
        {
            FirstName = newEmpReq.FirstName!,
            LastName = newEmpReq.LastName!,
            Dob = newEmpReq.Dob,
            Email = newEmpReq.Email,
            SkillLevelIds = new List<ObjectId>(),
            IsActive = newEmpReq.IsActive,
            Age = DateTime.UtcNow.Year - newEmpReq.Dob!.Value.ToUniversalTime().Year
        };

        foreach (var newEmployeeSkillLevelId in newEmpReq.SkillLevelIds)
        {
          var skillExists = await _employeeSkillLevelService.GetSkillLevelByIdAsync(newEmployeeSkillLevelId);

          if (skillExists == null)
          {
              return BadRequest("Invalid skill entry, the skill doesnt exist");
          }
          
          newEmployee.SkillLevelIds.Add(new ObjectId(newEmployeeSkillLevelId));
        }

        await _employeeSkillLevelService.AddEmployeeAsync(newEmployee);
        
        return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, newEmployee.Id);
    }
    
    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(string? id, [FromBody] EmployeeEditDto? updateEmployeeReq)
    {
        if (updateEmployeeReq == null || id == null)
        {
            return BadRequest();
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var employee = await _employeeSkillLevelService.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            return BadRequest("Employee does not exist");
        }

        employee.FirstName = updateEmployeeReq.FirstName;
        employee.LastName = updateEmployeeReq.LastName;
        employee.Dob = updateEmployeeReq.Dob;
        employee.Email = updateEmployeeReq.Email;
        employee.IsActive = updateEmployeeReq.IsActive;
        employee.Age = DateTime.UtcNow.Year - updateEmployeeReq.Dob!.Value.Year;
        employee.SkillLevelIds.Clear();
        
        foreach (var updatedEmployeeSkillLevel in updateEmployeeReq.SkillLevelIds)
        {
            var skillExists = await _employeeSkillLevelService.GetSkillLevelByIdAsync(updatedEmployeeSkillLevel);

            if (skillExists == null)
            {
                return BadRequest("Invalid skill entry, the skill doesnt exist");
            }
          
            employee.SkillLevelIds.Add(new ObjectId(updatedEmployeeSkillLevel));
        }
        
        var isUpdatedEmployee = await _employeeSkillLevelService.UpdateEmployeeAsync(employee);

        return isUpdatedEmployee ? Ok(employee) : BadRequest("Employee Not Updated");
    }
    
    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var requestResult = await _employeeSkillLevelService.DeleteEmployeeAsync(id);

        return requestResult ? Ok("Deleted Successfully") : NotFound("Unsuccessful Delete");
    }
}