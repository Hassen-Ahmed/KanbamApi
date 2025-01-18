using KanbamApi.Dtos;
using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Put;
using KanbamApi.Models;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace KanbamApi.Services
{
    public class WorkspaceMemberService : IWorkspaceMemberService
    {
        private readonly IWorkspaceMembersRepo _workspacesMemberRepo;
        private readonly IBoardsRepo _boardsRepo;
        private readonly IBoardMemberRepo _boardMemberRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkspaceMemberService(
            IWorkspaceMembersRepo workspacesMemberRepo,
            IBoardsRepo boardsRepo,
            IBoardMemberRepo boardMemberRepo,
            UserManager<ApplicationUser> userManager
        )
        {
            _workspacesMemberRepo = workspacesMemberRepo;
            _boardsRepo = boardsRepo;
            _boardMemberRepo = boardMemberRepo;
            _userManager = userManager;
        }

        public async Task<List<WorkspaceMember>> Get()
        {
            var res = await _workspacesMemberRepo.GetAsyc();
            return res;
        }

        public async Task<List<DtoWorkspaceWithMemberGet>> GetMembersByWorkspaceIdAsync(
            string workspaceId
        )
        {
            var members = await _workspacesMemberRepo.GetMembersByWorkspaceId(workspaceId);
            return members;
        }

        public async Task<bool> CreateAsync(
            DtoWorkspaceMemberPost newWorspaceMember,
            string? currentUserId
        )
        {
            var user = await _userManager.FindByEmailAsync(newWorspaceMember.Email);

            if (user is null || user.Id.ToString() == currentUserId)
            {
                return false;
            }

            var currentUserRole = await _workspacesMemberRepo.Get_Role_ByUserId(currentUserId!);

            if (currentUserRole is null || currentUserRole is not "Admin")
            {
                Console.WriteLine("is not admin");
                return false;
            }

            // check if the member already exist using workspaceId and userId
            var isUserAMember =
                await _workspacesMemberRepo.IsUserAMember_Using_WorkspaceId_And_UserId(
                    newWorspaceMember.WorkspaceId,
                    user.Id.ToString()
                );

            if (isUserAMember)
                return false;

            WorkspaceMember newMember =
                new()
                {
                    UserId = user.Id.ToString(),
                    WorkspaceId = newWorspaceMember.WorkspaceId,
                    Role = newWorspaceMember.Role,
                    BoardAccessLevel = "All"
                };

            await _workspacesMemberRepo.Create(newMember);

            var boards = await _boardsRepo.Get_OnlyBoards_ByWorkspaceId(
                newWorspaceMember.WorkspaceId
            );

            var boardMembers = boards
                .Select(board => new BoardMember
                {
                    BoardId = board.Id,
                    UserId = user.Id.ToString(),
                    Role = newWorspaceMember.Role
                })
                .ToList();

            if (boardMembers.Count > 0)
            {
                await _boardMemberRepo.CreateMany(boardMembers);
            }

            return true;
        }

        public async Task<bool> PatchByIdAsync(
            string id,
            DtoWorkspaceMemberUpdate updateWorkspaceMember,
            string userId
        )
        {
            // check if the user is Admin before updating
            var isUserAdmin = await _workspacesMemberRepo.Is_User_Admin_ByUserId(userId);
            return isUserAdmin && await _workspacesMemberRepo.Patch(id, updateWorkspaceMember);
        }

        public async Task<bool> RemoveByIdAsync(string id, string currentUserId)
        {
            // check if the user is Admin before updating
            var isUserAdmin = await _workspacesMemberRepo.Is_User_Admin_ByUserId(currentUserId);
            return isUserAdmin && await _workspacesMemberRepo.RemoveById(id);
        }
    }
}
