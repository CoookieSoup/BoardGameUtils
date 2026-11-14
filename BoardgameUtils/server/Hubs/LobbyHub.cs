using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Hubs;

public class LobbyHub : Hub
{
    private AppDbContext _db;
    private TenantContext _tenant;

    public LobbyHub(AppDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }
    
    public async Task Watch(int roomId)
    {
        var roomExists = await _db.Rooms.AnyAsync(r => r.Id == roomId);

        if (!roomExists)
            throw new HubException("Room not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
    }

    public async Task JoinPlayerGroup()
    {
        if (_tenant.PlayerId == null)
            throw new HubException("Player not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"player_{_tenant.PlayerId}");
    }
    
}