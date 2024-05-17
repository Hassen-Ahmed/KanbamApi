using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class UsersRepo : IUsersRepo
{
    private readonly IMongoCollection<User> _usersCollection;

    public UsersRepo(KanbamDbRepository kanbamDbRepository)
    {
        _usersCollection = kanbamDbRepository.kanbamDatabase.GetCollection<User>(
            DotNetEnv.Env.GetString("USERS_COLLECTION_NAME")
        );
    }

    public async Task<string> GetUserIdAsync(string? email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var res = await _usersCollection.Find(filter).FirstOrDefaultAsync();
        var userId = res.Id;

        return userId is not null ? userId : "";
    }

    public async Task<bool> CreateNewUserAsync(User newUser)
    {
        await _usersCollection.InsertOneAsync(newUser);

        var res = await GetUserIdAsync(newUser.Email);

        return res is null ? false : true;
    }
}
