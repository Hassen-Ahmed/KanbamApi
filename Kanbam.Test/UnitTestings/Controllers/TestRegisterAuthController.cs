using FluentAssertions;
using Kanbam.Test.UnitTestings.Fixtures;
using Kanbam.Test.UnitTestings.Reset;
using KanbamApi.Controllers;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Kanbam.Test.UnitTestings.Controllers;

public class TestRegisterAuthController : TestBase
{
    private readonly Mock<IUsersService> _usersServiceMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IAuthData> _authControllerServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly AuthController _authController;
    private readonly Dictionary<string, string>? _inMemorySettings = new Dictionary<string, string>
    {
        { "KanbamSettings:Expiration:RefreshTokenDate", "15" }
    };

    public TestRegisterAuthController()
    {
        _usersServiceMock = new Mock<IUsersService>();
        _authServiceMock = new Mock<IAuthService>();
        _authControllerServiceMock = new Mock<IAuthData>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemorySettings!)
            .Build();

        _authController = new AuthController(
            _usersServiceMock.Object,
            _authServiceMock.Object,
            _authControllerServiceMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            configuration
        );
    }

    [Fact]
    public async Task Register_OnValidUserDetail_Return_200_Created()
    {
        // Assign
        UserRegistration validUser = UserRegFixture.ValidUser();

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

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync("test@gmail.com");

        // Act
        var result = (ObjectResult)await _authController.Register(validUser);

        // Asssert
        result.StatusCode.Should().Be(200);
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

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

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

        _authServiceMock.Setup(a => a.IsEmailExists(It.IsAny<string>())).ReturnsAsync((Auth)null!);
        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _usersServiceMock
            .Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync("671e4578ed4807a0a203a540");

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

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

        _usersServiceMock
            .Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync("671e4578ed4807a0a203a540");

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        _authServiceMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(false);

        _usersServiceMock.Setup(u => u.CreateAsync(It.IsAny<User>())).ReturnsAsync("");

        // Act
        var result = (ObjectResult)await _authController.Register(validUser);

        // Asssert
        result.StatusCode.Should().Be(500);
    }
}
