using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class List
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("UserId")]
    public string? UserId { get; set; }

    [BsonElement("Title")]
    public string? Title { get; set; }

    [BsonElement("IndexNumber")]
    public int IndexNumber { get; set; }

    [BsonElement("Cards")]
    public List<Card>? Cards { get; set; }

    public List()
    {
        Cards ??= [];
    }
}
