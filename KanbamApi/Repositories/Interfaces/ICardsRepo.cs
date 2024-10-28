using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface ICardsRepo
{
    Task<List<Card>> GetById(string listId);
    Task<List<Card>> GetByListId(string cardId);
    Task<Card> Create(Card newCard);
    Task<bool> Patch(string id, DtoCardUpdate dtoCardUpdate);
    Task<bool> RemoveById(string id);
    Task<bool> RemoveByListId(string listId);
}
