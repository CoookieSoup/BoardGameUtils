public enum ObjectiveType { Cooperative, Selfish }
public record Objective(string Code, string Text, ObjectiveType Type, int MinPlayers);

public static class ObjectiveDealer
{
    public static List<(Guid PlayerId, Objective Coop, Objective Selfish)> Deal(
        List<Guid> players, List<Objective> pool)
    {
        

        int playerCount = players.Count;

        if (playerCount < 2)
        {
            throw new ArgumentException("At least 2 players are required to deal objectives."); //TODO padaryt kad kazka rodytu
        }

        var eligibleObjectives = pool.Where(o => o.MinPlayers <= playerCount).ToList();

        List<Objective> shuffledCoopObjectives = eligibleObjectives.Where(o => o.Type == ObjectiveType.Cooperative).OrderBy(_ => Random.Shared.Next()).ToList();
        List<Objective> shuffledSelfishObjectives = eligibleObjectives.Where(o => o.Type == ObjectiveType.Selfish).OrderBy(_ => Random.Shared.Next()).ToList();

        if(shuffledCoopObjectives.Count < playerCount || shuffledSelfishObjectives.Count < playerCount)
        {
            throw new ArgumentException("Not enough objectives of each type to deal to all players."); //TODO padaryt kad kazka rodytu
        }

        var dealtObjectives = new List<(Guid PlayerId, Objective Coop, Objective Selfish)>();
        for (int i = 0; i < playerCount; i++)
        {
            dealtObjectives.Add((players[i], shuffledCoopObjectives[i], shuffledSelfishObjectives[i]));
        }

        return dealtObjectives;
    }
}