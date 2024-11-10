using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Dtos
{
    public class DtoWorkspaceWithMemberGet
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        [Required]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string WorkspaceId { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string BoardAccessLevel { get; set; } = string.Empty;
    }
}
