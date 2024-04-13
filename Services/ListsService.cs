using KanbamApi.Models;
using KanbamApi.Repo;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class ListsService
{
    private readonly IMongoCollection<List> _listsCollection;
    public ListsService( KanbamDbRepository kanbamDbRepository)
    {
            _listsCollection = kanbamDbRepository.kanbamDatabase.GetCollection<List>(DotNetEnv.Env.GetString("LISTS_COLLECTION_NAME"));
    }

   public async Task<List<List>> GetAsync() =>
        await _listsCollection.Find(_ => true).ToListAsync();

}