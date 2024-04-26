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

        public async Task CreateAsync(List newList) {
                await _listsCollection.InsertOneAsync(newList);
        }
        public async Task UpdateAsync(string id, List updatedList) =>
                await _listsCollection.ReplaceOneAsync(x => x.Id == id, updatedList);

        public async Task RemoveAsync(string id) =>
        await _listsCollection.DeleteOneAsync(x => x.Id == id);
}