using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Put;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;

namespace KanbamApi.Services
{
    public class WorkspaceMemberService : IWorkspaceMemberService
    {
        private readonly IWorkspaceMembersRepo _workspacesMemberRepo;

        public WorkspaceMemberService(IWorkspaceMembersRepo workspacesMemberRepo) =>
            _workspacesMemberRepo = workspacesMemberRepo;

        public async Task<List<WorkspaceMember>> Get()
        {
            var res = await _workspacesMemberRepo.GetAsyc();
            return res;
        }

        public async Task<DtoWorkspaceMemberPost> CreateAsync(
            DtoWorkspaceMemberPost newWorspaceMember,
            string userId
        )
        {
            WorkspaceMember newMember =
                new()
                {
                    UserId = userId,
                    WorkspaceId = newWorspaceMember.workspaceId,
                    Role = newWorspaceMember.Role,
                };

            await _workspacesMemberRepo.Create(newMember);
            return newWorspaceMember;
        }

        // public async Task UpdateAsync(string id, WorkspaceMember updatedWorkspaceMember) =>
        //     await _workspacesMemberRepo.Update(id, updatedWorkspaceMember);

        public async Task<bool> PatchByIdAsync(
            string id,
            DtoWorkspaceMemberUpdate updateWorkspaceMember
        )
        {
            return await _workspacesMemberRepo.Patch(id, updateWorkspaceMember);
        }

        public async Task<bool> RemoveByIdAsync(string id) =>
            await _workspacesMemberRepo.RemoveById(id);

        public async Task<bool> RemoveByWorkspaceIdAsync(string workspaceId) =>
            await _workspacesMemberRepo.RemoveByWorkspaceId(workspaceId);
    }
}
