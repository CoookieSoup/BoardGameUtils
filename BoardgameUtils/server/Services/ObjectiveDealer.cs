using Server.Models;

public static class ObjectiveDealer
{
    public static List<PlayerObjective> Deal(Room room, List<NemesisObjectiveTemplate>? catalog = null)
   {
        catalog ??= NemesisObjectiveCatalog.AllNemesisObjectives; // This is for unit tests
        var players = room.Players;

        if (players.Count < 2)
            throw new ArgumentException("At least 2 players are required.");

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

        if (coop.Count < players.Count || selfish.Count < players.Count)
            throw new ArgumentException("Not enough objectives.");

        var result = new List<PlayerObjective>();

        for (int i = 0; i < players.Count; i++)
        {
            result.Add(new PlayerObjective
            {
                PlayerId = players[i].Id,
                RoomId = room.Id,

                Title = coop[i].Title,
                Type = coop[i].Type,
                MinPlayers = coop[i].MinPlayers
            });

            result.Add(new PlayerObjective
            {
                PlayerId = players[i].Id,
                RoomId = room.Id,

                Title = selfish[i].Title,
                Type = selfish[i].Type,
                MinPlayers = selfish[i].MinPlayers
            });
        }

        return result;
    }
}