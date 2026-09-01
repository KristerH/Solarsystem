namespace Solarsystem.Simulation;

/// <summary>
/// Möten på himlen: när två planeter står i samma riktning sett från jorden,
/// eller när en planet står mitt emot solen.
///
/// Poängen med att räkna dem <b>sett från jorden</b> och inte från solen är
/// inte en detalj. Den stora konjunktionen mellan Jupiter och Saturnus 2020
/// hamnar på rätt dag, 21 december, om man frågar från jorden – men sju veckor
/// fel, 1 november, om man jämför deras heliocentriska longituder. Det är från
/// jorden man ser dem, och det är jordens eget läge som avgör när de ser ut att
/// mötas.
///
/// Sökningen har samma form som startfönstren i <see cref="Mission"/>: stega
/// dagvis, hitta ett dalläge, förfina. Skillnaden är att det som minimeras här
/// är en vinkel på himlen i stället för en fart.
/// </summary>
public static class SkyEvent
{
    public enum Kind
    {
        /// <summary>Två kroppar står i samma riktning sett från jorden.</summary>
        Conjunction,

        /// <summary>Planeten står mitt emot solen, alltså närmast jorden och synlig hela natten.</summary>
        Opposition,

        /// <summary>Månen går framför solen: nymåne nära en av månbanans noder.</summary>
        SolarEclipse,

        /// <summary>Månen går in i jordens skugga: fullmåne nära en av noderna.</summary>
        LunarEclipse,
    }

    /// <summary>Ett möte: när det sker, hur nära på himlen, och hur långt bort kroppen är.</summary>
    public sealed record Meeting(double Day, double SeparationDeg, double DistanceAu);

    /// <summary>Ett val i väljaren: vad som ska sökas och mellan vilka kroppar.</summary>
    public sealed record Choice(string Label, Kind Kind, CelestialBody A, CelestialBody? B);

    const double CoarseStepDays = 1.0;

    /// <summary>Hur långt fram sökningen går innan den ger upp. Pluto behöver mest: 248 år.</summary>
    const double MaxSearchDays = 300.0 * 365.25;

    /// <summary>
    /// Hur nära två kroppar måste stå för att det ska räknas som ett möte. Fem
    /// grader är ungefär ett kikarfält – står de längre isär än så är det ingen
    /// som skulle kalla det en konjunktion, bara ett närmande.
    /// </summary>
    const double ConjunctionLimitDeg = 5.0;

    /// <summary>
    /// Hur nära solen månen måste komma för att det ska bli solförmörkelse
    /// någonstans på jorden. Solen och månen är båda drygt en halv grad breda,
    /// och parallaxen från olika platser på jordklotet flyttar månen upp till en
    /// grad – tillsammans blir gränsen drygt en och en halv grad räknat från
    /// jordens medelpunkt.
    /// </summary>
    const double SolarEclipseLimitDeg = 1.55;

    /// <summary>
    /// Motsvarande för månförmörkelse. Jordens kärnskugga är på månens avstånd
    /// ungefär 0,7 grader i radie och månen 0,26, så en bit av månen hamnar i
    /// skuggan när mitten kommer inom en grad från den punkt som ligger rakt
    /// mitt emot solen.
    /// </summary>
    const double LunarEclipseLimitDeg = 1.0;

    /// <summary>
    /// Vinkeln mellan månen och solen sett från jorden – eller från den punkt
    /// som ligger rakt mitt emot solen, vilket är där jordens skugga faller.
    ///
    /// Månens banelement är geocentriska: dess läge <b>är</b> redan riktningen
    /// från jorden. Att dra bort jordens läge en gång till, som för planeterna,
    /// hade lagt jordbanan ovanpå månbanan och gett svar på tio grader fel.
    /// </summary>
    static double EclipseCost(Kind kind, double day)
    {
        var toSun = (-Earth.PositionAuAt(day)).Normalized();
        var toMoon = SolarSystemData.Moon.PositionAuAt(day).Normalized();
        double separation = Math.Acos(Math.Clamp(Vec3.Dot(toSun, toMoon), -1.0, 1.0))
                            * 180.0 / Math.PI;
        return kind == Kind.SolarEclipse ? separation : 180.0 - separation;
    }

    static CelestialBody Earth => SolarSystemData.Planets.First(p => p.Name == "Jorden");

    /// <summary>
    /// Vinkeln mellan två riktningar sett från jorden, i grader. Null betyder
    /// solen, som ligger i origo.
    /// </summary>
    static double AngleFromEarth(CelestialBody? a, CelestialBody? b, double day)
    {
        var earth = Earth.PositionAuAt(day);
        var da = ((a?.PositionAuAt(day) ?? default) - earth).Normalized();
        var db = ((b?.PositionAuAt(day) ?? default) - earth).Normalized();
        return Math.Acos(Math.Clamp(Vec3.Dot(da, db), -1.0, 1.0)) * 180.0 / Math.PI;
    }

    /// <summary>
    /// Storheten som ska bli så liten som möjligt. Vid konjunktion är det
    /// avståndet på himlen mellan de två kropparna; vid opposition är det hur
    /// långt planeten är från att stå rakt mitt emot solen.
    /// </summary>
    static double Cost(Kind kind, CelestialBody a, CelestialBody? b, double day) => kind switch
    {
        Kind.Conjunction => AngleFromEarth(a, b, day),
        Kind.Opposition => 180.0 - AngleFromEarth(null, a, day),
        _ => EclipseCost(kind, day),
    };

    /// <summary>Hur nära mötet måste vara för att räknas som ett möte alls.</summary>
    static double LimitFor(Kind kind) => kind switch
    {
        Kind.Conjunction => ConjunctionLimitDeg,
        Kind.SolarEclipse => SolarEclipseLimitDeg,
        Kind.LunarEclipse => LunarEclipseLimitDeg,
        _ => 180.0,                             // en opposition har ingen gräns
    };

    /// <summary>
    /// Nästa möte efter den givna dagen, eller null om inget hittas inom
    /// sökfönstret. Anropas när man klickar, så sökningen får kosta något – ett
    /// helt sekel är några tiondels sekund.
    /// </summary>
    public static Meeting? Next(Kind kind, CelestialBody a, CelestialBody? b, double fromDay)
    {
        double f0 = Cost(kind, a, b, fromDay);
        double f1 = Cost(kind, a, b, fromDay + CoarseStepDays);

        for (double d = fromDay + CoarseStepDays; d < fromDay + MaxSearchDays; d += CoarseStepDays)
        {
            double f2 = Cost(kind, a, b, d + CoarseStepDays);

            // Ett dalläge ligger inklämt mellan de tre proven.
            if (f1 < f0 && f1 <= f2)
            {
                double day = Refine(kind, a, b, d - CoarseStepDays, d + CoarseStepDays);

                // Ett närmande på trettio grader är ingen konjunktion, och en
                // nymåne långt från noden är ingen förmörkelse. Sök vidare.
                if (Cost(kind, a, b, day) > LimitFor(kind))
                {
                    f0 = f1; f1 = f2;
                    continue;
                }

                if (kind is Kind.SolarEclipse or Kind.LunarEclipse)
                    return new Meeting(day, EclipseCost(kind, day),
                        SolarSystemData.Moon.PositionAuAt(day).Length);

                double separation = kind == Kind.Conjunction
                    ? AngleFromEarth(a, b, day)
                    : AngleFromEarth(null, a, day);
                double au = (a.PositionAuAt(day) - Earth.PositionAuAt(day)).Length;
                return new Meeting(day, separation, au);
            }

            f0 = f1;
            f1 = f2;
        }

        return null;
    }

    /// <summary>
    /// Klämmer in dalläget med gyllene snittet. Vinkeln har inget enkelt
    /// uttryck att derivera, så sökningen jämför bara värden – tillräckligt
    /// många varv för att komma under en minut i tid.
    /// </summary>
    static double Refine(Kind kind, CelestialBody a, CelestialBody? b, double lo, double hi)
    {
        const double phi = 0.6180339887498949;
        double c = hi - (hi - lo) * phi, d = lo + (hi - lo) * phi;
        double fc = Cost(kind, a, b, c), fd = Cost(kind, a, b, d);

        for (int k = 0; k < 40 && hi - lo > 1e-4; k++)
        {
            if (fc < fd)
            {
                hi = d; d = c; fd = fc;
                c = hi - (hi - lo) * phi;
                fc = Cost(kind, a, b, c);
            }
            else
            {
                lo = c; c = d; fc = fd;
                d = lo + (hi - lo) * phi;
                fd = Cost(kind, a, b, d);
            }
        }

        return 0.5 * (lo + hi);
    }

    /// <summary>
    /// Vad väljaren erbjuder. Oppositionerna gäller de kroppar som kan ha
    /// någon – de som ligger utanför jordens bana. Merkurius och Venus kan
    /// aldrig komma mitt emot solen sett härifrån; de går alltid nära den på
    /// himlen, vilket i sig är värt att veta.
    ///
    /// Konjunktionerna är de par som går att se med blotta ögat, alltså de
    /// ljusa planeterna. Att ha med Neptunus hade varit meningslöst.
    /// </summary>
    public static readonly Choice[] Choices = BuildChoices();

    static Choice[] BuildChoices()
    {
        CelestialBody B(string n) => SolarSystemData.Planets.First(p => p.Name == n);
        var list = new List<Choice>();

        foreach (string name in new[]
                 { "Mars", "Jupiter", "Saturnus", "Uranus", "Neptunus", "Pluto" })
            list.Add(new Choice($"{name} i opposition", Kind.Opposition, B(name), null));

        (string A, string B)[] pairs =
        [
            ("Venus", "Jupiter"), ("Venus", "Mars"), ("Venus", "Saturnus"),
            ("Mars", "Jupiter"), ("Mars", "Saturnus"), ("Jupiter", "Saturnus"),
        ];
        foreach (var (a, b) in pairs)
            list.Add(new Choice($"{a} möter {b}", Kind.Conjunction, B(a), B(b)));

        list.Add(new Choice("Solförmörkelse", Kind.SolarEclipse, SolarSystemData.Moon, null));
        list.Add(new Choice("Månförmörkelse", Kind.LunarEclipse, SolarSystemData.Moon, null));

        return [.. list];
    }
}
