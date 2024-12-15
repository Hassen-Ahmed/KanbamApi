using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class CardsService : ICardsService
{
    private readonly IKanbamDbContext _kanbamDbContext;
    private readonly ICardsRepo _cardsRepo;

    public CardsService(ICardsRepo cardsRepo, IKanbamDbContext kanbamDbContext)
    {
        _cardsRepo = cardsRepo;
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<Card>> GetByIdAsync(string cardId) => await _cardsRepo.GetById(cardId);

    public async Task<List<Card>> GetByListIdAsync(string listId) =>
        await _cardsRepo.GetByListId(listId);

    public async Task<Card> GetOneByIdAsync(string cardId) => await _cardsRepo.GetOneById(cardId);

    public async Task<Card> CreateAsync(DtoCardPost dtoNewCard)
    {
        Card newList =
            new()
            {
                ListId = dtoNewCard.ListId!,
                Title = dtoNewCard.Title!,
                StartDate = dtoNewCard.StartDate,
                IndexNumber = dtoNewCard.IndexNumber,
            };

        var resCard = await _cardsRepo.Create(newList);
        return resCard;
    }

    public async Task<string?> CreateCommentAsync(string cardId, Comment newComment)
    {
        return await _cardsRepo.CreateComment(cardId, newComment);
    }

    public async Task<bool> PatchByIdAsync(string id, DtoCardUpdate dtoCardUpdate) =>
        await _cardsRepo.Patch(id, dtoCardUpdate);

    public async Task<bool> RemoveByIdAsync(string id) => await _cardsRepo.RemoveById(id);

    public async Task<bool> RemoveByListIdAsync(string listId) =>
        await _cardsRepo.RemoveByListId(listId);

    public async Task<bool> RemoveCommentByIdAsync(string cardId, string commentId) =>
        await _cardsRepo.RemoveCommentById(cardId, commentId);
}
