using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KanbamApi.Repositories
{
    public class BoardsRepo : IBoardsRepo
    {
        private readonly IKanbamDbContext _kanbamDbContext;

        public BoardsRepo(IKanbamDbContext kanbamDbContext)
        {
            _kanbamDbContext = kanbamDbContext;
        }

        public async Task<List<Board>> GetAll()
        {
            var filter = Builders<Board>.Filter.Empty;
            return await _kanbamDbContext.BoardsCollection.FindSync(filter).ToListAsync();
        }

        public async Task<bool> IsBoardIdExistByBoardId(string boardId)
        {
            var filter = Builders<Board>.Filter.Eq(b => b.Id, boardId);
            return await _kanbamDbContext.BoardsCollection.Find(filter).AnyAsync();
        }

        public async Task<bool> IsBoardsExist_ByWorkspaceId(string workspaceId)
        {
            var filter = Builders<Board>.Filter.Eq(b => b.WorkspaceId, workspaceId);
            return await _kanbamDbContext.BoardsCollection.Find(filter).AnyAsync();
        }

        public async Task<string?> GetWorkspaceIdByBoardId(string boardId)
        {
            if (string.IsNullOrWhiteSpace(boardId))
                return null;

            var filter = Builders<Board>.Filter.Eq(b => b.Id, boardId);
            var board = await _kanbamDbContext.BoardsCollection.Find(filter).FirstOrDefaultAsync();

            return board?.WorkspaceId;
        }

        public async Task<List<BoardWithMemberDetails>> GetAllBoards_WithMembers_ByWorkspaceId(
            string workspaceId,
            string userId
        )
        {
            var workspaceObjectId = new ObjectId(workspaceId);
            var userObjectId = new ObjectId(userId);

            var pipeline = new List<BsonDocument>();

            pipeline.Add(new BsonDocument("$match", new BsonDocument("UserId", userObjectId)));

            pipeline.Add(
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        { "from", "Boards" },
                        { "localField", "BoardId" },
                        { "foreignField", "_id" },
                        { "as", "boardsDetails" }
                    }
                )
            );

            pipeline.Add(new BsonDocument("$unwind", "$boardsDetails"));

            pipeline.Add(
                new BsonDocument(
                    "$project",
                    new BsonDocument
                    {
                        { "_id", 0 },
                        { "BoardId", "$boardsDetails._id" },
                        { "WorkspaceId", "$boardsDetails.WorkspaceId" },
                        { "Name", "$boardsDetails.Name" },
                        { "Description", "$boardsDetails.Description" },
                        { "Role", "$Role" },
                    }
                )
            );

            pipeline.Add(
                new BsonDocument("$match", new BsonDocument("WorkspaceId", workspaceObjectId))
            );

            var result = await _kanbamDbContext
                .BoardMembersCollection.Aggregate<BoardWithMemberDetails>(pipeline)
                .ToListAsync();
            return result;

            // var pipeline = new[]
            // {
            //     new BsonDocument("$match", new BsonDocument("UserId", userObjectId)),
            //     new BsonDocument(
            //         "$lookup",
            //         new BsonDocument
            //         {
            //             { "from", "Boards" },
            //             { "localField", "BoardId" },
            //             { "foreignField", "_id" },
            //             { "as", "boardsDetails" }
            //         }
            //     ),
            //     new BsonDocument("$unwind", "$boardsDetails"),
            //     new BsonDocument(
            //         "$project",
            //         new BsonDocument
            //         {
            //             { "_id", 0 },
            //             { "BoardId", "$boardsDetails._id" },
            //             { "WorkspaceId", "$boardsDetails.WorkspaceId" },
            //             { "Name", "$boardsDetails.Name" },
            //             { "Description", "$boardsDetails.Description" },
            //             { "Role", "$Role" },
            //         }
            //     ),
            //     new BsonDocument("$match", new BsonDocument("WorkspaceId", workspaceObjectId)),
            // };

            // var result = await _kanbamDbContext
            //     .BoardMembersCollection.Aggregate<BoardWithMemberDetails>(pipeline)
            //     .ToListAsync();
            // return result;
        }

        public async Task<List<Board>> Get_OnlyBoards_ByWorkspaceId(string workspaceId)
        {
            var filter = Builders<Board>.Filter.Eq(b => b.WorkspaceId, workspaceId);
            return await _kanbamDbContext.BoardsCollection.Find(filter).ToListAsync();
        }

        public async Task<Board> Create(Board newBoard)
        {
            await _kanbamDbContext.BoardsCollection.InsertOneAsync(newBoard);
            return newBoard;
        }

        public async Task<bool> Patch(string boardId, DtoBoardUpdate updateBoard)
        {
            var updateDefinitionBuilder = Builders<Board>.Update;
            var updateDefinition = new List<UpdateDefinition<Board>>();

            if (!string.IsNullOrEmpty(updateBoard.Name))
            {
                updateDefinition.Add(updateDefinitionBuilder.Set(b => b.Name, updateBoard.Name));
            }
            if (!string.IsNullOrEmpty(updateBoard.WorkspaceId))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(b => b.WorkspaceId, updateBoard.WorkspaceId)
                );
            }
            if (!string.IsNullOrEmpty(updateBoard.Description))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(b => b.Description, updateBoard.Description)
                );
            }
            if (updateDefinition.Count == 0)
            {
                return false;
            }

            var filter = Builders<Board>.Filter.Eq(b => b.Id, boardId);

            var result = await _kanbamDbContext.BoardsCollection.UpdateOneAsync(
                filter,
                updateDefinitionBuilder.Combine(updateDefinition)
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveByBoardId(string boardId)
        {
            var filter = Builders<Board>.Filter.Eq(b => b.Id, boardId);
            var res = await _kanbamDbContext.BoardsCollection.DeleteOneAsync(filter);
            return res.DeletedCount > 0;
        }

        public async Task<bool> RemoveMany_With_Members_ByWorkspaceId(string workspaceId)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var boardFilter = Builders<Board>.Filter.Eq(b => b.WorkspaceId, workspaceId);

                var boardIds = await _kanbamDbContext
                    .BoardsCollection.Find(boardFilter)
                    .Project(b => b.Id)
                    .ToListAsync();

                if (boardIds.Count == 0)
                {
                    await session.AbortTransactionAsync();
                    return false;
                }

                var boardMembersFilter = Builders<BoardMember>.Filter.In(
                    bm => bm.BoardId,
                    boardIds
                );
                var deletedBoardMembers =
                    await _kanbamDbContext.BoardMembersCollection.DeleteManyAsync(
                        boardMembersFilter
                    );

                var deletedBoards = await _kanbamDbContext.BoardsCollection.DeleteManyAsync(
                    boardFilter
                );

                await session.CommitTransactionAsync();
                return deletedBoardMembers.DeletedCount > 0 || deletedBoards.DeletedCount > 0;
            }
            catch (MongoException ex)
            {
                await session.AbortTransactionAsync();
                Console.WriteLine($"MongoException occurred: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}
