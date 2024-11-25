using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models
{
    public class ListWithCards
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string BoardId { get; set; } = string.Empty;

        [BsonRequired]
        [BsonElement("IndexNumber")]
        [Required]
        public int IndexNumber { get; set; }

        [Required]
        public List<Card>? Cards { get; set; }
    }
}
