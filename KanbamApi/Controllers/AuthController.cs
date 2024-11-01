using System.Security.Cryptography;
using FluentValidation;
using FluentValidation.Results;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IValidator<UserLogin> _validatorLogin;
    private readonly IValidator<UserRegistration> _validatorRegistration;
    private readonly IUsersService _usersService;
    private readonly IAuthRepo _authRepo;
    private readonly IAuthData _authControllerService;

    public AuthController(
        IValidator<UserRegistration> validatorRegistration,
        IAuthRepo authRepo,
        IUsersService usersService,
        IAuthData authControllerService,
        IValidator<UserLogin> validatorLogin
    )
    {
        _validatorRegistration = validatorRegistration;
        _authRepo = authRepo;
        _usersService = usersService;
        _authControllerService = authControllerService;
        _validatorLogin = validatorLogin;
    }

    [AllowAnonymous]
    [HttpPost("Registarion")]
    public async Task<IActionResult> Register(UserRegistration userRegistration)
    {
        // validation
        ValidationResult result = await _validatorRegistration.ValidateAsync(userRegistration);

        if (!result.IsValid)
            return BadRequest(result);
        //

        var res = await _authRepo.CheckEmailExist(userRegistration.Email);

        if (res is not null)
            return BadRequest("Email already exist!");

        byte[] passwordSalt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = _authControllerService.GeneratePasswordHash(
            $"{userRegistration.Password}",
            passwordSalt
        );

        Auth authEntity =
            new()
            {
                Email = userRegistration.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

        User newUser =
            new()
            {
                Email = userRegistration.Email,
                UserName =
                    $"{userRegistration.Email?.Substring(0, userRegistration.Email.IndexOf('@'))}"
            };

        var isAuthCreated = await _authRepo.CreateAsync(authEntity);
        var isUserCreated = await _usersService.CreateAsync(newUser);

        var succefulRegistration = new Dictionary<string, object>
        {
            { "message", "Registration successful!" },
            {
                "user",
                new Dictionary<string, string>
                {
                    {
                        "username",
                        $"{userRegistration.Email?.Substring(0, userRegistration.Email.IndexOf('@'))}"
                    },
                    { "email", authEntity.Email! }
                }
            }
        };

        if (isUserCreated is not null && isAuthCreated)
            return CreatedAtAction(
                nameof(Register),
                new { id = authEntity.Email },
                succefulRegistration
            );

        return StatusCode(500, "Something wrong with Creating new User!");
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login(UserLogin userLogin)
    {
        // validation
        ValidationResult result = await _validatorLogin.ValidateAsync(userLogin);

        if (!result.IsValid)
            return BadRequest(result);

        // check if exist
        var res = await _authRepo.CheckEmailExist(userLogin.Email);

        if (res is null)
            return StatusCode(401, "Wrong Email address.");

        // check if password is correct by create hash and salt.
        byte[] passwordHash = _authControllerService.GeneratePasswordHash(
            $"{userLogin.Password}",
            res.PasswordSalt
        );

        for (int i = 0; i < passwordHash.Length; i++)
            if (passwordHash[i] != res.PasswordHash[i])
                return StatusCode(401, "Unauthorized Request!");

        var userId = await _usersService.GetUserIdByEmail(userLogin.Email!);

        if (userId.Length == 0)
            return StatusCode(401, "Unauthorized User!");

        return StatusCode(
            201,
            new Dictionary<string, string>
            {
                { "token", _authControllerService.GenerateToken(userId) }
            }
        );
    }
}
