using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class Card {
   

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ListId { get; set;} 
    [BsonElement("IndexNumber")] 
    public int IndexNumber { get; set;}
    [BsonElement("Title")]
    public string? Title { get; set;}  
    [BsonElement("Description")]
    public string? Priority { get; set;}  
    [BsonElement("Priority")]
    public string? Description { get; set;} 
    [BsonElement("Comments")]
    public List<string>? Comments { get; set; }

    public Card() {
        Comments ??= [];
    }
}
