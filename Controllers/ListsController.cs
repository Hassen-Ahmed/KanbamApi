
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

        //   _mapper = new Mapper(new MapperConfiguration(
        //     cfg => cfg.CreateMap<UserRegistration, UserComplete>()
        // ));
    }

    [HttpGet]
    public async Task<List<List>> Get() =>
        await _listsService.GetAsync();

    [HttpPost]
    public async Task<IActionResult> CreateList(List newList) {
        await _listsService.CreateAsync(newList);
        return CreatedAtAction(nameof(CreateList), new { id = newList.Id }, newList);
    } 
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateList(string id ,List updatedList) {
            await _listsService.UpdateAsync(id, updatedList);
            return CreatedAtAction(nameof(UpdateList), new { id = updatedList.Id }, updatedList);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string listId) {

        await _listsService.RemoveAsync(listId);
        await _cardsService.RemoveManyByListIdAsync(listId);
        
        return NoContent();
    }
}