using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Patch;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IWorkspacesRepo _workspacesRepo;
        private readonly IWorkspaceMembersRepo _workspacesMemberRepo;
        private readonly IKanbamDbContext _kanbamDbContext;

        public WorkspaceService(
            IWorkspacesRepo workspacesRepo,
            IWorkspaceMembersRepo workspacesMemberRepo,
            IKanbamDbContext kanbamDbContext
        )
        {
            _workspacesRepo = workspacesRepo;
            _workspacesMemberRepo = workspacesMemberRepo;
            _kanbamDbContext = kanbamDbContext;
        }

        // public async Task<IEnumerable<Workspace>> GetWorkspaceById(string workspaceId) =>
        //     await _workspacesRepo.GetById(workspaceId);

        public async Task<bool> IsWorkspaceExist_Using_WorkspaceIdAsync(string workspaceId)
        {
            return await _workspacesRepo.IsWorkspaceExist_Using_WorkspaceId(workspaceId);
        }

        public Task<IEnumerable<WorkspaceWithMemberDetails>> GetWorkspaces_With_Members_ByUserId(
            string userId
        )
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(
                        nameof(userId),
                        "Provided ID cannot be null or empty."
                    );
                }

                return _workspacesRepo.GetAllWorkspace_ByUserId(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreateAsync(string userId, Workspace newWorspace)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var createdWorkspace = await _workspacesRepo.Create(newWorspace);
                var workspaceId = createdWorkspace.Id;

                WorkspaceMember newMember =
                    new()
                    {
                        UserId = userId,
                        WorkspaceId = workspaceId,
                        Role = "Admin",
                        BoardAccessLevel = "All"
                    };

                await _workspacesMemberRepo.Create(newMember);

                await session.CommitTransactionAsync();
            }
            catch (MongoException)
            {
                await session.AbortTransactionAsync();
                throw;
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        // public async Task UpdateAsync(string id, Workspace updatedWorkspace) =>
        //     await _workspacesRepo.Update(id, updatedWorkspace);

        public async Task<bool> PatchByIdAsync(
            string workspaceId,
            DtoWorkspaceUpdate updateWorkspace
        )
        {
            return await _workspacesRepo.Patch(workspaceId, updateWorkspace);
        }

        public async Task<bool> RemoveAsync(string workspaceId)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();
            try
            {
                var resultOne = await _workspacesRepo.Remove(workspaceId);
                var resultTwo = await _workspacesMemberRepo.RemoveByWorkspaceId(workspaceId);
                // await _boardService.RemoveByWorkspaceId(workspaceId);
                await session.CommitTransactionAsync();
                return resultOne && resultTwo;
            }
            catch (MongoException)
            {
                await session.AbortTransactionAsync();
                throw;
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}
