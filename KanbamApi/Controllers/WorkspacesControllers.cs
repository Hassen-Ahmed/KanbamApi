using KanbamApi.Dtos.Patch;
using KanbamApi.Dtos.Posts;
using KanbamApi.Models;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IUsersService _usersService;

    public WorkspacesController(IWorkspaceService workspaceService, IUsersService usersService)
    {
        _workspaceService = workspaceService;
        _usersService = usersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWorkspacesByUserId()
    {
        var userId = User.FindFirst("userId")?.Value;

        if (userId is null)
        {
            return Unauthorized("Wrong userId!");
        }

        try
        {
            var userDetail = await _usersService.GetByIdAsync(userId);
            var workspaces = await _workspaceService.GetWorkspaces_With_Members_ByUserId(userId);

            return Ok(
                new
                {
                    workspaces,
                    userDetail = new Dictionary<string, string>
                    {
                        { "userName", userDetail[0].UserName! },
                        { "email", userDetail[0].Email! },
                    },
                }
            );
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard(DtoWorkspacePost newWorkspace)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value!;

            Workspace workspace =
                new() { Name = newWorkspace.Name, Description = newWorkspace.Description };

            await _workspaceService.CreateAsync(userId, workspace);

            return StatusCode(201, newWorkspace);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{workspaceId}")]
    public async Task<IActionResult> PatchWorkspaceById(
        string workspaceId,
        DtoWorkspaceUpdate updateWorkspace
    )
    {
        if (!ObjectId.TryParse(workspaceId, out var _))
        {
            return BadRequest("Invalid workspaceId.");
        }

        var userId = User.FindFirst("userId")?.Value!;

        var updated = await _workspaceService.PatchByIdAsync(workspaceId, updateWorkspace, userId);

        if (!updated)
            return NotFound("Workspace not found or nothing to update.");

        return NoContent();
    }

    [HttpDelete("{workspaceId}")]
    public async Task<IActionResult> Delete(string workspaceId)
    {
        if (!ObjectId.TryParse(workspaceId, out var _))
        {
            return BadRequest("Invalid workspaceId.");
        }
        try
        {
            var userId = User.FindFirst("userId")?.Value!;
            var res = await _workspaceService.Remove_With_MembersAsync(workspaceId, userId);

            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
