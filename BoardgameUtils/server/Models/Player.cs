namespace Server.Models;

public class Player
{
    public int Id { get; set; }
    public string Nickname { get; set; } = "";
    public int RoomId { get; set; }
    public Room? Room { get; set; }
}