using KanbamApi.Dtos;
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
        private readonly IBoardsRepo _boardsRepo;
        private readonly IBoardMemberRepo _boardMemberRepo;
        private readonly IUsersService _usersService;

        public WorkspaceMemberService(
            IWorkspaceMembersRepo workspacesMemberRepo,
            IBoardsRepo boardsRepo,
            IBoardMemberRepo boardMemberRepo,
            IUsersService usersService
        )
        {
            _workspacesMemberRepo = workspacesMemberRepo;
            _boardsRepo = boardsRepo;
            _boardMemberRepo = boardMemberRepo;
            _usersService = usersService;
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
            var userIdOfNewMember = await _usersService.GetUserIdByEmailAsync(
                newWorspaceMember.Email
            );

            if (userIdOfNewMember is null || userIdOfNewMember.Equals(currentUserId))
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
                    userIdOfNewMember
                );

            if (isUserAMember)
                return false;

            WorkspaceMember newMember =
                new()
                {
                    UserId = userIdOfNewMember,
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
                    UserId = userIdOfNewMember,
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
            DtoWorkspaceMemberUpdate updateWorkspaceMember
        )
        {
            return await _workspacesMemberRepo.Patch(id, updateWorkspaceMember);
        }

        public async Task<bool> RemoveByIdAsync(string id) =>
            await _workspacesMemberRepo.RemoveById(id);
    }
}
