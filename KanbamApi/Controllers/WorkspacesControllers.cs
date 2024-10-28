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

    public WorkspacesController(IWorkspaceService workspaceService) =>
        _workspaceService = workspaceService;

    [HttpGet]
    public async Task<IActionResult> GetAllWorkspacesByUserId()
    {
        var userId = User.FindFirst("userId")?.Value;

        try
        {
            var workspaces = await _workspaceService.GetWorkspaces_With_Members_ByUserId(userId!);
            return Ok(new Dictionary<string, object> { { "workspaces", workspaces } });
        }
        catch (Exception)
        {
            throw;
        }
    }

    //
    // [HttpGet("{workspaceId}")]
    // public async Task<IActionResult> GetWorkspaceId(string workspaceId)
    // {
    //     try
    //     {
    //         var res = await _workspaceService.GetWorkspaceById(workspaceId);
    //         return StatusCode(200, res);
    //     }
    //     catch (Exception)
    //     {
    //         return StatusCode(
    //             StatusCodes.Status400BadRequest,
    //             "An error occurred while processing the request."
    //         );
    //     }
    // }


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
        var updated = await _workspaceService.PatchByIdAsync(workspaceId, updateWorkspace);

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
            var res = await _workspaceService.RemoveAsync(workspaceId);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
