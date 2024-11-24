using FluentAssertions;
using Kanbam.Test.UnitTestings.Fixtures;
using Kanbam.Test.UnitTestings.Reset;
using KanbamApi.Controllers;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kanbam.Test.UnitTestings.Controllers;

public class TestRegisterAuthController : TestBase
{
    private readonly Mock<IUsersService> _usersServiceMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly AuthController _authController;

    public TestRegisterAuthController()
    {
        _usersServiceMock = new Mock<IUsersService>();
        _authServiceMock = new Mock<IAuthService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        _authController = new AuthController(
            _usersServiceMock.Object,
            _authServiceMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task Register_OnValidUserDetail_Return_200_Created()
    {
        // Assign
        UserRegistration validUser = UserRegFixture.ValidUser();

        _authServiceMock
            .Setup(a => a.IsEmailExists("falsetest@gmail.com"))
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
            .Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync("test9@gmail.com");

        // Act
        var result = (ObjectResult)await _authController.Register(validUser);

        // Asssert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(new { message = "The registration was successfull!" });
    }

    [Fact]
    public async Task Register_OnInValidUserDetail_Return_400_BadRequest()
    {
        // Assign
        UserRegistration inValidUser = UserRegFixture.InValidUser();

        _authServiceMock.Setup(a => a.IsEmailExists(It.IsAny<string>())).ReturnsAsync((Auth)null!);
        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync("671e4578ed4807a0a203a540");

        // Act
        _authController.ModelState.AddModelError("Email", "Email is required");

        var result = (ObjectResult)await _authController.Register(inValidUser);

        // Asssert
        result.StatusCode.Should().Be(400);
        result.Value.Should().BeOfType<SerializableError>();
    }

    [Fact]
    public async Task Register_OnEmailIsAlreadyExist_Return_400_BadRequest()
    {
        // Assign
        UserRegistration validUser = UserRegFixture.ValidUser();

        _authServiceMock.Setup(a => a.IsEmailExists(It.IsAny<string>())).ReturnsAsync(new Auth());
        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync("671e4578ed4807a0a203a540");

        // Act
        var result = (ObjectResult)await _authController.Register(validUser);

        // Assert
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Register_OnInternalError_Return_500()
    {
        // Assign
        UserRegistration validUser = UserRegFixture.ValidUser();

        _authServiceMock.Setup(a => a.IsEmailExists("test@gmail.com")).ReturnsAsync((Auth)null!);

        _usersServiceMock
            .Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync("671e4578ed4807a0a203a540");

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(false);

        _usersServiceMock.Setup(u => u.CreateAsync(It.IsAny<User>())).ReturnsAsync("something");

        // Act
        var result = (ObjectResult)await _authController.Register(validUser);

        // Asssert
        result.StatusCode.Should().Be(500);
        result.Value.Should().Be("Something wrong with Creating new User!");
    }
}
