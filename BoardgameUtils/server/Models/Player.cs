using System.Security.Cryptography;

namespace Server.Models;

public class Player
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Nickname { get; set; } = "";

    public string Token { get; set; } = "";
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public bool IsLobbyLeader { get; set; }
    public List<PlayerObjective> Objectives { get; set; } = new();

    public Player(string nickname, int roomId, int tenantId, bool isLobbyLeader = false)
    {
        Nickname = nickname;
        IsLobbyLeader = isLobbyLeader;
        RoomId = roomId;
        TenantId = tenantId;
        Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}