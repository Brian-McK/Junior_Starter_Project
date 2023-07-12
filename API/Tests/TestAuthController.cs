using API.Controllers;
using API.DTO;
using API.Interfaces;
using API.Models;
using API.Tests.MockData;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.Tests;

public class TestAuthController
{
    private readonly Mock<IEmployeeSkillLevelService> _mockEmployeeSkillLevelService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly IConfigurationRoot _configuration;
    private readonly Mock<IResponseCookies> _cookies;

    public TestAuthController()
    {
        _mockEmployeeSkillLevelService = new Mock<IEmployeeSkillLevelService>();

        _mockTokenService = new Mock<ITokenService>();

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
        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object);

        var expectedExistingUser = AuthMockData.GetUser();

        var registerUserDetails = new UserReqDto
        {
            Username = expectedExistingUser.Username,
            Password = expectedExistingUser.PasswordHash
        };

        _mockEmployeeSkillLevelService.Setup(service => service.GetUserByUsernameAsync(registerUserDetails.Username!))!
            .ReturnsAsync(expectedExistingUser);

        var result = await controller.RegisterUser(registerUserDetails);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RegisterUser_WhenUserDoesNotExist_ReturnsOkResult()
    {
        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object);
        
        var userThatDoesntExist = AuthMockData.GetUserDoesntExist();

        var registerUserDetails = new UserReqDto
        {
            Username = userThatDoesntExist.Username,
            Password = userThatDoesntExist.PasswordHash
        };

        var expectedNonExistingUser = AuthMockData.GetUsers().Find(u => u.Username == registerUserDetails.Username);

        _mockEmployeeSkillLevelService.Setup(s => s.GetUserByUsernameAsync(registerUserDetails.Username!))!
            .ReturnsAsync(expectedNonExistingUser);

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
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.Setup(c => c.Response.Cookies).Returns(_cookies.Object);

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        var userThatExists = AuthMockData.GetUser();

        var loginDetails = new UserReqDto
        {
            Username = userThatExists.Username,
            Password = "jimmy"
        };

        _mockEmployeeSkillLevelService.Setup(s => s.GetUserByUsernameAsync(loginDetails.Username!))
            .ReturnsAsync(userThatExists);

        const string jwt = "sfdsdsdsdsddssds";
        
        _mockTokenService.Setup(s => s.GenerateJwtToken(loginDetails.Username!,"Admin"))
            .Returns(jwt);

        var refresh = new RefreshToken
        {
            Token = "sdssdssfsfsfsffs",
            CreatedDate = DateTime.Now.Date,
            Expires = DateTime.Now.AddDays(1)
        };
        
        _mockTokenService.Setup(s => s.GenerateRefreshToken(loginDetails.Username!,"Admin"))
            .Returns(refresh);

        _mockTokenService.Setup(t => t.AssignRefreshTokenToCookie(httpContextMock.Object.Response, "refreshToken", refresh));
        
        var result = await controller.Authenticate(loginDetails);

        Assert.IsType<OkObjectResult>(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<AuthResponse>(okResult.Value);
        var authResponse = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(loginDetails.Username, authResponse.Username);
        Assert.NotNull(authResponse.JwtToken);
        Assert.NotNull(authResponse.RefreshToken);
    }

    [Fact]
    public async Task Authenticate_WithNullLoginDetails_ReturnsBadRequest()
    {
        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object);

        var result = await controller.Authenticate(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Authenticate_WithNonExistentUser_ReturnsNotFound()
    {
        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object);

        var user = AuthMockData.GetUserDoesntExist();

        var users = AuthMockData.GetUsers();

        var loginDetails = new UserReqDto
        {
            Username = user.Username,
            Password = "testinvaliduser"
        };

        var userExists = users.Find(u => u.Username!.Equals(loginDetails.Username));

        _mockEmployeeSkillLevelService.Setup(s => s.GetUserByUsernameAsync(loginDetails.Username!))!
            .ReturnsAsync(userExists);

        var result = await controller.Authenticate(loginDetails);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User Not Found", notFoundResult.Value);
    }

    [Fact]
    public void Logout_RemovesRefreshTokenCookie_ReturnsNoContent()
    {
        var httpContextMock = new Mock<HttpContext>();

        var response = new Mock<HttpResponse>();

        httpContextMock.SetupGet(c => c.Response).Returns(response.Object);

        var cookies = new Mock<IResponseCookies>();

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        response.SetupGet(r => r.Cookies).Returns(cookies.Object);
        
        _mockTokenService.Setup(t => t.AssignRefreshTokenToCookie(httpContextMock.Object.Response, "refreshToken", It.IsAny<RefreshToken>()));

        _mockTokenService.Setup(t => t.RemoveCookie(httpContextMock.Object.Response, "refreshToken"));

        var result = controller.Logout() as NoContentResult;
        
        _mockTokenService.Verify(c => c.RemoveCookie(httpContextMock.Object.Response, "refreshToken"));

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
    }
    
    [Fact]
    public async Task RefreshToken_WithValidData_ReturnsOkResult()
    {
        var httpContextMock = new Mock<HttpContext>();

        var response = new Mock<HttpResponse>();

        httpContextMock.SetupGet(c => c.Response).Returns(response.Object);

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        var username = AuthMockData.GetUser().Username;
        const string validRefreshToken = "validRefreshToken";
        const string newJwtToken = "newJwtToken";

        var refreshToken = new KeyValuePair<string, string>("refreshToken", validRefreshToken);

        _mockTokenService.Setup(t => t.GetTokenFromCookies(controller.Request, "refreshToken"))
            .Returns(refreshToken);
        
        _mockTokenService.Setup(t => t.IsValidToken(refreshToken, username!, "Admin"))
            .Returns(true);
        
        _mockTokenService.Setup(t => t.GenerateJwtToken(username!, "Admin"))
            .Returns(newJwtToken);
        
        var result = await controller.RefreshToken(username!);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic jsonResponse = okResult.Value!;
        Assert.Equal(username, jsonResponse.Username);
        Assert.Equal(newJwtToken, jsonResponse.JwtToken);
    }
    
    [Fact]
    public async Task RefreshToken_WithInvalidData_ReturnsUnauthorizedResult()
    {
        var httpContextMock = new Mock<HttpContext>();

        var response = new Mock<HttpResponse>();

        httpContextMock.SetupGet(c => c.Response).Returns(response.Object);

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };
        
        var username = AuthMockData.GetUser().Username;

        _mockTokenService.Setup(t => t.GetTokenFromCookies(controller.Request, "refreshToken"))
            .Returns((KeyValuePair<string, string>?)null);
        
        var result = await controller.RefreshToken(username!);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
    
    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsForbidResult()
    {
        var httpContextMock = new Mock<HttpContext>();

        var response = new Mock<HttpResponse>();

        httpContextMock.SetupGet(c => c.Response).Returns(response.Object);

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _mockTokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };
        
        var username = AuthMockData.GetUser().Username;
        
        const string refreshTokenStr = "validRefreshToken";
        
        var validRefreshToken = new KeyValuePair<string, string>("refreshToken", refreshTokenStr);

        _mockTokenService.Setup(t => t.GetTokenFromCookies(controller.Request, "refreshToken"))
            .Returns(validRefreshToken);
        
        _mockTokenService.Setup(t => t.IsValidToken(validRefreshToken, username!, "Admin"))
            .Returns(false);
        
        var result = await controller.RefreshToken(username!);
        
        Assert.IsType<ForbidResult>(result);
    }
}