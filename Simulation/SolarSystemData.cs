using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En himlakropp med keplerska banelement (epok J2000).
/// Vinklar i grader: banlutning i, uppstigande nodens longitud Ω,
/// perihelielongitud ϖ samt medellongitud L0 vid epoken.
/// </summary>
/// <summary>
/// Ett ringsystem kring en planet. Radierna anges i planetradier och planet
/// tas direkt ur planetens rotationsaxel, så att ringarna hamnar i samma plan
/// som månarna utan att lutningen behöver skrivas en gång till.
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
    BodyAxis Axis,
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

    /// <summary>
    /// Kroppens rotationsaxel och rotationstid, eller null för kroppar där det
    /// inte spelar någon roll. Månar och ringar i ekvatorsplanet läser sitt
    /// banplan härifrån, så samma två tal beskriver både polen och deras banor.
    /// </summary>
    public BodyAxis? Axis { get; init; }

    /// <summary>
    /// Ytkarta, för de kroppar som ritas som glob när man zoomat in nog. Null
    /// betyder att kroppen ritas som en skiva med ljus och skugga – antingen för
    /// att den är för liten för att synas, eller för att det inte finns någon yta
    /// att visa (Venus moln, Titans dis).
    /// </summary>
    public SurfaceMap? Surface { get; init; }

    /// <summary>
    /// Hur fort banans uppstigande nod vandrar, i grader per dygn. Negativt
    /// betyder baklänges, vilket är det vanliga.
    ///
    /// Ett banplan ligger inte stilla. Månens nod går ett helt varv baklänges på
    /// 18,6 år, och det är den rörelsen som gör att förmörkelsesäsongerna glider
    /// nitton dygn bakåt varje år i stället för att infalla på samma datum. Med
    /// noden fastlåst blir den saken omöjlig att visa.
    ///
    /// Noll för allt som inte har någon känd eller märkbar precession.
    /// </summary>
    public double AscNodeRateDegPerDay { get; init; }

    /// <summary>
    /// Hur fort periheliet vandrar, i grader per dygn. Positivt betyder framåt.
    ///
    /// Hör ihop med noden och måste sättas tillsammans med den. Skälet är att
    /// medelanomalin räknas som medellongitud minus perihelielongitud: låter man
    /// periheliet stå stilla medan noden rör sig, får banan rätt plan men fel
    /// läge i planet. Månens perigeum går ett varv framåt på 8,85 år, och den
    /// rörelsen är dessutom det som skiljer det anomalistiska månvarvet (27,55
    /// dygn) från det sideriska (27,32).
    /// </summary>
    public double PerihelionRateDegPerDay { get; init; }

    /// <summary>
    /// Kroppens gravitationsparameter G·M i AU³/dygn² – måttet på hur hårt den
    /// drar i det som kretsar kring den. Ur den följer omloppstiderna: ett varv
    /// på halva storaxeln a tar 2π·√(a³/µ). Behövs bara för de kroppar som
    /// något faktiskt kretsar kring i appen, och är noll för de övriga.
    /// </summary>
    public double Mu { get; init; }

    /// <summary>Position i världskoordinater (Y = norr om ekliptikan) vid given tid.</summary>
    public Vector3 PositionAt(double daysSinceJ2000, float unitsPerAu)
    {
        var p = PositionAuAt(daysSinceJ2000);
        return new Vector3(
            (float)(p.X * unitsPerAu), (float)(p.Y * unitsPerAu), (float)(p.Z * unitsPerAu));
    }

    /// <summary>
    /// Samma läge i AU, men utan att gå ned till enkel precision.
    ///
    /// Ritningen behöver inte det, men banbyggandet gör det: en bana som ska
    /// räknas fram ur ett läge och en hastighet tappar annars siffror redan
    /// innan räkningen börjar. Se <see cref="Vec3"/> för varför det slår så hårt
    /// just där.
    /// </summary>
    public Vec3 PositionAuAt(double daysSinceJ2000)
    {
        double meanMotion = 360.0 / OrbitalPeriodDays;
        // Medelanomalin räknas mot det vandrande periheliet. Att dra bort dess
        // rörelse här är också det som gör att medellongituden ändå går sitt
        // sideriska varv – de två effekterna tar ut varandra, precis som de gör
        // i verkligheten.
        double mDeg = MeanLonJ2000Deg + meanMotion * daysSinceJ2000
                      - PerihelionAt(daysSinceJ2000);
        // Vik ned till ett varv innan omvandlingen till radianer. Snabba månar
        // (Phobos hinner tre varv per dygn) ger annars miljontals grader, vilket
        // tär på flyttalsprecisionen. Banpositionen är oförändrad.
        double M = DegToRad(mDeg % 360.0);
        double E = SolveKepler(M, Eccentricity);
        return ToWorldAu(E, daysSinceJ2000);
    }

    /// <summary>Uppstigande nodens longitud vid given tid.</summary>
    public double AscNodeAt(double daysSinceJ2000)
        => AscNodeDeg + AscNodeRateDegPerDay * daysSinceJ2000;

    /// <summary>Perihelielongituden vid given tid.</summary>
    public double PerihelionAt(double daysSinceJ2000)
        => PerihelionLonDeg + PerihelionRateDegPerDay * daysSinceJ2000;

    /// <summary>Hela banellipsen som punktlista (sluten kurva, jämnt samplad i excentrisk anomali).</summary>
    /// <param name="daysSinceJ2000">
    /// Vilken dag banan ska ritas för. Spelar roll bara för kroppar vars plan
    /// vrider sig; för de övriga ser banan likadan ut i alla tider.
    /// </param>
    public Vector3[] OrbitPath(int samples, float unitsPerAu, double daysSinceJ2000 = 0)
    {
        var pts = new Vector3[samples];
        for (int k = 0; k < samples; k++)
        {
            var p = ToWorldAu(2.0 * Math.PI * k / samples, daysSinceJ2000);
            pts[k] = new Vector3(
                (float)(p.X * unitsPerAu), (float)(p.Y * unitsPerAu), (float)(p.Z * unitsPerAu));
        }
        return pts;
    }

    Vec3 ToWorldAu(double E, double daysSinceJ2000)
    {
        double e = Eccentricity;
        // Position i banplanet (fokus = solen).
        double xv = SemiMajorAu * (Math.Cos(E) - e);
        double yv = SemiMajorAu * Math.Sqrt(1.0 - e * e) * Math.Sin(E);

        double node = AscNodeAt(daysSinceJ2000);
        double w = DegToRad(PerihelionAt(daysSinceJ2000) - node); // periheliets argument
        double O = DegToRad(node);
        double i = DegToRad(InclinationDeg);
        double cw = Math.Cos(w), sw = Math.Sin(w);
        double cO = Math.Cos(O), sO = Math.Sin(O);
        double ci = Math.Cos(i), si = Math.Sin(i);

        // Rotation banplan -> ekliptiska koordinater.
        double x = (cw * cO - sw * sO * ci) * xv + (-sw * cO - cw * sO * ci) * yv;
        double y = (cw * sO + sw * cO * ci) * xv + (-sw * sO + cw * cO * ci) * yv;
        double z = (sw * si) * xv + (cw * si) * yv;

        // Ekliptikans plan läggs horisontellt; norr (+z) pekar uppåt (+Y).
        return new Vec3(x, z, -y);
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

    /// <summary>
    /// Solens gravitationsparameter G·M i AU³/dygn². Att jordens bana med
    /// a = 1 AU tar 365,26 dygn är just detta tal skrivet baklänges.
    /// </summary>
    public const double SunMu = 2.959122082855911e-4;

    /// <summary>
    /// Jordens och månens gemensamma gravitationsparameter, 403 503 km³/s²
    /// omräknat till AU och dygn. Solens är 330 000 gånger större – därför tar
    /// månens varv kring jorden 27 dygn medan jordens varv kring solen tar ett år,
    /// trots att avstånden skiljer nästan 400 gånger.
    /// </summary>
    public const double EarthMu = 8.9971e-10;

    /// <summary>
    /// Jupiters gravitationsparameter (126 687 000 km³/s²), för sonder som
    /// kretsar kring planeten. Att den stämmer syns på månarna: talet ger Io ett
    /// varv på 1,770 dygn, mot uppmätta 1,769.
    /// </summary>
    public const double JupiterMu = 2.8248e-7;

    /// <summary>
    /// Saturnus gravitationsparameter (37 931 000 km³/s²). Ger Titan ett varv på
    /// 15,95 dygn, vilket är precis dess uppmätta omloppstid.
    /// </summary>
    public const double SaturnMu = 8.4573e-8;

    public const double SunRadiusKm = 696_340.0;
    public static readonly DateTime EpochJ2000 = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    // ---------------------------------------------------------- rotationsaxlar
    //
    // Axlarna är räknade ur IAU:s polriktningar (rektascension och deklination
    // för nordpolen, plus nollmeridianens läge W0) och omräknade till ekliptiska
    // koordinater. Nordpolen följer högerhandsregeln, så lutningar över 90 grader
    // betyder retrograd rotation – se BodyAxis för varför konventionen ser ut så.
    //
    // Att talen stämmer går att pröva mot annat än sig självt: Merkurius axel
    // hamnar 7,0 grader från ekliptikan med noden 48,2, vilket är dess egen
    // banlutning och bannod på pricken – planeten står alltså rakt upp i sin
    // egen bana, precis som uppmätt. Jordens data ger subsolär punkt 0,8 grader
    // öster om Greenwich vid J2000,0 (middag i Greenwich den 1 januari 2000,
    // tidsekvationen inräknad) och deklination -23,0 grader, alltså mitt i vintern.

    /// <summary>
    /// Merkurius: 58,6 dygn, exakt två tredjedelar av dess år – en 3:2-resonans
    /// med solen. Lutningen mot den egna banan är bara 0,03 grader, den minsta i
    /// solsystemet; att talet nedan ändå är 7,0 beror på att banan själv lutar 7,0.
    /// </summary>
    public static readonly BodyAxis MercuryAxis = new(7.037, 48.24, 58.6461459, 291.274);

    /// <summary>
    /// Venus roterar baklänges: polen pekar nästan rakt söderut om ekliptikan
    /// (178,8 grader). Ett varv tar 243 dygn, längre än dess år på 225 – dygnet
    /// är alltså längre än året. Varför den vänts upp och ner vet ingen säkert.
    /// </summary>
    public static readonly BodyAxis VenusAxis = new(178.761, 300.19, 243.0184840, 137.449);

    /// <summary>
    /// Jorden: 23,44 graders lutning – hela förklaringen till årstiderna – och
    /// ett varv per stjärndygn, 23 h 56 min 4,1 s. Det är fyra minuter kortare än
    /// ett soldygn, eftersom jorden hinner flytta sig ett stycke i sin bana under
    /// tiden. Nollmeridianen är hämtad ur samma uttryck för stjärntiden som
    /// stjärnhimlen använder, så himlen och jordklotet vrider sig i takt.
    /// </summary>
    public static readonly BodyAxis EarthAxis = new(23.4392911, 180.0, 0.9972695663290739, 100.46061837);

    /// <summary>
    /// Månen är bunden: ett varv kring axeln på exakt en omloppsbana, därför samma
    /// sida vänd mot oss. Axeln lutar bara 1,5 grader mot ekliptikan men 6,7 grader
    /// mot månens egen bana, och den pekar alltid åt motsatt håll mot banpolen
    /// (Cassinis andra lag). Att banan är elliptisk gör att månen ändå vaggar
    /// sex grader fram och tillbaka i longitud – den libration som visar oss lite
    /// mer än halva månen.
    /// </summary>
    public static readonly BodyAxis MoonAxis = new(1.5424, 305.045, 27.32166, 93.293);

    /// <summary>Mars dygn är nästan jordens: 24 h 37 min. Lutningen 25,4 grader ger den årstider.</summary>
    public static readonly BodyAxis MarsAxis = new(25.404, 84.84, 1.0259568, 133.120);

    /// <summary>
    /// Jupiter snurrar fortast av alla: 9 h 55 min, trots att den är störst.
    /// Axeln står nästan rakt upp (2,2 grader), så Jupiter saknar årstider.
    /// </summary>
    public static readonly BodyAxis JupiterAxis = new(2.217, 337.82, 0.4135383, 305.363);

    /// <summary>Saturnus: 10 h 39 min och 28,1 graders lutning – det är den lutningen ringarna visar upp.</summary>
    public static readonly BodyAxis SaturnAxis = new(28.052, 169.53, 0.4440093, 358.934);

    /// <summary>
    /// Uranus ligger på sidan och rullar: 97,7 grader betyder att axeln nästan
    /// ligger i banplanet och att rotationen är retrograd. Under dess 84 år långa
    /// varv pekar än den ena, än den andra polen mot solen, med fyrtio års dag och
    /// fyrtio års natt.
    /// </summary>
    public static readonly BodyAxis UranusAxis = new(97.722, 167.65, 0.7183333, 331.131);

    /// <summary>Neptunus: 16 h 06 min och 28,0 graders lutning, nästan samma som Saturnus.</summary>
    public static readonly BodyAxis NeptuneAxis = new(28.026, 49.24, 0.6712500, 228.657);

    /// <summary>
    /// Pluto roterar baklänges (lutning 112,8 grader) och är bunden till Charon:
    /// 6,387 dygn är både Plutos dygn och Charons omloppstid. De två vänder alltså
    /// ständigt samma sida mot varandra – det enda paret i solsystemet som gör så.
    /// </summary>
    public static readonly BodyAxis PlutoAxis = new(112.816, 227.35, 6.3872230, 319.809);

    /// <summary>
    /// Månen med geocentriska medelbanelement (J2000): banan beräknas kring
    /// jorden i stället för kring solen, med samma Kepler-matematik.
    /// Ett varv tar 27,3 dygn (siderisk månad).
    /// </summary>
    public static readonly CelestialBody Moon = new(
        "Månen", Color.FromArgb("#BEBEB6"), 1_737.4,
        0.0025696 /* = 384 399 km */, 0.0549, 5.145, 125.045, 83.353, 218.316, 27.32166)
    {
        Axis = MoonAxis,
        Surface = SurfaceMap.Moon,
        // Noden ett varv baklänges på 18,6 år, perigeum ett varv framåt på 8,85.
        // De två talen är de äldsta i hela appen: babylonierna hade nodcykeln
        // redan på 500-talet f.Kr. och kunde förutsäga förmörkelser med den.
        AscNodeRateDegPerDay = -0.0529539,
        PerihelionRateDegPerDay = 0.1114041,
    };

    // Mars två små, oregelbundna månar – troligen infångade asteroider. De
    // kretsar mycket nära Mars: Phobos på bara 2,8 marsradier (jämför Månens
    // 60 jordradier). Banorna ligger i Mars ekvatorsplan, som lutar 25,4° mot
    // ekliptikan – det är därför banlutningen inte är nära noll. Planet läses ur
    // MarsAxis, så det står bara på ett ställe.
    // Not: faslägena (medellongituderna) är approximativa. Med omloppstider på
    // timmar hinner även ett mycket litet periodfel bli hela varv över de
    // årtionden appen kan simulera, så månarnas exakta placering i banan vid
    // ett givet datum går ändå inte att lita på. Avstånd, storlekar, banplan
    // och omloppstider är däremot verkliga.
    public static readonly CelestialBody Phobos = new(
        "Phobos", Color.FromArgb("#A89684"), 11.267,
        9_376.0 / AuKm, 0.0151, MarsAxis.InclinationDeg, MarsAxis.NodeDeg,
        0.0, 0.0, 0.3189100);

    public static readonly CelestialBody Deimos = new(
        "Deimos", Color.FromArgb("#B8A794"), 6.2,
        23_463.2 / AuKm, 0.00033, MarsAxis.InclinationDeg, MarsAxis.NodeDeg,
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
    // De fyra galileiska månarna och Titan är bundna: ett varv kring axeln per
    // varv i banan, så samma sida är alltid vänd mot planeten. Axeln står
    // vinkelrätt mot banplanet, alltså samma lutning och nod som banan, och
    // rotationstiden är banans egen. Nollmeridianerna är uträknade så att
    // longitud noll pekar mot planeten vid epoken, med månens MEDELläge som
    // ankare – tas det verkliga läget hamnar nollan i ett librationsytterläge
    // och vaggningen blir osymmetrisk.

    public static readonly CelestialBody Io = new(
        "Io", Color.FromArgb("#E0C96A"), 1_821.6,
        421_800.0 / AuKm, 0.0041, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 0.0, 1.769138)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 1.769138, 202.180),
        Surface = SurfaceMap.Io,
    };

    public static readonly CelestialBody Europa = new(
        "Europa", Color.FromArgb("#DCD3C4"), 1_560.8,
        671_100.0 / AuKm, 0.0094, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 0.0, 3.551181)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 3.551181, 202.180),
        Surface = SurfaceMap.Europa,
    };

    public static readonly CelestialBody Ganymedes = new(
        "Ganymedes", Color.FromArgb("#9E8E7C"), 2_634.1,
        1_070_400.0 / AuKm, 0.0013, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 90.0, 7.154553)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 7.154553, 292.180),
        Surface = SurfaceMap.Ganymede,
    };

    public static readonly CelestialBody Callisto = new(
        "Callisto", Color.FromArgb("#7C7065"), 2_410.3,
        1_882_700.0 / AuKm, 0.0074, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 180.0, 16.689018)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 16.689018, 22.180),
        Surface = SurfaceMap.Callisto,
    };

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
    //
    // Medellongituden är inte hämtad ur en efemerid utan satt så att Charon
    // hamnar över Plutos nollmeridian, vilket är just vad tidvattenlåsningen
    // innebär – IAU definierar Plutos nollmeridian som den som pekar mot Charon.
    // Följden är att Sputnik Planitia, som ligger kring 175 grader öst, vänder
    // sig bort från Charon. Så ser det verkligen ut, och det är förmodligen
    // ingen slump: slätten är tung nog att ha vridit hela Pluto på plats.
    public static readonly CelestialBody Charon = new(
        "Charon", Color.FromArgb("#9A9188"), 606.0,
        19_591.4 / AuKm, 0.0002, PlutoAxis.InclinationDeg, PlutoAxis.NodeDeg,
        0.0, 188.4, 6.387230)
    {
        MassFraction = 0.1085,
    };

    // Saturnus tre mest kända månar. Banorna ligger i Saturnus ekvatorsplan,
    // samma plan som ringarna lutar i (28,0 grader mot ekliptikan).
    // Enceladus är solsystemets ljusaste kropp – en isvit måne med gejsrar som
    // sprutar vatten från ett hav under isen. Titan är större än Merkurius och
    // den enda månen med tät atmosfär, med sjöar av flytande metan.
    public static readonly CelestialBody Enceladus = new(
        "Enceladus", Color.FromArgb("#F0F4F5"), 252.1,
        237_948.0 / AuKm, 0.0047, SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg,
        0.0, 0.0, 1.370218);

    public static readonly CelestialBody Rhea = new(
        "Rhea", Color.FromArgb("#C8C6C0"), 763.8,
        527_108.0 / AuKm, 0.0010, SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg,
        0.0, 120.0, 4.518212);

    public static readonly CelestialBody Titan = new(
        "Titan", Color.FromArgb("#D9A05B"), 2_574.7,
        1_221_870.0 / AuKm, 0.0288, SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg,
        0.0, 240.0, 15.945421)
    {
        // Titan får ingen ytkarta, och det är svaret på uppgiften snarare än en
        // lucka: dimman är ogenomskinlig och ingen yta syns. Månen ritas som en
        // jämnt orange skiva med ljus och skugga, precis som Venus. Axeln finns
        // ändå med, eftersom bundenheten är sann oavsett om den syns.
        Axis = new BodyAxis(SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg, 15.945421, 250.470),
    };

    // Uranus månar, uppkallade efter figurer hos Shakespeare och Pope. Eftersom
    // Uranus ligger på sidan står hela månsystemet nästan på högkant mot
    // ekliptikan: lutningen de läser ur UranusAxis är 97,7 grader, alltså över 90.
    // Uranus roterar retrograd och månarna följer sin planets rotation. Samma plan
    // med lutningen 82,3 grader hade gett rätt plan men fel färdriktning.

    public static readonly CelestialBody Miranda = new(
        "Miranda", Color.FromArgb("#A8A5A0"), 235.8,
        129_390.0 / AuKm, 0.0013, UranusAxis.InclinationDeg, UranusAxis.NodeDeg,
        0.0, 0.0, 1.413479);

    public static readonly CelestialBody Titania = new(
        "Titania", Color.FromArgb("#B5AAA0"), 788.4,
        435_910.0 / AuKm, 0.0011, UranusAxis.InclinationDeg, UranusAxis.NodeDeg,
        0.0, 140.0, 8.706234);

    public static readonly CelestialBody Oberon = new(
        "Oberon", Color.FromArgb("#9C9088"), 761.4,
        583_520.0 / AuKm, 0.0014, UranusAxis.InclinationDeg, UranusAxis.NodeDeg,
        0.0, 260.0, 13.463234);

    // Triton är solsystemets stora undantag: den kretsar RETROGRAD, alltså åt
    // motsatt håll mot Neptunus rotation och mot allt annat i den här appen.
    // En måne som bildats tillsammans med sin planet kan inte göra så – Triton
    // är därför med all sannolikhet en infångad dvärgplanet från Kuiperbältet.
    // Banlutningen över 90 grader är just det som gör rörelsen retrograd.
    // Not: banplanet precesserar med ca 640 års period, så orienteringen nedan
    // är läget vid epoken snarare än en bestående egenskap.
    // Triton har eget banplan eftersom den inte följer ekvatorn; ringarna läser
    // sitt ur NeptuneAxis.
    public static readonly CelestialBody Triton = new(
        "Triton", Color.FromArgb("#D8CFC8"), 1_353.4,
        354_759.0 / AuKm, 0.000016, 130.0, 240.93, 0.0, 0.0, 5.876854)
    {
        // Ett varv på ungefär 640 år, alltså 0,0015 grader per dygn. Riktningen
        // följer av att banan är retrograd: Neptunus tillplattning vrider noden
        // med en hastighet som går som cosinus för banlutningen, och lutningen
        // är över 90 grader. Där de flesta månar får sin nod dragen baklänges
        // vandrar Tritons alltså framåt.
        AscNodeRateDegPerDay = 360.0 / (640.0 * 365.25),
    };

    // Ringsystemen. Alla fyra jätteplaneter har ringar – inte bara Saturnus.
    // Radierna är verkliga, uttryckta i planetradier:
    //   Jupiter   122 500 – 129 000 km  (den tunna huvudringen av damm)
    //   Saturnus   74 700 – 136 800 km  (C-ringen ut till A-ringens ytterkant)
    //   Uranus     38 000 –  51 150 km  (de smala, kolmörka ringarna)
    //   Neptunus   41 900 –  62 933 km  (ut till Adams-ringen)
    // Saturnus ringar är ljusa isPartiklar; de tre andra är så mörka att de
    // upptäcktes först på 1970- och 80-talen.
    static readonly PlanetRing JupiterRing = new(
        1.75f, 1.85f, JupiterAxis, Color.FromRgba(0.58f, 0.45f, 0.36f, 0.30f), 25f);

    static readonly PlanetRing SaturnRing = new(
        1.283f, 2.349f, SaturnAxis, Color.FromRgba(0.85f, 0.78f, 0.60f, 0.55f), 3f);

    static readonly PlanetRing UranusRing = new(
        1.50f, 2.02f, UranusAxis, Color.FromRgba(0.55f, 0.63f, 0.70f, 0.30f), 25f);

    static readonly PlanetRing NeptuneRing = new(
        1.70f, 2.56f, NeptuneAxis, Color.FromRgba(0.50f, 0.58f, 0.74f, 0.26f), 25f);

    /// <summary>
    /// Dvärgplaneten Ceres, den i särklass största kroppen i asteroidbältet –
    /// den rymmer ungefär en fjärdedel av hela bältets massa. Ligger utanför
    /// planetlistan och ritas tillsammans med bältet.
    /// </summary>
    public static readonly CelestialBody Ceres = new(
        "Ceres", Color.FromArgb("#A79C90"), 469.7,
        2.7675, 0.0758, 10.593, 80.393, 153.990, 249.979, 1_681.63);

    /// <summary>
    /// Halleys komet: den enda ljusstarka kometen med så kort omloppstid att en
    /// människa kan hinna se den två gånger.
    ///
    /// Banan är allt som planeternas inte är. Excentriciteten 0,967 drar ut den
    /// så till den grad att kometen svänger in innanför Venus bana i perihelium
    /// (0,586 AU) och ut förbi Neptunus i aphelium (35,1 AU) – sextio gånger
    /// längre bort som mest än som minst. Av det följer farten, genom Keplers
    /// andra lag: 54 km/s vid perihelium och under 1 km/s vid aphelium. Kometen
    /// tillbringar alltså nästan hela sitt varv långt ute i kylan och rusar genom
    /// det inre solsystemet på några månader.
    ///
    /// Banlutningen 162 grader betyder att den går RETROGRAD, mot planeternas
    /// färdriktning. Det är därför mötet med jorden 1986 blev så kort: de två
    /// kropparna kom mot varandra i stället för att köra i kapp.
    ///
    /// Elementen är förankrade i två kända perihelier, 9 februari 1986 och
    /// 28 juli 2061. Det ger omloppstiden 27 563 dygn (75,5 år), och ur den
    /// följer halva storaxeln.
    ///
    /// Radien avser kärnan, som är en potatisformad klump på ungefär 15 x 8 x 8
    /// km och svartare än kol – ljuset kommer inte från den utan från gasen och
    /// dammet omkring, vilket också är vad färgen nedan beskriver.
    ///
    /// **Förbehåll:** en fast keplerbana kan inte träffa alla perihelier. Halleys
    /// verkliga omloppstid varierar mellan 74 och 79 år, eftersom Jupiter och
    /// Saturnus drar i kometen vid varje varv och gasstrålarna från den upphettade
    /// kärnan knuffar den som en svag raket. Modellen lägger därför 1910 års
    /// perihelium i slutet av augusti i stället för den 20 april – fyra månader
    /// fel redan ett varv bakåt. Kring 1986 och 2061 stämmer den, och det är
    /// där den används.
    /// </summary>
    public static readonly CelestialBody Halley = new(
        "Halleys komet", Color.FromArgb("#CFEDE8"), 5.5,
        17.85745, 0.9671858, 162.262, 58.420, 169.753, 236.026, 27_562.54);

    // Banelement vid J2000 (NASA/JPL, medelvärden). Tillräckligt noggranna för att
    // planeternas positioner ungefär ska stämma med verkligheten för ett givet datum.
    public static readonly CelestialBody[] Planets =
    [
        new("Merkurius", Color.FromArgb("#B5A79B"),  2_439.7, 0.38710, 0.20563, 7.005,  48.331,  77.456, 252.251,    87.969) { Axis = MercuryAxis, Surface = SurfaceMap.Mercury },
        new("Venus",     Color.FromArgb("#E8CDA0"),  6_051.8, 0.72333, 0.00677, 3.395,  76.680, 131.564, 181.980,   224.701) { Axis = VenusAxis, Surface = SurfaceMap.Venus },
        new("Jorden",    Color.FromArgb("#4C8CE8"),  6_371.0, 1.00000, 0.01671, 0.000, -11.261, 102.947, 100.464,   365.256) { Moons = [Moon], Mu = EarthMu, Axis = EarthAxis, Surface = SurfaceMap.Earth },
        new("Mars",      Color.FromArgb("#D96C4A"),  3_389.5, 1.52371, 0.09339, 1.850,  49.559, 336.041, 355.445,   686.980) { Axis = MarsAxis, Moons = [Phobos, Deimos], Surface = SurfaceMap.Mars },
        new("Jupiter",   Color.FromArgb("#D8B48A"), 69_911.0, 5.20289, 0.04839, 1.304, 100.474,  14.728,  34.397, 4_332.59) { Axis = JupiterAxis, Moons = [Io, Europa, Ganymedes, Callisto], Ring = JupiterRing, Mu = JupiterMu, Surface = SurfaceMap.Jupiter },
        new("Saturnus",  Color.FromArgb("#E8D5A8"), 58_232.0, 9.53668, 0.05386, 2.486, 113.662,  92.599,  49.954, 10_759.22) { Axis = SaturnAxis, Moons = [Enceladus, Rhea, Titan], Ring = SaturnRing, Mu = SaturnMu, Surface = SurfaceMap.Saturn },
        new("Uranus",    Color.FromArgb("#9BD4E4"), 25_362.0, 19.18916, 0.04726, 0.773, 74.017, 170.954, 313.238, 30_688.5) { Axis = UranusAxis, Moons = [Miranda, Titania, Oberon], Ring = UranusRing, Surface = SurfaceMap.Uranus },
        new("Neptunus",  Color.FromArgb("#5A78E8"), 24_622.0, 30.06992, 0.00859, 1.770, 131.784,  44.965, 304.880, 60_182.0) { Axis = NeptuneAxis, Moons = [Triton], Ring = NeptuneRing, Surface = SurfaceMap.Neptune },
        // Dvärgplaneten Pluto: kraftigt lutande (17°) och excentrisk bana som
        // tidvis går innanför Neptunus. Ett varv tar nästan 248 år.
        new("Pluto",     Color.FromArgb("#C4AB94"),  1_188.3, 39.48212, 0.24883, 17.140, 110.304, 224.069, 238.929, 90_560.0) { Axis = PlutoAxis, Moons = [Charon], Surface = SurfaceMap.Pluto },
    ];
}
