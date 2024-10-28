using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IBoardMemberService
    {
        Task<List<BoardMember>> GetAllAsync();
        Task<BoardMember> CreateAsync(BoardMember newBoardMember);
        Task<bool> PatchByIdAsync(string workspaceId, DtoBoardMemberUpdate updateBoardMember);
        Task<bool> RemoveById(string id);
    }
}
