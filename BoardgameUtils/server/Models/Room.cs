namespace Server.Models;

public class Room
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string GameType { get; set; } = "";
    public string State { get; set; } = "Lobby";
    public int HostPlayerId { get; set; }

    public List<Player> Players { get; set; } = new();
    public int MaxPlayers { get; internal set; }
}