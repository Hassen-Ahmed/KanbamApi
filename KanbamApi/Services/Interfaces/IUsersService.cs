using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces;

public interface IUsersService
{
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetByIdAsync(string id);
    Task<string> GetUserIdByEmail(string id);
    Task<string?> CreateAsync(User newUser);
    Task<bool> PatchByIdAsync(string userId, DtoUsersUpdate updateUser);
}
