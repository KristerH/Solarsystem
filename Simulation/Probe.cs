using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En punkt där en sond bevisligen befann sig: en planet ett visst datum, eller
/// ett känt läge i rymden ett visst datum. Uppskjutningen räknas som jorden.
/// </summary>
public sealed class Waypoint
{
    /// <summary>Vad punkten heter, för milstolpar och felsökning.</summary>
    public string Name { get; }

    /// <summary>Datumet sonden var där.</summary>
    public DateTime Date { get; }

    /// <summary>Samma datum i dygn sedan J2000.</summary>
    public double Day => (Date - SolarSystemData.EpochJ2000).TotalDays;

    readonly CelestialBody? _body;
    readonly Vector3 _fixedAu;

    Waypoint(string name, DateTime date, CelestialBody? body, Vector3 fixedAu)
    {
        Name = name;
        Date = date;
        _body = body;
        _fixedAu = fixedAu;
    }

    /// <summary>Sonden var vid en planet det här datumet.</summary>
    public static Waypoint At(CelestialBody body, DateTime date)
        => new(body.Name, date, body, default);

    /// <summary>
    /// Sonden var på ett känt avstånd i en känd riktning på himlen. Så anges
    /// sondernas nuvarande lägen, och det är den uppgiften som bestämmer hur
    /// den sista sträckan ut ur solsystemet lutar: banan dit räknas fram ur
    /// avståndet och riktningen, i stället för att lutningen matas in.
    /// </summary>
    public static Waypoint InSky(string name, DateTime date, double distanceAu,
        double raHours, double decDeg)
        => new(name, date, null,
            StarCatalog.EquatorialToWorld(raHours, decDeg) * (float)distanceAu);

    /// <summary>Punktens läge i AU, soltcentriskt.</summary>
    public Vector3 PositionAu() => _body?.PositionAt(Day, 1f) ?? _fixedAu;
}

/// <summary>
/// Ett ben av en sonds färd: banan från en punkt till nästa. Banan är den som
/// verkligen går mellan de två lägena på exakt den tid som förflöt mellan
/// datumen, och den är oftast en hyperbel – sonderna har fart nog att aldrig
/// komma tillbaka.
/// </summary>
public sealed record ProbeLeg(string From, string To, double StartDay, double EndDay, Conic Path)
{
    /// <summary>Hur länge benet varade, i dygn.</summary>
    public double Days => EndDay - StartDay;
}

/// <summary>
/// En verklig rymdsond, byggd ur de datum den faktiskt passerade planeterna.
///
/// Banelement matas alltså inte in. I stället får varje ben av färden vara den
/// bana som går från en planet till nästa på exakt den tid passagerna tog,
/// räknad ur appens egna planetpositioner med Lambert-lösaren. Två saker följer
/// av det: sonden hamnar vid rätt planet rätt dag av sig själv, och farten
/// hoppar uppåt vid varje passage utan att någon har lagt in hoppet – det är
/// gravitationsslungan, och den är hela förklaringen till hur sonderna kunde nå
/// så långt.
///
/// Sista benet går ut till sondens läge i dag, angivet som avstånd och riktning
/// på himlen. Därför blir också lutningen ut ur ekliptikan ett resultat och inte
/// en inmatning.
/// </summary>
public sealed class Probe
{
    /// <summary>Sondens namn, visas vid pricken.</summary>
    public string Name { get; }

    /// <summary>Färgen sonden och dess spår ritas i.</summary>
    public Color Color { get; }

    /// <summary>Färdens ben, i tidsordning.</summary>
    public IReadOnlyList<ProbeLeg> Legs { get; }

    /// <summary>Uppskjutningsdagen, i dygn sedan J2000.</summary>
    public double LaunchDay { get; }

    Probe(string name, Color color, IReadOnlyList<ProbeLeg> legs)
    {
        Name = name;
        Color = color;
        Legs = legs;
        LaunchDay = legs.Count > 0 ? legs[0].StartDay : 0;
    }

    /// <summary>
    /// Bygger sonden ur punkterna den passerade. Ett ben som inte går att lösa
    /// hoppas över; det märks i att Legs blir kortare än väntat, och kontrolleras
    /// bäst utanför appen.
    /// </summary>
    public static Probe Build(string name, Color color, params Waypoint[] waypoints)
    {
        var legs = new List<ProbeLeg>(waypoints.Length);

        for (int i = 0; i + 1 < waypoints.Length; i++)
        {
            Waypoint from = waypoints[i], to = waypoints[i + 1];
            double travelDays = to.Day - from.Day;
            if (travelDays <= 0)
                continue;

            var r1 = from.PositionAu();
            var r2 = to.PositionAu();
            if (!Lambert.Solve(r1, r2, travelDays, SolarSystemData.SunMu, out var v1, out _))
                continue;

            if (Conic.FromState(r1, v1, from.Day, SolarSystemData.SunMu) is { } path)
                legs.Add(new ProbeLeg(from.Name, to.Name, from.Day, to.Day, path));
        }

        return new Probe(name, color, legs);
    }

    /// <summary>Sant när sonden har skjutits upp och alltså finns att rita.</summary>
    public bool Exists(double day) => Legs.Count > 0 && day >= LaunchDay;

    /// <summary>
    /// Benet sonden befinner sig på. Efter sista punkten fortsätter den på sitt
    /// sista ben – banan gäller ju vidare, sonden är fortfarande på väg utåt.
    /// </summary>
    public ProbeLeg? LegAt(double day)
    {
        if (!Exists(day))
            return null;

        var leg = Legs[0];
        for (int i = 1; i < Legs.Count && Legs[i].StartDay <= day; i++)
            leg = Legs[i];
        return leg;
    }

    /// <summary>Sondens läge, eller null innan den skjutits upp.</summary>
    public Vector3? PositionAt(double day, float unitsPerAu)
        => LegAt(day)?.Path.PositionAt(day, unitsPerAu);

    /// <summary>Sondens avstånd från solen i AU, eller noll innan uppskjutningen.</summary>
    public double DistanceAu(double day)
        => LegAt(day)?.Path.DistanceAu(day) ?? 0.0;

    /// <summary>
    /// Sondens fart i km/s. Farten hoppar vid varje planetpassage, eftersom
    /// benen möts i samma läge men med olika hastighet – det är slungan.
    /// </summary>
    public double SpeedKmPerSecond(double day)
        => LegAt(day)?.Path.SpeedKmPerSecond(day) ?? 0.0;
}
