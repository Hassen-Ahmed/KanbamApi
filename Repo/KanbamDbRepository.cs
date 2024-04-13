
using KanbamApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace KanbamApi.Repo;

public class KanbamDbRepository{
     public IMongoDatabase kanbamDatabase;

     public KanbamDbRepository() {
     
          var mongoClient = new MongoClient(DotNetEnv.Env.GetString("CONNECTION_STRING"));
          
          kanbamDatabase = mongoClient.GetDatabase(DotNetEnv.Env.GetString("DB_NAME"));

     }
}