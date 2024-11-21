using System.Security.Cryptography;
using System.Text;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace KanbamApi.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepo _authRepo;

    public AuthService(IAuthRepo authRepo)
    {
        _authRepo = authRepo;
    }

    public async Task<List<Auth>> GetAsync()
    {
        return await _authRepo.GetAsync();
    }

    public async Task<Auth?> IsEmailExists(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return null;
        }
        return await _authRepo.IsEmailExists(email);
    }

    public async Task<bool> CreateAsync(Auth newAuth)
    {
        return await _authRepo.CreateAsync(newAuth);
    }

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

    public bool ValidatePasswordHash(byte[] passwordHashGenerated, byte[] passwordHashFromDb)
    {
        if (CryptographicOperations.FixedTimeEquals(passwordHashGenerated, passwordHashFromDb))
            return true;

        return false;
    }
}
