using API.Controllers;
using API.DTO;
using API.Interfaces;
using API.Models;
using API.Tests.MockData;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using IConfiguration = Castle.Core.Configuration.IConfiguration;

namespace API.Tests;

public class TestAuthController
{
    private readonly Mock<IEmployeeSkillLevelService> _mockEmployeeSkillLevelService;
    private readonly IConfigurationRoot _configuration;
    private readonly Mock<IResponseCookies> _cookies;

    public TestAuthController()
    {
        _mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appsettings.json", false, false)
            .AddEnvironmentVariables()
            .Build();
        
        _cookies = new Mock<IResponseCookies>();
    }

    [Fact]
    public async Task RegisterUser_ReturnsBadRequest_WhenUserAlreadyExists()
    {
        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, null);

        var userThatExists = AuthMockData.GetUser();
        
        var registerUserDetails = new UserReqDto
        {
            Username = userThatExists.Username,
            Password = userThatExists.PasswordHash
        };
        
        var existingUser = AuthMockData.GetUsers().Find(u => u.Username == userThatExists.Username);
        
        _mockEmployeeSkillLevelService.Setup(service => service.GetUserByUsernameAsync(registerUserDetails.Username!))!
            .ReturnsAsync(existingUser);
        
        var result = await controller.RegisterUser(registerUserDetails);
        
        Assert.IsType<BadRequestResult>(result);
    }
    
    [Fact]
    public async Task RegisterUser_WhenUserDoesNotExist_ReturnsOkResult()
    {
        var httpContextMock = new Mock<HttpContext>();
        
        httpContextMock.Setup(c => c.Response.Cookies).Returns(_cookies.Object);

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        var userThatDoesntExist = AuthMockData.GetUserDoesntExist();
        
        var registerUserDetails = new UserReqDto
        {
            Username = userThatDoesntExist.Username,
            Password = userThatDoesntExist.PasswordHash
        };
        
        var existingUser = AuthMockData.GetUsers().Find(u => u.Username == registerUserDetails.Username);
        
        _mockEmployeeSkillLevelService.Setup(s => s.GetUserByUsernameAsync(registerUserDetails.Username!))!
            .ReturnsAsync(existingUser);
        
        var result = await controller.RegisterUser(registerUserDetails);
        
        Assert.IsType<OkObjectResult>(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<User>(okResult.Value);
        var user = Assert.IsType<User>(okResult.Value);
        Assert.Equal(registerUserDetails.Username, user.Username);
        Assert.True(BCrypt.Net.BCrypt.Verify(registerUserDetails.Password, user.PasswordHash));
    }
    
    [Fact]
    public async Task Authenticate_WithValidCredentials_ReturnsOkResult()
    {
        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _configuration);

        var userThatExists = AuthMockData.GetUser();
        
        var loginDetails = new UserReqDto
        {
            Username = userThatExists.Username,
            Password = "jimmy"
        };

        _mockEmployeeSkillLevelService.Setup(s => s.GetUserByUsernameAsync(loginDetails.Username!))
            .ReturnsAsync(userThatExists);
        
        var result = await controller.Authenticate(loginDetails);
        
        Assert.IsType<OkObjectResult>(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<AuthResponse>(okResult.Value);
        var authResponse = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(loginDetails.Username, authResponse.Username);
        Assert.NotNull(authResponse.JwtToken);
        Assert.NotNull(authResponse.RefreshToken);
    }
}