using KanbamApi.Models;
using KanbamApi.Repo;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class CardsService
{
    private readonly IMongoCollection<Card> _cardsCollection;

    public CardsService(KanbamDbRepository kanbamDbRepository)
    {
        _cardsCollection = kanbamDbRepository.kanbamDatabase.GetCollection<Card>(
            DotNetEnv.Env.GetString("CARDS_COLLECTION_NAME")
        );
    }

    public async Task<List<Card>> GetByListIdAsync(string listId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.ListId, listId);
        return await _cardsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<List<Card>> GetByCardIdAsync(string cardId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, cardId);
        return await _cardsCollection.FindSync(filter).ToListAsync();
    }

    public async Task CreateAsync(Card newCard)
    {
        await _cardsCollection.InsertOneAsync(newCard);
    }

    public async Task UpdateAsync(string id, Card updatedCard)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        await _cardsCollection.ReplaceOneAsync(filter, updatedCard);
    }

    public async Task RemoveAsync(string id)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        await _cardsCollection.DeleteOneAsync(filter);
    }

    public async Task RemoveManyByListIdAsync(string listId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.ListId, listId);
        await _cardsCollection.DeleteManyAsync(filter);
    }
}
