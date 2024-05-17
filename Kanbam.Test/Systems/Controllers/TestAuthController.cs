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

namespace Kanbam.Test.Systems.Controllers;

public class TestAuthController
{
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

        var validatUserRegMock = new Mock<IValidator<UserRegistration>>();
        validatUserRegMock
            .Setup(v => v.ValidateAsync(validUser, default))
            .ReturnsAsync(new ValidationResult());

        var authRepoMock = new Mock<IAuthRepo>();
        authRepoMock.Setup(a => a.CheckEmailExist(It.IsAny<string>())).ReturnsAsync((Auth)null!);
        authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        var userRepoMock = new Mock<IUsersRepo>();
        userRepoMock.Setup(u => u.CreateNewUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        var authControllerServiceMock = new Mock<IAuthControllerService>();
        authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        // Act

        AuthController authController =
            new(
                validatUserRegMock.Object,
                authRepoMock.Object,
                userRepoMock.Object,
                authControllerServiceMock.Object,
                null!
            );

        var result = (ObjectResult)await authController.Register(validUser);

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

        var validatUserRegMock = new Mock<IValidator<UserRegistration>>();
        var validationResult = new ValidationResult();

        validationResult.Errors.Add(new ValidationFailure("Email", "Email is required!"));

        validatUserRegMock
            .Setup(v => v.ValidateAsync(inValidUser, default))
            .ReturnsAsync(validationResult);

        var authRepoMock = new Mock<IAuthRepo>();
        authRepoMock.Setup(a => a.CheckEmailExist(It.IsAny<string>())).ReturnsAsync((Auth)null!);
        authRepoMock.Setup(a => a.CreateAsync(It.IsAny<Auth>())).ReturnsAsync(true);

        var userRepoMock = new Mock<IUsersRepo>();
        userRepoMock.Setup(u => u.CreateNewUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        var authControllerServiceMock = new Mock<IAuthControllerService>();
        authControllerServiceMock
            .Setup(a => a.GeneratePasswordHash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(new byte[128 / 8]);

        // Act

        AuthController authController =
            new(
                validatUserRegMock.Object,
                authRepoMock.Object,
                userRepoMock.Object,
                authControllerServiceMock.Object,
                null!
            );

        var result = (ObjectResult)await authController.Register(inValidUser);

        // Assert
        result.StatusCode.Should().Be(400);
        result.Value?.ToString().Should().Be("Email is required!");
    }
}
