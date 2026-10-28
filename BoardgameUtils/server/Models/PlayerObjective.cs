using Server.Enums;

namespace Server.Models;


public class PlayerObjective
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Type { get; set; } = "";

    public int MaxPlayers { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }
}