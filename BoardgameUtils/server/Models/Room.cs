namespace Server.Models;

public class Room
{
    private const int MinPlayerCount = 2;
    private const int MaxPlayerCount = 5;

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int Id { get; set; }
    public string Code { get; private set; } = "";
    public string GameType { get; set; } = "";
    public RoomState State { get; private set; } = RoomState.Lobby;

    public List<Player> Players { get; private set; } = new();
    public int MaxPlayers { get; private set; }

    public List<PlayerObjective> Objectives { get; set; } = new();

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
            roomId: Id
        );
        Players.Add(player);
        return player;
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

    public static Room Create(string code, string gameType, int maxPlayers, int tenantId)
    {
        if (maxPlayers < MinPlayerCount || maxPlayers > MaxPlayerCount)
        {
            throw new InvalidOperationException($"Player count must be between {MinPlayerCount} and {MaxPlayerCount}");
        }

        return new Room
        (
            code: code,
            gameType: gameType,
            maxPlayers: maxPlayers
        )
        {
            TenantId = tenantId
        };
    }

    public Room(string code, string gameType, int maxPlayers)
    {
        Code = code;
        GameType = gameType;
        MaxPlayers = maxPlayers;
    }
}