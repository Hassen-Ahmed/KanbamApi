using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class AuthRepo : IAuthRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public AuthRepo(IKanbamDbContext kanbamDbContext)
    {
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<Auth>> GetAsync() =>
        await _kanbamDbContext.AuthCollection.Find(_ => true).ToListAsync();

    public async Task<Auth> IsEmailExists(string? email)
    {
        Auth result = await _kanbamDbContext
            .AuthCollection.Find(x => x.Email == email)
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<bool> CreateAsync(Auth newAuth)
    {
        await _kanbamDbContext.AuthCollection.InsertOneAsync(newAuth);

        var res = await IsEmailExists(newAuth.Email);

        return res is null ? false : true;
    }
}
