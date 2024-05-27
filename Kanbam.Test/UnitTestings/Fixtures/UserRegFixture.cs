using KanbamApi.Models.AuthModels;

namespace Kanbam.Test.UnitTestings.Fixtures;

public class UserRegFixture
{
    public static UserRegistration ValidUser() =>
        new()
        {
            Email = "test@gmail.com",
            Password = "#hassenbest1",
            PasswordConfirm = "#hassenbest1",
        };

    public static UserRegistration InValidUser() =>
        new()
        {
            Email = "test@gmail.com",
            Password = "pw",
            PasswordConfirm = "",
        };
}
