using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces;

public interface IListsService
{
    Task<List<List>> GetAllAsync();
    Task<bool> IsListIdExistByListIdAsync(string listId);
    Task<List<List>> GetAllByBoardIdAsync(string boardId);
    Task<List> CreateAsync(DtoListPost newListDto);
    Task<bool> PatchByIdAsync(string listId, DtoListsUpdate dtoListsUpdate);
    Task<bool> RemoveByIdAsync(string listId);
}
