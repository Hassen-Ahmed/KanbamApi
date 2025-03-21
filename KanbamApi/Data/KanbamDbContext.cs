using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using MongoDB.Driver;

namespace KanbamApi.Data;

public class KanbamDbContext : IKanbamDbContext
{
    private readonly IMongoCollection<Card> _cardsCollection;
    private readonly IMongoCollection<List> _listsCollection;
    private readonly IMongoCollection<Board> _boardsCollection;
    private readonly IMongoCollection<BoardMember> _boardMembersCollection;
    private readonly IMongoCollection<Workspace> _workspacesCollection;
    private readonly IMongoCollection<WorkspaceMember> _workspaceMembersCollection;
    private readonly IMongoCollection<Donation> _donationsCollection;
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
        _donationsCollection = kanbamDatabase.GetCollection<Donation>(
            DotNetEnv.Env.GetString("DONATIONS_COLLECTION_NAME")
        );
    }

    public IMongoCollection<Card> CardsCollection => _cardsCollection;
    public IMongoCollection<List> ListsCollection => _listsCollection;
    public IMongoCollection<Board> BoardsCollection => _boardsCollection;
    public IMongoCollection<BoardMember> BoardMembersCollection => _boardMembersCollection;
    public IMongoCollection<Workspace> WorkspacesCollection => _workspacesCollection;
    public IMongoCollection<WorkspaceMember> WorkspaceMembersCollection =>
        _workspaceMembersCollection;
    public IMongoCollection<Donation> DonationsCollection => _donationsCollection;
    public IMongoDatabase KanbamDatabase => _kanbamDatabase;
    public IMongoClient MongoClient => _mongoClient;
}
