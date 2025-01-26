using KanbamApi.Models.AuthModels;

namespace KanbamApi.Services.Interfaces.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailRequest emailRequest);
}
