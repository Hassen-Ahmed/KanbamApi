using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Hubs;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardsService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IHubContext<BoardHub> _hubContext;
    private readonly ILogger<BoardsController> _logger;
    private readonly IGeneralValidation _generalValidation;

    public BoardsController(
        IBoardService boardsService,
        IWorkspaceService workspaceService,
        IWorkspaceMemberService workspaceMemberService,
        IHubContext<BoardHub> hubContext,
        ILogger<BoardsController> logger,
        IGeneralValidation generalValidation
    )
    {
        _boardsService = boardsService;
        _workspaceService = workspaceService;
        _hubContext = hubContext;
        _logger = logger;
        _generalValidation = generalValidation;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBoards()
    {
        try
        {
            var boards = await _boardsService.GetAllAsync();
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

    [HttpGet("{workspaceId}")]
    public async Task<IActionResult> GetAllBoardsByWorkspaceId(string workspaceId)
    {
        if (
            !_generalValidation.IsValidObjectId(workspaceId)
            || !await _workspaceService.IsWorkspaceExist_Using_WorkspaceIdAsync(workspaceId)
        )
            return BadRequest("Invalid workspaceId.");

        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var boards = await _boardsService.GetBoards_With_Members_ByWorkspaceId_Async(
                workspaceId,
                userId!
            );

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

    [HttpPost]
    public async Task<IActionResult> CreateBoard(DtoBoardPost newBoard)
    {
        if (!await _workspaceService.IsWorkspaceExist_Using_WorkspaceIdAsync(newBoard.WorkspaceId))
            return BadRequest("Invalid boardId.");

        var userId = User.FindFirst("userId")?.Value;
        try
        {
            var boardId = await _boardsService.CreateAsync(userId!, newBoard);
            var groupId = newBoard.WorkspaceId;
            var createdBoard = new
            {
                BoardId = boardId,
                newBoard.WorkspaceId,
                newBoard.Name,
                newBoard.Description,
            };

            try
            {
                await _hubContext
                    .Clients.Group(groupId)
                    .SendAsync("ReceiveBoardCreated", createdBoard, $"{groupId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                );
            }

            return StatusCode(201, createdBoard);
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

    [HttpPatch("{boardId}")]
    public async Task<IActionResult> PatchBoardById(
        string boardId,
        [FromBody] DtoBoardUpdate updateBoard
    )
    {
        if (!_generalValidation.IsValidObjectId(boardId))
            return BadRequest("Invalid boardId.");

        var userId = User.FindFirst("userId")?.Value;

        var updated = await _boardsService.PatchByIdAsync(boardId, updateBoard, userId!);

        if (!updated)
            return NotFound("Board not found or nothing to update.");

        var groupId = updateBoard.WorkspaceId;
        var updatedBoard = new
        {
            BoardId = boardId,
            updateBoard.WorkspaceId,
            updateBoard.Name,
            updateBoard.Description,
        };

        try
        {
            await _hubContext
                .Clients.Group(groupId!)
                .SendAsync("ReceiveBoardUpdate", updatedBoard, $"{groupId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }

        return NoContent();
    }

    [HttpDelete("{boardId}/{workspaceId}")]
    public async Task<IActionResult> RemoveBoard(string boardId, string workspaceId)
    {
        if (!_generalValidation.IsValidObjectId(boardId))
            return BadRequest("Invalid boardId.");

        var userId = User.FindFirst("userId")?.Value;

        try
        {
            var res = await _boardsService.RemoveByIdAsync(boardId, userId!);

            if (!res)
                NotFound();

            var groupId = workspaceId;

            try
            {
                await _hubContext
                    .Clients.Group(groupId!)
                    .SendAsync("ReceiveBoardDelete", boardId, $"{groupId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                );
            }

            return NoContent();
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
