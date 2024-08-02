using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class VisitorsRepo : IVisitorsRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public VisitorsRepo(IKanbamDbContext kanbamDbContext)
    {
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<Visitor>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<Visitor>.Filter.Eq(v => v.UserId, userId);
        return await _kanbamDbContext.VisitorsCollection.FindSync(filter).ToListAsync();
    }

    public async Task<Visitor> CreateAsync(Visitor visitor)
    {
        await _kanbamDbContext.VisitorsCollection.InsertOneAsync(visitor);
        return visitor;
    }

    public async Task UpdateAsync(Visitor visitor, string userId)
    {
        var filter = Builders<Visitor>.Filter.Eq(v => v.UserId, userId);
        await _kanbamDbContext.VisitorsCollection.ReplaceOneAsync(filter, visitor);
    }
}
