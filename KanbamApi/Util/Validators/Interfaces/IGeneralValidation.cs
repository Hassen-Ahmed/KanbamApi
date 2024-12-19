namespace KanbamApi.Util.Validators
{
    public interface IGeneralValidation
    {
        bool IsValidEmail(string email);
        bool IsValidObjectId(string? id);
    }
}
