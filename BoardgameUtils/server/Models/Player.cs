using System.Security.Cryptography;

namespace Server.Models;

public class Player
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Nickname { get; set; } = "";

    public string Token { get; private set; } = "";
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public bool IsLobbyLeader { get; private set; }
    public List<PlayerObjective> Objectives { get; set; } = new();

    public Player(string nickname, int roomId, int tenantId, bool isLobbyLeader = false)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            throw new InvalidOperationException("Player name is required");
        }

        Nickname = nickname;
        IsLobbyLeader = isLobbyLeader;
        RoomId = roomId;
        TenantId = tenantId;
        Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }

    public List<PlayerObjective> ChooseObjective(int objectiveId)
    {
        if (Objectives.Any(o => o.IsChosen))
        {
            throw new InvalidOperationException("Objective already chosen");
        }

        var chosen = Objectives.FirstOrDefault(o => o.Id == objectiveId);
        if (chosen == null)
        {
            throw new InvalidOperationException("Objective not found");
        }

        chosen.Choose();

        return Objectives.Where(o => o != chosen).ToList();
    }
}