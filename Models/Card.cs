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
}
