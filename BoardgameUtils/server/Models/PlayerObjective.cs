namespace Server.Models;


public class PlayerObjective
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public ObjectiveType Type { get; set; }

    public int MinPlayers { get; set; }
    public bool IsChosen { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }
}