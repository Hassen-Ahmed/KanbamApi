using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class BoardWithMemberDetails
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string BoardId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string WorkspaceId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
