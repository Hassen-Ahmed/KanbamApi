
using KanbamApi.Models;
using KanbamApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ListsController : ControllerBase {
    private readonly ListsService _listsService;
    private readonly CardsService _cardsService;
    public ListsController(ListsService listsService,CardsService cardsService) {
        _listsService = listsService;
        _cardsService = cardsService;
    }

   [HttpGet]
    public async Task<List<List>> Get() =>
        await _listsService.GetAsync();

    [HttpPost]
    public async Task<IActionResult> Post(List newList) {
        await _listsService.CreateAsync(newList);
        return CreatedAtAction(nameof(Get), new { id = newList.Id }, newList);
    } 

    [HttpDelete]
    public async Task<IActionResult> Delete(string listId) {

        await _listsService.RemoveAsync(listId);
        await _cardsService.RemoveManyByListIdAsync(listId);
        
        return NoContent();
    }
}