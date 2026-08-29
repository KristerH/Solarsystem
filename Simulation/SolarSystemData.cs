using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En himlakropp med keplerska banelement (epok J2000).
/// Vinklar i grader: banlutning i, uppstigande nodens longitud Ω,
/// perihelielongitud ϖ samt medellongitud L0 vid epoken.
/// </summary>
/// <summary>
/// Ett ringsystem kring en planet. Radierna anges i planetradier och planet
/// beskrivs med samma lutning och nod som planetens ekvator, så att ringarna
/// hamnar i samma plan som månarna.
/// </summary>
/// <param name="MinScreenRadius">
/// Ringen ritas först när den når så här många pixlar på skärmen. Saturnus
/// ringar syns med blotta ögat i ett litet teleskop och får därför ett lågt
/// värde; de övriga tre är så svaga att de upptäcktes först med rymdsonder,
/// och dyker därför upp först vid rejäl inzoomning.
/// </param>
public sealed record PlanetRing(
    float InnerRadii,
    float OuterRadii,
    double InclinationDeg,
    double AscNodeDeg,
    Color Color,
    float MinScreenRadius);

public sealed record CelestialBody(
    string Name,
    Color BodyColor,
    double RadiusKm,
    double SemiMajorAu,
    double Eccentricity,
    double InclinationDeg,
    double AscNodeDeg,
    double PerihelionLonDeg,
    double MeanLonJ2000Deg,
    double OrbitalPeriodDays)
{
    /// <summary>
    /// Månar som kretsar kring den här kroppen. Deras banelement är
    /// planetcentriska – PositionAt ger då förskjutningen från planeten,
    /// inte från solen.
    /// </summary>
    public CelestialBody[] Moons { get; init; } = [];

    /// <summary>
    /// Månens andel av systemets totala massa. Styr hur mycket moderkroppen
    /// själv vaggar kring den gemensamma tyngdpunkten. Noll (standard) betyder
    /// att moderkroppen står stilla, vilket räcker för alla lätta månar.
    /// </summary>
    public double MassFraction { get; init; }

    /// <summary>Planetens ringsystem, eller null för kroppar utan ringar.</summary>
    public PlanetRing? Ring { get; init; }

    /// <summary>Position i världskoordinater (Y = norr om ekliptikan) vid given tid.</summary>
    public Vector3 PositionAt(double daysSinceJ2000, float unitsPerAu)
    {
        double meanMotion = 360.0 / OrbitalPeriodDays;
        double mDeg = MeanLonJ2000Deg + meanMotion * daysSinceJ2000 - PerihelionLonDeg;
        // Vik ned till ett varv innan omvandlingen till radianer. Snabba månar
        // (Phobos hinner tre varv per dygn) ger annars miljontals grader, vilket
        // tär på flyttalsprecisionen. Banpositionen är oförändrad.
        double M = DegToRad(mDeg % 360.0);
        double E = SolveKepler(M, Eccentricity);
        return ToWorld(E, unitsPerAu);
    }

    /// <summary>Hela banellipsen som punktlista (sluten kurva, jämnt samplad i excentrisk anomali).</summary>
    public Vector3[] OrbitPath(int samples, float unitsPerAu)
    {
        var pts = new Vector3[samples];
        for (int k = 0; k < samples; k++)
            pts[k] = ToWorld(2.0 * Math.PI * k / samples, unitsPerAu);
        return pts;
    }

    Vector3 ToWorld(double E, float unitsPerAu)
    {
        double e = Eccentricity;
        // Position i banplanet (fokus = solen).
        double xv = SemiMajorAu * (Math.Cos(E) - e);
        double yv = SemiMajorAu * Math.Sqrt(1.0 - e * e) * Math.Sin(E);

        double w = DegToRad(PerihelionLonDeg - AscNodeDeg); // periheliets argument
        double O = DegToRad(AscNodeDeg);
        double i = DegToRad(InclinationDeg);
        double cw = Math.Cos(w), sw = Math.Sin(w);
        double cO = Math.Cos(O), sO = Math.Sin(O);
        double ci = Math.Cos(i), si = Math.Sin(i);

        // Rotation banplan -> ekliptiska koordinater.
        double x = (cw * cO - sw * sO * ci) * xv + (-sw * cO - cw * sO * ci) * yv;
        double y = (cw * sO + sw * cO * ci) * xv + (-sw * sO + cw * cO * ci) * yv;
        double z = (sw * si) * xv + (cw * si) * yv;

        // Ekliptikans plan läggs horisontellt; norr (+z) pekar uppåt (+Y).
        return new Vector3(
            (float)(x * unitsPerAu),
            (float)(z * unitsPerAu),
            (float)(-y * unitsPerAu));
    }

    static double SolveKepler(double M, double e)
    {
        // Newton-Raphson på Keplers ekvation E - e·sin E = M.
        double E = M;
        for (int k = 0; k < 12; k++)
            E -= (E - e * Math.Sin(E) - M) / (1.0 - e * Math.Cos(E));
        return E;
    }

    static double DegToRad(double d) => d * Math.PI / 180.0;
}

public static class SolarSystemData
{
    public const double AuKm = 149_597_870.7;
    public const double SunRadiusKm = 696_340.0;
    public static readonly DateTime EpochJ2000 = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Månen med geocentriska medelbanelement (J2000): banan beräknas kring
    /// jorden i stället för kring solen, med samma Kepler-matematik.
    /// Ett varv tar 27,3 dygn (siderisk månad).
    /// </summary>
    public static readonly CelestialBody Moon = new(
        "Månen", Color.FromArgb("#BEBEB6"), 1_737.4,
        0.0025696 /* = 384 399 km */, 0.0549, 5.145, 125.045, 83.353, 218.316, 27.32166);

    // Mars två små, oregelbundna månar – troligen infångade asteroider. De
    // kretsar mycket nära Mars: Phobos på bara 2,8 marsradier (jämför Månens
    // 60 jordradier). Banorna ligger i Mars ekvatorsplan, som lutar 26,7° mot
    // ekliptikan – det är därför banlutningen nedan inte är nära noll.
    // Not: faslägena (medellongituderna) är approximativa. Med omloppstider på
    // timmar hinner även ett mycket litet periodfel bli hela varv över de
    // årtionden appen kan simulera, så månarnas exakta placering i banan vid
    // ett givet datum går ändå inte att lita på. Avstånd, storlekar, banplan
    // och omloppstider är däremot verkliga.
    const double MarsEquatorInclinationDeg = 26.74;
    const double MarsEquatorNodeDeg = 262.85;

    public static readonly CelestialBody Phobos = new(
        "Phobos", Color.FromArgb("#A89684"), 11.267,
        9_376.0 / AuKm, 0.0151, MarsEquatorInclinationDeg, MarsEquatorNodeDeg,
        0.0, 0.0, 0.3189100);

    public static readonly CelestialBody Deimos = new(
        "Deimos", Color.FromArgb("#B8A794"), 6.2,
        23_463.2 / AuKm, 0.00033, MarsEquatorInclinationDeg, MarsEquatorNodeDeg,
        0.0, 180.0, 1.2624407);

    // Jupiters fyra stora månar, de som Galilei såg 1610 och som avslöjade att
    // allt inte kretsar kring jorden. Banorna ligger i Jupiters ekvatorsplan,
    // som bara lutar 2,2° mot ekliptikan (Jupiter står nästan rakt upp).
    //
    // Faslägena är valda så att Laplace-resonansen gäller:
    //     medellongitud(Io) - 3*medellongitud(Europa) + 2*medellongitud(Ganymedes) = 180°
    // Eftersom omloppstiderna redan uppfyller resonansen i medelrörelse hålls
    // villkoret över tid. Följden är att de tre inre månarna aldrig kan stå på
    // linje samtidigt – när Io och Europa möts står Ganymedes alltid 90° bort.
    const double JupiterEquatorInclinationDeg = 2.22;
    const double JupiterEquatorNodeDeg = 157.82;

    public static readonly CelestialBody Io = new(
        "Io", Color.FromArgb("#E0C96A"), 1_821.6,
        421_800.0 / AuKm, 0.0041, JupiterEquatorInclinationDeg, JupiterEquatorNodeDeg,
        0.0, 0.0, 1.769138);

    public static readonly CelestialBody Europa = new(
        "Europa", Color.FromArgb("#DCD3C4"), 1_560.8,
        671_100.0 / AuKm, 0.0094, JupiterEquatorInclinationDeg, JupiterEquatorNodeDeg,
        0.0, 0.0, 3.551181);

    public static readonly CelestialBody Ganymedes = new(
        "Ganymedes", Color.FromArgb("#9E8E7C"), 2_634.1,
        1_070_400.0 / AuKm, 0.0013, JupiterEquatorInclinationDeg, JupiterEquatorNodeDeg,
        0.0, 90.0, 7.154553);

    public static readonly CelestialBody Callisto = new(
        "Callisto", Color.FromArgb("#7C7065"), 2_410.3,
        1_882_700.0 / AuKm, 0.0074, JupiterEquatorInclinationDeg, JupiterEquatorNodeDeg,
        0.0, 180.0, 16.689018);

    // Charon, Plutos stora följeslagare. Med halva Plutos diameter och en
    // åttondel av dess massa är paret nästan en dubbelplanet: systemets
    // gemensamma tyngdpunkt ligger 2 126 km från Plutos centrum, alltså
    // utanför Pluto självt (som har radien 1 188 km). Därför vaggar Pluto
    // synligt kring tyngdpunkten i stället för att stå stilla.
    //
    // Banan ligger i Plutos ekvatorsplan. Eftersom Pluto roterar retrograd
    // lutar det planet mer än 90 grader mot ekliptikan – Charon går alltså
    // "baklänges" jämfört med de flesta månar. De två är dessutom helt
    // tidvattenlåsta: de vänder ständigt samma sida mot varandra.
    public static readonly CelestialBody Charon = new(
        "Charon", Color.FromArgb("#9A9188"), 606.0,
        19_591.4 / AuKm, 0.0002, 112.82, 47.32, 0.0, 0.0, 6.387230)
    {
        MassFraction = 0.1085,
    };

    // Saturnus tre mest kända månar. Banorna ligger i Saturnus ekvatorsplan,
    // samma plan som ringarna lutar i (28,0 grader mot ekliptikan).
    // Enceladus är solsystemets ljusaste kropp – en isvit måne med gejsrar som
    // sprutar vatten från ett hav under isen. Titan är större än Merkurius och
    // den enda månen med tät atmosfär, med sjöar av flytande metan.
    const double SaturnEquatorInclinationDeg = 28.05;
    const double SaturnEquatorNodeDeg = 349.53;

    public static readonly CelestialBody Enceladus = new(
        "Enceladus", Color.FromArgb("#F0F4F5"), 252.1,
        237_948.0 / AuKm, 0.0047, SaturnEquatorInclinationDeg, SaturnEquatorNodeDeg,
        0.0, 0.0, 1.370218);

    public static readonly CelestialBody Rhea = new(
        "Rhea", Color.FromArgb("#C8C6C0"), 763.8,
        527_108.0 / AuKm, 0.0010, SaturnEquatorInclinationDeg, SaturnEquatorNodeDeg,
        0.0, 120.0, 4.518212);

    public static readonly CelestialBody Titan = new(
        "Titan", Color.FromArgb("#D9A05B"), 2_574.7,
        1_221_870.0 / AuKm, 0.0288, SaturnEquatorInclinationDeg, SaturnEquatorNodeDeg,
        0.0, 240.0, 15.945421);

    // Uranus månar, uppkallade efter figurer hos Shakespeare och Pope. Eftersom
    // Uranus ligger på sidan står hela månsystemet nästan på högkant mot
    // ekliptikan. Banlutningen är satt till 97,7 grader, alltså över 90: Uranus
    // roterar retrograd, och månarna följer sin planets rotation. Samma plan med
    // lutningen 82,3 grader hade gett rätt plan men fel färdriktning.
    const double UranusEquatorInclinationDeg = 97.72;
    const double UranusEquatorNodeDeg = 347.67;

    public static readonly CelestialBody Miranda = new(
        "Miranda", Color.FromArgb("#A8A5A0"), 235.8,
        129_390.0 / AuKm, 0.0013, UranusEquatorInclinationDeg, UranusEquatorNodeDeg,
        0.0, 0.0, 1.413479);

    public static readonly CelestialBody Titania = new(
        "Titania", Color.FromArgb("#B5AAA0"), 788.4,
        435_910.0 / AuKm, 0.0011, UranusEquatorInclinationDeg, UranusEquatorNodeDeg,
        0.0, 140.0, 8.706234);

    public static readonly CelestialBody Oberon = new(
        "Oberon", Color.FromArgb("#9C9088"), 761.4,
        583_520.0 / AuKm, 0.0014, UranusEquatorInclinationDeg, UranusEquatorNodeDeg,
        0.0, 260.0, 13.463234);

    // Triton är solsystemets stora undantag: den kretsar RETROGRAD, alltså åt
    // motsatt håll mot Neptunus rotation och mot allt annat i den här appen.
    // En måne som bildats tillsammans med sin planet kan inte göra så – Triton
    // är därför med all sannolikhet en infångad dvärgplanet från Kuiperbältet.
    // Banlutningen över 90 grader är just det som gör rörelsen retrograd.
    // Not: banplanet precesserar med ca 640 års period, så orienteringen nedan
    // är läget vid epoken snarare än en bestående egenskap.
    // Neptunus ekvatorsplan, räknat ur planetens polriktning. Används av
    // ringarna; Triton har eget banplan eftersom den inte följer ekvatorn.
    const double NeptuneEquatorInclinationDeg = 28.03;
    const double NeptuneEquatorNodeDeg = 229.24;

    public static readonly CelestialBody Triton = new(
        "Triton", Color.FromArgb("#D8CFC8"), 1_353.4,
        354_759.0 / AuKm, 0.000016, 130.0, 60.93, 0.0, 0.0, 5.876854);

    // Ringsystemen. Alla fyra jätteplaneter har ringar – inte bara Saturnus.
    // Radierna är verkliga, uttryckta i planetradier:
    //   Jupiter   122 500 – 129 000 km  (den tunna huvudringen av damm)
    //   Saturnus   74 700 – 136 800 km  (C-ringen ut till A-ringens ytterkant)
    //   Uranus     38 000 –  51 150 km  (de smala, kolmörka ringarna)
    //   Neptunus   41 900 –  62 933 km  (ut till Adams-ringen)
    // Saturnus ringar är ljusa isPartiklar; de tre andra är så mörka att de
    // upptäcktes först på 1970- och 80-talen.
    static readonly PlanetRing JupiterRing = new(
        1.75f, 1.85f, JupiterEquatorInclinationDeg, JupiterEquatorNodeDeg,
        Color.FromRgba(0.58f, 0.45f, 0.36f, 0.30f), 25f);

    static readonly PlanetRing SaturnRing = new(
        1.283f, 2.349f, SaturnEquatorInclinationDeg, SaturnEquatorNodeDeg,
        Color.FromRgba(0.85f, 0.78f, 0.60f, 0.55f), 3f);

    static readonly PlanetRing UranusRing = new(
        1.50f, 2.02f, UranusEquatorInclinationDeg, UranusEquatorNodeDeg,
        Color.FromRgba(0.55f, 0.63f, 0.70f, 0.30f), 25f);

    static readonly PlanetRing NeptuneRing = new(
        1.70f, 2.56f, NeptuneEquatorInclinationDeg, NeptuneEquatorNodeDeg,
        Color.FromRgba(0.50f, 0.58f, 0.74f, 0.26f), 25f);

    // Banelement vid J2000 (NASA/JPL, medelvärden). Tillräckligt noggranna för att
    // planeternas positioner ungefär ska stämma med verkligheten för ett givet datum.
    public static readonly CelestialBody[] Planets =
    [
        new("Merkurius", Color.FromArgb("#B5A79B"),  2_439.7, 0.38710, 0.20563, 7.005,  48.331,  77.456, 252.251,    87.969),
        new("Venus",     Color.FromArgb("#E8CDA0"),  6_051.8, 0.72333, 0.00677, 3.395,  76.680, 131.564, 181.980,   224.701),
        new("Jorden",    Color.FromArgb("#4C8CE8"),  6_371.0, 1.00000, 0.01671, 0.000, -11.261, 102.947, 100.464,   365.256) { Moons = [Moon] },
        new("Mars",      Color.FromArgb("#D96C4A"),  3_389.5, 1.52371, 0.09339, 1.850,  49.559, 336.041, 355.445,   686.980) { Moons = [Phobos, Deimos] },
        new("Jupiter",   Color.FromArgb("#D8B48A"), 69_911.0, 5.20289, 0.04839, 1.304, 100.474,  14.728,  34.397, 4_332.59) { Moons = [Io, Europa, Ganymedes, Callisto], Ring = JupiterRing },
        new("Saturnus",  Color.FromArgb("#E8D5A8"), 58_232.0, 9.53668, 0.05386, 2.486, 113.662,  92.599,  49.954, 10_759.22) { Moons = [Enceladus, Rhea, Titan], Ring = SaturnRing },
        new("Uranus",    Color.FromArgb("#9BD4E4"), 25_362.0, 19.18916, 0.04726, 0.773, 74.017, 170.954, 313.238, 30_688.5) { Moons = [Miranda, Titania, Oberon], Ring = UranusRing },
        new("Neptunus",  Color.FromArgb("#5A78E8"), 24_622.0, 30.06992, 0.00859, 1.770, 131.784,  44.965, 304.880, 60_182.0) { Moons = [Triton], Ring = NeptuneRing },
        // Dvärgplaneten Pluto: kraftigt lutande (17°) och excentrisk bana som
        // tidvis går innanför Neptunus. Ett varv tar nästan 248 år.
        new("Pluto",     Color.FromArgb("#C4AB94"),  1_188.3, 39.48212, 0.24883, 17.140, 110.304, 224.069, 238.929, 90_560.0) { Moons = [Charon] },
    ];
}
