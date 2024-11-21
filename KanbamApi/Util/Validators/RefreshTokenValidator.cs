using FluentValidation;
using KanbamApi.Models;
using MongoDB.Bson;

namespace KanbamApi.Util.Validators;

public class RefreshTokenValidator : AbstractValidator<RefreshToken>
{
    public RefreshTokenValidator()
    {
        RuleFor(t => t.UserId).NotEmpty().WithMessage("UserId is required.");

        RuleFor(t => t.Token)
            .NotEmpty()
            .WithMessage("Token is required.")
            .Must(IsGuidOrValidByteSize)
            .WithMessage("Token must be a valid GUID.");

        RuleFor(x => x.TokenExpiryTime)
            .NotEmpty()
            .WithMessage("TokenExpiryTime is required.")
            .Must(BeAValidDate)
            .WithMessage("TokenExpiryTime must be a valid date and not in the past.");
    }

    private bool IsGuidOrValidByteSize(Guid token)
    {
        if (Guid.TryParse(token.ToString(), out _))
            return true;

        return false;
    }

    private bool BeAValidDate(DateTime date) => date > DateTime.UtcNow;
}
