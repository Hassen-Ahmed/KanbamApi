using KanbamApi.Dtos.Patch;
using KanbamApi.Models;

namespace KanbamApi.Services.Interfaces
{
    public interface IWorkspaceService
    {
        // Task<IEnumerable<Workspace>> GetWorkspaceById(string workspaceId);
        Task<IEnumerable<WorkspaceWithMemberDetails>> GetWorkspaces_With_Members_ByUserId(
            string userId
        );
        Task CreateAsync(string userId, Workspace newWorspace);

        // Task UpdateAsync(string id, Workspace updatedWorkspace);
        Task<bool> PatchByIdAsync(string workspaceId, DtoWorkspaceUpdate updateWorkspaceTo);
        Task<bool> RemoveAsync(string id);
    };
}
