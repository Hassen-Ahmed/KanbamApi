using KanbamApi.Models;
using KanbamApi.Repo;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class ListsService
{
        private readonly IMongoCollection<List> _listsCollection;
         private readonly IMongoCollection<Card> _cardsCollection;
        private readonly IMongoCollection<User> _usersCollection;
     
        public ListsService( KanbamDbRepository kanbamDbRepository)
        {
                _listsCollection = kanbamDbRepository.kanbamDatabase.GetCollection<List>(DotNetEnv.Env.GetString("LISTS_COLLECTION_NAME"));
                _cardsCollection = kanbamDbRepository.kanbamDatabase.GetCollection<Card>(DotNetEnv.Env.GetString("CARDS_COLLECTION_NAME"));
                _usersCollection = kanbamDbRepository.kanbamDatabase.GetCollection<User>(DotNetEnv.Env.GetString("USERS_COLLECTION_NAME"));

        }

        public async Task<List<List>> GetAsync() =>
                await _listsCollection.Find(_ => true).ToListAsync();

        public async Task<List<List>> GetListsWithCardsByUserId(string userId)
        {
                if (userId is null)
                        return new List<List>();

                var pipeline = new[]
                {
                        PipelineStageDefinitionBuilder.Match<List>(x => x.UserId == userId),
                        PipelineStageDefinitionBuilder.Lookup<List, Card, List>(
                                _cardsCollection,
                                x => x.Id,
                                c => c.ListId,
                                listWithCards => listWithCards.Cards
                        )
                };      

                var cursor = await _listsCollection.AggregateAsync<List>(pipeline);
                return await cursor.ToListAsync();
        }
        

       
      
        public async Task CreateAsync(List newList) {
                await _listsCollection.InsertOneAsync(newList);
        }
        
        public async Task UpdateAsync(string id, List updatedList) =>
                await _listsCollection.ReplaceOneAsync(x => x.Id == id, updatedList);

        public async Task RemoveAsync(string id) =>
        await _listsCollection.DeleteOneAsync(x => x.Id == id);

}