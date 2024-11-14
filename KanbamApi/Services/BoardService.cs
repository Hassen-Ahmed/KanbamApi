using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Posts;
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
        private readonly IWorkspaceMembersRepo _workspaceMembersRepo;
        private readonly IKanbamDbContext _kanbamDbContext;

        public BoardService(
            IBoardsRepo boardsRepo,
            IBoardMemberRepo boardMemberRepo,
            IWorkspaceMembersRepo workspaceMembersRepo,
            IKanbamDbContext kanbamDbContext
        )
        {
            _boardRepo = boardsRepo;
            _boardMemberRepo = boardMemberRepo;
            _workspaceMembersRepo = workspaceMembersRepo;
            _kanbamDbContext = kanbamDbContext;
        }

        public async Task<List<Board>> GetAllAsync() => await _boardRepo.GetAll();

        public async Task<bool> IsBoardIdExistByBoardIdAsync(string boardId)
        {
            return await _boardRepo.IsBoardIdExistByBoardId(boardId);
        }

        public async Task<string?> GetWorkspaceIdByBoardIdAsync(string boardId)
        {
            return await _boardRepo.GetWorkspaceIdByBoardId(boardId);
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

                return await _boardRepo.GetAllBoards_WithMembers_ByWorkspaceId(workspaceId, userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreateAsync(string userId, DtoBoardPost newBoard)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                Board board =
                    new()
                    {
                        Name = newBoard.Name,
                        WorkspaceId = newBoard.WorkspaceId,
                        Description = newBoard.Description
                    };

                var createdBoard = await _boardRepo.Create(board);

                var workspaceMembers = await _workspaceMembersRepo.Get_Members_By_WorkspaceId(
                    newBoard.WorkspaceId
                );

                var boardMembers = workspaceMembers
                    .Select(wm => new BoardMember
                    {
                        BoardId = createdBoard.Id,
                        UserId = wm.UserId,
                        Role = wm.Role
                    })
                    .ToList();

                if (boardMembers.Count > 0)
                {
                    await _boardMemberRepo.CreateMany(boardMembers);
                }

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
                var isBoardDeleted = await _boardRepo.RemoveByBoardId(boardId);
                var isBoardMemberDeleted = await _boardMemberRepo.RemoveByBoardId(boardId);
                // await _listsService.RemoveByBoardId(boardId);

                await session.CommitTransactionAsync();
                return isBoardDeleted && isBoardMemberDeleted;
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

        public async Task<bool> RemoveManyByWorkspaceIdAsync(string workspaceId)
        {
            var areBoardsDeleted = await _boardRepo.RemoveMany_With_Members_ByWorkspaceId(
                workspaceId
            );
            return areBoardsDeleted;
        }
    }
}
