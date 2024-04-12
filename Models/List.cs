using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class List {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public  string? Id { get; set;}
    public string? UserId { get; set;} 
    public string? Title { get; set;} 
    public int IndexNumber { get; set;}
    public Boolean IsDragging { get; set; }
}


