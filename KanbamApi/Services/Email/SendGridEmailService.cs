using KanbamApi.Models.AuthModels;
using KanbamApi.Services.Interfaces.Email;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace KanbamApi.Services.Email;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(ILogger<SendGridEmailService> logger)
    {
        _apiKey = DotNetEnv.Env.GetString("SENDGRID_API_KEY");
        _fromEmail = DotNetEnv.Env.GetString("SENDGRID_FROM_EMAIL");
        _fromName = DotNetEnv.Env.GetString("SENDGRID_FROM_NAME");
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
    {
        try
        {
            var apiKey = _apiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var subject = emailRequest.Subject;
            var to = new EmailAddress(emailRequest.ToEmail, "Example User");
            var plainTextContent = emailRequest.Body;
            var htmlContent = emailRequest.Body;
            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlContent
            );
            var response = await client.SendEmailAsync(msg);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", emailRequest.ToEmail);
            return false;
        }
    }
}
