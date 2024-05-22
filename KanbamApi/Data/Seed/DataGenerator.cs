using Bogus;
using KanbamApi.Models;
using MongoDB.Bson;

namespace KanbamApi.Data.Seed;

public class DataGenerator
{
    private List<string> _listIds = [];
    private List<string> _userIds = [];
    private List<string> _emails = [];

    public List<Auth> GenerateAuth(int count)
    {
        var fakerAuth = new Faker<Auth>()
            .RuleFor(a => a.Email, f => f.Internet.Email())
            .RuleFor(a => a.PasswordHash, f => new byte[128 / 8])
            .RuleFor(a => a.PasswordSalt, f => new byte[128 / 8])
            .FinishWith((f, a) => _emails.Add(a.Email!));

        return fakerAuth.Generate(count);
    }

    public List<User> GenerateUsers(int count)
    {
        var fakerUsers = new Faker<User>()
            .RuleFor(u => u.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(u => u.Email, f => _emails[f.IndexFaker])
            .RuleFor(u => u.UserName, (f, u) => $"{u.Email}"[0..^(int)(u.Email?.IndexOf('@'))!])
            .FinishWith((f, u) => _userIds.Add(u.Id!));

        return fakerUsers.Generate(count);
    }

    public List<List> GenerateLists(int count)
    {
        var fakerLists = new Faker<List>()
            .RuleFor(l => l.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(l => l.UserId, f => f.PickRandom(_userIds))
            .RuleFor(l => l.Title, f => f.Lorem.Sentence())
            .RuleFor(l => l.IndexNumber, f => f.IndexFaker)
            .RuleFor(l => l.Cards, f => new List<Card>())
            .FinishWith((f, l) => _listIds.Add(l.Id!));

        return fakerLists.Generate(count);
    }

    public List<Card> GenerateCards(int count)
    {
        var fakerCards = new Faker<Card>()
            .RuleFor(c => c.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.ListId, f => f.PickRandom(_listIds))
            .RuleFor(c => c.Title, f => f.Lorem.Word())
            .RuleFor(c => c.IndexNumber, f => f.IndexFaker)
            .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
            .RuleFor(c => c.Priority, f => "")
            .RuleFor(l => l.Comments, f => new List<string>());

        return fakerCards.Generate(count);
    }
}
