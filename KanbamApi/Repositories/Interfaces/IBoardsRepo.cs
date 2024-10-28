using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IBoardsRepo
    {
        Task<List<BoardWithMemberDetails>> GetAllBoards_ByUserId(string workspaceId, string userId);

        Task<List<Board>> GetAll();
        Task<Board> Create(Board newBoard);
        Task<bool> Patch(string boardId, DtoBoardUpdate updateBoard);

        // Task Update(string id, Board updatedBoard);
        Task<bool> RemoveByBoardId(string boardId);
    }
}
