namespace KanbamApi.Services.Interfaces;

public interface ICloudFlareTurnstileService
{
    Task<bool> VerifyTokenAsync(string token, string ip);
}
