using KanbamApi.Models;
using KanbamApi.Util;

namespace KanbamApi.Services.Interfaces;

public interface IRefreshTokenService
{
    Task<Result<RefreshToken>> GetRefreshTokensByTokenAsync(Guid token);
    Task<Result<bool>> SaveRefreshTokenAsync(string userId, Guid refreshToken);

    Task<Result<bool>> Update_RefreshToken_ById_Async(string id, Guid newToken);
    Task<Result<bool>> DeleteRefreshTokenAsync(Guid token);
}
