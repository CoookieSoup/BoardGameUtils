using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs;

public class LobbyHub : Hub
{
    public Task Watch(string code) =>
        Groups.AddToGroupAsync(Context.ConnectionId, code);
}