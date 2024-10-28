using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Put;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IWorkspaceMemberService
    {
        Task<List<WorkspaceMember>> Get();
        Task<DtoWorkspaceMemberPost> CreateAsync(
            DtoWorkspaceMemberPost newWorspaceMember,
            string userId
        );

        // Task UpdateAsync(string id, WorkspaceMember updatedWorkspaceMember);
        Task<bool> PatchByIdAsync(
            string workspaceId,
            DtoWorkspaceMemberUpdate updateWorkspaceMember
        );
        Task<bool> RemoveByIdAsync(string id);
        Task<bool> RemoveByWorkspaceIdAsync(string workspaceId);
    }
}
