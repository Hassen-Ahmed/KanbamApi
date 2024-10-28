using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Put;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
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

        public async Task<WorkspaceMember> Create(WorkspaceMember newWorkspaceMember)
        {
            await _kanbamDbContext.WorkspaceMembersCollection.InsertOneAsync(newWorkspaceMember);
            return newWorkspaceMember;
        }

        public async Task<bool> Patch(string id, DtoWorkspaceMemberUpdate updateWorkspaceMember)
        {
            var updateDefinitionBuilder = Builders<WorkspaceMember>.Update;
            var updateDefinition = new List<UpdateDefinition<WorkspaceMember>>();

            if (!string.IsNullOrEmpty(updateWorkspaceMember.WorkspaceId))
            {
                updateDefinition.Add(
                    updateDefinitionBuilder.Set(
                        w => w.WorkspaceId,
                        updateWorkspaceMember.WorkspaceId
                    )
                );
            }

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
            var filter = Builders<WorkspaceMember>.Filter.Eq(wm => wm.Id, id);
            var res = await _kanbamDbContext.WorkspaceMembersCollection.DeleteOneAsync(filter);
            return res.DeletedCount > 0;
        }

        public async Task<bool> RemoveByWorkspaceId(string workspaceId)
        {
            var filter = Builders<WorkspaceMember>.Filter.Eq(wm => wm.WorkspaceId, workspaceId);
            var res = await _kanbamDbContext.WorkspaceMembersCollection.DeleteOneAsync(filter);
            return res.DeletedCount > 0;
        }
    }
}
