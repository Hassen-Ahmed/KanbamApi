using KanbamApi.Core;
using KanbamApi.Models.AuthModels;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenService _tokenService, UserManager<ApplicationUser> _userManager)
    : ControllerBase
{
    [HttpPost(ApiRoutesAuth.Register)]
    public async Task<IActionResult> Register([FromBody] UserRegistration userRegistration)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var authDetail = await _userManager.FindByEmailAsync(userRegistration.Email);

        if (authDetail is not null)
            return BadRequest(new { error = ErrorMessages.EmailExists });

        ApplicationUser user =
            new()
            {
                Email = userRegistration.Email,
                UserName = userRegistration.Email?.Split('@').FirstOrDefault()
            };
        IdentityResult createUser = await _userManager.CreateAsync(user, userRegistration.Password);

        if (!createUser.Succeeded)
            return StatusCode(
                500,
                $"Failed to create: {string.Join(", ", createUser.Errors.Select(e => e.Description))}"
            );

        return Ok(new { message = ErrorMessages.SuccessfullRegistered });
    }

    [HttpPost(ApiRoutesAuth.Login)]
    public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
    {
        // validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // check if exists
        var user = await _userManager.FindByEmailAsync(userLogin.Email);

        if (user is null)
            return BadRequest(new { error = ErrorMessages.InvalidEmail });

        // Check password match.
        var isPasswordMatch = await _userManager.CheckPasswordAsync(user!, userLogin.Password);

        if (!isPasswordMatch)
            return BadRequest(new { error = ErrorMessages.InvalidPassword });

        var result = await CreateAndUpdateTokens(user);
        if (result is null)
            return BadRequest(new { error = ErrorMessages.FailedRefreshTokenUpdate });

        SetRefreshTokenCookie(result.NewRefreshToken, result.ExpiredDate);
        return Ok(new { accessToken = result.NewAccessToken });
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

        if (!Guid.TryParse(refreshTokenFromCookie, out var refreshTokenParsed))
            return Unauthorized(new { error = "Invalid typeof refresh token." });

        var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshTokenParsed);

        if (user is null)
            return NotFound();

        // check if RefreshTokenExpiryTime is expired
        if (user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return BadRequest(new { error = "Expired RefreshToken" });

        var result = await CreateAndUpdateTokens(user);
        if (result is null)
            return BadRequest(new { Message = ErrorMessages.FailedRefreshTokenUpdate });

        SetRefreshTokenCookie(result.NewRefreshToken, user.RefreshTokenExpiryTime);
        return Ok(
            new
            {
                Message = "The registration was successful!",
                accessToken = result.NewAccessToken
            }
        );
    }

    [HttpPost(ApiRoutesAuth.Revoke)]
    public async Task<IActionResult> RevokeRefreshToken()
    {
        if (
            Request.Cookies.TryGetValue($"{TokenType.RefreshToken}", out var refreshTokenFromCookie)
            && Guid.TryParse(refreshTokenFromCookie, out var refreshTokenParsed)
        )
        {
            Response.Cookies.Delete($"{TokenType.RefreshToken}");
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshTokenParsed);

            if (user is not null)
            {
                user.RefreshToken = new Guid();
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);
            }

            return Ok();
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
                SameSite = SameSiteMode.None,
                Expires = expirationDate
            }
        );
    }

    private async Task<NewTokens?> CreateAndUpdateTokens(ApplicationUser user)
    {
        var userIdString = user.Id.ToString();
        var claims = _tokenService.GenerateClaims(user.UserName!, userIdString);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expireDate = DateTime.UtcNow.AddDays((int)TokenExpiration.RefreshTokenDate);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expireDate;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return null;

        return new NewTokens(accessToken, refreshToken, expireDate);
    }

    private class NewTokens(string accToken, Guid refToken, DateTime expiredDate)
    {
        public string NewAccessToken { get; } = accToken;
        public Guid NewRefreshToken { get; } = refToken;
        public DateTime ExpiredDate { get; } = expiredDate;
    }
}
