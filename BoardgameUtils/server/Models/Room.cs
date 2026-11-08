namespace Server.Models;

public class Room
{
    public int TenantId { get; set; }
    public int Id { get; set; }
    public string Code { get; private set; } = "";
    public string GameType { get; set; } = "";
    public string State { get; private set; } = "Lobby";
    public int HostPlayerId { get; set; }

    public List<Player> Players { get; private set; } = new();
    public int MaxPlayers { get; private set; }

    public List<PlayerObjective> Objectives { get; set; } = new();

    public void setState(string state)
    {
        State = state;
    }

    public Room(string code, string gameType, int maxPlayers)
    {
        Code = code;
        GameType = gameType;
        MaxPlayers = maxPlayers;
    }
}