namespace Server.Models;

public class Room
{
    private const int MinPlayerCount = 2;
    private const int MaxPlayerCount = 5;
    private const int MaxMessageLength = 500;

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int Id { get; set; }
    public string Code { get; private set; } = "";

    public string Password { get; private set; } = "";
    public string GameType { get; set; } = "";
    public RoomState State { get; private set; } = RoomState.Lobby;
    public bool ObjectivesRevealed { get; private set; }

    public List<Player> Players { get; private set; } = new();
    public int MaxPlayers { get; private set; }

    public List<PlayerObjective> Objectives { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();

    public Player AddPlayer(string nickname)
    {
        if (State != RoomState.Lobby)
        {
            throw new InvalidOperationException("Game has already started");
        }
        if (Players.Count >= MaxPlayers)
        {
            throw new InvalidOperationException("Room is full");
        }

        var player = new Player
        (
            nickname: nickname,
            roomId: Id,
            tenantId: TenantId
        );
        Players.Add(player);
        return player;
    }

    public ChatMessage AddMessage(int playerId, string? text, int? recipientId)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Message cannot be empty");
        }
        if (text.Length > MaxMessageLength)
        {
            throw new InvalidOperationException($"Message cannot be longer than {MaxMessageLength} characters");
        }
        if (!Players.Any(p => p.Id == playerId))
        {
            throw new InvalidOperationException("Player is not in this room");
        }
        if (recipientId != null && !Players.Any(p => p.Id == recipientId))
        {
            throw new InvalidOperationException("Recipient is not in this room");
        }

        var message = new ChatMessage
        {
            Text = text.Trim(),
            SentAt = DateTime.UtcNow,
            TenantId = TenantId,
            RoomId = Id,
            PlayerId = playerId,
            RecipientId = recipientId
        };
        Messages.Add(message);
        return message;
    }

    public void Start()
    {
        if (State != RoomState.Lobby)
        {
            throw new InvalidOperationException("Game has already started.");
        }
        if (Players.Count < MinPlayerCount)
        {
            throw new InvalidOperationException("Not enough players.");
        }

        State = RoomState.Playing;
    }

    public void RevealObjectives()
    {
        if (State != RoomState.Playing)
        {
            throw new InvalidOperationException("Game has not started yet.");
        }

        ObjectivesRevealed = true;
    }

    public static Room Create(string code, string gameType, int maxPlayers, string password)
    {
        if (maxPlayers < MinPlayerCount || maxPlayers > MaxPlayerCount)
        {
            throw new InvalidOperationException($"Player count must be between {MinPlayerCount} and {MaxPlayerCount}");
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Room password is required");
        }

        return new Room
        (
            code: code,
            gameType: gameType,
            maxPlayers: maxPlayers
        )
        {
            Password = password,
            Tenant = new Tenant { Name = $"Room {code}" }
        };
    }

    private Room(string code, string gameType, int maxPlayers)
    {
        Code = code;
        GameType = gameType;
        MaxPlayers = maxPlayers;
    }
}