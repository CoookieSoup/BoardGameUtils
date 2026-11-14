using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting("join")]
    public async Task<IActionResult> Join(string code, JoinRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.name))
        {
            return ApiResponse.UserError("Player name is required");
        }
        if (string.IsNullOrWhiteSpace(req.password))
        {
            return ApiResponse.UserError("Room password is required");
        }

        try
        {
            var room = await _db.Rooms
                .IgnoreQueryFilters()
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.Code == code.ToUpper() && r.Password == req.password);

            if (room == null)
            {
                return ApiResponse.UserError("Wrong room code or password");
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
                token = player.Token,
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to join room", ex);
        }
    }

    public record JoinRequest(string name, string password);


    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateRequest req)
    {
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
                password: req.password
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

            var player = new Player
            (
                nickname: req.name,
                isLobbyLeader: true,
                roomId: room.Id,
                tenantId: room.TenantId
            );

            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return ApiResponse.Success(new
            {
                playerId = player.Id,
                roomCode = lobbyCode,
                roomId = room.Id,
                token = player.Token
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse.ServerError("Failed to create room", ex);
        }
    }

    public record CreateRequest(string name, int maxPlayerCount, string password);

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
                        id = p.Id,
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

            var isLeader = room.Players.Any(p => p.Id == _tenant.PlayerId && p.IsLobbyLeader);
            if (!isLeader)
            {
                return ApiResponse.UserError("Only the lobby leader can start the game");
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
    public async Task<IActionResult> GetObjectives(string code)
    {
        try
        {
            var roomExists = await _db.Rooms.AnyAsync(r => r.Code == code.ToUpper());

            if (!roomExists)
            {
                return ApiResponse.UserError("Room not found");
            }

            var objectives = await _db.PlayerObjectives
                .Where(o => o.PlayerId == _tenant.PlayerId && o.Room!.Code == code.ToUpper())
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
            .Where(o => o.PlayerId == _tenant.PlayerId && o.Room!.Code == code.ToUpper())
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
    public record SelectObjectiveRequest(int objectiveId);

    [HttpPost("{code}/messages")]
    public async Task<IActionResult> SendMessage(string code, SendMessageRequest req)
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

            ChatMessage message;
            try
            {
                message = room.AddMessage(_tenant.PlayerId ?? 0, req.text, req.recipientId);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse.UserError(ex.Message);
            }

            await _db.SaveChangesAsync();

            if (message.RecipientId == null)
            {
                await _hub.Clients.Group(room.Id.ToString()).SendAsync("ChatChanged");
            }
            else
            {
                await _hub.Clients.Group($"player_{message.PlayerId}").SendAsync("ChatChanged");
                await _hub.Clients.Group($"player_{message.RecipientId}").SendAsync("ChatChanged");
            }

            return ApiResponse.Success(new { });
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to send message", ex);
        }
    }

    public record SendMessageRequest(string? text, int? recipientId);

    [HttpGet("{code}/messages")]
    public async Task<IActionResult> GetMessages(string code)
    {
        try
        {
            var roomExists = await _db.Rooms.AnyAsync(r => r.Code == code.ToUpper());

            if (!roomExists)
            {
                return ApiResponse.UserError("Room not found");
            }

            var playerId = _tenant.PlayerId;
            var latestMessages = await _db.ChatMessages
                .Where(m => m.Room!.Code == code.ToUpper())
                .Where(m => m.RecipientId == null || m.PlayerId == playerId || m.RecipientId == playerId)
                .OrderByDescending(m => m.Id)
                .Take(100)
                .Select(m => new
                {
                    id = m.Id,
                    senderNickname = m.Player!.Nickname,
                    text = m.Text,
                    sentAt = m.SentAt,
                    recipientNickname = m.Recipient == null ? null : m.Recipient.Nickname
                })
                .ToListAsync();

            latestMessages.Reverse();

            return ApiResponse.Success(latestMessages);
        }
        catch (Exception ex)
        {
            return ApiResponse.ServerError("Failed to load messages", ex);
        }
    }
}