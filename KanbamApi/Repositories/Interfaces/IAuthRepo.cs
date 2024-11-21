using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface IAuthRepo
{
    Task<List<Auth>> GetAsync();
    Task<Auth> IsEmailExists(string? email);
    Task<bool> CreateAsync(Auth newAuth);
}
