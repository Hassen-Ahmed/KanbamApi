namespace KanbamApi.Util;

public class EmailTemplateService
{
    private static string LoadEmailTemplate(string fileName)
    {
        string templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "templates",
            fileName
        );
        string template = File.ReadAllText(templatePath);
        return template;
    }

    public static string GenerateDonnerHTMLContent(string email, int amount)
    {
        var firstName = email.Contains("@") ? email.Split('@')[0] : "Supporter";

        var template = LoadEmailTemplate("donation-email.html");
        return template
            .Replace("{{FirstName}}", firstName)
            .Replace("{{Amount}}", $"{amount}")
            .Replace("{{Date}}", DateTime.UtcNow.ToString("dd MMM yyyy"));
    }

    public static string GeneratePasswordResetHTMLContent(string email, string link)
    {
        var template = LoadEmailTemplate("reset-password.html");
        return template.Replace("{{Email}}", email).Replace("{{Link}}", link);
    }
}
