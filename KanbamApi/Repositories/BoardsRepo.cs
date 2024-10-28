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

        public async Task<List<BoardWithMemberDetails>> GetAllBoards_ByUserId(
            string workspaceId,
            string userId
        )
        {
            var userIdObject = new ObjectId(userId);

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("UserId", userIdObject)),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        { "from", "Boards" },
                        { "localField", "BoardId" },
                        { "foreignField", "_id" },
                        { "as", "boardsDetails" }
                    }
                ),
                new BsonDocument("$unwind", "$boardsDetails"),
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
            };

            var result = await _kanbamDbContext
                .BoardMembersCollection.Aggregate<BoardWithMemberDetails>(pipeline)
                .ToListAsync();
            return result;
        }

        public async Task<List<Board>> GetAll()
        {
            var filter = Builders<Board>.Filter.Empty;
            return await _kanbamDbContext.BoardsCollection.FindSync(filter).ToListAsync();
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
    }
}
