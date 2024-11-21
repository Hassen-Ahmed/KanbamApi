using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces;

public interface IAuthService
{
    Task<List<Auth>> GetAsync();
    Task<Auth?> IsEmailExists(string? email);
    Task<bool> CreateAsync(Auth newAuth);
    byte[] GeneratePasswordHash(string password, byte[] passwordSalt);
    bool ValidatePasswordHash(byte[] passwordHashGenerated, byte[] passwordHashFromDb);
}
