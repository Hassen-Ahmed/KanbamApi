using KanbamApi.Dtos.Update;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly ILogger<UsersController> _logger;
    private readonly IGeneralValidation _generalValidation;

    public UsersController(
        IUsersService usersService,
        ILogger<UsersController> logger,
        IGeneralValidation generalValidation
    )
    {
        _usersService = usersService;
        _logger = logger;
        _generalValidation = generalValidation;
    }

    [HttpGet("{passcode}/secret")]
    public async Task<IActionResult> Get(string passcode)
    {
        var secretKey = DotNetEnv.Env.GetString("SECRET_KEY");

        if (passcode != secretKey)
            return StatusCode(400, "Bad request");

        var users = await _usersService.GetAllAsync();
        return users is not null ? Ok(new { users }) : NotFound();
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById(string userId)
    {
        if (!_generalValidation.IsValidObjectId(userId))
            return BadRequest("Invalid boardId.");

        try
        {
            var res = await _usersService.GetByIdAsync(userId);

            return res is not null ? Ok(new { user = res[0] }) : NotFound();
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

        var updated = await _usersService.PatchByIdAsync(userId, updateUser);

        if (!updated)
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
