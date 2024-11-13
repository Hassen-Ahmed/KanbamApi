using KanbamApi.Dtos;
using KanbamApi.Dtos.Put;
using KanbamApi.Models;

namespace KanbamApi.Repositories.Interfaces
{
    public interface IWorkspaceMembersRepo
    {
        Task<List<WorkspaceMember>> GetAsyc();
        Task<List<WorkspaceMember>> Get_Members_By_WorkspaceId(string workspaceId);
        Task<string?> Get_Role_ByUserId(string userId);
        Task<bool> IsUserAMember_Using_WorkspaceId_And_UserId(string workspaceId, string userId);

        Task<List<DtoWorkspaceWithMemberGet>> GetMembersByWorkspaceId(string workspaceId);
        Task<WorkspaceMember> Create(WorkspaceMember newWorkspaceMember);

        Task<bool> Patch(string id, DtoWorkspaceMemberUpdate updateWorkspaceMember);
        Task<bool> RemoveById(string workspaceId);
        Task<bool> RemoveByWorkspaceId(string workspaceId);
    }
}
