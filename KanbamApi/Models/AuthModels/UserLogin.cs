using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models.AuthModels;

public class UserLogin
{
    [BsonElement("Email")]
    public string? Email { get; set; } = "";

    [BsonElement("Password")]
    public string? Password { get; set; } = "";
}
