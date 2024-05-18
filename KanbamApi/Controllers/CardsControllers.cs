using KanbamApi.Dtos;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ICardsRepo _cardsRepo;

    public CardsController(ICardsRepo cardsRepo) => _cardsRepo = cardsRepo;

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
    public async Task<IActionResult> CreateNewCard(DtoCardPost newCard)
    {
        try
        {
            Card card =
                new()
                {
                    ListId = newCard.ListId,
                    Title = newCard.Title,
                    IndexNumber = newCard.IndexNumber,
                };

            await _cardsRepo.CreateAsync(card);
            return CreatedAtAction(nameof(CreateNewCard), new { }, newCard);
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
