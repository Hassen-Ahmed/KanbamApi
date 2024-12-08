using KanbamApi.Dtos;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace KanbamApi.Hubs;

public class ListHub : Hub
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

    public async Task AddList(string groupId, DtoListPost items) =>
        await Clients.Group(groupId).SendAsync("ReceiveListCreated", items, groupId);

    public async Task UdateList(string groupId, List items, string userId) =>
        await Clients.Group(groupId).SendAsync("ReceiveListUpdate", items, userId, groupId);

    public async Task DeleteList(string groupId, string listId) =>
        await Clients.Group(groupId).SendAsync("ReceiveListDelete", listId, groupId);
}
