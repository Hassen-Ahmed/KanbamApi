using System.Text.RegularExpressions;
using MongoDB.Bson;

namespace KanbamApi.Util.Validators;

public class GeneralValidation : IGeneralValidation
{
    public bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        return emailRegex.IsMatch(email);
    }

    public bool IsValidObjectId(string? id) => ObjectId.TryParse(id, out var _);
}
