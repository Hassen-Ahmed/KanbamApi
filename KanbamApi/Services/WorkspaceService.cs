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
            IKanbamDbContext kanbamDbContext,
            IBoardService boardService
        )
        {
            _workspacesRepo = workspacesRepo;
            _workspacesMemberRepo = workspacesMemberRepo;
            _kanbamDbContext = kanbamDbContext;
        }

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

        public async Task<bool> PatchByIdAsync(
            string workspaceId,
            DtoWorkspaceUpdate updateWorkspace,
            string userId
        )
        {
            // check if the user is Admin before updating
            var isUserAdmin = await _workspacesMemberRepo.Is_User_Admin_ByUserId(userId);
            return isUserAdmin && await _workspacesRepo.Patch(workspaceId, updateWorkspace);
        }

        public async Task<bool> Remove_With_MembersAsync(string workspaceId, string userId)
        {
            // check if the user is Admin before deleting
            var isUserAdmin = await _workspacesMemberRepo.Is_User_Admin_ByUserId(userId);
            return isUserAdmin && await _workspacesRepo.Remove_With_Members(workspaceId);
        }
    }
}
