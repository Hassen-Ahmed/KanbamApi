using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;

namespace KanbamApi.Services
{
    public class BoardMemberService : IBoardMemberService
    {
        private readonly IBoardMemberRepo _boardMemberRepo;
        private readonly IKanbamDbContext _kanbamDbContext;
        private readonly IUsersService _usersService;
        private readonly IBoardService _boardService;
        private readonly IWorkspaceMembersRepo _workspaceMembersRepo;

        public BoardMemberService(
            IBoardMemberRepo boardMemberRepo,
            IKanbamDbContext kanbamDbContext,
            IUsersService usersService,
            IBoardService boardService,
            IWorkspaceMembersRepo workspaceMembersRepo
        )
        {
            _boardService = boardService;
            _boardMemberRepo = boardMemberRepo;
            _kanbamDbContext = kanbamDbContext;
            _usersService = usersService;
            _workspaceMembersRepo = workspaceMembersRepo;
        }

        public async Task<List<BoardMember>> GetAllAsync()
        {
            return await _boardMemberRepo.GetAll();
        }

        public async Task<bool> CreateAsync(
            DtoBoardMemberPost newBoardMember,
            string? currentUserId
        )
        {
            var userIdOfNewMember = await _usersService.GetUserIdByEmailAsync(newBoardMember.Email);

            if (userIdOfNewMember is null || userIdOfNewMember.Equals(currentUserId))
            {
                return false;
            }

            var workspaceId = await _boardService.GetWorkspaceIdByBoardIdAsync(
                newBoardMember.BoardId
            );

            if (workspaceId is null)
            {
                return false;
            }

            BoardMember newMember =
                new()
                {
                    UserId = userIdOfNewMember,
                    BoardId = newBoardMember.BoardId,
                    Role = newBoardMember.Role,
                };

            WorkspaceMember newWorkspaceMember =
                new()
                {
                    UserId = userIdOfNewMember,
                    WorkspaceId = workspaceId,
                    Role = newBoardMember.Role,
                    BoardAccessLevel = "Some"
                };

            await _boardMemberRepo.Create(newMember);
            await _workspaceMembersRepo.Create(newWorkspaceMember);

            return true;
        }

        public async Task<bool> PatchByIdAsync(
            string workspaceId,
            DtoBoardMemberUpdate updateBoardMember
        )
        {
            return await _boardMemberRepo.Patch(workspaceId, updateBoardMember);
        }

        public async Task<bool> RemoveById(string id)
        {
            return await _boardMemberRepo.RemoveById(id);
        }
    }
}
