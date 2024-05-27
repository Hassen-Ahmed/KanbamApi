using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Kanbam.Test.Fixtures;
using Kanbam.Test.Reset;
using KanbamApi.Controllers;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kanbam.Test.Controllers;

public class TestLoginAuthController : TestBase
{
    private readonly Mock<IAuthRepo> _authRepoMock;
    private readonly Mock<IUsersRepo> _userRepoMock;
    private readonly Mock<IAuthData> _authControllerServiceMock;
    private readonly AuthController _authController;
    private readonly Mock<IValidator<UserLogin>> _validatLoginMock;

    public TestLoginAuthController()
    {
        _authRepoMock = new Mock<IAuthRepo>();
        _userRepoMock = new Mock<IUsersRepo>();
        _authControllerServiceMock = new Mock<IAuthData>();
        _validatLoginMock = new Mock<IValidator<UserLogin>>();

        _authController = new AuthController(
            null!,
            _authRepoMock.Object,
            _userRepoMock.Object,
            _authControllerServiceMock.Object,
            _validatLoginMock.Object
        );
    }

    [Fact]
    public async Task Login_OnValidUserDetail_ReturnOkResult_200_WithTokenkey()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        _validatLoginMock
            .Setup(x => x.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        _authRepoMock
            .Setup(a => a.CheckEmailExist(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock
            .Setup(u => u.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync("userIdLength>0");

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        _authControllerServiceMock
            .Setup(a => a.GenerateToken(It.IsAny<string>()))
            .Returns("this is my token.");

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        var castedValue = (Dictionary<string, string>)result.Value!;
        var token = castedValue["token"];

        result.StatusCode.Should().Be(201);
        token.Should().Be("this is my token.");
        _validatLoginMock.Verify(a => a.ValidateAsync(validUser, default), Times.Once);
    }

    [Fact]
    public async Task Login_OnInValidUserDetail_Return_400_BadRequest()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.InValidUser();

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Email", "Invalid credential!"));

        _validatLoginMock
            .Setup(x => x.ValidateAsync(validUser, default))
            .ReturnsAsync(validationResult);

        _authRepoMock
            .Setup(a => a.CheckEmailExist(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock
            .Setup(u => u.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync("userIdLength>0");

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        _authControllerServiceMock
            .Setup(a => a.GenerateToken(It.IsAny<string>()))
            .Returns("this is my token.");

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(400);
        result.Value?.ToString().Should().BeSameAs("Invalid credential!");
    }

    [Fact]
    public async Task Login_OnEmailNotExist_Return_401_BadRequest()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        _validatLoginMock
            .Setup(x => x.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        _authRepoMock.Setup(a => a.CheckEmailExist(It.IsAny<string>())).ReturnsAsync((Auth)null!);

        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock
            .Setup(u => u.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync("userIdLength>0");

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        _authControllerServiceMock
            .Setup(a => a.GenerateToken(It.IsAny<string>()))
            .Returns("this is my token.");

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(401);
        result.Value?.ToString().Should().Be("Wrong Email address.");
    }

    [Fact]
    public async Task Login_OnWrongPasswordHash_Return_401_UnAuthorized()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        _validatLoginMock
            .Setup(x => x.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        _authRepoMock
            .Setup(a => a.CheckEmailExist(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock
            .Setup(u => u.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync("userIdLength>0");

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

        _authControllerServiceMock
            .Setup(a => a.GenerateToken(It.IsAny<string>()))
            .Returns("this is my token.");

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(401);
        result.Value?.ToString().Should().Be("Unauthorized Request!");
    }

    [Fact]
    public async Task Login_OnWrongUserId_Return_401_UnAuthorized()
    {
        // Assign
        UserLogin validUser = UserLoginFixture.ValidUser();

        _validatLoginMock
            .Setup(x => x.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        _authRepoMock
            .Setup(a => a.CheckEmailExist(It.IsAny<string>()))
            .ReturnsAsync(
                new Auth()
                {
                    Email = "test@gmail.com",
                    PasswordHash = new byte[128 / 8],
                    PasswordSalt = new byte[128 / 8]
                }
            );

        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock.Setup(u => u.GetUserIdAsync(It.IsAny<string>())).ReturnsAsync("");

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        _authControllerServiceMock
            .Setup(a => a.GenerateToken(It.IsAny<string>()))
            .Returns("this is my token.");

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        result.StatusCode.Should().Be(401);
        result.Value?.ToString().Should().Be("Unauthorized User!");
    }
}
