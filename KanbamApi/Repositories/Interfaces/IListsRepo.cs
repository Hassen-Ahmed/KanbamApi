using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces;

public interface IListsRepo
{
    Task<List<List>> GetAll();
    Task<List<List>> GetAllByBoardId(string boardId);
    Task<List> Create(List newList);
    Task<bool> Patch(string id, DtoListsUpdate dtoListsUpdate);

    Task<bool> Remove(string id);
}
