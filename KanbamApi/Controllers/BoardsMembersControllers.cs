using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("FixedWindow")]
public class BoardsMembersController : ControllerBase
{
    private readonly IBoardMemberService _boardMemberService;
    private readonly IGeneralValidation _generalValidation;
    private readonly ILogger<BoardsMembersController> _logger;

    public BoardsMembersController(
        IBoardMemberService boardMemberService,
        IGeneralValidation generalValidation,
        ILogger<BoardsMembersController> logger
    )
    {
        _boardMemberService = boardMemberService;
        _generalValidation = generalValidation;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBoardMembers()
    {
        try
        {
            var boards = await _boardMemberService.GetAllAsync();
            return boards is not null ? Ok(new { boards }) : NotFound();
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

    [HttpGet("{boardId}")]
    public async Task<IActionResult> GetAllBoardMembersByBoardId(string boardId)
    {
        if (!_generalValidation.IsValidObjectId(boardId))
            return BadRequest("Invalid boardId.");

        try
        {
            var boardMembers = await _boardMemberService.GetMembersByBoardIdAsync(boardId);
            return boardMembers.Count > 0 ? Ok(new { boardMembers }) : NotFound();
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
    public async Task<IActionResult> CreateBoard(DtoBoardMemberPost newBoardMember)
    {
        if (!_generalValidation.IsValidEmail(newBoardMember.Email))
            return BadRequest("Invalid email address.");

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
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpPatch("{boardMemberId}")]
    public async Task<IActionResult> PatchWorkspaceById(
        string boardMemberId,
        [FromBody] DtoBoardMemberUpdate updateBoardMember
    )
    {
        if (!_generalValidation.IsValidObjectId(boardMemberId))
            return BadRequest("Invalid boardMemberId.");

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
        if (!_generalValidation.IsValidObjectId(boardMemberId))
            return BadRequest("Invalid boardMemberId.");

        try
        {
            var currentUserId = User.FindFirst("userId")?.Value;

            var res = await _boardMemberService.RemoveById(boardMemberId, currentUserId!);
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
