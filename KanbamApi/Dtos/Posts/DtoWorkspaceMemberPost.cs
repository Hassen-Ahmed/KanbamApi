using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Dtos.Posts
{
    public class DtoWorkspaceMemberPost
    {
        [BsonRequired]
        public string workspaceId { get; set; }

        [BsonRequired]
        public string Role { get; set; }
    }
}
