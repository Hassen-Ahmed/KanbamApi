using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IBoardsRepo
    {
        Task<List<Board>> GetAll();
        Task<bool> IsBoardIdExistByBoardId(string boardId);
        Task<string?> GetWorkspaceIdByBoardId(string boardId);
        Task<List<BoardWithMemberDetails>> GetAllBoards_WithMembers_ByWorkspaceId(
            string workspaceId,
            string userId
        );
        Task<List<Board>> Get_OnlyBoards_ByWorkspaceId(string workspaceId);

        Task<Board> Create(Board newBoard);
        Task<bool> Patch(string boardId, DtoBoardUpdate updateBoard);

        Task<bool> RemoveByBoardId(string boardId);
        Task<bool> RemoveMany_With_Members_ByWorkspaceId(string workspaceId);
    }
}
