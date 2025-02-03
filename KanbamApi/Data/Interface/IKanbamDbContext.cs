using KanbamApi.Models;
using MongoDB.Driver;

namespace KanbamApi.Data.Interfaces;

public interface IKanbamDbContext
{
    IMongoCollection<Card> CardsCollection { get; }
    IMongoCollection<List> ListsCollection { get; }
    IMongoCollection<Board> BoardsCollection { get; }
    IMongoCollection<BoardMember> BoardMembersCollection { get; }
    IMongoCollection<Workspace> WorkspacesCollection { get; }
    IMongoCollection<WorkspaceMember> WorkspaceMembersCollection { get; }
    IMongoCollection<Donation> DonationsCollection { get; }
    IMongoDatabase KanbamDatabase { get; }
    IMongoClient MongoClient { get; }
}
