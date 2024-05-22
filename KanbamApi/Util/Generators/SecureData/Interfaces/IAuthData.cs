namespace KanbamApi.Util.Generators.SecureData.Interfaces;

public interface IAuthData
{
    byte[] GeneratePasswordHash(string password, byte[] passwordSalt);
    string GenerateToken(string userId);
}
