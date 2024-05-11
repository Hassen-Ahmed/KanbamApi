using KanbamApi.Models;
using KanbamApi.Repo;
using MongoDB.Driver;

namespace KanbamApi.Services.AuthServices;

public class AuthService
{
    private readonly IMongoCollection<Auth> _authCollection;

    public AuthService(KanbamDbRepository kanbamDbRepository)
    {
        _authCollection = kanbamDbRepository.kanbamDatabase.GetCollection<Auth>(
            DotNetEnv.Env.GetString("AUTH_COLLECTION_NAME")
        );
    }

    public async Task<List<Auth>> GetAsync() => await _authCollection.Find(_ => true).ToListAsync();

    public async Task<Auth> CheckAuth(string? email)
    {
        Auth? result = await _authCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
        return result;
    }

    public async Task<bool> CreateAsync(Auth newAuth)
    {
        await _authCollection.InsertOneAsync(newAuth);

        var res = await CheckAuth(newAuth.Email);

        return res is null ? false : true;
    }
}
