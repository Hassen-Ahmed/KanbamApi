using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class WorkspaceWithMemberDetails
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string WorkspaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Role { get; set; }
    }
}
