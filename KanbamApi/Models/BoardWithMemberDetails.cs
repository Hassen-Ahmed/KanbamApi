using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class BoardWithMemberDetails
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string BoardId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string WorkspaceId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
