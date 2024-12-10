using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface ICardsRepo
{
    Task<List<Card>> GetById(string listId);
    Task<List<Card>> GetByListId(string cardId);
    Task<Card> Create(Card newCard);
    Task<string?> CreateComment(string cardId, Comment newComment);
    Task<bool> Patch(string id, DtoCardUpdate dtoCardUpdate);
    Task<bool> RemoveById(string id);
    Task<bool> RemoveByListId(string listId);
    Task<bool> RemoveCommentById(string cardId, string commentId);
}
