using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;

namespace KanbamApi.Models.MongoDbIdentity;

public class ApplicationRole : MongoRole<ObjectId> { }
