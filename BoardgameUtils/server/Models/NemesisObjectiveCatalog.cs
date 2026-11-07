namespace Server.Models;
public record NemesisObjectiveTemplate(
    string Title,
    int Type, // 1 = Cooperative, 2 = Selfish
    int MaxPlayers
);

public static class NemesisObjectiveCatalog
{
    public static readonly List<NemesisObjectiveTemplate> AllNemesisObjectives = new()
    {
        new("Išsiųsti signalą ir hibernuoti", 1, 2),
        new("Užtikrinti, kad laivas pasiektų Žemę", 1, 2),
        new("Užtikrinti, kad laivas pasiektų Marsą", 1, 2),
        new("Atrasti įsibrovėlių silpnybes", 1, 3),
        new("Sunaikinti lizdą", 1, 3),
        new("Karalienė turi būti negyva", 1, 4),
        new("Kad kiekvienas kambarys būtų išieškotas", 1, 5),

        new("Žaidėjas 1 turi neišgyventi", 2, 2),
        new("Žaidėjas 2 turi neišgyventi", 2, 2),
        new("Žaidėjas 3 turi neišgyventi", 2, 3),
        new("Žaidėjas 4 turi neišgyventi", 2, 4),
        new("Žaidėjas 5 turi neišgyventi", 2, 5),
        new("Laivas turi būti sunaikintas", 2, 2),
        new("Išgyventi ir turėti 7 daiktus", 2, 3)
    };
}