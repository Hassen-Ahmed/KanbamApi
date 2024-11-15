using KanbamApi.Dtos;
using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IBoardMemberService
    {
        Task<List<BoardMember>> GetAllAsync();
        Task<List<DtoBoardWithMemberGet>> GetMembersByBoardIdAsync(string boardId);
        Task<bool> CreateAsync(DtoBoardMemberPost newBoardMember, string? currentUserId);
        Task<bool> PatchByIdAsync(
            string workspaceId,
            DtoBoardMemberUpdate updateBoardMember,
            string currentUserId
        );
        Task<bool> RemoveById(string id);
    }
}
