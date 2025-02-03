using Google.Apis.Auth;
using KanbamApi.Core;
using KanbamApi.Models.AuthModels;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Services.Interfaces;
using KanbamApi.Services.Interfaces.Email;
using KanbamApi.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IEmailService emailService
    )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    [HttpPost(ApiRoutesAuth.Register)]
    public async Task<IActionResult> Register([FromBody] UserRegistration userRegistration)
    {
        // validate userRegistration
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
        // validate userLogin
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

    [HttpPost(ApiRoutesAuth.ForgotPassword)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDetail forgotPasswordDetail
    )
    {
        // validate forgotPasswordDetail
        if (!ModelState.IsValid)
            return Ok(ModelState);

        // check if exists
        var user = await _userManager.FindByEmailAsync(forgotPasswordDetail.Email);
        if (user is null)
            return Ok(new { error = ErrorMessages.InvalidEmail });

        // generate token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // send email
        await SendResetEmail(token, forgotPasswordDetail.Email);
        return Ok(new { message = $"Email sent.", email = $"{forgotPasswordDetail.Email}" });
    }

    [HttpPost(ApiRoutesAuth.ResetPassword)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDetail resetPasswordDetail
    )
    {
        // validate the resetPasswordDetail
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // check if exists
        var user = await _userManager.FindByEmailAsync(resetPasswordDetail.Email);
        if (user is null)
            return BadRequest(new { error = ErrorMessages.InvalidEmail });

        // Reset password with newPassword
        var result = await _userManager.ResetPasswordAsync(
            user,
            resetPasswordDetail.Token,
            resetPasswordDetail.NewPassword
        );

        if (!result.Succeeded)
            return BadRequest(new { Message = "The token is invalid or has expired." });

        return Ok(new { message = "Password reset successfully." });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = [DotNetEnv.Env.GetString("GOOGLE_CLIENT_ID")]
        };
        try
        {
            // Validate the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(model.Token, settings);
            var name = payload.Name;

            // Check if the user exists in my database
            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                // Create a new user if they don’t exist
                user = new ApplicationUser { UserName = payload.Email, Email = payload.Email, };

                var creatingResult = await _userManager.CreateAsync(user);

                if (!creatingResult.Succeeded)
                    return StatusCode(
                        500,
                        $"Failed to create: {string.Join(", ", creatingResult.Errors.Select(e => e.Description))}"
                    );
            }

            // Generate and return a JWT access and refresh token, and also update user detail
            var result = await CreateAndUpdateTokens(user);
            if (result is null)
                return BadRequest(new { error = ErrorMessages.FailedRefreshTokenUpdate });

            SetRefreshTokenCookie(result.NewRefreshToken, result.ExpiredDate);
            return Ok(new { accessToken = result.NewAccessToken });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Invalid Google token.", details = ex.Message });
        }
    }

    /// Helpers
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

    private async Task<bool> SendResetEmail(string token, string email)
    {
        // Some special chars must be encode and then decode later from FrontEnd side.
        var encodedResetToken = Uri.EscapeDataString(token);
        var encodedResetEmail = Uri.EscapeDataString(email);
        var domainName = DotNetEnv.Env.GetString("FRONT_END_DOMAIN");
        var frontEndUrl =
            $"{domainName}/auth/reset-password?resetToken={encodedResetToken}&email={encodedResetEmail}";

        var body = ConstantData.GenerateHTMLContent(email, frontEndUrl);

        EmailRequest emailToSend = new(email, "Only for testing purpose.", body);
        var sendEmail = await _emailService?.SendEmailAsync(emailToSend)!;

        return true;
    }
}
