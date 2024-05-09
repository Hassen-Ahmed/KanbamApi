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

        public async Task<List<List>> GetListsWithCardsByUserId(string userId)
        {
                 if (string.IsNullOrEmpty(userId))
                {
                        throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
                }

                try
                {
                        var filter = Builders<List>.Filter.Eq(x => x.UserId, userId);

                        var lookupStage = PipelineStageDefinitionBuilder.Lookup<List, Card, List>(
                        _cardsCollection,
                        x => x.Id,
                        c => c.ListId,
                        listWithCards => listWithCards.Cards
                        );

                        var pipeline = new[] { PipelineStageDefinitionBuilder.Match(filter), lookupStage };

                        var cursor = await _listsCollection.AggregateAsync<List>(pipeline);
                        return await cursor.ToListAsync();
                                
                }

                catch (Exception ex)
                {
                        throw new ApplicationException("An error occurred while retrieving lists with cards.", ex);
                }
        }
        
      
        public async Task CreateAsync(List newList) {
                await _listsCollection.InsertOneAsync(newList);
        }
        
        public async Task UpdateAsync(string id, List updatedList)  {
                var filter = Builders<List>.Filter.Eq("_id", id);
                await _listsCollection.ReplaceOneAsync(filter, updatedList);
        }

        public async Task RemoveAsync(string id) {

                var filter = Builders<List>.Filter.Eq("_id", id);
                await _listsCollection.DeleteOneAsync(filter);

        }
}