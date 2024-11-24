using System.Text;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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
}
