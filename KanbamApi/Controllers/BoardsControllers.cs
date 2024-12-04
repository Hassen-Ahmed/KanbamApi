using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Hubs;
using KanbamApi.Models;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardsService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly IHubContext<WorkspaceHub> _hubContext;

    public BoardsController(
        IBoardService boardsService,
        IWorkspaceService workspaceService,
        IWorkspaceMemberService workspaceMemberService,
        IHubContext<WorkspaceHub> hubContext
    )
    {
        _boardsService = boardsService;
        _workspaceService = workspaceService;
        _workspaceMemberService = workspaceMemberService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBoards()
    {
        try
        {
            var boards = await _boardsService.GetAllAsync();
            return Ok(new { boards });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{workspaceId}")]
    public async Task<IActionResult> GetAllBoardsByWorkspaceId(string workspaceId)
    {
        if (
            !ObjectId.TryParse(workspaceId, out var _)
            || !await _workspaceService.IsWorkspaceExist_Using_WorkspaceIdAsync(workspaceId)
        )
        {
            return BadRequest("Invalid workspaceId.");
        }

        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var boards = await _boardsService.GetBoards_With_Members_ByWorkspaceId_Async(
                workspaceId,
                userId!
            );

            return Ok(new { boards });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard(DtoBoardPost newBoard)
    {
        if (!await _workspaceService.IsWorkspaceExist_Using_WorkspaceIdAsync(newBoard.WorkspaceId))
        {
            return BadRequest("Invalid boardId.");
        }

        var userId = User.FindFirst("userId")?.Value;
        try
        {
            await _boardsService.CreateAsync(userId!, newBoard);

            var groupId = newBoard.WorkspaceId;

            await _hubContext
                .Clients.Group(groupId)
                .SendAsync("ReceiveWorkspaceUpdate", newBoard, groupId);

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

        var userId = User.FindFirst("userId")?.Value;

        var updated = await _boardsService.PatchByIdAsync(boardId, updateBoard, userId!);

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

        var userId = User.FindFirst("userId")?.Value;

        try
        {
            var res = await _boardsService.RemoveByIdAsync(boardId, userId!);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
