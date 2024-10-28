using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class WorkspaceMember
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string WorkspaceId { get; set; }

        [BsonRequired]
        [Required]
        public string Role { get; set; }
    }
}
