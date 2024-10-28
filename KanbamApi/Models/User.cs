using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Required]
    public string Id { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("Email")]
    public string? Email { get; set; }
    public string? UserName { get; set; }
}
