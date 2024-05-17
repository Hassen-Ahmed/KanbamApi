namespace KanbamApi.Services.AuthServices;

public interface IAuthControllerService
{
    byte[] GeneratePasswordHash(string password, byte[] passwordSalt);
    string GenerateToken(string userId);
}
