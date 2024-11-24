using KanbamApi.Models;
using KanbamApi.Util;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IRefreshTokenRepo
    {
        Task<Result<RefreshToken>> GetRefreshTokensByToken(Guid token);
        Task<Result<bool>> SaveRefreshToken(RefreshToken refreshToken);
        Task<Result<bool>> Update_RefreshToken_ById(string id, Guid newToken);
        Task<Result<bool>> DeleteRefreshToken(Guid token);
        Task<Result<bool>> DeleteRefreshTokenByUserId(string userId);
    }
}
