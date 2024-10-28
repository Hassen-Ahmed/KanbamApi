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

    public async Task<List<List>> GetAllByBoardIdAsync(string boardId) =>
        await _listsRepo.GetAllByBoardId(boardId);

    public async Task<DtoListPost> CreateAsync(DtoListPost newListDto)
    {
        var filter = Builders<List>.Filter.Empty;
        CountOptions opts = new() { Hint = "_id_" };

        var count = _kanbamDbContext.ListsCollection.CountDocuments(filter, opts);
        var indexNumber = Convert.ToInt32(count);

        List newList =
            new()
            {
                Title = newListDto.Title,
                BoardId = newListDto.BoardId,
                IndexNumber = indexNumber + 1,
            };

        await _listsRepo.Create(newList);
        return newListDto;
    }

    public async Task<bool> PatchByIdAsync(string listId, DtoListsUpdate dtoListsUpdate) =>
        await _listsRepo.Patch(listId, dtoListsUpdate);

    public async Task<bool> RemoveByIdAsync(string listId) => await _listsRepo.Remove(listId);
}
