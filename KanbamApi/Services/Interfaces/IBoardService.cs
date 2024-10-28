using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IBoardService
    {
        Task<List<BoardWithMemberDetails>> GetBoards_With_Members_ByWorkspaceId_Async(
            string workspaceId,
            string userId
        );

        Task<List<Board>> GetAllAsync();
        Task CreateAsync(string userId, Board newBoard);
        Task<bool> PatchByIdAsync(string boardId, DtoBoardUpdate updateBoard);
        Task<bool> RemoveByIdAsync(string boardId);
    }
}
