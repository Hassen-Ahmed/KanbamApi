using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class Card
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ListId { get; set; }

    [BsonRequired]
    [BsonElement("Title")]
    public string? Title { get; set; }

    [BsonRequired]
    [BsonElement("IndexNumber")]
    public int IndexNumber { get; set; }

    [BsonElement("Description")]
    public string? Description { get; set; }

    [BsonElement("Priority")]
    public string? Priority { get; set; }

    [BsonElement("Comments")]
    public List<string>? Comments { get; set; }

    [BsonElement("StartDate")]
    public DateTime? StartDate { get; set; }

    [BsonElement("DueDate")]
    public DateTime? DueDate { get; set; }

    [BsonElement("DueDateReminder")]
    public DateTime? DueDateReminder { get; set; }

    public Card()
    {
        Comments ??= [];
    }
}
