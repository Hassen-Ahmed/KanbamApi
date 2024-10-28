using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface IUsersRepo
{
    Task<List<User>> GetAll();
    Task<List<User>> GetById(string id);

    Task<string> GetUserIdByEmail(string? email);
    Task<string> Create(User newUser);
    Task<bool> Patch(string id, User newUser);
    Task<bool> RemoveById(string id);
}
