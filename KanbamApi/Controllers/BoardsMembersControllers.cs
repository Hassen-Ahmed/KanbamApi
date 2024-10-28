using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardsMembersController : ControllerBase
{
    private readonly IBoardMemberService _boardMemberService;

    public BoardsMembersController(IBoardMemberService boardMemberService) =>
        _boardMemberService = boardMemberService;

    [HttpGet]
    public async Task<IActionResult> GetAllBoardMembers()
    {
        try
        {
            var boards = await _boardMemberService.GetAllAsync();
            return Ok(new Dictionary<string, object> { { "boards", boards } });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard(DtoBoardMemberPost newBoard)
    {
        var userId = User.FindFirst("userId")?.Value;
        try
        {
            BoardMember boardMember =
                new()
                {
                    UserId = userId!,
                    BoardId = newBoard.BoardId,
                    Role = newBoard.Role
                };

            await _boardMemberService.CreateAsync(boardMember);
            return StatusCode(201, newBoard);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{boardMemberId}")]
    public async Task<IActionResult> PatchWorkspaceById(
        string boardMemberId,
        [FromBody] DtoBoardMemberUpdate updateBoardMember
    )
    {
        if (!ObjectId.TryParse(boardMemberId, out var _))
        {
            return BadRequest("Invalid boardMemberId.");
        }

        var updated = await _boardMemberService.PatchByIdAsync(boardMemberId, updateBoardMember);

        if (!updated)
            return NotFound("BoardMember not found or nothing to update.");

        return NoContent();
    }

    [HttpDelete("{boardMemberId}")]
    public async Task<IActionResult> Delete(string boardMemberId)
    {
        if (!ObjectId.TryParse(boardMemberId, out var _))
        {
            return BadRequest("Invalid boardMemberId.");
        }
        try
        {
            var res = await _boardMemberService.RemoveById(boardMemberId);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
