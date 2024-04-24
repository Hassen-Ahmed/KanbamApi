using KanbamApi.Models;
using KanbamApi.Repo;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class CardsService
{
    private readonly IMongoCollection<Card> _cardsCollection;
    public CardsService( KanbamDbRepository kanbamDbRepository)
    {
         _cardsCollection = kanbamDbRepository.kanbamDatabase.GetCollection<Card>(DotNetEnv.Env.GetString("CARDS_COLLECTION_NAME"));
    }

   //   public async Task<List<Card>> GetAsync() =>
   //      await _cardsCollection.FindSync(_ => true).ToListAsync();

     public async Task<List<Card>> GetByListIdAsync(string listId) =>
        await _cardsCollection.FindSync(x => x.ListId == listId).ToListAsync();

     public async Task<List<Card>> GetByCardIdAsync(string listId) =>
        await _cardsCollection.FindSync(x => x.ListId == listId).ToListAsync();

     public async Task CreateAsync(Card newCard) {
         await _cardsCollection.InsertOneAsync(newCard);
     }

     public async Task UpdateAsync(string id, Card updatedCard) =>
        await _cardsCollection.ReplaceOneAsync(x => x.Id == id, updatedCard);

     public async Task RemoveAsync(string id) =>
        await _cardsCollection.DeleteOneAsync(x => x.Id == id);

     public async Task RemoveManyByListIdAsync(string listId) =>
        await _cardsCollection.DeleteManyAsync(x => x.ListId == listId);
}