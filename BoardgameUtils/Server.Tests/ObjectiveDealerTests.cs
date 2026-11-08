using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Server.Controllers;
using Server.Hubs;
using Server.Models;
using Xunit;

namespace Server.Tests;

public class ObjectiveDealerTests
{
    private const ObjectiveType Cooperative = ObjectiveType.Cooperative;
    private const ObjectiveType Selfish = ObjectiveType.Selfish;

    private static AppDbContext DbFor(string dbName, int tenantId) => new
    (
        options: new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options,
        tenant: new TenantContext { TenantId = tenantId }
    );

    private static RoomsController ControllerFor(AppDbContext db, int tenantId) => new
    (
        db: db,
        hub: new Mock<IHubContext<LobbyHub>>().Object,
        tenant: new TenantContext { TenantId = tenantId }
    );

    private static string ErrorMessage(IActionResult result) =>
        JsonSerializer.Serialize(((ObjectResult)result).Value);

    // Room with the given number of players. Player ids are unique across rooms (room 1: 101, 102, ...)
    private static Room MakeRoom(int roomId, int playerCount)
    {
        var room = new Room
        (
            code: "ABCDE",
            gameType: "Nemesis",
            maxPlayers: 5
        )
        {
            Id = roomId,
            TenantId = 1
        };

        for (int i = 1; i <= playerCount; i++)
        {
            room.Players.Add(new Player
            (
                nickname: $"p{i}",
                roomId: roomId
            )
            {
                Id = roomId * 100 + i
            });
        }

        return room;
    }

    [Fact]
    public void Deal_FourPlayers_EachGetsOneCoopAndOneSelfish_AllDistinct()
    {
        var room = MakeRoom(roomId: 1, playerCount: 4);

        var result = ObjectiveDealer.Deal(room);

        Assert.Equal(8, result.Count);
        Assert.Equal(8, result.Select(o => o.Title).Distinct().Count());

        foreach (var player in room.Players)
        {
            var playerObjectives = result.Where(o => o.PlayerId == player.Id);
            Assert.Single(playerObjectives, o => o.Type == Cooperative);
            Assert.Single(playerObjectives, o => o.Type == Selfish);
        }

        // These two need 5 players, so they must never be dealt to 4
        Assert.DoesNotContain(result, o => o.Title == "Kad kiekvienas kambarys būtų išieškotas");
        Assert.DoesNotContain(result, o => o.Title == "Žaidėjas 5 turi neišgyventi");
    }

    [Fact]
    public void AddPlayer_AfterGameStarted_IsRejected()
    {
        var room = MakeRoom(roomId: 1, playerCount: 4);
        room.Start();

        var ex = Assert.Throws<InvalidOperationException>(() => room.AddPlayer("fifth"));

        Assert.Equal("Game has already started", ex.Message);
        Assert.Equal(4, room.Players.Count);
    }

    [Fact]
    public void Deal_NotEnoughSelfishObjectives_Throws_AndAssignsNothing()
    {
        var room = MakeRoom(roomId: 1, playerCount: 3);
        var reducedCatalog = new List<NemesisObjectiveTemplate>
        {
            new("K1", Cooperative, 2), new("K2", Cooperative, 2), new("K3", Cooperative, 2),
            new("K4", Cooperative, 3), new("K5", Cooperative, 3), new("K6", Cooperative, 4),
            new("K7", Cooperative, 5),
            new("S6", Selfish, 2), new("S7", Selfish, 3),
        };

        var ex = Assert.Throws<ArgumentException>(() => ObjectiveDealer.Deal(room, reducedCatalog));

        Assert.Equal("Not enough objectives.", ex.Message);
        Assert.Equal(RoomState.Lobby, room.State);
        Assert.All(room.Players, p => Assert.Empty(p.Objectives));
    }

    [Fact]
    public async Task TenantA_CannotJoin_TenantB_Room()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var seed = DbFor(dbName, tenantId: 2))
        {
            seed.Rooms.Add(new Room
            (
                code: "BBBBB",
                gameType: "Nemesis",
                maxPlayers: 5
            )
            {
                TenantId = 2
            });
            await seed.SaveChangesAsync();
        }

        await using var asTenantA = DbFor(dbName, tenantId: 1);
        var controller = ControllerFor(asTenantA, tenantId: 1);

        var result = await controller.Join("BBBBB", new RoomsController.JoinRequest("intruder"));

        Assert.Contains("Room not found", ErrorMessage(result));
        await using var asTenantB = DbFor(dbName, tenantId: 2);
        Assert.Equal(0, await asTenantB.Players.CountAsync());
    }
}
