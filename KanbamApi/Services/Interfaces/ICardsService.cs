using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces;

public interface ICardsService
{
    Task<List<Card>> GetByIdAsync(string cardId);
    Task<List<Card>> GetByListIdAsync(string listId);
    Task<Card> CreateAsync(DtoCardPost dtoNewCard);
    Task<bool> PatchByIdAsync(string id, DtoCardUpdate dtoCardUpdate);
    Task<bool> RemoveByIdAsync(string id);
    Task<bool> RemoveByListIdAsync(string listId);
}
