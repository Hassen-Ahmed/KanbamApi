using KanbamApi.Models.AuthModels;

namespace Kanbam.Test.UnitTestings.Fixtures;

public class UserLoginFixture
{
    public static UserLogin ValidUser() =>
        new()
        {
            Email = "test@gmail.com",
            Password = "#Hassenbest1",
            Token = "token"
        };

    public static UserLogin InValidUser() =>
        new()
        {
            Email = "Wrong",
            Password = "hassenbest",
            Token = "token"
        };
}
