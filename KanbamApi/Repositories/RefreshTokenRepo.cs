using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Util;
using MongoDB.Driver;

namespace KanbamApi.Repositories;

public class RefreshTokenRepo : IRefreshTokenRepo
{
    private readonly IKanbamDbContext _kanbamDbContext;

    public RefreshTokenRepo(IKanbamDbContext kanbamDbContext) => _kanbamDbContext = kanbamDbContext;

    public async Task<Result<RefreshToken>> GetRefreshTokensByToken(Guid token)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);

        var result = await _kanbamDbContext
            .RefreshTokensCollection.Find(filter)
            .FirstOrDefaultAsync();

        if (result == null)
        {
            var err = new Error(404, "Token not found");
            return Result<RefreshToken>.Failure(err);
        }

        return Result<RefreshToken>.Success(result);
    }

    public async Task<Result<bool>> SaveRefreshToken(RefreshToken refreshToken)
    {
        await _kanbamDbContext.RefreshTokensCollection.InsertOneAsync(refreshToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> Update_RefreshToken_ById(string id, Guid newToken)
    {
        var updateDefinitionBuilder = Builders<RefreshToken>.Update;
        var updateDefinition = new List<UpdateDefinition<RefreshToken>>();

        if (!string.IsNullOrEmpty(newToken.ToString()))
        {
            updateDefinition.Add(updateDefinitionBuilder.Set(rt => rt.Token, newToken));
        }

        if (updateDefinition.Count == 0)
        {
            var err1 = new Error(400, "Nothing to update.");
            return Result<bool>.Failure(err1);
        }

        var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Id, id);

        var result = await _kanbamDbContext.RefreshTokensCollection.UpdateOneAsync(
            filter,
            updateDefinitionBuilder.Combine(updateDefinition)
        );

        if (result.ModifiedCount > 0)
        {
            return Result<bool>.Success(true);
        }

        var err2 = new Error(400, "Nothing to update.");
        return Result<bool>.Failure(err2);
    }

    public async Task<Result<bool>> DeleteRefreshToken(Guid token)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
        var result = await _kanbamDbContext.RefreshTokensCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            var err = new Error(404, "Not found.");
            return Result<bool>.Failure(err);
        }

        return Result<bool>.Success(true);
    }
}
