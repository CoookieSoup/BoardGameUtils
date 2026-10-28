using Server.Enums;

namespace Server.Models;


public class PlayerObjective
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public int ObjectiveId { get; set; }
    public Objective? Objective { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
}