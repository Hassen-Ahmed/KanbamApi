using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos;
using KanbamApi.Dtos.Put;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KanbamApi.Repositories
{
    public class WorkspaceMembersRepo : IWorkspaceMembersRepo
    {
        private readonly IKanbamDbContext _kanbamDbContext;

        public WorkspaceMembersRepo(IKanbamDbContext kanbamDbContext) =>
            _kanbamDbContext = kanbamDbContext;

        public async Task<List<WorkspaceMember>> GetAsyc()
        {
            var filter = Builders<WorkspaceMember>.Filter.Empty;
            return await _kanbamDbContext.WorkspaceMembersCollection.FindSync(filter).ToListAsync();
        }

        public async Task<List<WorkspaceMember>> Get_Members_By_WorkspaceId(string workspaceId)
        {
            var filter = Builders<WorkspaceMember>.Filter.And(
                Builders<WorkspaceMember>.Filter.Eq(wm => wm.WorkspaceId, workspaceId),
                Builders<WorkspaceMember>.Filter.Eq(wm => wm.BoardAccessLevel, "All")
            );
            return await _kanbamDbContext.WorkspaceMembersCollection.Find(filter).ToListAsync();
        }

        public async Task<string?> Get_Role_ByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var filter = Builders<WorkspaceMember>.Filter.Eq(wm => wm.UserId, userId);
            var member = await _kanbamDbContext
                .WorkspaceMembersCollection.Find(filter)
                .FirstOrDefaultAsync();

            return member?.Role;
        }

        public async Task<List<DtoWorkspaceWithMemberGet>> GetMembersByWorkspaceId(
            string workspaceId
        )
        {
            var workspaceIdObject = new ObjectId(workspaceId);

            var pipeline = new[]
            {
                new BsonDocument(
                    "$match",
                    new BsonDocument
                    {
                        { "WorkspaceId", workspaceIdObject },
                        { "BoardAccessLevel", "All" }
                    }
                ),
                new BsonDocument(
                    "$lookup",
                    new BsonDocument
                    {
                        { "from", "Users" },
                        { "localField", "UserId" },
                        { "foreignField", "_id" },
                        { "as", "workspaceDetails" }
                    }
                ),
                new BsonDocument("$unwind", "$workspaceDetails"),
                new BsonDocument(
                    "$project",
                    new BsonDocument
                    {
                        { "_id", 1 },
                        { "UserId", "$workspaceDetails._id" },
                        { "UserName", "$workspaceDetails.UserName" },
                        { "Email", "$workspaceDetails.Email" },
                        { "WorkspaceId", "$WorkspaceId" },
                        { "BoardAccessLevel", "$BoardAccessLevel" },
                        { "Role", "$Role" },
                    }
                ),
            };

            var result = await _kanbamDbContext
                .WorkspaceMembersCollection.Aggregate<DtoWorkspaceWithMemberGet>(pipeline)
                .ToListAsync();

            return result;
        }

        public async Task<bool> IsUserAMember_Using_WorkspaceId_And_UserId(
            string workspaceId,
            string userId
        )
        {
            var filter = Builders<WorkspaceMember>.Filter.And(
                Builders<WorkspaceMember>.Filter.Eq(w => w.WorkspaceId, workspaceId),
                Builders<WorkspaceMember>.Filter.Eq(w => w.UserId, userId)
            );

            return await _kanbamDbContext.WorkspaceMembersCollection.Find(filter).AnyAsync();
        }

        public async Task<bool> Is_User_Admin_ByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var filter = Builders<WorkspaceMember>.Filter.And(
                Builders<WorkspaceMember>.Filter.Eq(bm => bm.UserId, userId),
                Builders<WorkspaceMember>.Filter.Eq(bm => bm.Role, "Admin")
            );

            var isAdmin = await _kanbamDbContext.WorkspaceMembersCollection.Find(filter).AnyAsync();

            return isAdmin;
        }

        public async Task<WorkspaceMember> Create(WorkspaceMember newWorkspaceMember)
        {
            await _kanbamDbContext.WorkspaceMembersCollection.InsertOneAsync(newWorkspaceMember);
            return newWorkspaceMember;
        }

        public async Task<bool> Patch(string id, DtoWorkspaceMemberUpdate updateWorkspaceMember)
        {
            var updateDefinitionBuilder = Builders<WorkspaceMember>.Update;
            var updateDefinition = new List<UpdateDefinition<WorkspaceMember>>();

            if (!string.IsNullOrEmpty(updateWorkspaceMember.Role))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(w => w.Role, updateWorkspaceMember.Role)
                );
            }

            if (updateDefinition.Count == 0)
            {
                return false;
            }

            var filter = Builders<WorkspaceMember>.Filter.Eq(w => w.Id, id);

            var result = await _kanbamDbContext.WorkspaceMembersCollection.UpdateOneAsync(
                filter,
                updateDefinitionBuilder.Combine(updateDefinition)
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveById(string id)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var filterWorkspace = Builders<WorkspaceMember>.Filter.Eq(wm => wm.Id, id);

                var member = await _kanbamDbContext
                    .WorkspaceMembersCollection.Find(filterWorkspace)
                    .FirstOrDefaultAsync();

                if (member == null)
                {
                    await session.AbortTransactionAsync();
                    return false;
                }

                var deleteResult = await _kanbamDbContext.WorkspaceMembersCollection.DeleteOneAsync(
                    filterWorkspace
                );

                if (deleteResult.DeletedCount == 0)
                {
                    await session.AbortTransactionAsync();
                    return false;
                }

                var userId = member.UserId;

                var boardMembersFilter = Builders<BoardMember>.Filter.Eq(bm => bm.UserId, userId);

                var deletedBoardMembers =
                    await _kanbamDbContext.BoardMembersCollection.DeleteManyAsync(
                        boardMembersFilter
                    );

                await session.CommitTransactionAsync();
                return true;
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
