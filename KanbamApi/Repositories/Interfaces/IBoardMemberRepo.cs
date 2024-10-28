using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IBoardMemberRepo
    {
        Task<List<BoardMember>> GetAll();
        Task<BoardMember> Create(BoardMember newBoardMember);

        // Task UpdateAsync(string id, BoardMember updatedWorkspaceMember);
        Task<bool> Patch(string boardMemberId, DtoBoardMemberUpdate updateBoardMember);
        Task<bool> RemoveById(string id);
        Task<bool> RemoveByBoardId(string boardId);
    }
}
