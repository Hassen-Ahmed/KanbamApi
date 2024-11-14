using KanbamApi.Dtos.Patch;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IWorkspaceService
    {
        // Task<IEnumerable<Workspace>> GetWorkspaceById(string workspaceId);
        Task<bool> IsWorkspaceExist_Using_WorkspaceIdAsync(string workspaceId);
        Task<IEnumerable<WorkspaceWithMemberDetails>> GetWorkspaces_With_Members_ByUserId(
            string userId
        );
        Task CreateAsync(string userId, Workspace newWorspace);

        Task<bool> PatchByIdAsync(
            string workspaceId,
            DtoWorkspaceUpdate updateWorkspaceTo,
            string userId
        );
        Task<bool> Remove_With_MembersAsync(string id, string userId);
    };
}
