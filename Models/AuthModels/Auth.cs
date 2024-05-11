using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class Auth
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? Email { get; set; } = null!;

    [BsonRepresentation(BsonType.Binary)]
    public byte[] PasswordHash { get; set; }

    [BsonRepresentation(BsonType.Binary)]
    public byte[] PasswordSalt { get; set; }

    public Auth()
    {
        PasswordHash ??= [];
        PasswordSalt ??= [];
    }
}
