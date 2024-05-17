using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface ICardsRepo
{
    Task<List<Card>> GetByListIdAsync(string listId);
    Task<List<Card>> GetByCardIdAsync(string cardId);
    Task CreateAsync(Card newCard);
    Task UpdateAsync(string id, Card updatedCard);
    Task RemoveAsync(string id);
    Task RemoveManyByListIdAsync(string listId);
}
