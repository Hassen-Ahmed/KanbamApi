using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Put;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkspacesMembersController : ControllerBase
{
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly IGeneralValidation _generalValidation;

    public WorkspacesMembersController(
        IWorkspaceMemberService workspaceMemberService,
        IGeneralValidation generalValidation
    )
    {
        _workspaceMemberService = workspaceMemberService;
        _generalValidation = generalValidation;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMembers()
    {
        try
        {
            var workspacesMembers = await _workspaceMemberService.Get();
            return Ok(new { workspacesMembers });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpGet("{workspaceId}")]
    public async Task<IActionResult> GetAllMembersByWorkspaceId(string workspaceId)
    {
        try
        {
            var workspacesMembers = await _workspaceMemberService.GetMembersByWorkspaceIdAsync(
                workspaceId
            );

            return Ok(new { workspacesMembers });
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateMember(
        [FromBody] DtoWorkspaceMemberPost newWorkspaceMember
    )
    {
        if (!_generalValidation.IsValidEmail(newWorkspaceMember.Email))
        {
            return BadRequest("Invalid email address.");
        }

        var currentUserId = User.FindFirst("userId")?.Value;

        try
        {
            var createdWorkspace = await _workspaceMemberService.CreateAsync(
                newWorkspaceMember,
                currentUserId
            );

            return createdWorkspace ? StatusCode(201, newWorkspaceMember) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{workspaceMemberId}")]
    public async Task<IActionResult> PatchWorkspaceById(
        string workspaceMemberId,
        [FromBody] DtoWorkspaceMemberUpdate updateWorkspaceMember
    )
    {
        if (!ObjectId.TryParse(workspaceMemberId, out var _))
        {
            return BadRequest("Invalid workspaceMemberId.");
        }

        var currentUserId = User.FindFirst("userId")?.Value;

        var updated = await _workspaceMemberService.PatchByIdAsync(
            workspaceMemberId,
            updateWorkspaceMember,
            currentUserId!
        );

        if (!updated)
            return NotFound("WorkspaceMember not found or nothing to update.");

        return NoContent();
    }

    [HttpDelete("{workspaceMemberId}")]
    public async Task<IActionResult> Delete(string workspaceMemberId)
    {
        if (!ObjectId.TryParse(workspaceMemberId, out var _))
        {
            return BadRequest("Invalid workspaceMemberId.");
        }
        try
        {
            var res = await _workspaceMemberService.RemoveByIdAsync(workspaceMemberId);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
