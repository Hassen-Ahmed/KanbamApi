using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class UsersRepo : IUsersRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public UsersRepo(IKanbamDbContext kanbamDbContext) => _kanbamDbContext = kanbamDbContext;

    public async Task<List<User>> GetAll()
    {
        var filter = Builders<User>.Filter.Empty;
        return await _kanbamDbContext.UsersCollection.FindSync(filter).ToListAsync();
    }

    public async Task<List<User>> GetById(string id)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        return await _kanbamDbContext.UsersCollection.FindSync(filter).ToListAsync();
    }

    public async Task<string?> GetUserIdByEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var user = await _kanbamDbContext.UsersCollection.Find(filter).FirstOrDefaultAsync();

        return user?.Id;
    }

    public async Task<string> Create(User newUser)
    {
        await _kanbamDbContext.UsersCollection.InsertOneAsync(newUser);
        return newUser.Id;
    }

    public async Task<bool> Patch(string id, User newUser)
    {
        var updateDefinitionBuilder = Builders<User>.Update;
        var updateDefinition = new List<UpdateDefinition<User>>();

        if (!string.IsNullOrEmpty(newUser.Email))
        {
            updateDefinition.Add(updateDefinitionBuilder.Set(u => u.Email, newUser.Email));
        }

        if (!string.IsNullOrEmpty(newUser.UserName))
        {
            updateDefinition.Add(updateDefinitionBuilder.Set(u => u.UserName, newUser.UserName));
        }

        if (updateDefinition.Count == 0)
        {
            return false;
        }

        var filter = Builders<User>.Filter.Eq(u => u.Id, id);

        var result = await _kanbamDbContext.UsersCollection.UpdateOneAsync(
            filter,
            updateDefinitionBuilder.Combine(updateDefinition)
        );

        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveById(string id)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var res = await _kanbamDbContext.UsersCollection.DeleteOneAsync(filter);

        return res.DeletedCount > 0;
    }
}
