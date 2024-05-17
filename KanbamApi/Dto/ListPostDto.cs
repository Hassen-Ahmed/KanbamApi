using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class ListPostDto
{
    public string? Title { get; set; }
    public int IndexNumber { get; set; }
    public Boolean? IsDragging { get; set; } = false;
}
