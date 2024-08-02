using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class Visitor
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("IpAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("Timestamp")]
    public List<DateTime>? Timestamp { get; set; }

    [BsonElement("HttpMethod")]
    public List<string>? HttpMethod { get; set; }

    [BsonElement("Path")]
    public List<string>? Path { get; set; }

    [BsonElement("UserAgent")]
    public List<string>? UserAgent { get; set; }

    [BsonElement("UserId")]
    public string? UserId { get; set; }

    public Visitor()
    {
        Timestamp ??= [];
        HttpMethod ??= [];
        Path ??= [];
        UserAgent ??= [];
    }
}
