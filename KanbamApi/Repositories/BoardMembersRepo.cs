using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Repositories
{
    public class BoardMemberRepo : IBoardMemberRepo
    {
        private readonly IKanbamDbContext _kanbamDbContext;

        public BoardMemberRepo(IKanbamDbContext kanbamDbContext)
        {
            _kanbamDbContext = kanbamDbContext;
        }

        public async Task<List<BoardMember>> GetAll()
        {
            var filter = Builders<BoardMember>.Filter.Empty;
            return await _kanbamDbContext.BoardMembersCollection.FindSync(filter).ToListAsync();
        }

        public async Task<bool> IsUserIdExist(string userId)
        {
            var filter = Builders<BoardMember>.Filter.Eq(b => b.UserId, userId);
            return await _kanbamDbContext.BoardMembersCollection.Find(filter).AnyAsync();
        }

        public async Task<BoardMember> Create(BoardMember newBoardMember)
        {
            await _kanbamDbContext.BoardMembersCollection.InsertOneAsync(newBoardMember);
            return newBoardMember;
        }

        public async Task<List<BoardMember>> CreateMany(List<BoardMember> newBoardMemberList)
        {
            await _kanbamDbContext.BoardMembersCollection.InsertManyAsync(newBoardMemberList);
            return newBoardMemberList;
        }

        public async Task<bool> Patch(string boardMemberId, DtoBoardMemberUpdate updateBoardMember)
        {
            var updateDefinitionBuilder = Builders<BoardMember>.Update;
            var updateDefinition = new List<UpdateDefinition<BoardMember>>();

            if (!string.IsNullOrEmpty(updateBoardMember.UserId))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(bm => bm.UserId, updateBoardMember.UserId)
                );
            }

            if (!string.IsNullOrEmpty(updateBoardMember.BoardId))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(bm => bm.BoardId, updateBoardMember.BoardId)
                );
            }
            if (!string.IsNullOrEmpty(updateBoardMember.Role))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(bm => bm.Role, updateBoardMember.Role)
                );
            }

            if (updateDefinition.Count == 0)
            {
                return false;
            }

            var filter = Builders<BoardMember>.Filter.Eq(bm => bm.Id, boardMemberId);

            var result = await _kanbamDbContext.BoardMembersCollection.UpdateOneAsync(
                filter,
                updateDefinitionBuilder.Combine(updateDefinition)
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveById(string id)
        {
            var filter = Builders<BoardMember>.Filter.Eq(bm => bm.Id, id);
            var res = await _kanbamDbContext.BoardMembersCollection.DeleteOneAsync(filter);

            return res.DeletedCount > 0;
        }

        public async Task<bool> RemoveByBoardId(string boardId)
        {
            var filter = Builders<BoardMember>.Filter.Eq(bm => bm.BoardId, boardId);
            var res = await _kanbamDbContext.BoardMembersCollection.DeleteOneAsync(filter);

            return res.DeletedCount > 0;
        }
    }
}
