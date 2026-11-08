namespace Server.Models;
public record NemesisObjectiveTemplate(
    string Title,
    ObjectiveType Type,
    int MinPlayers
);

public static class NemesisObjectiveCatalog
{
    public static readonly List<NemesisObjectiveTemplate> AllNemesisObjectives = new()
    {
        new("Išsiųsti signalą ir hibernuoti", ObjectiveType.Cooperative, 2),
        new("Užtikrinti, kad laivas pasiektų Žemę", ObjectiveType.Cooperative, 2),
        new("Užtikrinti, kad laivas pasiektų Marsą", ObjectiveType.Cooperative, 2),
        new("Atrasti įsibrovėlių silpnybes", ObjectiveType.Cooperative, 3),
        new("Sunaikinti lizdą", ObjectiveType.Cooperative, 3),
        new("Karalienė turi būti negyva", ObjectiveType.Cooperative, 4),
        new("Kad kiekvienas kambarys būtų išieškotas", ObjectiveType.Cooperative, 5),

        new("Žaidėjas 1 turi neišgyventi", ObjectiveType.Selfish, 2),
        new("Žaidėjas 2 turi neišgyventi", ObjectiveType.Selfish, 2),
        new("Žaidėjas 3 turi neišgyventi", ObjectiveType.Selfish, 3),
        new("Žaidėjas 4 turi neišgyventi", ObjectiveType.Selfish, 4),
        new("Žaidėjas 5 turi neišgyventi", ObjectiveType.Selfish, 5),
        new("Laivas turi būti sunaikintas", ObjectiveType.Selfish, 2),
        new("Išgyventi ir turėti 7 daiktus", ObjectiveType.Selfish, 3)
    };
}