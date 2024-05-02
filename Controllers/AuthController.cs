using System.Security.Cryptography;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Services;
using KanbamApi.Services.AuthServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
private readonly AuthService _authService;
private readonly UsersService   _usersService;
private readonly IAuthControllerService _authControllerService;

    public AuthController(AuthService authService, IAuthControllerService authControllerService, UsersService usersService){

        _authService = authService;
        _authControllerService = authControllerService;
        _usersService = usersService;
    }

    [Authorize]
    [HttpGet]
    public async Task<List<Auth>> Get() =>
        await _authService.GetAsync();
    

    [AllowAnonymous]
    [HttpPost("Registarion")]
    public async Task<IActionResult> Register(UserRegistration userRegistration)
    {
        if (userRegistration.Password != userRegistration.PasswordConfirm) throw new Exception("Password does not match!");

        var res = await _authService.CheckAuth(userRegistration.Email);

        if (res is not null)
            throw new Exception("Email already exist.");

        byte[] passwordSalt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = _authControllerService.GeneratePasswordHash($"{userRegistration.Password}", passwordSalt);

        Auth authEntity = new()
        {
            Email = userRegistration.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        User newUser = new() {
            Email = userRegistration.Email,
            UserName = userRegistration?.Email?.Substring(0, userRegistration.Email.IndexOf('@')),
        };

        var isUserCreated = await _usersService.CreateNewUserAsync(newUser);
        var isAuthCreated = await _authService.CreateAsync(authEntity);
        
       var succefulRegistration = new Dictionary<string, object>
                {
                    { "message", "Registration successful" },
                    { "user", new Dictionary<string, string>
                        {
                            { "username",authEntity.Email! },
                            { "email", authEntity.Email! }
                        }
                    }
                };

        if (isUserCreated && isAuthCreated)
            return CreatedAtAction(nameof(Get), new { id = authEntity.Email },succefulRegistration );
        
        throw new Exception("Something wrong with Creating new User!");
        
    }

    [AllowAnonymous]
    [HttpPost("Login")]
      public async Task<IActionResult> Login(UserLogin userLogin){
        // check if exist

        var res = await _authService.CheckAuth(userLogin.Email);

        if (res is null) return StatusCode(401, "Wrong Email address.");

        // check if password is correct by create hash and salt. 
        byte[] passwordHash = _authControllerService.GeneratePasswordHash($"{userLogin.Password}", res.PasswordSalt);

        for (int i = 0; i < passwordHash.Length; i++)
            if (passwordHash[i] != res.PasswordHash[i]) return StatusCode(401, "Unauthorized Request!");

        var userId = await _usersService.GetUserIdAsync(userLogin.Email);

        if (userId.Length == 0) return StatusCode(401, "Unauthorized User!");

        return StatusCode(201, new Dictionary<string, string> {
            {"token" , _authControllerService.GenerateToken(userId)}
        });
    }
}