using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface IListsRepo
{
    Task<List<List>> GetListsWithCardsByUserId(string userId);
    Task<List> CreateAsync(List newList);
    Task UpdateAsync(string id, List updatedList);
    Task RemoveAsync(string id);
}
