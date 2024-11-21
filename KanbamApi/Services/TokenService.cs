using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KanbamApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace KanbamApi.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string? SecretKey = DotNetEnv.Env.GetString("TOKEN_KEY");

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        SymmetricSecurityKey tokenKey = new(Encoding.UTF8.GetBytes($"{SecretKey}"));

        SigningCredentials credentials = new(tokenKey, SecurityAlgorithms.HmacSha512Signature);
        var expirationDateOfAccessToken = _configuration.GetValue<int>(
            "KanbamSettings:Expiration:AccessTokenMinute"
        );

        SecurityTokenDescriptor descriptor =
            new()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddMinutes(expirationDateOfAccessToken),

                // Issuer =  DotNetEnv.Env.GetString("VALID_ISSUER"),
                // Audience = DotNetEnv.Env.GetString("VALID_AUDIENCE"),
            };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(descriptor);

        return tokenHandler.WriteToken(token);
    }

    public Guid GenerateRefreshToken()
    {
        return Guid.NewGuid();
    }

    public ClaimsPrincipal? ValidatePrincipal(string token)
    {
        if (SecretKey is null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "yourissuer",
                    ValidAudience = "youraudience",
                    IssuerSigningKey = key,
                    ValidateLifetime = true
                },
                out _
            );

            return principal;
        }
        catch
        {
            return null;
        }
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
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false,
                    // ValidIssuer = "your-issuer",
                    // ValidAudience = "your-audience",
                    IssuerSigningKey = key
                },
                out SecurityToken validatedToken
            );

            // Contains the claims from the expired token
            return principal;
        }
        catch (Exception)
        {
            return null!;
        }
    }
}
