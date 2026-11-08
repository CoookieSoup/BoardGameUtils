using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Models;

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

    public async Task StartGame(int roomId)
    {
        var room = await _db.Rooms
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
            throw new HubException("Room not found.");

        if (room.State != RoomState.Lobby)
            throw new HubException("Game has already started.");

        if (room.Players.Count < 2)
            throw new HubException("Not enough players.");

        var objectives = ObjectiveDealer.Deal(room);

        _db.PlayerObjectives.AddRange(objectives);
        room.setState(RoomState.Playing);

        await _db.SaveChangesAsync();
        await Clients.Group(room.Id.ToString()).SendAsync("GameStarted");

        foreach (var player in room.Players)
        {
            var playerObjectives = objectives
                .Where(o => o.PlayerId == player.Id)
                .Select(o => new
                {
                    o.Id,
                    o.Title,
                    o.Type
                })
                .ToList();

            await Clients.Group($"player_{player.Id}").SendAsync("ObjectivesReceived", playerObjectives);
        }
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