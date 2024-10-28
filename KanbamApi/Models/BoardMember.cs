using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class BoardMember
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("UserId")]
        [Required]
        public string UserId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("BoardId")]
        [Required]
        public string BoardId { get; set; } = string.Empty;

        [BsonElement("Role")]
        public string Role { get; set; } = string.Empty;
    }
}
