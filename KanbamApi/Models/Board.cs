using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class Board
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("WorkspaceId")]
        public string WorkspaceId { get; set; }

        [BsonElement("Description")]
        public string? Description { get; set; }
    }
}
