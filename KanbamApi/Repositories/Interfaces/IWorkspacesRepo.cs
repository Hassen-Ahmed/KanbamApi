using KanbamApi.Dtos.Patch;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IWorkspacesRepo
    {
        Task<bool> IsWorkspaceExist_Using_WorkspaceId(string workspaceId);
        Task<IEnumerable<WorkspaceWithMemberDetails>> GetAllWorkspace_ByUserId(string userId);
        Task<Workspace> Create(Workspace newWorkspace);

        Task<bool> Patch(string workspaceId, DtoWorkspaceUpdate updateWorkspace);
        Task<bool> Remove_With_Members(string workspaceId);
    }
}
