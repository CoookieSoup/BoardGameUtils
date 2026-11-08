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
    private readonly TenantContext _tenant;

    public RoomsController(AppDbContext db, IHubContext<LobbyHub> hub, TenantContext tenant)
    {
        _db = db;
        _hub = hub;
        _tenant = tenant;
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

            Player player;
            try
            {
                player = room.AddPlayer(req.name);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse.UserError(ex.Message);
            }

            await _db.SaveChangesAsync();
            await _hub.Clients.Group(room.Id.ToString()).SendAsync("PlayersChanged");

            return ApiResponse.Success(new
            {
                playerId = player.Id,
                roomCode = room.Code,
                roomId = room.Id,
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
        if (_tenant.TenantId == null)
        {
            return ApiResponse.UserError("Tenant is required");
        }
        if (!await _db.Tenants.AnyAsync(t => t.Id == _tenant.TenantId))
        {
            return ApiResponse.UserError("Tenant not found");
        }
        if (string.IsNullOrWhiteSpace(req.name))
        {
            return ApiResponse.UserError("Player name is required");
        }

        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var lobbyCode = new string(Enumerable.Range(0, 5)
            .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());

        Room room;
        try
        {
            room = Room.Create(
                code: lobbyCode,
                gameType: "Nemesis",
                maxPlayers: req.maxPlayerCount,
                tenantId: _tenant.TenantId.Value
            );
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse.UserError(ex.Message);
        }

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
                    .ThenInclude(p => p.Objectives)
                .FirstOrDefaultAsync(r => r.Code == code.ToUpper());

            if (currentRoom == null)
            {
                return ApiResponse.UserError("Room not found");
            }

            var playerList = currentRoom.Players
                .Select(p =>
                {
                    var chosen = p.Objectives.FirstOrDefault(o => o.IsChosen);
                    return new
                    {
                        nickname = p.Nickname,
                        objective = chosen == null ? null : new
                        {
                            id = chosen.Id,
                            title = chosen.Title,
                            type = chosen.Type,
                            minPlayers = chosen.MinPlayers
                        }
                    };
                })
                .ToList();

            return ApiResponse.Success(playerList);
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to get player information", ex);
        }
    }

    [HttpPost("{code}/start")]
    public async Task<IActionResult> Start(string code)
    {
        try
        {
            var room = await _db.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.Code == code.ToUpper());

            if (room == null)
            {
                return ApiResponse.UserError("Room not found");
            }

            try
            {
                room.Start();
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse.UserError(ex.Message);
            }

            _db.PlayerObjectives.AddRange(ObjectiveDealer.Deal(room));

            await _db.SaveChangesAsync();
            await _hub.Clients.Group(room.Id.ToString()).SendAsync("GameStarted");

            return ApiResponse.Success(new { });
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to start game", ex);
        }
    }

    [HttpGet("{code}/objectives")]
    public async Task<IActionResult> GetObjectives(string code, [FromQuery] int playerId)
    {
        try
        {
            var roomExists = await _db.Rooms.AnyAsync(r => r.Code == code.ToUpper());

            if (!roomExists)
            {
                return ApiResponse.UserError("Room not found");
            }

            var objectives = await _db.PlayerObjectives
                .Where(o => o.PlayerId == playerId && o.Room!.Code == code.ToUpper())
                .Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    type = o.Type
                })
                .ToListAsync();

            return ApiResponse.Success(objectives);
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to load objectives", ex);
        }
    }

    [HttpPost("{code}/selectObjective")]
    public async Task<IActionResult> SelectObjective(string code, SelectObjectiveRequest req)
    {
        try
        {
            var objectives = await _db.PlayerObjectives
            .Where(o => o.PlayerId == req.playerId && o.Room!.Code == code.ToUpper())
            .ToListAsync();

            var chosenObjective = objectives.FirstOrDefault(o => o.Id == req.objectiveId);
            if (chosenObjective == null)
            {
                return ApiResponse.UserError("Objective not found");
            }

            chosenObjective.IsChosen = true;
            _db.PlayerObjectives.RemoveRange(objectives.Where(o => o != chosenObjective));
            await _db.SaveChangesAsync();

            await _hub.Clients.Group(chosenObjective.RoomId.ToString()).SendAsync("PlayersChanged");
            return ApiResponse.Success(new { });
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to select objective", ex);
        }
        
    }
    public record SelectObjectiveRequest(int playerId, int objectiveId);
}