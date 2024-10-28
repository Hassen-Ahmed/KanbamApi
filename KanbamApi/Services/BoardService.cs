using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardsRepo _boardRepo;
        private readonly IBoardMemberRepo _boardMemberRepo;
        private readonly IKanbamDbContext _kanbamDbContext;

        public BoardService(
            IBoardsRepo boardsRepo,
            IBoardMemberRepo boardMemberRepo,
            IKanbamDbContext kanbamDbContext
        )
        {
            _boardRepo = boardsRepo;
            _boardMemberRepo = boardMemberRepo;
            _kanbamDbContext = kanbamDbContext;
        }

        public async Task<List<BoardWithMemberDetails>> GetBoards_With_Members_ByWorkspaceId_Async(
            string workspaceId,
            string userId
        )
        {
            try
            {
                if (string.IsNullOrEmpty(workspaceId))
                {
                    throw new ArgumentNullException(
                        nameof(workspaceId),
                        "Provided ID cannot be null or empty."
                    );
                }
                return await _boardRepo.GetAllBoards_ByUserId(workspaceId, userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Board>> GetAllAsync() => await _boardRepo.GetAll();

        public async Task CreateAsync(string userId, Board newBoard)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var createdBoard = await _boardRepo.Create(newBoard);

                var boardId = createdBoard.Id;

                BoardMember newMember =
                    new()
                    {
                        UserId = userId,
                        BoardId = boardId,
                        Role = "Admin",
                    };

                await _boardMemberRepo.Create(newMember);

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

        public async Task<bool> PatchByIdAsync(string boardId, DtoBoardUpdate updateBoard)
        {
            return await _boardRepo.Patch(boardId, updateBoard);
        }

        public async Task<bool> RemoveByIdAsync(string boardId)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();
            try
            {
                var resultOne = await _boardRepo.RemoveByBoardId(boardId);
                var resultTwo = await _boardMemberRepo.RemoveByBoardId(boardId);
                // await _listsService.RemoveByBoardId(boardId);

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
