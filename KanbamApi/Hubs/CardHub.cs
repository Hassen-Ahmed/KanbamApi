using KanbamApi.Dtos;
using KanbamApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KanbamApi.Hubs;

[Authorize]
public class CardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var groupId = httpContext?.Request.Query["groupId"];

        if (!string.IsNullOrEmpty(groupId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId!);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        var groupId = httpContext?.Request.Query["groupId"];

        if (!string.IsNullOrEmpty(groupId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId!);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddCard(string groupId, DtoCardPost newCard) =>
        await Clients.Group(groupId).SendAsync("ReceiveCardCreated", newCard, groupId);

    public async Task AddCardComment(string groupId, Comment newCardComment) =>
        await Clients
            .Group(groupId)
            .SendAsync("ReceiveCardCommentCreated", newCardComment, groupId);

    public async Task UdateCard(string groupId, Card card, string userId) =>
        await Clients.Group(groupId).SendAsync("ReceiveCardUpdate", card, userId, groupId);

    public async Task DeleteCard(string groupId, string cardId) =>
        await Clients.Group(groupId).SendAsync("ReceiveCardDelete", cardId, groupId);

    public async Task DeleteCardComment(
        string groupId,
        string commentId,
        string cardId,
        string userId
    ) =>
        await Clients
            .Group(groupId)
            .SendAsync("ReceiveCardCommentDelete", commentId, cardId, userId, groupId);
}
