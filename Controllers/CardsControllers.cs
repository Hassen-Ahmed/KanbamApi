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

   [HttpGet]
    public async Task<List<Card>> Get() =>
        await _cardsService.GetAsync();
    

    [HttpPost]
    public async Task<IActionResult> Post(Card newCard)
    {
        await _cardsService.CreateAsync(newCard);
        return CreatedAtAction(nameof(Get), new { id = newCard.Id }, newCard);
    }

}