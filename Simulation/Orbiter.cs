using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En sond som kretsar kring en planet i stället för att fara förbi den:
/// Cassini vid Saturnus och Juno vid Jupiter. Enklare fall än de fem sonder
/// som lämnat solsystemet – här är banan en vanlig ellips, precis som månarnas.
///
/// En viktig skillnad mot Voyager och de andra: de banorna är återskapade ur
/// verkliga datum, så sonderna står på rätt plats rätt dag. Här går det inte.
/// Cassini flög nästan tre hundra olika varv kring Saturnus under tretton år,
/// med omloppstider från en vecka till fyra månader och lutningar från
/// ringplanet ända upp till 75 grader. Det som visas är därför ett
/// representativt varv: storleken, formen, omloppstiden och banplanet är
/// verkliga, men var sonden befinner sig i banan ett givet datum är det inte.
/// </summary>
public sealed class Orbiter
{
    /// <summary>Sondens namn, visas vid pricken.</summary>
    public string Name { get; }

    /// <summary>Färgen sonden och dess bana ritas i.</summary>
    public Color Color { get; }

    /// <summary>Planeten sonden kretsar kring.</summary>
    public CelestialBody Center { get; }

    /// <summary>Banan, räknad kring planeten och inte kring solen.</summary>
    public Conic Path { get; }

    /// <summary>Dagen sonden gick in i omloppsbana.</summary>
    public double ArrivalDay { get; }

    /// <summary>Dagen uppdraget tog slut, eller null medan det pågår.</summary>
    public double? EndDay { get; }

    /// <summary>Vad som hände på slutet, för panelen. Tom sträng när uppdraget pågår.</summary>
    public string Ending { get; }

    Orbiter(string name, Color color, CelestialBody center, Conic path,
        double arrivalDay, double? endDay, string ending)
    {
        Name = name;
        Color = color;
        Center = center;
        Path = path;
        ArrivalDay = arrivalDay;
        EndDay = endDay;
        Ending = ending;
    }

    /// <summary>
    /// Bygger sonden ur banans storlek och form uttryckt i planetradier, som är
    /// det mått uppgifterna om sådana här banor brukar anges i.
    ///
    /// Banplanet anges i förhållande till planetens ekvator, inte till
    /// ekliptikan: en polär bana är 90 grader mot ekvatorn oavsett hur planeten
    /// själv lutar. Lutningen läggs därför till planetens egen, med samma
    /// uppstigande nod – att vrida ekvatorsplanet ett kvarts varv kring nodlinjen
    /// ger just ett plan genom båda polerna.
    /// </summary>
    public static Orbiter? Build(string name, Color color, CelestialBody center,
        double periapsisRadii, double apoapsisRadii,
        double inclinationToEquatorDeg, double equatorInclinationDeg, double equatorNodeDeg,
        double argPeriapsisDeg, DateTime arrival, DateTime? end = null, string ending = "")
    {
        if (center.Mu <= 0 || periapsisRadii <= 0 || apoapsisRadii < periapsisRadii)
            return null;

        double rp = periapsisRadii * center.RadiusKm / SolarSystemData.AuKm;
        double ra = apoapsisRadii * center.RadiusKm / SolarSystemData.AuKm;
        double semiMajorAu = 0.5 * (rp + ra);
        double eccentricity = (ra - rp) / (ra + rp);

        double arrivalDay = (arrival - SolarSystemData.EpochJ2000).TotalDays;

        var path = Conic.FromElements(semiMajorAu, eccentricity,
            equatorInclinationDeg + inclinationToEquatorDeg, equatorNodeDeg,
            argPeriapsisDeg, arrivalDay, center.Mu);

        return new Orbiter(name, color, center, path, arrivalDay,
            end is { } e ? (e - SolarSystemData.EpochJ2000).TotalDays : null, ending);
    }

    /// <summary>Sant när sonden kretsar kring planeten just den dagen.</summary>
    public bool Exists(double day)
        => day >= ArrivalDay && (EndDay is not double end || day <= end);

    /// <summary>Omloppstiden i dygn.</summary>
    public double PeriodDays => Path.PeriodDays ?? 0.0;

    /// <summary>Sondens läge i förhållande till planeten.</summary>
    public Vector3 PositionAt(double day, float unitsPerAu)
        => Path.PositionAt(day, unitsPerAu);

    /// <summary>Sondens fart i km/s. Störst vid periapsis, precis som för allt annat.</summary>
    public double SpeedKmPerSecond(double day) => Path.SpeedKmPerSecond(day);

    /// <summary>
    /// Hela banellipsen som punktlista, planetcentriskt. Ett varv räcker –
    /// banan är sluten.
    /// </summary>
    public Vector3[] OrbitPath(int samples, float unitsPerAu)
    {
        var points = new Vector3[samples];
        double period = PeriodDays;

        for (int i = 0; i < samples; i++)
            points[i] = Path.PositionAt(ArrivalDay + period * i / samples, unitsPerAu);

        return points;
    }
}
