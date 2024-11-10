using KanbamApi.Dtos.Update;
using KanbamApi.Repositories;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
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

    public UsersController(IUsersService usersService) => _usersService = usersService;

    [HttpGet("{passcode}/secret")]
    public async Task<IActionResult> Get(string passcode)
    {
        var secretKey = DotNetEnv.Env.GetString("SECRET_KEY");

        if (passcode != secretKey)
            return StatusCode(400, "Bad request");

        var users = await _usersService.GetAllAsync();
        return Ok(new { users });
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById(string userId)
    {
        if (!ObjectId.TryParse(userId, out var _))
        {
            return BadRequest("Invalid boardId.");
        }
        try
        {
            var res = await _usersService.GetByIdAsync(userId);

            return Ok(new { user = res[0] });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{userId}")]
    public async Task<IActionResult> Update(string userId, DtoUsersUpdate updateUser)
    {
        if (!ObjectId.TryParse(userId, out var _))
        {
            return BadRequest("Invalid userId.");
        }

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
