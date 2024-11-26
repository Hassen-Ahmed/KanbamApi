using System.Security.Claims;
using System.Security.Cryptography;
using KanbamApi.Core;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Services;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(
        IUsersService usersService,
        IAuthService authService,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService
    )
    {
        _usersService = usersService;
        _authService = authService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost(ApiRoutesAuth.Register)]
    public async Task<IActionResult> Register([FromBody] UserRegistration userRegistration)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var authDetail = await _authService.IsEmailExists(userRegistration.Email);

        if (authDetail is not null)
            return BadRequest(new { error = "Email already exists!" });

        var isDataSaved = await SaveAuthAndUserDetail(userRegistration);

        if (isDataSaved.IsFailure)
            return StatusCode(isDataSaved.Error.CodeStatus, isDataSaved.Error.Message);

        return Ok(new { message = "The registration was successfull!" });
    }

    [HttpPost(ApiRoutesAuth.Login)]
    public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
    {
        // validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // check if exists againest database
        var authEntity = await _authService.IsEmailExists(userLogin.Email);

        if (authEntity is null)
            return BadRequest(new { error = "Invalid email address." });

        // Check if password is correct by creating hash and salt.
        byte[] passwordHash = _authService.GeneratePasswordHash(
            $"{userLogin.Password}",
            authEntity.PasswordSalt
        );

        // Validate passwordHash
        if (!_authService.ValidatePasswordHash(passwordHash, authEntity.PasswordHash))
            return BadRequest(new { error = "Unauthorized Request!" });

        // Retrieve user details from database
        var user = await _usersService.GetUserByEmailAsync(userLogin.Email!);

        if (user is null)
            return BadRequest(new { error = "User not found." });

        // Generate tokens
        var claims = _tokenService.GenerateClaims(user.UserName!, user.Id);

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var expireDate = DateTime.UtcNow.AddDays((int)TokenExpiration.RefreshTokenDate);

        // Save refresh token
        var isRefreshTokenSaved = await _refreshTokenService.SaveRefreshTokenAsync(
            user.Id,
            refreshToken,
            expireDate
        );

        if (isRefreshTokenSaved is null || !isRefreshTokenSaved.IsSuccess)
            return StatusCode(500, "An unknow error occured");

        SetRefreshTokenCookie(refreshToken, expireDate);
        return Ok(new { accessToken });
    }

    [HttpPost(ApiRoutesAuth.RefreshToken)]
    public async Task<IActionResult> RefreshToken()
    {
        if (
            !Request.Cookies.TryGetValue(
                $"{TokenType.RefreshToken}",
                out var refreshTokenFromCookie
            )
        )
        {
            return BadRequest(new { error = "No refresh token found." });
        }

        // use detail like userId and username from expired accessToken
        if (!Guid.TryParse(refreshTokenFromCookie, out var refreshTokenParsed))
            return Unauthorized(new { error = "Invalid typeof refresh token." });

        // get refreshToken entity from Databases
        var storedRefreshTokenEntity = await _refreshTokenService.GetRefreshTokensByTokenAsync(
            refreshTokenParsed
        );

        var isDataUpdated = await UpdateRefreshToken(storedRefreshTokenEntity, refreshTokenParsed);

        if (isDataUpdated.IsFailure)
        {
            return Unauthorized(new { error = isDataUpdated.Error.Message });
        }

        SetRefreshTokenCookie(
            isDataUpdated.Value.NewRefreshToken,
            storedRefreshTokenEntity.Value?.TokenExpiryTime
        );
        return Ok(new { accessToken = isDataUpdated.Value.NewAccessToken });
    }

    [HttpPost(ApiRoutesAuth.Revoke)]
    public async Task<IActionResult> RevokeRefreshToken()
    {
        if (
            Request.Cookies.TryGetValue($"{TokenType.RefreshToken}", out var refreshTokenFromCookie)
            && Guid.TryParse(refreshTokenFromCookie, out var refreshTokenParsed)
        )
        {
            await _refreshTokenService.DeleteRefreshTokenAsync(refreshTokenParsed);
            Response.Cookies.Delete($"{TokenType.RefreshToken}");

            return NoContent();
        }

        return BadRequest(new { error = "Nothing to revoke." });
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

    private async Task<Result<string>> GenerateUsename(
        Result<RefreshToken> storedRefreshToken,
        Guid refreshTokenParsed
    )
    {
        if (
            storedRefreshToken.IsFailure
            || storedRefreshToken.Value?.TokenExpiryTime < DateTime.UtcNow
        )
        {
            await _refreshTokenService.DeleteRefreshTokenAsync(refreshTokenParsed);
            // Response.Cookies.Delete($"{TokenType.RefreshToken}");

            Error error = new(401, "Invalid or expired refresh token.");
            return Result<string>.Failure(error);
        }

        // get username by userId from database
        var username = await _usersService.GetUsernameByIdAsync(storedRefreshToken.Value?.UserId!);

        if (username is null)
        {
            Error error = new(401, "Invalid or expired refresh token.");
            return Result<string>.Failure(error);
        }

        return Result<string>.Success(username);
    }

    private async Task<Result<bool>> SaveAuthAndUserDetail(UserRegistration userRegistration)
    {
        byte[] passwordSalt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = _authService.GeneratePasswordHash(
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
            Error error = new(500, "Something wrong with Creating new User!");
            return Result<bool>.Failure(error);
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<NewTokens>> UpdateRefreshToken(
        Result<RefreshToken> storedRefreshToken,
        Guid refreshTokenParsed
    )
    {
        var username = await GenerateUsename(storedRefreshToken, refreshTokenParsed);

        if (username.IsFailure)
        {
            Error error = new(500, username.Error.Message);
            return Result<NewTokens>.Failure(error);
        }

        // Generate token
        var claims = _tokenService.GenerateClaims(
            username.Value,
            storedRefreshToken.Value?.UserId!
        );

        var newAccessToken = _tokenService.GenerateAccessToken(claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // save refreshToken to Databases
        var isSaved = await _refreshTokenService.UpdateRefreshTokenByIdAsync(
            storedRefreshToken.Value?.Id!,
            newRefreshToken
        );

        if (isSaved.IsFailure)
        {
            Error error = new(500, "Unauthorize access.");
            return Result<NewTokens>.Failure(error);
        }

        NewTokens newTokens =
            new() { NewAccessToken = newAccessToken, NewRefreshToken = newRefreshToken };

        return Result<NewTokens>.Success(newTokens);
    }
}

public class NewTokens
{
    public string NewAccessToken { get; set; } = string.Empty;
    public Guid NewRefreshToken { get; set; }
}
