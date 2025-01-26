using KanbamApi.Models.AuthModels;
using KanbamApi.Services.Interfaces.Email;

namespace KanbamApi.Services.Email;

public class FallbackEmailService : IEmailService
{
    private readonly IEmailServiceFactory _emailServiceFactory;
    private readonly string[] _fallbackOrder = ["smtp", "sendgrid"];

    public FallbackEmailService(IEmailServiceFactory emailServiceFactory) =>
        _emailServiceFactory = emailServiceFactory;

    public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
    {
        foreach (var serviceType in _fallbackOrder)
        {
            try
            {
                var emailService = _emailServiceFactory.CreateEmailService(serviceType);
                if (emailService is null)
                    continue;

                var success = await emailService.SendEmailAsync(emailRequest);
                if (success)
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        return false;
    }
}
