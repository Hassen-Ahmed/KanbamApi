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
        _kanbamDbContext.WorkspacesCollection.DeleteMany(FilterDefinition<Workspace>.Empty);
        _kanbamDbContext.WorkspaceMembersCollection.DeleteMany(
            FilterDefinition<WorkspaceMember>.Empty
        );

        DataGenerator dataGenerator = new();
        var lists = dataGenerator.GenerateLists(2);
        var cards = dataGenerator.GenerateCards(2);
        var workspaces = dataGenerator.GenerateWorkspaces(2);
        var workspacesMembers = dataGenerator.GenerateWorkspaceMembers(2);

        await _kanbamDbContext.ListsCollection.InsertManyAsync(lists);
        await _kanbamDbContext.CardsCollection.InsertManyAsync(cards);
        await _kanbamDbContext.WorkspacesCollection.InsertManyAsync(workspaces);
        await _kanbamDbContext.WorkspaceMembersCollection.InsertManyAsync(workspacesMembers);
    }
}
