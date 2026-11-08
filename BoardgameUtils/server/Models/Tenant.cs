namespace Server.Models;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class TenantContext
{
    public int? TenantId { get; set; }
}