using KanbamApi.Models;
using KanbamApi.Util;

namespace KanbamApi.Services.Interfaces;

public interface IRefreshTokenService
{
    Task<Result<RefreshToken>> GetRefreshTokensByTokenAsync(Guid token);
    Task<Result<bool>> SaveRefreshTokenAsync(
        string userId,
        Guid refreshToken,
        DateTime expirationDate
    );

    Task<Result<bool>> UpdateRefreshTokenByIdAsync(string id, Guid newToken);
    Task<Result<bool>> DeleteRefreshTokenAsync(Guid token);
    Task<Result<bool>> DeleteRefreshTokenByUserIdAsync(string? userId);
}
