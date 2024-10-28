using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Required]
    public string Id { get; set; }

    [BsonRequired]
    [BsonElement("Email")]
    public string? Email { get; set; } = null!;
    public string? UserName { get; set; } = null!;
}
