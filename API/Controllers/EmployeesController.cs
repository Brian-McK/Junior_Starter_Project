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

        return Ok(employees);
    }
    
    [HttpGet("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmployeeById(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var employee = await _employeeSkillLevelService.GetEmployeeByIdAsync(id);

        return Ok(employee);
    }
    
    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddNewEmployee([FromBody] EmployeeCreateDto newEmpReq)
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
            SkillLevelIds = new List<ObjectId>(),
            IsActive = newEmpReq.IsActive,
            Age = DateTime.Now.Year - newEmpReq.Dob!.Value.Year
        };

        foreach (var newEmployeeSkillLevelId in newEmpReq.SkillLevelIds)
        {
            newEmployee.SkillLevelIds.Add(new ObjectId(newEmployeeSkillLevelId));
        }

        await _employeeSkillLevelService.AddEmployeeAsync(newEmployee);
        
        return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, newEmployee.Id);
    }
    
    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(string? id, [FromBody] EmployeeEditDto? updateEmployeeReq)
    {
        if (updateEmployeeReq == null)
        {
            return BadRequest();
        }
        
        var employee = await _employeeSkillLevelService.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            return BadRequest();
        }

        employee.FirstName = updateEmployeeReq.FirstName;
        employee.LastName = updateEmployeeReq.LastName;
        employee.Dob = updateEmployeeReq.Dob;
        employee.Email = updateEmployeeReq.Email;
        employee.IsActive = updateEmployeeReq.IsActive;
        employee.Age = DateTime.Now.Year - updateEmployeeReq.Dob!.Value.Year;

        var isUpdatedEmployee = await _employeeSkillLevelService.UpdateEmployeeAsync(employee);

        return isUpdatedEmployee ? Ok(employee) : BadRequest();
    }
    
    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(string? id)
    {
        if (id == null)
        {
            return BadRequest();
        }
        
        var requestResult = await _employeeSkillLevelService.DeleteEmployeeAsync(id);

        return requestResult ? Ok() : NotFound();
    }
}