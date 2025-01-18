using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models.MongoDbIdentity;

[BsonIgnoreExtraElements]
public class ApplicationUser : MongoUser<ObjectId>
{
    [BsonRequired]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid RefreshToken { get; set; }

    [BsonRequired]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
