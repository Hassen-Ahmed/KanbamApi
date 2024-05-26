using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbamApi.Models.AuthModels;

namespace Kanbam.Test.Fixtures;

public class UserLoginFixture
{
    public static UserLogin ValidUser() =>
        new() { Email = "test@gmail.com", Password = "#hassenbest1", };

    public static UserLogin InValidUser() =>
        new() { Email = "Wrong@gmail.com", Password = "#hassenbest9", };
}
