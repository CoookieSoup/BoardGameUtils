namespace Server.Models;

public record NemesisObjectiveTemplate(
    string Title,
    string Type,
    int MaxPlayers
);

public static class NemesisObjectiveCatalog
{
    public static readonly List<NemesisObjectiveTemplate> AllNemesisObjectives = new()
    {
        new("Išsiųsti signalą ir hibernuoti", "kooperatyvinis", 2),
        new("Užtikrinti, kad laivas pasiektų Žemę", "kooperatyvinis", 2),
        new("Užtikrinti, kad laivas pasiektų Marsą", "kooperatyvinis", 2),
        new("Atrarsti įsibrovėlių silpnybes", "kooperatyvinis", 3),
        new("Sunaikinti lizdą", "kooperatyvinis", 3),
        new("Karalienė turi būti negyva", "kooperatyvinis", 4),
        new("Kad kiekvienas kambarys būtų išieškotas", "kooperatyvinis", 5),

        new("Žaidėjas 1 turi neišgyventi", "savanaudiškas", 2),
        new("Žaidėjas 2 turi neišgyventi", "savanaudiškas", 2),
        new("Žaidėjas 3 turi neišgyventi", "savanaudiškas", 3),
        new("Žaidėjas 4 turi neišgyventi", "savanaudiškas", 4),
        new("Žaidėjas 5 turi neišgyventi", "savanaudiškas", 5),
        new("Laivas turi būti sunaikintas", "savanaudiškas", 2),
        new("Išgyventi ir turėti 7 daiktus", "savanaudiškas", 3)
    };
}