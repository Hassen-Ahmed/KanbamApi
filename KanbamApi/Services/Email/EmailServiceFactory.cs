using KanbamApi.Services.Interfaces.Email;

namespace KanbamApi.Services.Email;

public class EmailServiceFactory : IEmailServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmailServiceFactory(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public IEmailService? CreateEmailService(string serviceType)
    {
        return serviceType.ToLower() switch
        {
            "smtp" => _serviceProvider.GetRequiredService<SmtpEmailService>(),
            "sendgrid" => _serviceProvider.GetRequiredService<SendGridEmailService>(),
            _
                => throw new NotSupportedException(
                    $"Email service type '{serviceType}' is not supported."
                )
        };
    }
}
