using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using API.Controllers;
using API.DTO;
using API.Interfaces;
using API.Models;
using API.Tests.MockData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.Setup(c => c.Response.Cookies).Returns(_cookies.Object);

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _configuration)
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
        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, null);

        var result = await controller.Authenticate(null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Authenticate_WithNonExistentUser_ReturnsNotFound()
    {
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.Setup(c => c.Response.Cookies).Returns(_cookies.Object);

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, null);

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

        var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        response.SetupGet(r => r.Cookies).Returns(cookies.Object);

        cookies.Setup(c => c.Append("refreshToken", It.IsAny<string>(), It.IsAny<CookieOptions>()));

        cookies.Setup(c => c.Delete("refreshToken", It.IsAny<CookieOptions>()));

        var result = controller.Logout() as NoContentResult;

        cookies.Verify(c => c.Delete("refreshToken", It.IsAny<CookieOptions>()));

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status204NoContent, result.StatusCode);
    }
    
    // [Fact]
    // public void SetRefreshTokenToCookie_SetsCookieWithCorrectOptions()
    // {
    //     var refreshToken = new RefreshToken
    //     {
    //         Token = "refreshToken",
    //         Expires = DateTime.UtcNow.AddHours(1)
    //     };
    //
    //     var responseMock = new Mock<HttpResponse>();
    //     var cookiesMock = new Mock<IResponseCookies>();
    //
    //     responseMock.SetupGet(r => r.Cookies).Returns(cookiesMock.Object);
    //
    //     var httpContextMock = new Mock<HttpContext>();
    //     httpContextMock.SetupGet(c => c.Response).Returns(responseMock.Object);
    //
    //     var controller = new AuthController(_mockEmployeeSkillLevelService.Object, _configuration)
    //     {
    //         ControllerContext = new ControllerContext
    //         {
    //             HttpContext = httpContextMock.Object
    //         }
    //     };
    //     
    //     controller.SetRefreshTokenToCookie(refreshToken);
    //     
    //     cookiesMock.Verify(c => c.Append("refreshToken", refreshToken.Token, It.Is<CookieOptions>(options =>
    //         options.HttpOnly && options.Expires == refreshToken.Expires && options.SameSite == SameSiteMode.None && options.Secure
    //     )), Times.Once);
    // }
    
    // TODO
    // [Fact]
    // public void RefreshToken_WithoutRefreshTokenCookie_ReturnsUnauthorized()
    // {
    //   
    // }

    // TODO
    // [Fact]
    // public void RefreshToken_WithValidToken_ReturnsNewJwtToken()
    // {
    //   
    // }
}