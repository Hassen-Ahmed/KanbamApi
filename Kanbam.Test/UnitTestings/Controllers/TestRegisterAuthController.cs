using FluentAssertions;
using Kanbam.Test.UnitTestings.Fixtures;
using Kanbam.Test.UnitTestings.Reset;
using KanbamApi.Controllers;
using KanbamApi.Core;
using KanbamApi.Models.AuthModels;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kanbam.Test.UnitTestings.Controllers;

public class TestRegisterAuthController : TestBase
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthController _authController;

    public TestRegisterAuthController()
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
        _authController = new AuthController(
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            null!,
            null!
        );
    }

    [Fact]
    public async Task Register_OnValidUserDetail_Return_200_Created()
    {
        // Assign
        UserRegistration validUser = UserRegFixture.ValidUser();

        _userManagerMock
            .Setup(a => a.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(a => a.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = (ObjectResult)await _authController.Register(validUser);

        // Asssert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(new { message = ErrorMessages.SuccessfullRegistered });
    }

    [Fact]
    public async Task Register_OnInValidUserDetail_Return_400_BadRequest()
    {
        // Assign
        UserRegistration inValidUser = UserRegFixture.InValidUser();
        _userManagerMock
            .Setup(a => a.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(a => a.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Returns(Task.FromResult(IdentityResult.Success));

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
        var user = new ApplicationUser
        {
            Email = validUser.Email,
            UserName = validUser.Email.Split("@").FirstOrDefault()
        };
        _userManagerMock.Setup(a => a.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

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
        _userManagerMock
            .Setup(a => a.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        _userManagerMock
            .Setup(a => a.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Returns(Task.FromResult(IdentityResult.Failed()));

        // Act
        var result = (ObjectResult)await _authController.Register(validUser);

        // Asssert
        result.StatusCode.Should().Be(500);
        result.Value?.ToString()?.Contains("Failed to create").Should().BeTrue();
    }
}
