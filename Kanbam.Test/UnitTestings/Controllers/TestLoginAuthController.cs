using FluentAssertions;
using Kanbam.Test.UnitTestings.Fixtures;
using Kanbam.Test.UnitTestings.Reset;
using KanbamApi.Controllers;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Kanbam.Test.UnitTestings.Controllers;

public class TestLoginAuthController : TestBase
{
    private readonly Mock<IUsersService> _usersServiceMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpResponse> _mockHttpResponse;
    private readonly Mock<IResponseCookies> _mockResponseCookies;
    private readonly AuthController _authController;

    public TestLoginAuthController()
    {
        _usersServiceMock = new Mock<IUsersService>();
        _authServiceMock = new Mock<IAuthService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpResponse = new Mock<HttpResponse>();
        _mockResponseCookies = new Mock<IResponseCookies>();

        _authController = new AuthController(
            _usersServiceMock.Object,
            _authServiceMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task Login_OnValidUserDetail_ReturnOkResult_200_Ok()
    {
        // Assign

        UserLogin validUser = UserLoginFixture.ValidUser();

        _mockHttpResponse.Setup(r => r.Cookies).Returns(_mockResponseCookies.Object);
        _mockHttpContext.Setup(c => c.Response).Returns(_mockHttpResponse.Object);
        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };

        _authServiceMock
            .Setup(a => a.IsEmailExists(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authServiceMock
            .Setup(a => a.ValidatePasswordHash(It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns(true);

        _usersServiceMock
            .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(
                new User() { Email = "test@gmail.com", UserName = "671e4578ed4807a0a203a540" }
            );

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _refreshTokenServiceMock
            .Setup(a =>
                a.SaveRefreshTokenAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>())
            )
            .ReturnsAsync(Result<bool>.Success(true));

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

        _authServiceMock
            .Setup(a => a.IsEmailExists(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        // Act
        _authController.ModelState.AddModelError("Email", "Email is required");

        var result = (ObjectResult)await _authController.Login(inValidUser);

        // Asssert
        result.StatusCode.Should().Be(400);
        result.Value.Should().BeOfType<SerializableError>();
    }

    [Fact]
    public async Task Login_OnEmailNotExist_Return_401_Unauthorized()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        _authServiceMock.Setup(a => a.IsEmailExists(It.IsAny<string>())).ReturnsAsync((Auth)null!);

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_OnWrongPasswordHash_Return_401_UnAuthorized()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        _authServiceMock
            .Setup(a => a.IsEmailExists(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_OnWrongUserId_Return_401_UnAuthorized()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        _authServiceMock
            .Setup(a => a.IsEmailExists(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(401);
    }
}
