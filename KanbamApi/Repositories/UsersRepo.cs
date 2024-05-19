using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class UsersRepo : IUsersRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public UsersRepo(IKanbamDbContext kanbamDbContext)
    {
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<string> GetUserIdAsync(string? email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var res = await _kanbamDbContext.UsersCollection.Find(filter).FirstOrDefaultAsync();
        var userId = res.Id;

        return userId is not null ? userId : "";
    }

    public async Task<bool> CreateNewUserAsync(User newUser)
    {
        await _kanbamDbContext.UsersCollection.InsertOneAsync(newUser);

        var res = await GetUserIdAsync(newUser.Email);

        return res is null ? false : true;
    }
}
