using KanbamApi.Dtos;
using KanbamApi.Dtos.Posts;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace KanbamApi.Services
{
    public class BoardMemberService : IBoardMemberService
    {
        private readonly IBoardMemberRepo _boardMemberRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBoardService _boardService;
        private readonly IWorkspaceMembersRepo _workspaceMembersRepo;

        public BoardMemberService(
            IBoardMemberRepo boardMemberRepo,
            UserManager<ApplicationUser> userManager,
            IBoardService boardService,
            IWorkspaceMembersRepo workspaceMembersRepo
        )
        {
            _boardService = boardService;
            _boardMemberRepo = boardMemberRepo;
            _userManager = userManager;
            _workspaceMembersRepo = workspaceMembersRepo;
        }

        public async Task<List<BoardMember>> GetAllAsync()
        {
            return await _boardMemberRepo.GetAll();
        }

        public async Task<List<DtoBoardWithMemberGet>> GetMembersByBoardIdAsync(string boardId)
        {
            var members = await _boardMemberRepo.GetAllByBoardId(boardId);
            return members;
        }

        public async Task<bool> CreateAsync(
            DtoBoardMemberPost newBoardMember,
            string? currentUserId
        )
        {
            var user = await _userManager.FindByEmailAsync(newBoardMember.Email);

            if (user is null || user.Id.ToString() == currentUserId)
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
                    UserId = user.Id.ToString(),
                    BoardId = newBoardMember.BoardId,
                    Role = newBoardMember.Role,
                };

            WorkspaceMember newWorkspaceMember =
                new()
                {
                    UserId = user.Id.ToString(),
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
            DtoBoardMemberUpdate updateBoardMember,
            string currentUserId
        )
        {
            var isUserAdmin = await _boardMemberRepo.Is_User_Admin_ByUserId(currentUserId);
            return isUserAdmin && await _boardMemberRepo.Patch(workspaceId, updateBoardMember);
        }

        public async Task<bool> RemoveById(string id, string currentUserId)
        {
            var isUserAdmin = await _boardMemberRepo.Is_User_Admin_ByUserId(currentUserId);
            return isUserAdmin && await _boardMemberRepo.RemoveById(id);
        }
    }
}
