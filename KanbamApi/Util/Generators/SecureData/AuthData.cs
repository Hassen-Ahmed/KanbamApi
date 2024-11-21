using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace KanbamApi.Util.Generators.SecureData;

public class AuthData : IAuthData
{
    public byte[] GeneratePasswordHash(string password, byte[] passwordSalt)
    {
        string passwordSaltPlusPasswordKey =
            DotNetEnv.Env.GetString("PASSWORD_KEY") + Convert.ToBase64String(passwordSalt);

        byte[] passwordHash = KeyDerivation.Pbkdf2(
            password: $"{password}",
            salt: Encoding.UTF8.GetBytes(passwordSaltPlusPasswordKey),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8
        );

        return passwordHash;
    }

    public string GenerateToken(string userId)
    {
        Claim[] claims = [new Claim("userId", userId), new Claim("role", "User")];

        string? tokenKeyString = DotNetEnv.Env.GetString("TOKEN_KEY");

        SymmetricSecurityKey tokenKey = new(Encoding.UTF8.GetBytes($"{tokenKeyString}"));

        SigningCredentials credentials = new(tokenKey, SecurityAlgorithms.HmacSha256Signature);

        SecurityTokenDescriptor descriptor =
            new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = credentials,

                // uncomment after host this API
                // Issuer =  DotNetEnv.Env.GetString("VALID_ISSUER"),
                // Audience = DotNetEnv.Env.GetString("VALID_AUDIENCE"),
            };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(descriptor);

        return tokenHandler.WriteToken(token);
    }
}
