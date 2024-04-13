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

   public async Task<List<Card>> GetAsync() =>
        await _cardsCollection.Find(_ => true).ToListAsync();

   public async Task CreateAsync(Card newCard) {

        await _cardsCollection.InsertOneAsync(newCard);

   }
}