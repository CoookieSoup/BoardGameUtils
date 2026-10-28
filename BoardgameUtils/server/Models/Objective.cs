using Server.Enums;

namespace Server.Models;


public class Objective
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Text { get; set; } = "";
    public ObjectiveType Type { get; set; }
    public int MinPlayers { get; set; } = 2;
}