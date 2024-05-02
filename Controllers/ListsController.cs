
using KanbamApi.Models;
using KanbamApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ListsController : ControllerBase {
    private readonly ListsService _listsService;
    private readonly CardsService _cardsService;
    private readonly UsersService _usersService;
    public ListsController(ListsService listsService,CardsService cardsService,UsersService usersService ) {
        _listsService = listsService;
        _cardsService = cardsService;
        _usersService = usersService;
    }
   


    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetListsWithCardsByUserId() {
            
            try
                {
                var userId = User.FindFirst("userId")?.Value;
                var res = await _listsService.GetListsWithCardsByUserId(userId!);
                return Ok(res);
            }
            catch (System.Exception)
            {
                throw;
            }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateList(List newList) {
        try
        {    
            var userId = User.FindFirst("userId")?.Value;
            newList.UserId = userId;
            newList.Cards = [];
            await _listsService.CreateAsync(newList);
            return CreatedAtAction(nameof(CreateList), new { id = newList.Id }, newList);
          }
        catch (System.Exception)
        {
            throw;
        }
    } 
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateList(string id ,List updatedList) {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            updatedList.Id = id;
            updatedList.UserId = userId;

            await _listsService.UpdateAsync(id, updatedList);
            return CreatedAtAction(nameof(UpdateList), new { id = updatedList.Id }, updatedList);
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }


    [Authorize]
    [HttpDelete("{listId}")]
    public async Task<IActionResult> RemoveList(string listId) {
        try
        {
            await _listsService.RemoveAsync(listId);
            await _cardsService.RemoveManyByListIdAsync(listId);
            
            return NoContent(); 
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}