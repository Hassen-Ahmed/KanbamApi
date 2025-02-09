using System.Text;
using KanbamApi.Models.AuthModels;
using KanbamApi.Services.Interfaces;
using Newtonsoft.Json;

namespace KanbamApi.Services;

public class CloudFlareTurnstileService : ICloudFlareTurnstileService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _secretKey;

    public CloudFlareTurnstileService(IHttpClientFactory httpClientFactory, string secretKey)
    {
        _httpClientFactory = httpClientFactory;
        _secretKey = secretKey;
    }

    public async Task<bool> VerifyTokenAsync(string token, string ip)
    {
        var values = new
        {
            secret = _secretKey,
            response = token,
            remoteip = ip
        };

        var content = JsonConvert.SerializeObject(values);
        var stringContent = new StringContent(content, Encoding.UTF8, @"application/json");

        var httpClient = _httpClientFactory.CreateClient();
        var url = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
        var response = await httpClient.PostAsync(url, stringContent);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<TurnstileVerificationResponse>(jsonResponse);

        return result?.Success ?? false;
    }
}
