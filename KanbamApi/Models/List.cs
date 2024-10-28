using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class List
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Required]
    public string Id { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("BoardId")]
    [Required]
    public string BoardId { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("Title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("IndexNumber")]
    [Required]
    public int IndexNumber { get; set; }
}
