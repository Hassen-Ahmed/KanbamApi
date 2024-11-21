using System.Security.Claims;

namespace KanbamApi.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);

        Guid GenerateRefreshToken();

        ClaimsPrincipal? ValidatePrincipal(string token);

        List<Claim> GenerateClaims(string username, string userId);

        // string HashRefreshToken(string token) ;

        ClaimsPrincipal DecodeExpiredToken(string expiredToken);
    }
}