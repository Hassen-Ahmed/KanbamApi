using KanbamApi.Dtos.Put;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IWorkspaceMembersRepo
    {
        Task<List<WorkspaceMember>> GetAsyc();
        Task<WorkspaceMember> Create(WorkspaceMember newWorkspaceMember);

        // Task Update(string id, WorkspaceMember updatedWorkspaceMember);
        Task<bool> Patch(string id, DtoWorkspaceMemberUpdate updateWorkspaceMember);
        Task<bool> RemoveById(string workspaceId);
        Task<bool> RemoveByWorkspaceId(string workspaceId);
    }
}
