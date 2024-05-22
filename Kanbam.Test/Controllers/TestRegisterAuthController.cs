using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Kanbam.Test.Reset;
using KanbamApi.Controllers;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kanbam.Test.Controllers;

public class TestRegisterAuthController : TestBase
{
    private readonly Mock<IValidator<UserRegistration>> _validatUserRegMock;
    private readonly Mock<IAuthRepo> _authRepoMock;
    private readonly Mock<IUsersRepo> _userRepoMock;
    private readonly Mock<IAuthData> _authControllerServiceMock;
    private readonly AuthController _authController;

    public TestRegisterAuthController()
    {
        _validatUserRegMock = new Mock<IValidator<UserRegistration>>();
        _authRepoMock = new Mock<IAuthRepo>();
        _userRepoMock = new Mock<IUsersRepo>();
        _authControllerServiceMock = new Mock<IAuthData>();

        _authController = new AuthController(
            _validatUserRegMock.Object,
            _authRepoMock.Object,
            _userRepoMock.Object,
            _authControllerServiceMock.Object,
            null!
        );
    }

    [Fact]
    public async Task Register_OnValidUserDetail_Return_200_And_ResponseBodyMessage()
    {
        // Assign
        UserRegistration validUser =
            new()
            {
                Email = "test@gmail.com",
                Password = "#hassenbest1",
                PasswordConfirm = "#hassenbest1",
            };

        _validatUserRegMock
            .Setup(v => v.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        _authRepoMock.Setup(a => a.CheckEmailExist(It.IsAny<string>())).ReturnsAsync((Auth)null!);
        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock.Setup(u => u.CreateNewUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        // Act

        var result = (ObjectResult)await _authController.Register(validUser);

        // Assert
        var castedValue = (Dictionary<string, object>)result.Value!;
        var message = castedValue["message"];
        var user = (Dictionary<string, string>)castedValue["user"];

        result.StatusCode.Should().Be(201);
        message.Should().Be("Registration successful!");
        user["username"]
            .Should()
            .Be($"{validUser.Email?.Substring(0, validUser.Email.IndexOf('@'))}");
        user["email"].Should().Be(validUser.Email);
    }

    [Fact]
    public async Task Register_OnWrongUserDetail_Return_400_BadRequest()
    {
        // Assign
        UserRegistration inValidUser =
            new()
            {
                Email = "hassen@gmail.com",
                Password = "",
                PasswordConfirm = "",
            };

        var validationResult = new ValidationResult();

        validationResult.Errors.Add(new ValidationFailure("Email", "Email is required!"));

        _validatUserRegMock
            .Setup(v => v.ValidateAsync(inValidUser, default))
            .ReturnsAsync(validationResult);

        _authRepoMock.Setup(a => a.CheckEmailExist(It.IsAny<string>())).ReturnsAsync((Auth)null!);
        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock.Setup(u => u.CreateNewUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        // Act

        var result = (ObjectResult)await _authController.Register(inValidUser);

        // Assert
        result.StatusCode.Should().Be(400);
        result.Value?.ToString().Should().Be("Email is required!");
    }

    [Fact]
    public async Task Register_OnEmailIsAlreadyExist_Return_400_BadRequest()
    {
        // Assign
        UserRegistration validUser =
            new()
            {
                Email = "test@gmail.com",
                Password = "#hassenbest1",
                PasswordConfirm = "#hassenbest1",
            };

        _validatUserRegMock
            .Setup(v => v.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        _authRepoMock.Setup(a => a.CheckEmailExist(It.IsAny<string>())).ReturnsAsync(new Auth());
        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        _userRepoMock.Setup(u => u.CreateNewUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        // Act

        var result = (ObjectResult)await _authController.Register(validUser);

        // Assert

        result.StatusCode.Should().Be(400);
        result.Value.Should().Be("Email already exist!");
    }

    [Fact]
    public async Task Register_OnInternalError_Return_500()
    {
        // Assign
        UserRegistration validUser =
            new()
            {
                Email = "test@gmail.com",
                Password = "#hassenbest1",
                PasswordConfirm = "#hassenbest1",
            };

        _validatUserRegMock
            .Setup(v => v.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        _authRepoMock.Setup(a => a.CheckEmailExist(It.IsAny<string>())).ReturnsAsync((Auth)null!);
        _authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(false);

        _userRepoMock.Setup(u => u.CreateNewUserAsync(It.IsAny<User>())).ReturnsAsync(false);

        _authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        // Act

        var result = (ObjectResult)await _authController.Register(validUser);

        // Assert

        result.StatusCode.Should().Be(500);
        result.Value.Should().Be("Something wrong with Creating new User!");
    }
}
