using System.Security.Cryptography;
using KanbamApi.Core;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly IAuthService _authService;
    private readonly IAuthData _authControllerService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUsersService usersService,
        IAuthService authService,
        IAuthData authControllerService,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IConfiguration configuration
    )
    {
        _usersService = usersService;
        _authService = authService;
        _authControllerService = authControllerService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
    }

    [HttpPost(ApiRoutesAuth.Register)]
    public async Task<IActionResult> Register([FromBody] UserRegistration userRegistration)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var authDetail = await _authService.IsEmailExists(userRegistration.Email);

        if (authDetail is not null)
            return BadRequest(new { error = "Email already exists!" });

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

        var isAuthCreated = await _authService.CreateAsync(authEntity);
        var isUserCreated = await _usersService.CreateAsync(newUser);

        if (isUserCreated is null || !isAuthCreated)
        {
            return StatusCode(500, "Something wrong with Creating new User!");
        }

        return Ok("The registration was successfull!");
    }

    [HttpPost(ApiRoutesAuth.Login)]
    public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
    {
        // validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // check if exists againest database
        var authEntity = await _authService.IsEmailExists(userLogin.Email);

        if (authEntity is null)
            return Unauthorized("Invalid email.");

        // Check if password is correct by creating hash and salt.
        byte[] passwordHash = _authService.GeneratePasswordHash(
            $"{userLogin.Password}",
            authEntity.PasswordSalt
        );

        // Validate passwordHash
        if (!_authService.ValidatePasswordHash(passwordHash, authEntity.PasswordHash))
            return Unauthorized("Unauthorized Request!");

        // Retrieve user details from database
        var user = await _usersService.GetUserByEmailAsync(userLogin.Email!);

        if (user is null)
            return Unauthorized("User not found.");

        // Generate tokens
        var claims = _tokenService.GenerateClaims(user.UserName!, user.Id);

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var expirationDateOfRefreshToken = _configuration.GetValue<int>(
            "KanbamSettings:Expiration:RefreshTokenDate"
        );

        var isRefreshTokenSaved = await _refreshTokenService.SaveRefreshTokenAsync(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(expirationDateOfRefreshToken)
        );

        if (isRefreshTokenSaved is null || !isRefreshTokenSaved.IsSuccess)
            return StatusCode(500, "An unknow error occured");

        SetRefreshTokenCookie(refreshToken, DateTime.UtcNow.AddDays(expirationDateOfRefreshToken));

        return Ok(new { accessToken });
    }

    [HttpPost(ApiRoutesAuth.RefreshToken)]
    public async Task<IActionResult> RefreshToken([FromBody] AccessTokenExpired accessToken)
    {
        if (!Request.Cookies.TryGetValue($"{TokenType.RefreshToken}", out var refreshToken))
            return Unauthorized("No refresh token found");

        // use detail like userId and username from expired accessToken
        if (accessToken.expiredAccessToken is null)
            return BadRequest(new { error = "Wrong token" });

        var principal = _tokenService.DecodeExpiredToken(accessToken.expiredAccessToken);

        if (!Guid.TryParse(refreshToken, out var tokenId))
            return Unauthorized("Wrong refreshToken");

        // get refreshToken from Databases
        var storedToken = await _refreshTokenService.GetRefreshTokensByTokenAsync(tokenId);

        if (principal is null || storedToken.Value.UserId != principal.FindFirst("userId")?.Value)
            return Unauthorized("Wrong accessToken");

        if (storedToken.IsFailure || storedToken.Value.TokenExpiryTime < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token");

        // get username by userId from database
        var username = await _usersService.GetUsernameByIdAsync(storedToken.Value.UserId);
        if (username == null)
            return Unauthorized("Invalid username");

        // Generate token
        var claims = _tokenService.GenerateClaims(username, storedToken.Value.UserId);

        var newAccessToken = _tokenService.GenerateAccessToken(claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // save refreshToken to Databases
        var isSaved = await _refreshTokenService.Update_RefreshToken_ById_Async(
            storedToken.Value.Id,
            newRefreshToken
        );

        if (!isSaved.Value)
            return Unauthorized("Unauthorize access.");

        // update refreshToken
        Response.Cookies.Delete($"{TokenType.RefreshToken}");
        SetRefreshTokenCookie(newRefreshToken, storedToken.Value.TokenExpiryTime);

        return Ok(new { accessToken = newAccessToken });
    }

    [HttpPost(ApiRoutesAuth.Revoke)]
    public async Task<IActionResult> RevokeRefreshToken()
    {
        // check if the user is authentic by expired refreshToken or accessToken

        if (
            Request.Cookies.TryGetValue($"{TokenType.RefreshToken}", out var refreshToken)
            && Guid.TryParse(refreshToken, out var tokenId)
        )
        {
            await _refreshTokenService.DeleteRefreshTokenAsync(tokenId);

            Response.Cookies.Delete($"{TokenType.RefreshToken}");

            return NoContent();
        }
        return BadRequest(new { error = "Wrong refreshToken" });
    }

    private void SetRefreshTokenCookie(Guid refreshToken, DateTime? expirationDate)
    {
        Response.Cookies.Append(
            $"{TokenType.RefreshToken}",
            refreshToken.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expirationDate
            }
        );
    }
}
