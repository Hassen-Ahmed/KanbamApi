using FluentAssertions;
using Kanbam.Test.UnitTestings.Fixtures;
using Kanbam.Test.UnitTestings.Reset;
using KanbamApi.Controllers;
using KanbamApi.Models.AuthModels;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kanbam.Test.UnitTestings.Controllers;

public class TestLoginAuthController : TestBase
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<HttpResponse> _mockHttpResponse;
    private readonly Mock<IResponseCookies> _mockResponseCookies;
    private readonly Mock<ICloudFlareTurnstileService> _cloudFlareTurnstileService;
    private readonly AuthController _authController;

    public TestLoginAuthController()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
        _cloudFlareTurnstileService = new Mock<ICloudFlareTurnstileService>();
        _authController = new AuthController(
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            null!,
            _cloudFlareTurnstileService.Object
        );

        _mockHttpResponse = new Mock<HttpResponse>();
        _mockResponseCookies = new Mock<IResponseCookies>();
    }

    [Fact]
    public async Task Login_OnValidUserDetail_ReturnOkResult_200_Ok()
    {
        // Assign

        UserLogin validUser = UserLoginFixture.ValidUser();
        var user = new ApplicationUser
        {
            Email = validUser.Email,
            UserName = validUser.Email.Split("@").FirstOrDefault()
        };

        var defaultHttpContext = new DefaultHttpContext();
        defaultHttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        _mockHttpResponse.Setup(r => r.Cookies).Returns(_mockResponseCookies.Object);

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = defaultHttpContext
        };

        _cloudFlareTurnstileService
            .Setup(cf => cf.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        _userManagerMock
            .Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(um => um.UpdateAsync(user))
            .Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Login_OnInValidUserDetail_Return_400_BadRequest()
    {
        // Assign
        UserLogin inValidUser = UserLoginFixture.InValidUser();

        var defaultHttpContext = new DefaultHttpContext();
        defaultHttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _mockHttpResponse.Setup(r => r.Cookies).Returns(_mockResponseCookies.Object);

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = defaultHttpContext
        };

        _cloudFlareTurnstileService
            .Setup(cf => cf.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(a => a.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        _authController.ModelState.AddModelError("Email", "Email is required");

        var result = (ObjectResult)await _authController.Login(inValidUser);

        // Asssert
        result.StatusCode.Should().Be(400);
        result.Value.Should().BeOfType<SerializableError>();
    }

    [Fact]
    public async Task Login_OnEmailNotExist_Return_400_BadRequest()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        var defaultHttpContext = new DefaultHttpContext();
        defaultHttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _mockHttpResponse.Setup(r => r.Cookies).Returns(_mockResponseCookies.Object);

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = defaultHttpContext
        };

        _cloudFlareTurnstileService
            .Setup(cf => cf.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(a => a.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Login_OnWrongPasswordHash_Return_400_BadRequest()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        var defaultHttpContext = new DefaultHttpContext();
        defaultHttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _mockHttpResponse.Setup(r => r.Cookies).Returns(_mockResponseCookies.Object);

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = defaultHttpContext
        };

        _cloudFlareTurnstileService
            .Setup(cf => cf.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));

        _userManagerMock
            .Setup(a => a.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(a => a.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Login_OnWrongUserId_Return_400_BadRequest()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        var defaultHttpContext = new DefaultHttpContext();
        defaultHttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _mockHttpResponse.Setup(r => r.Cookies).Returns(_mockResponseCookies.Object);

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = defaultHttpContext
        };

        _cloudFlareTurnstileService
            .Setup(cf => cf.VerifyTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));

        _userManagerMock
            .Setup(a => a.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(400);
    }
}
