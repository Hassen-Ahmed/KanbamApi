using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface IUsersRepo
{
    Task<string> GetUserIdAsync(string? email);
    Task<bool> CreateNewUserAsync(User newUser);
}
