using KanbamApi.Models;
using MongoDB.Driver;

namespace KanbamApi.Data.Interfaces;

public interface IKanbamDbContext
{
    IMongoCollection<Card> CardsCollection { get; }
    IMongoCollection<List> ListsCollection { get; }
    IMongoCollection<User> UsersCollection { get; }
    IMongoCollection<Auth> AuthCollection { get; }
    IMongoCollection<Visitor> VisitorsCollection { get; }
}
