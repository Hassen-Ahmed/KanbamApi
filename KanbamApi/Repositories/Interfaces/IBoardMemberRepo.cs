using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IBoardMemberRepo
    {
        Task<List<BoardMember>> GetAll();
        Task<bool> IsUserIdExist(string userId);
        Task<BoardMember> Create(BoardMember newBoardMember);
        Task<List<BoardMember>> CreateMany(List<BoardMember> newBoardMemberList);
        Task<bool> Patch(string boardMemberId, DtoBoardMemberUpdate updateBoardMember);
        Task<bool> RemoveById(string id);
        Task<bool> RemoveByBoardId(string boardId);
    }
}
