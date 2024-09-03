using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using MongoDB.Driver;

namespace KanbamApi.Data;

public class KanbamDbContext : IKanbamDbContext
{
    private readonly IMongoCollection<Card> _cardsCollection;
    private readonly IMongoCollection<List> _listsCollection;
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<Auth> _authCollection;

    public KanbamDbContext()
    {
        var mongoClient = new MongoClient(DotNetEnv.Env.GetString("CONNECTION_STRING"));

        var kanbamDatabase = mongoClient.GetDatabase(DotNetEnv.Env.GetString("DB_NAME"));

        _cardsCollection = kanbamDatabase.GetCollection<Card>(
            DotNetEnv.Env.GetString("CARDS_COLLECTION_NAME")
        );

        _listsCollection = kanbamDatabase.GetCollection<List>(
            DotNetEnv.Env.GetString("LISTS_COLLECTION_NAME")
        );

        _usersCollection = kanbamDatabase.GetCollection<User>(
            DotNetEnv.Env.GetString("USERS_COLLECTION_NAME")
        );

        _authCollection = kanbamDatabase.GetCollection<Auth>(
            DotNetEnv.Env.GetString("AUTH_COLLECTION_NAME")
        );
    }

    public IMongoCollection<Card> CardsCollection => _cardsCollection;
    public IMongoCollection<List> ListsCollection => _listsCollection;
    public IMongoCollection<User> UsersCollection => _usersCollection;
    public IMongoCollection<Auth> AuthCollection => _authCollection;
}
