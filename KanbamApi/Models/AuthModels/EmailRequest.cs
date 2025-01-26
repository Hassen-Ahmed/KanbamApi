namespace KanbamApi.Models.AuthModels;

public class EmailRequest(string toEmail, string subject, string body)
{
    public string ToEmail => toEmail;
    public string Subject => subject;
    public string Body => body;
}
