using Microsoft.EntityFrameworkCore;
using Server.Hubs;
using Server.Models;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TenantContext>(sp =>
{
    var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    return new TenantContext
    {
        TenantId = http?.Items["TenantId"] as int?,
        PlayerId = http?.Items["PlayerId"] as int?
    };
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
        var roomA = Room.Create
        (
            code: "AAAAAA",
            gameType: "Nemesis",
            maxPlayers: 5,
            password: "apple"
        );
        var roomB = Room.Create
        (
            code: "BBBBBB",
            gameType: "Nemesis",
            maxPlayers: 5,
            password: "banana"
        );
        db.Rooms.AddRange(roomA, roomB);
        await db.SaveChangesAsync();

        db.Players.AddRange(
            new Player
            (
                nickname: "Player1",
                roomId: roomA.Id,
                tenantId: roomA.TenantId,
                isLobbyLeader: true
            ),
            new Player
            (
                nickname: "Player2",
                roomId: roomA.Id,
                tenantId: roomA.TenantId
            ),
            new Player
            (
                nickname: "Player3",
                roomId: roomB.Id,
                tenantId: roomB.TenantId,
                isLobbyLeader: true
            ),
            new Player
            (
                nickname: "Player4",
                roomId: roomB.Id,
                tenantId: roomB.TenantId
            ));
        await db.SaveChangesAsync();

    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseCors();

app.Use(async (context, next) =>
{
    var header = context.Request.Headers.Authorization.FirstOrDefault() ?? "";
    var token = header.StartsWith("Bearer ") ? header["Bearer ".Length..] : context.Request.Query["access_token"].FirstOrDefault();

    if (!string.IsNullOrEmpty(token))
    {
        using var scope = context.RequestServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var player = await db.Players
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Token == token);

        if (player != null)
        {
            context.Items["TenantId"] = player.TenantId;
            context.Items["PlayerId"] = player.Id;
        }
    }

    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<LobbyHub>("/hubs/lobby");

app.Run();
