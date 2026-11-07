using System.Text.Json;

namespace Server.Tests;

public class ObjectiveDealerTests
{
    private const int Cooperative = 1;
    private const int Selfish = 2;

    // Room with the given number of players. Player ids are unique across rooms (room 1: 101, 102, ...)
    private static Room MakeRoom(int roomId, int playerCount)
    {
        var room = new Room(code: "ABCDE", gameType: "Nemesis", maxPlayers: 5) { Id = roomId };
        for (int i = 1; i <= playerCount; i++) room.Players.Add(new Player(nickname: $"p{i}", roomId: roomId) { Id = roomId * 100 + i });
        return room;
    }

    // 1. Normal case: 4 players, real catalog
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

    // 2. Conflict case: objectives already dealt, a 5th player tries to join
    [Fact]
    public async Task Join_AfterObjectivesDealt_IsRejected_AndNoPlayerAdded()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AppDbContext(options);

        var room = MakeRoom(roomId: 1, playerCount: 4);
        room.setState("Playing");
        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        var controller = new RoomsController(db, new Mock<IHubContext<LobbyHub>>().Object);

        var result = await controller.Join("ABCDE", new RoomsController.JoinRequest("fifth"));

        var json = JsonSerializer.Serialize(((ObjectResult)result).Value);
        Assert.Contains("Game has already started", json);
        Assert.Equal(4, await db.Players.CountAsync());
        Assert.DoesNotContain(await db.Players.ToListAsync(), p => p.Nickname == "fifth");
    }

    // 3. Error case: 3 players, only 2 usable selfish objectives
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
        Assert.Equal("Lobby", room.State);
        Assert.All(room.Players, p => Assert.Empty(p.Objectives));
    }

    // 4. Data isolation: dealing in room A never touches room B
    [Fact]
    public void Deal_InOneRoom_DoesNotTouchOtherRoom()
    {
        var roomA = MakeRoom(roomId: 1, playerCount: 3);
        var roomB = MakeRoom(roomId: 2, playerCount: 3);

        var result = ObjectiveDealer.Deal(roomA);

        Assert.All(result, o =>
        {
            Assert.Equal(roomA.Id, o.RoomId);
            Assert.Contains(o.PlayerId, roomA.Players.Select(p => p.Id));
        });
        Assert.All(roomB.Players, p => Assert.Empty(p.Objectives));
        Assert.Equal("Lobby", roomB.State);
    }
}