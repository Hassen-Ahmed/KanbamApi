using KanbamApi.Dtos;
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
public class ListsController : ControllerBase
{
    private readonly IListsService _listsService;
    private readonly IBoardService _boardService;
    private readonly IHubContext<ListHub> _hubContext;

    public ListsController(
        IListsService listsService,
        IBoardService boardService,
        IHubContext<ListHub> hubContext
    )
    {
        _listsService = listsService;
        _boardService = boardService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllList()
    {
        try
        {
            var lists = await _listsService.GetAllAsync();
            return Ok(new { lists });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{boardId}")]
    public async Task<ActionResult> GetAllListByBoardId(string boardId)
    {
        if (
            !ObjectId.TryParse(boardId, out var _)
            || !await _boardService.IsBoardIdExistByBoardIdAsync(boardId)
        )
        {
            return BadRequest("Invalid boardId.");
        }
        try
        {
            var lists = await _listsService.GetListsWithCardsByBoardIdAsync(boardId);
            return Ok(new { lists });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateList(DtoListPost newListDto)
    {
        if (!await _boardService.IsBoardIdExistByBoardIdAsync(newListDto.BoardId))
        {
            return BadRequest("Invalid boardId.");
        }

        try
        {
            var createdList = await _listsService.CreateAsync(newListDto);

            var groupId = newListDto.BoardId;

            await _hubContext
                .Clients.Group(groupId)
                .SendAsync("ReceiveListCreated", createdList, $"{groupId}");

            return StatusCode(201, createdList);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{listId}")]
    public async Task<IActionResult> UpdateList(string listId, DtoListsUpdate dtoListsUpdate)
    {
        if (!ObjectId.TryParse(listId, out var _))
        {
            return BadRequest("Invalid listId.");
        }
        var updated = await _listsService.PatchByIdAsync(listId, dtoListsUpdate);

        if (!updated)
            return NotFound("List not found or nothing to update.");

        var userId = User.FindFirst("userId")?.Value;
        var updatedList = new List()
        {
            Id = listId,
            BoardId = dtoListsUpdate.BoardId!,
            Title = dtoListsUpdate.Title!,
            IndexNumber = (int)dtoListsUpdate.IndexNumber!,
        };

        var groupId = dtoListsUpdate.BoardId;

        await _hubContext
            .Clients.Group(groupId!)
            .SendAsync("ReceiveListUpdate", updatedList, userId, $"{groupId}");

        return NoContent();
    }

    [HttpDelete("{listId}/{boardId}")]
    public async Task<IActionResult> Delete(string listId, string boardId)
    {
        if (!ObjectId.TryParse(listId, out var _))
        {
            return BadRequest("Invalid listId.");
        }
        try
        {
            var res = await _listsService.RemoveByIdAsync(listId);

            if (!res)
                return BadRequest();

            var groupId = boardId;

            await _hubContext
                .Clients.Group(groupId)
                .SendAsync("ReceiveListDelete", listId, $"{groupId}");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
