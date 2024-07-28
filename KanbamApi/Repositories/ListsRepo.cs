using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class ListsRepo : IListsRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public ListsRepo(IKanbamDbContext kanbamDbContext)
    {
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<List>> GetListsWithCardsByUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
        }

        var filter = Builders<List>.Filter.Eq(x => x.UserId, userId);

        var lookupStage = PipelineStageDefinitionBuilder.Lookup<List, Card, List>(
            _kanbamDbContext.CardsCollection,
            x => x.Id,
            c => c.ListId,
            listWithCards => listWithCards.Cards
        );

        var pipeline = new[] { PipelineStageDefinitionBuilder.Match(filter), lookupStage };

        var cursor = await _kanbamDbContext.ListsCollection.AggregateAsync<List>(pipeline);
        return await cursor.ToListAsync();
    }

    public async Task<List> CreateAsync(List newList)
    {
        await _kanbamDbContext.ListsCollection.InsertOneAsync(newList);
        return newList;
    }

    public async Task UpdateAsync(string id, List updatedList)
    {
        var filter = Builders<List>.Filter.Eq(l => l.Id, id);
        await _kanbamDbContext.ListsCollection.ReplaceOneAsync(filter, updatedList);
    }

    public async Task RemoveAsync(string id)
    {
        var filter = Builders<List>.Filter.Eq(l => l.Id, id);
        await _kanbamDbContext.ListsCollection.DeleteOneAsync(filter);
    }
}
