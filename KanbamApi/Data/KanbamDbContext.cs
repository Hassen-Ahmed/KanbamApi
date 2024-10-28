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
    private readonly IMongoCollection<Board> _boardsCollection;
    private readonly IMongoCollection<BoardMember> _boardMembersCollection;
    private readonly IMongoCollection<Workspace> _workspacesCollection;
    private readonly IMongoCollection<WorkspaceMember> _workspaceMembersCollection;
    private readonly IMongoDatabase _kanbamDatabase;
    private readonly IMongoClient _mongoClient;

    public KanbamDbContext()
    {
        var mongoClient = new MongoClient(DotNetEnv.Env.GetString("CONNECTION_STRING"));
        _mongoClient = mongoClient;

        var kanbamDatabase = mongoClient.GetDatabase(DotNetEnv.Env.GetString("DB_NAME"));
        _kanbamDatabase = mongoClient.GetDatabase(DotNetEnv.Env.GetString("DB_NAME"));

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

        _boardsCollection = kanbamDatabase.GetCollection<Board>(
            DotNetEnv.Env.GetString("BOARDS_COLLECTION_NAME")
        );
        _boardMembersCollection = kanbamDatabase.GetCollection<BoardMember>(
            DotNetEnv.Env.GetString("BOARDSMEMBERS_COLLECTION_NAME")
        );
        _workspacesCollection = kanbamDatabase.GetCollection<Workspace>(
            DotNetEnv.Env.GetString("WORSPACES_COLLECTION_NAME")
        );
        _workspaceMembersCollection = kanbamDatabase.GetCollection<WorkspaceMember>(
            DotNetEnv.Env.GetString("WORSPACESMEMBERS_COLLECTION_NAME")
        );
    }

    public IMongoCollection<Card> CardsCollection => _cardsCollection;
    public IMongoCollection<List> ListsCollection => _listsCollection;
    public IMongoCollection<User> UsersCollection => _usersCollection;
    public IMongoCollection<Auth> AuthCollection => _authCollection;
    public IMongoCollection<Board> BoardsCollection => _boardsCollection;
    public IMongoCollection<BoardMember> BoardMembersCollection => _boardMembersCollection;
    public IMongoCollection<Workspace> WorkspacesCollection => _workspacesCollection;
    public IMongoCollection<WorkspaceMember> WorkspaceMembersCollection =>
        _workspaceMembersCollection;
    public IMongoDatabase KanbamDatabase => _kanbamDatabase;
    public IMongoClient MongoClient => _mongoClient;
}
