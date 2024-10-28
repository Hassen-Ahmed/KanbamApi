using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class Board
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement("Name")]
        [Required]
        public string Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("WorkspaceId")]
        [Required]
        public string WorkspaceId { get; set; }

        [BsonElement("Description")]
        public string? Description { get; set; }
    }
}
