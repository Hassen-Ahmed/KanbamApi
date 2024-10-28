using System.ComponentModel.DataAnnotations;
using KanbamApi.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class Workspace
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string Id { get; set; } = string.Empty;

        [BsonRequired]
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
