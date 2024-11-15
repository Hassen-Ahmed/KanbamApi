using KanbamApi.Dtos;
using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Put;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IWorkspaceMemberService
    {
        Task<List<WorkspaceMember>> Get();
        Task<List<DtoWorkspaceWithMemberGet>> GetMembersByWorkspaceIdAsync(string workspaceId);
        Task<bool> CreateAsync(DtoWorkspaceMemberPost newWorspaceMember, string? currentUserId);

        Task<bool> PatchByIdAsync(
            string workspaceId,
            DtoWorkspaceMemberUpdate updateWorkspaceMember,
            string userId
        );
        Task<bool> RemoveByIdAsync(string id, string currentUserId);
    }
}
