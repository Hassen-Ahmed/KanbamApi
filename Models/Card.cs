using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class Card {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? ListId { get; set;} = null!;
    public int IndexNumber { get; set;}
    public string? Title { get; set;} = "";
    // public Boolean? IsDragging { get; set; } 
}
