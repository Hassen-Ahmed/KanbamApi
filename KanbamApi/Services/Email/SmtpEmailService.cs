using KanbamApi.Models.AuthModels;
using KanbamApi.Services.Interfaces.Email;
using MailKit.Net.Smtp;
using MimeKit;

namespace KanbamApi.Services.Email;

public class SmtpEmailService : IEmailService
{
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly int _smtpPort;
    private readonly string _smtpHost;

    public SmtpEmailService()
    {
        _smtpUsername = DotNetEnv.Env.GetString("SMTP_USERNAME");
        _smtpPassword = DotNetEnv.Env.GetString("SMTP_PASSWORD");
        _smtpPort = DotNetEnv.Env.GetInt("SMTP_PORT");
        _smtpHost = DotNetEnv.Env.GetString("SMTP_HOST");
    }

    public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Kanbam", _smtpUsername));
            email.To.Add(new MailboxAddress(emailRequest.ToEmail, emailRequest.ToEmail));
            email.Subject = emailRequest.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = emailRequest.Body };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, true);
            await smtp.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            return false;
        }
    }
}
