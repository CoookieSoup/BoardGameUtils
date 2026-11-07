namespace Server.Models;

public class Player
{
    public int Id { get; set; }
    public string Nickname { get; set; } = "";
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public bool IsLobbyLeader { get; set; }
    public List<PlayerObjective> Objectives { get; set; } = new();

    public Player(string nickname, int roomId, bool isLobbyLeader = false)
    {
        Nickname = nickname;
        IsLobbyLeader = isLobbyLeader;
        RoomId = roomId;
    }
}