using KanbamApi.Models;
using KanbamApi.Repositories;
using KanbamApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ListsController : ControllerBase
{
    private readonly ListsRepo _listsRepo;
    private readonly CardsRepo _cardsRepo;
    private readonly UsersRepo _usersService;

    public ListsController(ListsRepo listsRepo, CardsRepo cardsRepo, UsersRepo usersRepo)
    {
        _listsRepo = listsRepo;
        _cardsRepo = cardsRepo;
        _usersService = usersRepo;
    }

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetListsWithCardsByUserId()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var res = await _listsRepo.GetListsWithCardsByUserId(userId!);
            return Ok(res);
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
    public async Task<IActionResult> CreateList(List newList)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            newList.UserId = userId;
            newList.Cards = [];
            await _listsRepo.CreateAsync(newList);
            return CreatedAtAction(nameof(CreateList), new { id = newList.Id }, newList);
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
    public async Task<IActionResult> UpdateList(string id, List updatedList)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            updatedList.Id = id;
            updatedList.UserId = userId;

            await _listsRepo.UpdateAsync(id, updatedList);
            return CreatedAtAction(nameof(UpdateList), new { id = updatedList.Id }, updatedList);
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
    [HttpDelete("{listId}")]
    public async Task<IActionResult> RemoveList(string listId)
    {
        try
        {
            await _listsRepo.RemoveAsync(listId);
            await _cardsRepo.RemoveManyByListIdAsync(listId);

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
