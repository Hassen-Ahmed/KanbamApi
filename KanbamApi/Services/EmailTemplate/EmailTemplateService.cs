namespace KanbamApi.Util;

public class EmailTemplateService
{
    public static string LoadEmailTemplate(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    public static string GenerateDonnerHTMLContent(string email, int amount)
    {
        var firstName = email.Contains("@") ? email.Split('@')[0] : "Supporter";
        string templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "templates",
            "donation-email.html"
        );
        string template = File.ReadAllText(templatePath);
        return template
            .Replace("{{FirstName}}", firstName)
            .Replace("{{Amount}}", $"{amount}")
            .Replace("{{Date}}", DateTime.UtcNow.ToString("dd MMM yyyy"));
    }

    public static string GeneratePasswordResetHTMLContent(string email, string link)
    {
        var firstName = email.Contains("@") ? email.Split('@')[0] : "Supporter";
        string templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "templates",
            "reset-password.html"
        );
        string template = File.ReadAllText(templatePath);
        return template
            .Replace("{{Email}}", email)
            .Replace("{{Link}}", link)
            .Replace("{{Date}}", DateTime.UtcNow.ToString("dd MMM yyyy"));
    }
}
