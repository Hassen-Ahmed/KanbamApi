namespace KanbamApi.Services.Interfaces.Email;

public interface IEmailServiceFactory
{
    public IEmailService? CreateEmailService(string serviceType);
}
