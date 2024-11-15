using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Validators;
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
    private readonly IGeneralValidation _generalValidation;

    public BoardsMembersController(
        IBoardMemberService boardMemberService,
        IGeneralValidation generalValidation
    )
    {
        _boardMemberService = boardMemberService;
        _generalValidation = generalValidation;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBoardMembers()
    {
        try
        {
            var boards = await _boardMemberService.GetAllAsync();
            return Ok(new { boards });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{boardId}")]
    public async Task<IActionResult> GetAllBoardMembersByBoardId(string boardId)
    {
        if (!ObjectId.TryParse(boardId, out var _))
        {
            return BadRequest("Invalid boardId.");
        }

        try
        {
            var boardMembers = await _boardMemberService.GetMembersByBoardIdAsync(boardId);
            return boardMembers.Count > 0 ? Ok(new { boardMembers }) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard(DtoBoardMemberPost newBoardMember)
    {
        if (!_generalValidation.IsValidEmail(newBoardMember.Email))
        {
            return BadRequest("Invalid email address.");
        }

        var currentUserId = User.FindFirst("userId")?.Value;

        try
        {
            var createdWorkspace = await _boardMemberService.CreateAsync(
                newBoardMember,
                currentUserId
            );

            return createdWorkspace ? StatusCode(201, newBoardMember) : NotFound();
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

        var currentUserId = User.FindFirst("userId")?.Value;

        var updated = await _boardMemberService.PatchByIdAsync(
            boardMemberId,
            updateBoardMember,
            currentUserId!
        );

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
            var currentUserId = User.FindFirst("userId")?.Value;

            var res = await _boardMemberService.RemoveById(boardMemberId, currentUserId!);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
