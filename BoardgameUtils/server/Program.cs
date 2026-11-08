using Microsoft.EntityFrameworkCore;
using Server.Hubs;
using Server.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TenantContext>(sp =>
{
    var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var raw = http?.Request.Headers["X-Tenant-Id"].FirstOrDefault()
              ?? http?.Request.Query["tenantId"].FirstOrDefault();   // SignalR sends it in the URL
    return new TenantContext { TenantId = int.TryParse(raw, out var id) ? id : null };
});
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSignalR();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Tenants.AnyAsync())
    {
        // Pridejau nes reikalavimai prase "Paruoškite bent du klientus, pvz., dvi organizacijas, ir jų pavyzdinius duomenis"
        var org1 = new Tenant { Name = "Organisation 1" };
        var org2 = new Tenant { Name = "Organisation 2" };
        db.Tenants.AddRange(org1, org2);
        await db.SaveChangesAsync();

        
        db.Rooms.AddRange(
            new Room("AAAAAA", "Nemesis", 5)
            {
                TenantId = org1.Id,
                Players = { new Player("Player1", 0, true), new Player("Player2", 0) }
            },
            new Room("BBBBBB", "Nemesis", 5)
            {
                TenantId = org2.Id,
                Players = { new Player("Player3", 0, true), new Player("Player4", 0) }
            });
        await db.SaveChangesAsync();
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<LobbyHub>("/hubs/lobby");

app.Run();
