using KanbamApi.Models;
using KanbamApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase {
private readonly CardsService _cardsService;
    public CardsController(CardsService cardsService) =>
        _cardsService = cardsService;
    
    [Authorize]
    [HttpGet("{listId}/list")]
    public async Task<ActionResult<List<Card>>> GetByListId(string listId) {

         try
            {
                return await _cardsService.GetByListIdAsync(listId);
            }

        catch (Exception)

            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
    }

    [Authorize]
    [HttpGet("{cardId}/card")]
    public async Task<ActionResult<List<Card>>> GetByCardId(string cardId) {
        
        try
            {
                return await _cardsService.GetByCardIdAsync(cardId);
            }
        catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateNewCard(Card newCard) {
         try
            {
                await _cardsService.CreateAsync(newCard);
                return CreatedAtAction(nameof(CreateNewCard), new { id = newCard.Id }, newCard);

            }
        catch (Exception )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCard(string id ,Card updatedCard) {

         try
            {
                await _cardsService.UpdateAsync(id, updatedCard);
                return CreatedAtAction(nameof(UpdateCard), new { id = updatedCard.Id }, updatedCard);

            }
        catch (Exception )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
    }

    [Authorize]
    [HttpDelete("{cardId}")]
     public async Task<IActionResult> RemoveList(string cardId) {
         try
            {
                await _cardsService.RemoveAsync(cardId);
                return NoContent();

            }
        catch (Exception )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
    }
}