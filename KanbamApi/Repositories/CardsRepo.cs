using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class CardsRepo : ICardsRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public CardsRepo(IKanbamDbContext kanbamDbContext)
    {
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<Card>> GetById(string id)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        return await _kanbamDbContext.CardsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<List<Card>> GetByListId(string listId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.ListId, listId);
        return await _kanbamDbContext.CardsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<Card> Create(Card newCard)
    {
        await _kanbamDbContext.CardsCollection.InsertOneAsync(newCard);
        return newCard;
    }

    public async Task<string?> CreateComment(string cardId, Comment newComment)
    {
        newComment.Id = ObjectId.GenerateNewId().ToString();
        newComment.CreatedAt = DateTime.UtcNow;

        var filter = Builders<Card>.Filter.Eq(c => c.Id, cardId);

        var update = Builders<Card>.Update.Push(c => c.Comments, newComment);

        var result = await _kanbamDbContext.CardsCollection.UpdateOneAsync(filter, update);

        if (result.ModifiedCount > 0)
            return newComment.Id;

        return null;
    }

    public async Task<bool> Patch(string id, DtoCardUpdate dtoCardUpdate)
    {
        var updateDefinitionBuilder = Builders<Card>.Update;
        var updateDefinitions = new List<UpdateDefinition<Card>>();

        foreach (var property in typeof(DtoCardUpdate).GetProperties())
        {
            var value = property.GetValue(dtoCardUpdate);

            if (value != null)
            {
                var update = updateDefinitionBuilder.Set(property.Name, value);
                updateDefinitions.Add(update);
            }
        }

        if (!updateDefinitions.Any())
        {
            return false;
        }

        var combinedUpdate = updateDefinitionBuilder.Combine(updateDefinitions);
        var result = await _kanbamDbContext.CardsCollection.UpdateOneAsync(
            card => card.Id == id,
            combinedUpdate
        );

        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveById(string id)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, id);
        var res = await _kanbamDbContext.CardsCollection.DeleteOneAsync(filter);
        return res.DeletedCount > 0;
    }

    public async Task<bool> RemoveByListId(string listId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.ListId, listId);
        var res = await _kanbamDbContext.CardsCollection.DeleteManyAsync(filter);
        return res.DeletedCount > 0;
    }

    public async Task<bool> RemoveCommentById(string cardId, string commentId)
    {
        var filter = Builders<Card>.Filter.Eq(c => c.Id, cardId);
        var update = Builders<Card>.Update.PullFilter(
            c => c.Comments,
            comment => comment.Id == commentId
        );

        var result = await _kanbamDbContext.CardsCollection.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
    }
}
