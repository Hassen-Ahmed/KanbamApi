using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Hubs;
using KanbamApi.Models;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KanbamApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ICardsService _cardsService;
    private readonly IListsService _listsService;
    private readonly IHubContext<CardHub> _hubContext;
    private readonly ILogger<CardsController> _logger;
    private readonly IGeneralValidation _generalValidation;

    public CardsController(
        ICardsService cardsService,
        IListsService listsService,
        IHubContext<CardHub> hubContext,
        ILogger<CardsController> logger,
        IGeneralValidation generalValidation
    )
    {
        _cardsService = cardsService;
        _listsService = listsService;
        _hubContext = hubContext;
        _logger = logger;
        _generalValidation = generalValidation;
    }

    [HttpGet("{listId}/list")]
    public async Task<ActionResult<List<Card>>> GetByListId(string listId)
    {
        if (
            !_generalValidation.IsValidObjectId(listId)
            || !await _listsService.IsListIdExistByListIdAsync(listId)
        )
            return BadRequest("Invalid listId.");

        try
        {
            var cards = await _cardsService.GetByListIdAsync(listId);
            return cards is not null ? Ok(new { cards }) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpGet("{cardId}/card")]
    public async Task<ActionResult<Card>> GetByCardId(string cardId)
    {
        if (!_generalValidation.IsValidObjectId(cardId))
            return BadRequest("Invalid cardId.");

        try
        {
            var card = await _cardsService.GetByIdAsync(cardId);
            return card is not null ? Ok(new { card }) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewCard(DtoCardPost dtoNewCard)
    {
        if (!await _listsService.IsListIdExistByListIdAsync(dtoNewCard.ListId!))
            return NotFound();

        try
        {
            var createdCard = await _cardsService.CreateAsync(dtoNewCard);

            var groupId = dtoNewCard.ListId!;

            try
            {
                await _hubContext
                    .Clients.Group(groupId)
                    .SendAsync("ReceiveCardCreated", createdCard, $"{groupId}");
            }
            catch (Exception signalrEx)
            {
                _logger.LogError(
                    signalrEx,
                    $"Failed to notify group {groupId} of card creation.",
                    groupId
                );
            }

            return StatusCode(201, createdCard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpPost("{cardId}/comment")]
    public async Task<IActionResult> CreateComment(string cardId, [FromBody] Comment newComment)
    {
        if (!_generalValidation.IsValidObjectId(cardId))
            return BadRequest("Invalid id.");

        try
        {
            var commentId = await _cardsService.CreateCommentAsync(cardId, newComment);
            var cardById = await _cardsService.GetOneByIdAsync(cardId);

            if (commentId is null || cardById is null)
                return NotFound();

            newComment.Id = commentId;
            var groupId = cardById.ListId;

            try
            {
                await _hubContext
                    .Clients.Group(groupId)
                    .SendAsync("ReceiveCardCommentCreated", newComment, $"{groupId}");
            }
            catch (Exception signalrEx)
            {
                _logger.LogError(
                    signalrEx,
                    $"Failed to notify group {groupId} of card creation.",
                    groupId
                );
            }

            return Ok(new { newComment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCard(string id, DtoCardUpdate dtoCardUpdate)
    {
        if (!_generalValidation.IsValidObjectId(id))
            return BadRequest("Invalid id.");

        var updated = await _cardsService.PatchByIdAsync(id, dtoCardUpdate);

        if (!updated)
            return NotFound("Card not found or nothing to update.");

        var userId = User.FindFirst("userId")?.Value;
        dtoCardUpdate.Id = id;
        var groupId = dtoCardUpdate.ListId;

        try
        {
            await _hubContext
                .Clients.Group(groupId!)
                .SendAsync("ReceiveCardUpdate", dtoCardUpdate, userId, $"{groupId}");
        }
        catch (Exception signalrEx)
        {
            _logger.LogError(
                signalrEx,
                $"Failed to notify group {groupId} of card creation.",
                groupId
            );
        }

        return NoContent();
    }

    [HttpDelete("{cardId}/card/{listId}")]
    public async Task<IActionResult> RemoveById(string cardId, string listId)
    {
        if (!_generalValidation.IsValidObjectId(cardId))
            return BadRequest("Invalid cardId.");

        try
        {
            var result = await _cardsService.RemoveByIdAsync(cardId);

            if (!result)
                return NotFound();

            var groupId = listId!;

            try
            {
                await _hubContext
                    .Clients.Group(groupId)
                    .SendAsync("ReceiveCardDelete", cardId, $"{groupId}");
            }
            catch (Exception signalrEx)
            {
                _logger.LogError(
                    signalrEx,
                    $"Failed to notify group {groupId} of card creation.",
                    groupId
                );
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpDelete("{listId}/list")]
    public async Task<IActionResult> RemoveByListId(string listId)
    {
        if (!_generalValidation.IsValidObjectId(listId))
            return BadRequest("Invalid listId.");

        try
        {
            var result = await _cardsService.RemoveByListIdAsync(listId);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    [HttpDelete("{cardId}/{commentId}/comment")]
    public async Task<IActionResult> RemoveCommentById(string cardId, string commentId)
    {
        if (
            !_generalValidation.IsValidObjectId(cardId)
            || !_generalValidation.IsValidObjectId(commentId)
        )
            return BadRequest("Invalid cardId or commentId.");

        try
        {
            var result = await _cardsService.RemoveCommentByIdAsync(cardId, commentId);
            var cardById = await _cardsService.GetOneByIdAsync(cardId);
            if (!result || cardById is null)
                return NotFound();

            var groupId = cardById.ListId;

            await _hubContext
                .Clients.Group(groupId)
                .SendAsync("ReceiveCardCommentDelete", commentId, cardId, $"{groupId}");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            );
        }
    }
}
