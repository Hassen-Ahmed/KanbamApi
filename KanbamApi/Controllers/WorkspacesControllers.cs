using KanbamApi.Dtos.Patch;
using KanbamApi.Dtos.Posts;
using KanbamApi.Models;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("FixedWindow")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;
    private readonly ILogger<WorkspacesController> _logger;
    private readonly IGeneralValidation _generalValidation;

    public WorkspacesController(
        IWorkspaceService workspaceService,
        ILogger<WorkspacesController> logger,
        IGeneralValidation generalValidation
    )
    {
        _workspaceService = workspaceService;
        _logger = logger;
        _generalValidation = generalValidation;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWorkspacesByUserId()
    {
        var userId = User.FindFirst("userId")?.Value;

        if (userId is null)
            return Unauthorized("Wrong userId!");

        try
        {
            var workspaces = await _workspaceService.GetWorkspaces_With_Members_ByUserId(userId);
            if (workspaces is null)
                return NotFound();

            return Ok(new { workspaces });
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
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpPatch("{workspaceId}")]
    public async Task<IActionResult> PatchWorkspaceById(
        string workspaceId,
        DtoWorkspaceUpdate updateWorkspace
    )
    {
        if (!_generalValidation.IsValidObjectId(workspaceId))
            return BadRequest("Invalid workspaceId.");

        var userId = User.FindFirst("userId")?.Value!;

        var updated = await _workspaceService.PatchByIdAsync(workspaceId, updateWorkspace, userId);

        if (!updated)
            return NotFound("Workspace not found or nothing to update.");

        return NoContent();
    }

    [HttpDelete("{workspaceId}")]
    public async Task<IActionResult> Delete(string workspaceId)
    {
        if (!_generalValidation.IsValidObjectId(workspaceId))
            return BadRequest("Invalid workspaceId.");

        try
        {
            var userId = User.FindFirst("userId")?.Value!;
            var res = await _workspaceService.Remove_With_MembersAsync(workspaceId, userId);

            return res ? NoContent() : NotFound();
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
}
