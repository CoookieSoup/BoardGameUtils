using Microsoft.EntityFrameworkCore;
using Server.Models;

public class AppDbContext : DbContext
{
    private readonly TenantContext _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, TenantContext tenant) : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerObjective> PlayerObjectives => Set<PlayerObjective>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Room>().HasQueryFilter(r => r.TenantId == _tenant.TenantId);
        b.Entity<Player>().HasQueryFilter(p => p.TenantId == _tenant.TenantId);
        b.Entity<PlayerObjective>().HasQueryFilter(o => o.TenantId == _tenant.TenantId);
    }
}