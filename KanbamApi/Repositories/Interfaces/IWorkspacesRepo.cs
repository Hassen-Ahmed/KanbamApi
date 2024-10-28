using KanbamApi.Dtos.Patch;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IWorkspacesRepo
    {
        // Task<IEnumerable<Workspace>> GetById(string workspaceId);
        Task<IEnumerable<WorkspaceWithMemberDetails>> GetAllWorkspace_ByUserId(string userId);
        Task<Workspace> Create(Workspace newWorkspace);

        // Task Update(string id, Workspace updatedWorkspace);
        Task<bool> Patch(string workspaceId, DtoWorkspaceUpdate updateWorkspace);
        Task<bool> Remove(string id);
    }
}
