using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;


public class PlayerObjective
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public int Type { get; set; }

    public int MaxPlayers { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }
}