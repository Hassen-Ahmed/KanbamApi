using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class List
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("BoardId")]
    public string BoardId { get; set; }

    [BsonRequired]
    [BsonElement("Title")]
    public string Title { get; set; }

    [BsonRequired]
    [BsonElement("IndexNumber")]
    public int IndexNumber { get; set; }
}
