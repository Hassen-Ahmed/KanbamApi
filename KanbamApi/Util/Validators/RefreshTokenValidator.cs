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
            .Must(token => token != default(Guid))
            .WithMessage("Token must be a valid GUID.");

        RuleFor(x => x.TokenExpiryTime).NotEmpty().WithMessage("TokenExpiryTime is required.");
    }
}
