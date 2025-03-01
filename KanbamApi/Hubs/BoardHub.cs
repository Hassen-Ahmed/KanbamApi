using KanbamApi.Dtos.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KanbamApi.Hubs;

[Authorize]
public class BoardHub : Hub
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

    public async Task AddBoard(string groupId, DtoBoardPost items) =>
        await Clients.Group(groupId).SendAsync("ReceiveBoardCreated", items, groupId);

    public async Task UdateBoard(string groupId, DtoBoardPost items) =>
        await Clients.Group(groupId).SendAsync("ReceiveBoardUpdate", items, groupId);

    public async Task DeleteBoard(string groupId, string boardId) =>
        await Clients.Group(groupId).SendAsync("ReceiveBoardDelete", boardId, groupId);
}
