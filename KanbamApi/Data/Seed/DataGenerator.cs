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
            .RuleFor(l => l.BoardId, f => f.PickRandom(_userIds))
            .RuleFor(l => l.Title, f => f.Lorem.Sentence())
            .RuleFor(l => l.IndexNumber, f => f.IndexFaker)
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
            .RuleFor(l => l.Comments, f => new List<Comment>());

        return fakerCards.Generate(count);
    }

    public List<Workspace> GenerateWorkspaces(int count)
    {
        var fakerWorkspaces = new Faker<Workspace>()
            .RuleFor(c => c.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.Name, f => f.Lorem.Word())
            .RuleFor(c => c.Description, f => f.Lorem.Paragraph());

        return fakerWorkspaces.Generate(count);
    }

    public List<Board> GenerateBoards(int count)
    {
        var fakerBoards = new Faker<Board>()
            .RuleFor(c => c.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.Name, f => f.Lorem.Word())
            .RuleFor(c => c.WorkspaceId, f => f.Lorem.Paragraph())
            .RuleFor(c => c.Description, f => f.Lorem.Paragraph());

        return fakerBoards.Generate(count);
    }

    public List<BoardMember> GenerateBoardMembers(int count)
    {
        var fakerBoardMembers = new Faker<BoardMember>()
            .RuleFor(c => c.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.UserId, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.BoardId, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.Role, f => f.Lorem.Paragraph());

        return fakerBoardMembers.Generate(count);
    }

    public List<WorkspaceMember> GenerateWorkspaceMembers(int count)
    {
        var fakerWorkspaceMembers = new Faker<WorkspaceMember>()
            .RuleFor(c => c.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.UserId, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.WorkspaceId, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.Role, f => f.Lorem.Word());

        return fakerWorkspaceMembers.Generate(count);
    }

    public List<RefreshToken> GenerateRefreshToken(int count)
    {
        var fakerRefreshTokena = new Faker<RefreshToken>()
            .RuleFor(c => c.Id, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.UserId, f => $"{ObjectId.GenerateNewId()}")
            .RuleFor(c => c.Token, f => Guid.NewGuid())
            .RuleFor(c => c.TokenExpiryTime, f => f.DateTimeReference);

        return fakerRefreshTokena.Generate(count);
    }
}
