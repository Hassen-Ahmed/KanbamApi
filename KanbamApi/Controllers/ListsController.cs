using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ListsController : ControllerBase
{
    private readonly IListsService _listsService;
    private readonly IBoardService _boardService;

    public ListsController(IListsService listsService, IBoardService boardService)
    {
        _listsService = listsService;
        _boardService = boardService;
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
            var lists = await _listsService.GetAllByBoardIdAsync(boardId);
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

        return NoContent();
    }

    [HttpDelete("{listId}")]
    public async Task<IActionResult> Delete(string listId)
    {
        if (!ObjectId.TryParse(listId, out var _))
        {
            return BadRequest("Invalid listId.");
        }
        try
        {
            var res = await _listsService.RemoveByIdAsync(listId);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
