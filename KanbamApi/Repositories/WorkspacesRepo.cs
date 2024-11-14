using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Patch;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KanbamApi.Repositories
{
    public class WorkspacesRepo : IWorkspacesRepo
    {
        private readonly IKanbamDbContext _kanbamDbContext;
        private readonly IBoardsRepo _boardsRepo;

        public WorkspacesRepo(IKanbamDbContext kanbamDbContext, IBoardsRepo boardsRepo)
        {
            _kanbamDbContext = kanbamDbContext;
            _boardsRepo = boardsRepo;
        }

        public async Task<bool> IsWorkspaceExist_Using_WorkspaceId(string workspaceId)
        {
            var filter = Builders<Workspace>.Filter.Eq(w => w.Id, workspaceId);
            return await _kanbamDbContext.WorkspacesCollection.Find(filter).AnyAsync();
        }

        public async Task<IEnumerable<WorkspaceWithMemberDetails>> GetAllWorkspace_ByUserId(
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
                        { "from", "Workspaces" },
                        { "localField", "WorkspaceId" },
                        { "foreignField", "_id" },
                        { "as", "workspaceDetails" }
                    }
                ),
                new BsonDocument("$unwind", "$workspaceDetails"),
                new BsonDocument(
                    "$project",
                    new BsonDocument
                    {
                        { "_id", 0 },
                        { "WorkspaceId", "$workspaceDetails._id" },
                        { "Name", "$workspaceDetails.Name" },
                        { "Description", "$workspaceDetails.Description" },
                        { "Role", "$Role" },
                        { "BoardAccessLevel", "$BoardAccessLevel" },
                    }
                )
            };

            var result = await _kanbamDbContext
                .WorkspaceMembersCollection.Aggregate<WorkspaceWithMemberDetails>(pipeline)
                .ToListAsync();

            return result;
        }

        public async Task<Workspace> Create(Workspace newWorspace)
        {
            await _kanbamDbContext.WorkspacesCollection.InsertOneAsync(newWorspace);
            return newWorspace;
        }

        public async Task<bool> Patch(string workspaceId, DtoWorkspaceUpdate updateWorkspace)
        {
            var updateDefinitionBuilder = Builders<Workspace>.Update;
            var updateDefinition = new List<UpdateDefinition<Workspace>>();

            if (!string.IsNullOrEmpty(updateWorkspace.Name))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(w => w.Name, updateWorkspace.Name)
                );
            }

            if (!string.IsNullOrEmpty(updateWorkspace.Description))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(w => w.Description, updateWorkspace.Description)
                );
            }

            if (updateDefinition.Count == 0)
            {
                return false;
            }

            var filter = Builders<Workspace>.Filter.Eq(w => w.Id, workspaceId);

            var result = await _kanbamDbContext.WorkspacesCollection.UpdateOneAsync(
                filter,
                updateDefinitionBuilder.Combine(updateDefinition)
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> Remove_With_Members(string workspaceId)
        {
            using var session = await _kanbamDbContext.MongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // worksapces
                var workspaceFileter = Builders<Workspace>.Filter.Eq(w => w.Id, workspaceId);
                var deletedWorkspaces = await _kanbamDbContext.WorkspacesCollection.DeleteOneAsync(
                    session,
                    workspaceFileter
                );

                var workspaceMemberFileter = Builders<WorkspaceMember>.Filter.Eq(
                    wm => wm.WorkspaceId,
                    workspaceId
                );

                var deletedWorkspaceMember =
                    await _kanbamDbContext.WorkspaceMembersCollection.DeleteManyAsync(
                        session,
                        workspaceMemberFileter
                    );

                if (deletedWorkspaces.DeletedCount == 0 || deletedWorkspaceMember.DeletedCount == 0)
                {
                    await session.AbortTransactionAsync();
                    return false;
                }

                // check boards exist with this workspaceId
                var boardFilter = Builders<Board>.Filter.Eq(b => b.WorkspaceId, workspaceId);
                var boardsExist =
                    await _kanbamDbContext.BoardsCollection.CountDocumentsAsync(
                        session,
                        boardFilter
                    ) > 0;

                if (boardsExist)
                {
                    // boards and boardsMember
                    var boardsAndMembersDeletionResult =
                        await _boardsRepo.RemoveMany_With_Members_ByWorkspaceId(workspaceId);

                    if (!boardsAndMembersDeletionResult)
                    {
                        await session.AbortTransactionAsync();
                        return false;
                    }
                }

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
