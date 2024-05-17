using KanbamApi.Models;
using KanbamApi.Repositories;
using KanbamApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly CardsRepo _cardsRepo;

    public CardsController(CardsRepo cardsRepo) => _cardsRepo = cardsRepo;

    [Authorize]
    [HttpGet("{listId}/list")]
    public async Task<ActionResult<List<Card>>> GetByListId(string listId)
    {
        try
        {
            return await _cardsRepo.GetByListIdAsync(listId);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing the request."
            );
        }
    }

    [Authorize]
    [HttpGet("{cardId}/card")]
    public async Task<ActionResult<List<Card>>> GetByCardId(string cardId)
    {
        try
        {
            return await _cardsRepo.GetByCardIdAsync(cardId);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing the request."
            );
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateNewCard(Card newCard)
    {
        try
        {
            await _cardsRepo.CreateAsync(newCard);
            return CreatedAtAction(nameof(CreateNewCard), new { id = newCard.Id }, newCard);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing the request."
            );
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCard(string id, Card updatedCard)
    {
        try
        {
            await _cardsRepo.UpdateAsync(id, updatedCard);
            return CreatedAtAction(nameof(UpdateCard), new { id = updatedCard.Id }, updatedCard);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing the request."
            );
        }
    }

    [Authorize]
    [HttpDelete("{cardId}")]
    public async Task<IActionResult> RemoveList(string cardId)
    {
        try
        {
            await _cardsRepo.RemoveAsync(cardId);
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing the request."
            );
        }
    }
}
