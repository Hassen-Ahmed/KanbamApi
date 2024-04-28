using KanbamApi.Models;
using KanbamApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase {
private readonly CardsService _cardsService;
    public CardsController(CardsService cardsService) =>
        _cardsService = cardsService;

    [HttpGet("{listId}/list")]
    public async Task<List<Card>> GetByListId(string listId) =>
        await _cardsService.GetByListIdAsync(listId);
     
    [HttpGet("{cardId}/card")]
    public async Task<List<Card>> GetByCardId(string cardId) =>
        await _cardsService.GetByCardIdAsync(cardId);
    

    [HttpPost]
    public async Task<IActionResult> CreateNewCard(Card newCard)
    {
        await _cardsService.CreateAsync(newCard);
        return CreatedAtAction(nameof(CreateNewCard), new { id = newCard.Id }, newCard);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCard(string id ,Card updatedCard) {
            await _cardsService.UpdateAsync(id, updatedCard);
             return CreatedAtAction(nameof(UpdateCard), new { id = updatedCard.Id }, updatedCard);
    }

    [HttpDelete("{cardId}")]
     public async Task<IActionResult> RemoveList(string cardId) {

        await _cardsService.RemoveAsync(cardId);
        
        return NoContent();
    }


}