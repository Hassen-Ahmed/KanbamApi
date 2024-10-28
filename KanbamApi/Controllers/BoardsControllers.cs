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
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardsService;

    public BoardsController(IBoardService boardsService) => _boardsService = boardsService;

    [HttpGet]
    public async Task<IActionResult> GetAllBoards()
    {
        try
        {
            var boards = await _boardsService.GetAllAsync();
            return Ok(new Dictionary<string, object> { { "boards", boards } });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{workspaceId}")]
    public async Task<IActionResult> GetAllBoardsByWorkspaceId(string workspaceId)
    {
        if (!ObjectId.TryParse(workspaceId, out var _))
        {
            return BadRequest("Invalid workspaceId.");
        }

        var userId = User.FindFirst("userId")?.Value;
        try
        {
            var boards = await _boardsService.GetBoards_With_Members_ByWorkspaceId_Async(
                workspaceId,
                userId!
            );
            return Ok(new Dictionary<string, object> { { "boards", boards } });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard(DtoBoardPost newBoard)
    {
        var userId = User.FindFirst("userId")?.Value;
        try
        {
            Board board =
                new()
                {
                    Name = newBoard.Name,
                    WorkspaceId = newBoard.WorkspaceId,
                    Description = newBoard.Description
                };

            await _boardsService.CreateAsync(userId!, board);
            return StatusCode(201, newBoard);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{boardId}")]
    public async Task<IActionResult> PatchBoardById(
        string boardId,
        [FromBody] DtoBoardUpdate updateBoard
    )
    {
        if (!ObjectId.TryParse(boardId, out var _))
        {
            return BadRequest("Invalid boardId.");
        }

        var updated = await _boardsService.PatchByIdAsync(boardId, updateBoard);

        if (!updated)
            return NotFound("Board not found or nothing to update.");

        return NoContent();
    }

    [HttpDelete("{boardId}")]
    public async Task<IActionResult> RemoveBoard(string boardId)
    {
        if (!ObjectId.TryParse(boardId, out var _))
        {
            return BadRequest("Invalid boardId.");
        }
        try
        {
            var res = await _boardsService.RemoveByIdAsync(boardId);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
