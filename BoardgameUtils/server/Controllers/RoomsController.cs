using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<LobbyHub> _hub;

    public RoomsController(AppDbContext db, IHubContext<LobbyHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpPost("{code}/join")]
    public async Task<IActionResult> Join(string code, JoinRequest req)
    {

        var room = await _db.Rooms
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => r.Code == code.ToUpper());

        if (room == null)
        {
            return NotFound("Room not found");
        } else if (room.Players.Count >= room.MaxPlayers)
        {
            return BadRequest(new { message = "Room is full" });
        }
        var player = new Player
        {
            Nickname = req.Name,
            RoomId = room.Id
        };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        await _hub.Clients.Group(code).SendAsync("PlayersChanged");
        return Ok(new { playerId = player.Id });
    }
    public record JoinRequest(string Name);


    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateRequest req)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var lobbyCode = new string(Enumerable.Range(0, 5)
            .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());



        var room = new Room
        {
            Code = lobbyCode,
            GameType = "Default",
            State = "Lobby",
            MaxPlayers = req.maxPlayerCount
        };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        var player = new Player
        {
            Nickname = req.name,
            RoomId = room.Id
        };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return Ok(new { playerId = player.Id, roomCode = lobbyCode });
    }
    public record CreateRequest(string name, int maxPlayerCount);

    [HttpGet("{code}/getPlayerInfo")]
    public async Task<IActionResult> GetPlayerInfo(string code)
    {
        var currentRoom = await _db.Rooms
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => r.Code == code.ToUpper());
        
        if (currentRoom == null)
        {
            return NotFound("Room not found");
        }
        var playerList = currentRoom.Players;
        await _hub.Clients.Group(code).SendAsync("PlayersChanged");
        return Ok(new { playerList });
    }
}
