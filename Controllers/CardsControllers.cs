using KanbamApi.Models;
using KanbamApi.Services;
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
    public async Task<IActionResult> Post(Card newCard)
    {
        await _cardsService.CreateAsync(newCard);
        return CreatedAtAction(null, new { id = newCard.Id }, newCard);
    }

}