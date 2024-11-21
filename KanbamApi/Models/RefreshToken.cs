using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonRequired]
    public string UserId { get; set; } = string.Empty;

    [BsonRequired]
    public Guid Token { get; set; }

    [BsonRequired]
    public DateTime TokenExpiryTime { get; set; }
}
