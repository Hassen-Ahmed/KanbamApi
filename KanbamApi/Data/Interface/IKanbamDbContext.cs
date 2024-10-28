using KanbamApi.Models;
using MongoDB.Driver;

namespace KanbamApi.Data.Interfaces;

public interface IKanbamDbContext
{
    IMongoCollection<Card> CardsCollection { get; }
    IMongoCollection<List> ListsCollection { get; }
    IMongoCollection<User> UsersCollection { get; }
    IMongoCollection<Auth> AuthCollection { get; }
    IMongoCollection<Board> BoardsCollection { get; }
    IMongoCollection<BoardMember> BoardMembersCollection { get; }
    IMongoCollection<Workspace> WorkspacesCollection { get; }
    IMongoCollection<WorkspaceMember> WorkspaceMembersCollection { get; }
    IMongoDatabase KanbamDatabase { get; }
    IMongoClient MongoClient { get; }
}
