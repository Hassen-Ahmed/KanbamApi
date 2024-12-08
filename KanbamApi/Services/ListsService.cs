using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class ListsService : IListsService
{
    private readonly IListsRepo _listsRepo;
    private readonly IKanbamDbContext _kanbamDbContext;

    public ListsService(IListsRepo listsRepo, IKanbamDbContext kanbamDbContext)
    {
        _listsRepo = listsRepo;
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<List>> GetAllAsync() => await _listsRepo.GetAll();

    public async Task<bool> IsListIdExistByListIdAsync(string listId)
    {
        return await _listsRepo.IsListIdExistByListId(listId);
    }

    public async Task<List<List>> GetAllByBoardIdAsync(string boardId) =>
        await _listsRepo.GetAllByBoardId(boardId);

    public async Task<List<ListWithCards>> GetListsWithCardsByBoardIdAsync(string boardId) =>
        await _listsRepo.GetListsWithCardsByBoardId(boardId);

    public async Task<List> CreateAsync(DtoListPost newListDto)
    {
        List newList =
            new()
            {
                Title = newListDto.Title,
                BoardId = newListDto.BoardId,
                IndexNumber = newListDto.IndexNumber,
            };

        var resList = await _listsRepo.Create(newList);
        return resList;
    }

    public async Task<bool> PatchByIdAsync(string listId, DtoListsUpdate dtoListsUpdate) =>
        await _listsRepo.Patch(listId, dtoListsUpdate);

    public async Task<bool> RemoveByIdAsync(string listId) => await _listsRepo.Remove(listId);
}
