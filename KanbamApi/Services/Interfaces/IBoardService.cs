using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IBoardService
    {
        Task<List<Board>> GetAllAsync();
        Task<bool> IsBoardIdExistByBoardIdAsync(string boardId);
        Task<string?> GetWorkspaceIdByBoardIdAsync(string boardId);
        Task<List<BoardWithMemberDetails>> GetBoards_With_Members_ByWorkspaceId_Async(
            string workspaceId,
            string userId
        );
        Task<string?> CreateAsync(string userId, DtoBoardPost newBoard);
        Task<bool> PatchByIdAsync(string boardId, DtoBoardUpdate updateBoard, string userId);
        Task<bool> RemoveByIdAsync(string boardId, string userId);
        Task<bool> RemoveManyByWorkspaceIdAsync(string workspaceId);
    }
}
