using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Server.Hubs;

public class LobbyHub : Hub
{
    private AppDbContext _db;

    public LobbyHub(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task Watch(int roomId)
    {
        var roomExists = await _db.Rooms.AnyAsync(r => r.Id == roomId);

        if (!roomExists)
            throw new HubException("Room not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
    }

    public async Task JoinPlayerGroup(int playerId, int roomId)
    {
        var player = await _db.Players
            .FirstOrDefaultAsync(p => p.Id == playerId && p.RoomId == roomId);

        if (player == null)
            throw new HubException("Player not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId,$"player_{playerId}");
    }
    
}