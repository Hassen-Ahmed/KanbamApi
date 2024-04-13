using KanbamApi.Models;
using KanbamApi.Repo;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class UsersService
{
    private readonly IMongoCollection<User> _usersCollection;
    public UsersService(KanbamDbRepository kanbamDbRepository)
    {

         _usersCollection = kanbamDbRepository.kanbamDatabase.GetCollection<User>(DotNetEnv.Env.GetString("USERS_COLLECTION_NAME"));
    }

    public async Task<List<User>> GetAllUserAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<string> GetUserIdAsync(string? email) {
        var res =  await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
        var userId = res.Id;
        return userId is not null ? userId : "";
       
    }
    public async Task<bool> CreateNewUserAsync(User newUser)  {
        await _usersCollection.InsertOneAsync(newUser);

        var res = await GetUserIdAsync(newUser.Email);

        return res is null ? false : true;
    }

}