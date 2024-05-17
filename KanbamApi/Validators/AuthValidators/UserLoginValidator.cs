using FluentValidation;
using KanbamApi.Models.AuthModels;

namespace KanbamApi.Validators.AuthValidators;

public class UserLoginValidator : AbstractValidator<UserLogin>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.Email)
            .Matches(@"^[\w\.-]+@[a-zA-Z\\d-]+(\.[a-zA-Z]{2,})+$")
            .WithMessage("Wrong email address!");

        RuleFor(x => x.Password)
            .Matches(@"^(?=.*[0-9])(?=.*[a-zA-Z])(?=.*[#$@!%&*?])[A-Za-z0-9#$@!%&*?]{8,20}$")
            .WithMessage(
                "Password should be 8-20 character and should include at list 1 letter, 1 number and only 1 spcecial character!"
            );
        ;
    }
}
