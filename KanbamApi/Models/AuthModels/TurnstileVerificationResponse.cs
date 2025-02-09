namespace KanbamApi.Models.AuthModels;

public class TurnstileVerificationResponse
{
    public bool Success { get; set; }
    public string Challenge_ts { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public IEnumerable<string>? Error_codes { get; set; }
}
