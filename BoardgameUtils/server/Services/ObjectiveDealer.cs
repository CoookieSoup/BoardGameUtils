using Server.Models;

public static class ObjectiveDealer
{
    public static List<PlayerObjective> Deal(Room room, List<NemesisObjectiveTemplate>? catalog = null)
   {
        catalog ??= NemesisObjectiveCatalog.AllNemesisObjectives;
        var players = room.Players;

        if (players.Count < 2)
            throw new InvalidOperationException("At least 2 players are required.");

        var eligible = catalog
            .Where(o => o.MinPlayers <= players.Count)
            .ToList();

        var coop = eligible
            .Where(o => o.Type == ObjectiveType.Cooperative)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        var selfish = eligible
            .Where(o => o.Type == ObjectiveType.Selfish)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        if (coop.Count < players.Count)
            throw new InvalidOperationException($"Not enough cooperative objectives: need {players.Count}, have {coop.Count}");

        if (selfish.Count < players.Count)
            throw new InvalidOperationException($"Not enough selfish objectives: need {players.Count}, have {selfish.Count}");

        var result = new List<PlayerObjective>();

        for (int i = 0; i < players.Count; i++)
        {
            result.Add(new PlayerObjective
            {
                PlayerId = players[i].Id,
                RoomId = room.Id,
                TenantId = room.TenantId,

                Title = coop[i].Title,
                Type = coop[i].Type,
                MinPlayers = coop[i].MinPlayers
            });

            result.Add(new PlayerObjective
            {
                PlayerId = players[i].Id,
                RoomId = room.Id,
                TenantId = room.TenantId,

                Title = selfish[i].Title,
                Type = selfish[i].Type,
                MinPlayers = selfish[i].MinPlayers
            });
        }

        Validate(room, result);
        return result;
    }

    public static void Validate(Room room, List<PlayerObjective> dealt)
    {
        var playerCount = room.Players.Count;

        if (dealt.Select(o => o.Title).Distinct().Count() != dealt.Count)
            throw new InvalidOperationException("Objectives were dealt twice");

        if (dealt.Any(o => o.MinPlayers > playerCount))
            throw new InvalidOperationException("An objective needs more players than are in the room");

        foreach (var player in room.Players)
        {
            var mine = dealt.Where(o => o.PlayerId == player.Id).ToList();
            var coopCount = mine.Count(o => o.Type == ObjectiveType.Cooperative);
            var selfishCount = mine.Count(o => o.Type == ObjectiveType.Selfish);

            if (mine.Count != 2 || coopCount != 1 || selfishCount != 1)
                throw new InvalidOperationException($"Player {player.Nickname} did not get one cooperative and one selfish objective");
        }
    }
}