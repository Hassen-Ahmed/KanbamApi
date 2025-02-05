using KanbamApi.Dtos.Update;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Driver.Linq;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("FixedWindow")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UsersController> _logger;
    private readonly IGeneralValidation _generalValidation;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        ILogger<UsersController> logger,
        IGeneralValidation generalValidation
    )
    {
        _userManager = userManager;
        _logger = logger;
        _generalValidation = generalValidation;
    }

    [HttpGet("{passcode}/secret")]
    public async Task<IActionResult> Get(string passcode)
    {
        var secretKey = DotNetEnv.Env.GetString("SECRET_KEY");

        if (passcode != secretKey)
            return StatusCode(400, "Bad request");

        var users = await _userManager.Users.ToListAsync();
        return users is not null ? Ok(new { users }) : NotFound();
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById(string userId)
    {
        if (!_generalValidation.IsValidObjectId(userId))
            return BadRequest("Invalid boardId.");

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user is not null ? Ok(new { user = user }) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpPatch("{userId}")]
    public async Task<IActionResult> Update(string userId, DtoUsersUpdate updateUser)
    {
        if (!_generalValidation.IsValidObjectId(userId))
            return BadRequest("Invalid userId.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound("User not found with this userId!");

        user.UserName = updateUser.UserName;
        user.Email = updateUser.Email;
        var updated = await _userManager.UpdateAsync(user);

        if (!updated.Succeeded)
            return NotFound("User not found or nothing to update.");

        return NoContent();
    }

    // [HttpDelete]
    // public async Task<IActionResult> Delete()
    // {
    //     // remove users from WorkspaceMember, BoardMember, and Users documents
    //     // And also user related documents/table like Boards, Lists, and Cards....
    // }
}
