using Server.Models;

public static class ObjectiveDealer
{
    public static List<PlayerObjective> Deal(Room room)
    {
        var players = room.Players;

        if (players.Count < 2)
            throw new ArgumentException("At least 2 players are required.");

        var eligible = NemesisObjectiveCatalog.AllNemesisObjectives
            .Where(o => o.MaxPlayers <= players.Count)
            .ToList();

        var coop = eligible
            .Where(o => o.Type == 1)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        var selfish = eligible
            .Where(o => o.Type == 2)
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
                MaxPlayers = coop[i].MaxPlayers
            });

            result.Add(new PlayerObjective
            {
                PlayerId = players[i].Id,
                RoomId = room.Id,

                Title = selfish[i].Title,
                Type = selfish[i].Type,
                MaxPlayers = selfish[i].MaxPlayers
            });
        }

        return result;
    }
}