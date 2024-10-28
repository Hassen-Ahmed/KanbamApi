using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Put;
using KanbamApi.Services.Interfaces;
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

    public WorkspacesMembersController(IWorkspaceMemberService workspaceMemberService) =>
        _workspaceMemberService = workspaceMemberService;

    [HttpGet]
    public async Task<IActionResult> GetAllMembers()
    {
        try
        {
            var workspacesMembers = await _workspaceMemberService.Get();
            return Ok(
                new Dictionary<string, object> { { "workspacesMembers", workspacesMembers } }
            );
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
        var userId = User.FindFirst("userId")?.Value;

        try
        {
            var createdWorkspace = await _workspaceMemberService.CreateAsync(
                newWorkspaceMember,
                userId!
            );

            return StatusCode(201, createdWorkspace);
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
        var updated = await _workspaceMemberService.PatchByIdAsync(
            workspaceMemberId,
            updateWorkspaceMember
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
