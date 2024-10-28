using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class ListsRepo : IListsRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public ListsRepo(IKanbamDbContext kanbamDbContext) => _kanbamDbContext = kanbamDbContext;

    public async Task<List<List>> GetAll()
    {
        var filter = Builders<List>.Filter.Empty;
        return await _kanbamDbContext.ListsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<List<List>> GetAllByBoardId(string boardId)
    {
        var filter = Builders<List>.Filter.Eq(l => l.BoardId, boardId);
        return await _kanbamDbContext.ListsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<List> Create(List newList)
    {
        await _kanbamDbContext.ListsCollection.InsertOneAsync(newList);
        return newList;
    }

    public async Task<bool> Patch(string id, DtoListsUpdate dtoListsUpdate)
    {
        var updateDefinitionBuilder = Builders<List>.Update;
        var updateDefinition = new List<UpdateDefinition<List>>();

        if (!string.IsNullOrEmpty(dtoListsUpdate.Title))
        {
            updateDefinition.Add(updateDefinitionBuilder.Set(b => b.Title, dtoListsUpdate.Title));
        }
        if (!string.IsNullOrEmpty(dtoListsUpdate.BoardId))
        {
            updateDefinition.Add(
                updateDefinitionBuilder.Set(b => b.BoardId, dtoListsUpdate.BoardId)
            );
        }
        if (dtoListsUpdate.IndexNumber is not null)
        {
            updateDefinition.Add(
                updateDefinitionBuilder.Set(b => b.IndexNumber, dtoListsUpdate.IndexNumber)
            );
        }
        if (updateDefinition.Count == 0)
        {
            return false;
        }

        var filter = Builders<List>.Filter.Eq(b => b.Id, id);

        var result = await _kanbamDbContext.ListsCollection.UpdateOneAsync(
            filter,
            updateDefinitionBuilder.Combine(updateDefinition)
        );

        return result.ModifiedCount > 0;
    }

    public async Task<bool> Remove(string id)
    {
        var filter = Builders<List>.Filter.Eq(l => l.Id, id);
        var res = await _kanbamDbContext.ListsCollection.DeleteOneAsync(filter);
        return res.DeletedCount > 0;
    }
}
