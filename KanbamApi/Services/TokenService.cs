using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KanbamApi.Core;
using KanbamApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace KanbamApi.Services;

public class TokenService : ITokenService
{
    private readonly string? SecretKey = DotNetEnv.Env.GetString("TOKEN_KEY");

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        SymmetricSecurityKey tokenKey = new(Encoding.UTF8.GetBytes($"{SecretKey}"));

        SigningCredentials credentials = new(tokenKey, SecurityAlgorithms.HmacSha256Signature);

        SecurityTokenDescriptor descriptor =
            new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes((int)TokenExpiration.AccessTokenMinute),
                NotBefore = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1)),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = credentials,
                Issuer = DotNetEnv.Env.GetString("VALID_ISSUER"),
                Audience = DotNetEnv.Env.GetString("VALID_AUDIENCE"),
            };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(descriptor);

        return tokenHandler.WriteToken(token);
    }

    public Guid GenerateRefreshToken()
    {
        return Guid.NewGuid();
    }

    public List<Claim> GenerateClaims(string username, string userId)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, username),
            new Claim("userId", userId),
            new Claim(ClaimTypes.Role, "User"),
        ];

        return claims;
    }

    // public byte[] GenerateTokenRefreshHash(string tokenRefresh, byte[] tokenRefreshSalt)
    // {
    //     if (string.IsNullOrEmpty(tokenRefresh))
    //     {
    //         throw new ArgumentException(
    //             "Token refresh cannot be null or empty.",
    //             nameof(tokenRefresh)
    //         );
    //     }

    //     string passwordSaltPlusTokenRefreshKey =
    //         DotNetEnv.Env.GetString("TOKEN_REFRESH_KEY") + Convert.ToBase64String(tokenRefreshSalt);

    //     if (string.IsNullOrWhiteSpace(passwordSaltPlusTokenRefreshKey))
    //     {
    //         throw new InvalidOperationException(
    //             "TOKEN_REFRESH_KEY is not set in the environment variables."
    //         );
    //     }

    //     byte[] tokenRefreshHash = KeyDerivation.Pbkdf2(
    //         password: $"{tokenRefresh}",
    //         salt: Encoding.UTF8.GetBytes(passwordSaltPlusTokenRefreshKey),
    //         prf: KeyDerivationPrf.HMACSHA256,
    //         iterationCount: 100000,
    //         numBytesRequested: 256 / 8
    //     );

    //     return tokenRefreshHash;
    // }

    public ClaimsPrincipal DecodeExpiredToken(string expiredToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes($"{SecretKey}"));

        try
        {
            var principal = tokenHandler.ValidateToken(
                expiredToken,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = DotNetEnv.Env.GetString("VALID_ISSUER"),
                    ValidAudience = DotNetEnv.Env.GetString("VALID_AUDIENCE"),
                    IssuerSigningKey = key
                },
                out SecurityToken validatedToken
            );

            return principal;
        }
        catch (Exception)
        {
            return null!;
        }
    }
}
