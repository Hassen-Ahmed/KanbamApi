using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ICardsService _cardsService;
    private readonly IListsService _listsService;

    public CardsController(ICardsService cardsService, IListsService listsService)
    {
        _cardsService = cardsService;
        _listsService = listsService;
    }

    [HttpGet("{listId}/list")]
    public async Task<ActionResult<List<Card>>> GetByListId(string listId)
    {
        if (
            !ObjectId.TryParse(listId, out var _)
            || !await _listsService.IsListIdExistByListIdAsync(listId)
        )
        {
            return BadRequest("Invalid listId.");
        }
        try
        {
            var cards = await _cardsService.GetByListIdAsync(listId);
            return Ok(new { cards });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{cardId}/card")]
    public async Task<ActionResult<List<Card>>> GetByCardId(string cardId)
    {
        if (!ObjectId.TryParse(cardId, out var _))
        {
            return BadRequest("Invalid cardId.");
        }
        try
        {
            var card = await _cardsService.GetByIdAsync(cardId);
            return Ok(new { card });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewCard(DtoCardPost dtoNewCard)
    {
        if (!await _listsService.IsListIdExistByListIdAsync(dtoNewCard.ListId!))
        {
            return BadRequest("Invalid listId.");
        }

        try
        {
            var createdCard = await _cardsService.CreateAsync(dtoNewCard);
            return StatusCode(201, createdCard);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost("{cardId}/comment")]
    public async Task<IActionResult> CreateComment(string cardId, [FromBody] Comment newComment)
    {
        if (!ObjectId.TryParse(cardId, out var _))
        {
            return BadRequest("Invalid id.");
        }

        try
        {
            var commentId = await _cardsService.CreateCommentAsync(cardId, newComment);

            newComment.Id = commentId;

            return commentId is not null ? Ok(new { newComment }) : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCard(string id, DtoCardUpdate dtoCardUpdate)
    {
        if (!ObjectId.TryParse(id, out var _))
        {
            return BadRequest("Invalid id.");
        }

        var updated = await _cardsService.PatchByIdAsync(id, dtoCardUpdate);

        if (!updated)
            return NotFound("Card not found or nothing to update.");

        return NoContent();
    }

    [HttpDelete("{cardId}/card")]
    public async Task<IActionResult> RemoveById(string cardId)
    {
        if (!ObjectId.TryParse(cardId, out var _))
        {
            return BadRequest("Invalid cardId.");
        }
        try
        {
            var result = await _cardsService.RemoveByIdAsync(cardId);
            return result ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{listId}/list")]
    public async Task<IActionResult> RemoveByListId(string listId)
    {
        if (!ObjectId.TryParse(listId, out var _))
        {
            return BadRequest("Invalid listId.");
        }
        try
        {
            var result = await _cardsService.RemoveByListIdAsync(listId);
            return result ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{cardId}/{commentId}/comment")]
    public async Task<IActionResult> RemoveCommentById(string cardId, string commentId)
    {
        if (!ObjectId.TryParse(cardId, out var _) || !ObjectId.TryParse(commentId, out var _))
            return BadRequest("Invalid cardId or commentId.");

        try
        {
            var result = await _cardsService.RemoveCommentByIdAsync(cardId, commentId);
            return result ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
