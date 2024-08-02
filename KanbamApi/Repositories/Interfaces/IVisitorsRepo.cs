using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface IVisitorsRepo
{
    Task<List<Visitor>> GetByUserIdAsync(string userId);
    Task<Visitor> CreateAsync(Visitor visitor);
    Task UpdateAsync(Visitor visitor, string userId);
}
