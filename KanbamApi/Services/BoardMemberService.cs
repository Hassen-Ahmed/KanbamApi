using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;

namespace KanbamApi.Services
{
    public class BoardMemberService : IBoardMemberService
    {
        private readonly IBoardsRepo _boardRepo;
        private readonly IBoardMemberRepo _boardMemberRepo;
        private readonly IKanbamDbContext _kanbamDbContext;

        public BoardMemberService(
            IBoardsRepo boardsRepo,
            IBoardMemberRepo boardMemberRepo,
            IKanbamDbContext kanbamDbContext
        )
        {
            _boardRepo = boardsRepo;
            _boardMemberRepo = boardMemberRepo;
            _kanbamDbContext = kanbamDbContext;
        }

        public async Task<List<BoardMember>> GetAllAsync()
        {
            return await _boardMemberRepo.GetAll();
        }

        public async Task<BoardMember> CreateAsync(BoardMember newBoardMember)
        {
            return await _boardMemberRepo.Create(newBoardMember);
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
