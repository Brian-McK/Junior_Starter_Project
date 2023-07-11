using API.Controllers;
using API.DTO;
using API.Interfaces;
using API.Models;
using API.Tests.MockData;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace API.Tests;

public class TestEmployeesController
{
    [Fact]
    public async Task GetAllEmployees_ReturnsOkResultWithEmployees()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var expectedListOfEmployees = EmployeesMockData.GetEmployees();
            
        mockEmployeeSkillLevelService.Setup(service => service.GetAllEmployeesAsync())
            .ReturnsAsync(expectedListOfEmployees);
        
        var result = await controller.GetAllEmployees();
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEmployees = Assert.IsAssignableFrom<List<Employee>>(okResult.Value);
        Assert.Equal(expectedListOfEmployees, returnedEmployees);
    }
    
    [Fact]
    public async Task GetAllEmployees_ReturnsNoContentWhenNoEmployees()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var expectedEmptyListOfEmployees = new List<Employee>();

        mockEmployeeSkillLevelService.Setup(service => service.GetAllEmployeesAsync())
            .ReturnsAsync(expectedEmptyListOfEmployees);
        
        var result = await controller.GetAllEmployees();
        
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }
    
    [Fact]
    public async Task GetEmployeeById_ReturnsOkResult_WhenValidEmployeeId()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var employeeId = EmployeesMockData.GetEmployees().FirstOrDefault()!.ToString();

        var expectedEmployee = EmployeesMockData.GetEmployees().FirstOrDefault();

        mockEmployeeSkillLevelService.Setup(service => service.GetEmployeeByIdAsync(employeeId!))!
            .ReturnsAsync(expectedEmployee);
        
        var result = await controller.GetEmployeeById(employeeId);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEmployee = Assert.IsType<Employee>(okResult.Value);
        Assert.Equal(expectedEmployee, returnedEmployee);
    }
    
    [Fact]
    public async Task GetEmployeeById_ReturnsBadRequest_WhenEmployeeIdIsNull()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        const string employeeId = null!;
        
        var result = await controller.GetEmployeeById(employeeId);
        
        Assert.IsType<BadRequestResult>(result);
    }
    
    [Fact]
    public async Task GetEmployeeById_ReturnsNotFound_WhenEmployeeIdIsNotFound()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        const string employeeId = "323232323";
        
        var result = await controller.GetEmployeeById(employeeId);
        
        Assert.IsType<NotFoundObjectResult>(result);
    }
    
    [Fact]
    public async Task AddNewEmployee_ReturnsBadRequest_WhenRequestIsNull()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);
        
        var result = await controller.AddNewEmployee(null!);
        
        Assert.IsType<BadRequestResult>(result);
    }
    
    [Fact]
    public async Task AddNewEmployee_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var existingEmployeeEmail = EmployeesMockData.GetEmployees().FirstOrDefault()!.Email;
        
        var addEmployeeRequest = new AddEmployeeRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Dob = new DateTime(1990, 1, 1),
            Email = existingEmployeeEmail,
            IsActive = true,
            SkillLevelIds = new List<string>()
        };

        var expectedExistingEmployee = EmployeesMockData.GetEmployees().Find(emp => emp.Email == existingEmployeeEmail);
        
        mockEmployeeSkillLevelService.Setup(service => service.GetEmployeeByEmailAsync(existingEmployeeEmail!))!
            .ReturnsAsync(expectedExistingEmployee);
        
        var result = await controller.AddNewEmployee(addEmployeeRequest);
        
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Email Already Exists", conflictResult.Value);
    }
    
    [Fact]
    public async Task AddNewEmployee_ReturnsCreated_WhenValidAddEmployeeRequest()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var addEmployeeRequest = new AddEmployeeRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Dob = new DateTime(1990, 1, 1),
            Email = "johndoe@gmail.com",
            IsActive = true,
            SkillLevelIds = new List<string>()
        };

        var newEmployee = new Employee
        {
            FirstName = addEmployeeRequest.FirstName,
            LastName = addEmployeeRequest.LastName,
            Dob = addEmployeeRequest.Dob,
            Email = addEmployeeRequest.Email,
            IsActive = addEmployeeRequest.IsActive
        };

        mockEmployeeSkillLevelService.Setup(service => service.GetEmployeeByEmailAsync(addEmployeeRequest.Email))!
            .ReturnsAsync((Employee)null!);
        
        mockEmployeeSkillLevelService.Setup(service => service.AddEmployeeAsync(newEmployee))
            .Returns(Task.CompletedTask);
        
        var result = await controller.AddNewEmployee(addEmployeeRequest);
        
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EmployeesController.GetEmployeeById), createdAtActionResult.ActionName);
    }
    
    [Fact]
    public async Task UpdateEmployee_ReturnsBadRequest_WhenRequestIsNull()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);
        
        var result = await controller.UpdateEmployee(EmployeesMockData.GetEmployees().FirstOrDefault()!.Id, null);
        
        Assert.IsType<BadRequestResult>(result);
    }
    
    [Fact]
    public async Task UpdateEmployee_ReturnsBadRequest_WhenEmployeeNotFound()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        const string id = "232323232323";

        var updateEmployeeRequest = new EmployeeEditDto
        {
            FirstName = "John",
            LastName = "Doe",
            Dob = new DateTime(1990, 1, 1),
            Email = "johndoe@gmail.com",
            SkillLevelIds = new List<string>(),
            IsActive = true
        };

        mockEmployeeSkillLevelService.Setup(service => service.GetEmployeeByIdAsync(id))!
            .ReturnsAsync((Employee)null!);
        
        var result = await controller.UpdateEmployee(id, updateEmployeeRequest);
        
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsOkResult_WhenValidUpdateEmployeeRequest()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var employeeId = EmployeesMockData.GetEmployees().FirstOrDefault()!.Id;
        
        var updateEmployeeRequest = new EmployeeEditDto
        {
            FirstName = "John",
            LastName = "Doe",
            Dob = new DateTime(1990, 1, 1),
            Email = "johndoe@example.com",
            SkillLevelIds = SkillLevelsMockData.GetSkillLevels().Select(s => s.Id).ToList(),
            IsActive = true
        };

        var existingEmployee = EmployeesMockData.GetEmployees().Find(e => e.Id == employeeId);

        mockEmployeeSkillLevelService.Setup(service => service.GetEmployeeByIdAsync(employeeId))!
            .ReturnsAsync(existingEmployee);

        var expectedEmployeeWhenUpdated = new Employee
        {
            Id = existingEmployee!.Id,
            FirstName = updateEmployeeRequest.FirstName,
            LastName = updateEmployeeRequest.LastName,
            Dob = updateEmployeeRequest.Dob,
            Email = updateEmployeeRequest.Email,
            SkillLevelIds = updateEmployeeRequest.SkillLevelIds.Select(ObjectId.Parse).ToList(),
            IsActive = updateEmployeeRequest.IsActive
        };
        
        mockEmployeeSkillLevelService.Setup(service => service.UpdateEmployeeAsync(existingEmployee))
            .ReturnsAsync(true);
        
        var result = await controller.UpdateEmployee(employeeId, updateEmployeeRequest);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEmployee = Assert.IsType<Employee>(okResult.Value);
        Assert.Equal(employeeId, returnedEmployee.Id);
        Assert.Equal(expectedEmployeeWhenUpdated.FirstName, returnedEmployee.FirstName);
        Assert.Equal(expectedEmployeeWhenUpdated.LastName, returnedEmployee.LastName);
        Assert.Equal(expectedEmployeeWhenUpdated.Dob, returnedEmployee.Dob);
        Assert.Equal(expectedEmployeeWhenUpdated.Email, returnedEmployee.Email);
        Assert.Equal(expectedEmployeeWhenUpdated.SkillLevelIds.Count, returnedEmployee.SkillLevelIds.Count);
        Assert.Equal(expectedEmployeeWhenUpdated.IsActive, returnedEmployee.IsActive);
    }
    
    [Fact]
    public async Task DeleteEmployee_ReturnsBadRequest_WhenIdIsNull()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);
        
        var result = await controller.DeleteEmployee(null);
        
        Assert.IsType<BadRequestResult>(result);
    }
    
    [Fact]
    public async Task DeleteEmployee_ReturnsOkResult_WhenEmployeeDeletedSuccessfully()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var employeeId = EmployeesMockData.GetEmployees().FirstOrDefault()!.Id;

        mockEmployeeSkillLevelService.Setup(service => service.DeleteEmployeeAsync(employeeId))
            .ReturnsAsync(true);
        
        var result = await controller.DeleteEmployee(employeeId);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Deleted Successfully", okResult.Value);
    }
    
    [Fact]
    public async Task DeleteEmployee_ReturnsNotFound_WhenEmployeeDeleteUnsuccessful()
    {
        var mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();
        var controller = new EmployeesController(mockEmployeeSkillLevelService.Object);

        var employeeId = "hdhdhdhd";

        mockEmployeeSkillLevelService.Setup(service => service.DeleteEmployeeAsync(employeeId))
            .ReturnsAsync(false);
        
        var result = await controller.DeleteEmployee(employeeId);
        
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Unsuccessful Delete", notFoundResult.Value);
    }
}