namespace Server.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Text { get; set; } = "";
    public DateTime SentAt { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    public int? RecipientId { get; set; }
    public Player? Recipient { get; set; }
}
