using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using KanbamApi.Controllers;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.AuthServices;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kanbam.Test.Controllers;

public class TestLoginAuthController
{
    private readonly Mock<IAuthRepo> _authRepoMock;
    private readonly Mock<IUsersRepo> _userRepoMock;
    private readonly Mock<IAuthControllerService> _authControllerServiceMock;
    private readonly AuthController _authController;
    private readonly Mock<IValidator<UserLogin>> _validatLoginMock;

    public TestLoginAuthController()
    {
        _authRepoMock = new Mock<IAuthRepo>();
        _userRepoMock = new Mock<IUsersRepo>();
        _authControllerServiceMock = new Mock<IAuthControllerService>();
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
    public async Task Login_OnValidUserDetail_ReturnOkResult_200()
    {
        // Assign
        UserLogin validUser = new() { Email = "test@gmail.com", Password = "#hassenbest1", };

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

        // Act
        var result = (ObjectResult)await _authController.Login(validUser);

        // Assert
        var castedValue = (Dictionary<string, string>)result.Value!;
        var message = castedValue["message"];

        result.StatusCode.Should().Be(200);
        message.Should().Be("Successfuly Logedin!");
        _validatLoginMock.Verify(a => a.ValidateAsync(validUser, default), Times.Once);
    }
}
