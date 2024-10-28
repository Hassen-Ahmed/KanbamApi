using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class BoardMember
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("BoardId")]
        public string BoardId { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; }
    }
}
