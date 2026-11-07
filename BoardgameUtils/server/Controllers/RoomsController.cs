using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Utilities;

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
        if (string.IsNullOrWhiteSpace(req.name))
        {
            return ApiResponse.UserError("Player name is required");
        }

        try
        {
            var room = await _db.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.Code == code.ToUpper());

            if (room == null)
            {
                return ApiResponse.UserError("Room not found");
            }
            if (room.Players.Count >= room.MaxPlayers)
            {
                return ApiResponse.UserError("Room is full");
            }

            var player = new Player(
                nickname: req.name,
                roomId: room.Id
            );

            _db.Players.Add(player);
            await _db.SaveChangesAsync();
            await _hub.Clients.Group(room.Id.ToString()).SendAsync("PlayersChanged");

            return ApiResponse.Success(new
            {
                playerId = player.Id,
                roomCode = room.Code,
                roomId = room.Id
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to join room", ex);
        }
    }

    public record JoinRequest(string name);


    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.name))
        {
            return ApiResponse.UserError("Player name is required");
        }
        if (req.maxPlayerCount < 1 || req.maxPlayerCount > 5)
        {
            return ApiResponse.UserError("Player count must be between 1 and 5");
        }

        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var lobbyCode = new string(Enumerable.Range(0, 5)
            .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());

        var room = new Room(
            code: lobbyCode,
            gameType: "Nemesis",
            maxPlayers: req.maxPlayerCount
        );

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            var player = new Player(
                nickname: req.name,
                isLobbyLeader: true,
                roomId: room.Id
            );

            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return ApiResponse.Success(new
            {
                playerId = player.Id,
                roomCode = lobbyCode,
                roomId = room.Id
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse.ServerError("Failed to create room", ex);
        }
    }

    public record CreateRequest(string name, int maxPlayerCount);

    [HttpGet("{code}/getPlayerInfo")]
    public async Task<IActionResult> GetPlayerInfo(string code)
    {
        try
        {
            var currentRoom = await _db.Rooms
                .Include(r => r.Players)
                    .ThenInclude(p => p.Objective)
                .FirstOrDefaultAsync(r => r.Code == code.ToUpper());

            if (currentRoom == null)
            {
                return ApiResponse.UserError("Room not found");
            }

            var playerList = currentRoom.Players
                .Select(p => new
                {
                    nickname = p.Nickname,
                    objective = p.Objective == null ? null : new
                    {
                        id = p.Objective.Id,
                        title = p.Objective.Title,
                        type = p.Objective.Type,
                        maxPlayers = p.Objective.MaxPlayers
                    }
                })
                .ToList();

            return ApiResponse.Success(playerList);
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to get player information", ex);
        }
    }

    [HttpPost("{code}/selectObjective")]
    public async Task<IActionResult> SelectObjective(SelectObjectiveRequest req)
    {
        try
        {
            var player = await _db.Players
                .Include(p => p.Objective)
                .FirstOrDefaultAsync(p => p.Id == req.playerId);

            if (player == null)
            {
                return ApiResponse.UserError("Player not found");
            }

            if (player.Objective == null)
            {
                player.Objective = new PlayerObjective
                {
                    RoomId = player.RoomId,
                    PlayerId = player.Id
                };
            }

            player.Objective.Title = req.objective.Title;
            player.Objective.Type = req.objective.Type;
            player.Objective.MaxPlayers = req.objective.MaxPlayers;

            await _db.SaveChangesAsync();
            var room = await _db.Rooms.FindAsync(player.RoomId);
            if (room == null)
            {
                return ApiResponse.UserError("Room not found");
            }
            await _hub.Clients.Group(room.Id.ToString()).SendAsync("PlayersChanged");

            return ApiResponse.Success( new {} );
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to select objective", ex);
        }
        
    }
    public record SelectObjectiveRequest(int playerId, PlayerObjective objective);
}