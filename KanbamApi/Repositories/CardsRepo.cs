using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class CardsRepo : ICardsRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public CardsRepo(IKanbamDbContext kanbamDbContext)
    {
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<Card>> GetByListIdAsync(string listId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.ListId, listId);
        return await _kanbamDbContext.CardsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<List<Card>> GetByCardIdAsync(string cardId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, cardId);
        return await _kanbamDbContext.CardsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<Card> CreateAsync(Card newCard)
    {
        await _kanbamDbContext.CardsCollection.InsertOneAsync(newCard);
        return newCard;
    }

    public async Task UpdateAsync(string id, Card updatedCard)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        await _kanbamDbContext.CardsCollection.ReplaceOneAsync(filter, updatedCard);
    }

    public async Task RemoveAsync(string id)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        await _kanbamDbContext.CardsCollection.DeleteOneAsync(filter);
    }

    public async Task RemoveManyByListIdAsync(string listId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.ListId, listId);
        await _kanbamDbContext.CardsCollection.DeleteManyAsync(filter);
    }
}
