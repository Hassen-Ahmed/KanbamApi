using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class Card
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Required]
    public string Id { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    [Required]
    public string ListId { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("Title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    [BsonRequired]
    [BsonElement("IndexNumber")]
    public int IndexNumber { get; set; }

    [BsonElement("Description")]
    public string? Description { get; set; }

    [BsonElement("Priority")]
    public string? Priority { get; set; }

    [BsonElement("Comments")]
    public List<Comment> Comments { get; set; } = new List<Comment>();

    [BsonElement("StartDate")]
    public DateTime? StartDate { get; set; }

    [BsonElement("DueDate")]
    public DateTime? DueDate { get; set; }

    [BsonElement("DueDateReminder")]
    public DateTime? DueDateReminder { get; set; }
}
