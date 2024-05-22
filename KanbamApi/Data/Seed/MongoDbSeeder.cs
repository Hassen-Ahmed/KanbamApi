using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using MongoDB.Driver;

namespace KanbamApi.Data.Seed;

public class MongoDbSeeder : IMongoDbSeeder
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public MongoDbSeeder(IKanbamDbContext kanbamDbContext)
    {
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task SeedAsync()
    {
        _kanbamDbContext.CardsCollection.DeleteMany(FilterDefinition<Card>.Empty);
        _kanbamDbContext.ListsCollection.DeleteMany(FilterDefinition<List>.Empty);
        _kanbamDbContext.AuthCollection.DeleteMany(FilterDefinition<Auth>.Empty);
        _kanbamDbContext.UsersCollection.DeleteMany(FilterDefinition<User>.Empty);

        DataGenerator dataGenerator = new();
        var auth = dataGenerator.GenerateAuth(2);
        var users = dataGenerator.GenerateUsers(2);
        var lists = dataGenerator.GenerateLists(2);
        var cards = dataGenerator.GenerateCards(2);

        await _kanbamDbContext.AuthCollection.InsertManyAsync(auth);
        await _kanbamDbContext.UsersCollection.InsertManyAsync(users);
        await _kanbamDbContext.ListsCollection.InsertManyAsync(lists);
        await _kanbamDbContext.CardsCollection.InsertManyAsync(cards);
    }
}
