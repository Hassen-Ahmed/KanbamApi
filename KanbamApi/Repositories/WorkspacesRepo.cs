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

        public WorkspacesRepo(IKanbamDbContext kanbamDbContext)
        {
            _kanbamDbContext = kanbamDbContext;
        }

        // public async Task<IEnumerable<Workspace>> GetById(string workspaceId)
        // {
        //     var filter = Builders<Workspace>.Filter.Eq(w => w.Id, workspaceId);
        //     var res = await _kanbamDbContext.WorkspacesCollection.Find(filter).ToListAsync();
        //     return res;
        // }

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

        public async Task<bool> Remove(string id)
        {
            var filter = Builders<Workspace>.Filter.Eq(w => w.Id, id);
            var res = await _kanbamDbContext.WorkspacesCollection.DeleteOneAsync(filter);
            return res.DeletedCount > 0;
        }
    }
}
