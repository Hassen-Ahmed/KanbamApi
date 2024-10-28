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

    public CardsController(ICardsService cardsService) => _cardsService = cardsService;

    [HttpGet("{listId}/list")]
    public async Task<ActionResult<List<Card>>> GetByListId(string listId)
    {
        if (!ObjectId.TryParse(listId, out var _))
        {
            return BadRequest("Invalid listId.");
        }
        try
        {
            var cards = await _cardsService.GetByListIdAsync(listId);
            return Ok(new Dictionary<string, object> { { "cards", cards } });
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
            return Ok(new Dictionary<string, object> { { "card", card } });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewCard(DtoCardPost dtoNewCard)
    {
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
            var res = await _cardsService.RemoveByIdAsync(cardId);
            return res ? NoContent() : BadRequest();
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
            var res = await _cardsService.RemoveByListIdAsync(listId);
            return res ? NoContent() : BadRequest();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
