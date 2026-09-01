using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// Ett kägelsnitt kring en central kropp, byggt ur ett tillstånd: ett läge och
/// en hastighet vid en viss tidpunkt.
///
/// Planeterna beskrivs av fasta banelement som aldrig ändras, men en rymdsond
/// byter bana vid varje planetpassage. Då är tillståndet det naturliga sättet
/// att beskriva färden: hastigheten precis efter passagen bestämmer hela den
/// följande banan.
///
/// Klassen klarar både ellipser och hyperbler. Skillnaden ligger i farten. Är
/// den under den lokala flykthastigheten blir banan en sluten ellips; är den
/// över blir den en öppen hyperbel och kroppen kommer aldrig tillbaka. I
/// formlerna märks det på att halva storaxeln blir negativ – vilket är precis
/// vad de behöver för att fortsätta gälla.
///
/// Allt räknas i AU och dygn, med gravitationsparametern µ i AU³/dygn².
/// </summary>
public sealed class Conic
{
    /// <summary>Halva storaxeln i AU. Negativ för hyperbler.</summary>
    public double SemiMajorAu { get; }

    /// <summary>Excentricitet. Över 1 betyder att banan är öppen.</summary>
    public double Eccentricity { get; }

    /// <summary>Sant när banan är öppen och kroppen aldrig kommer tillbaka.</summary>
    public bool IsHyperbolic => Eccentricity > 1.0;

    /// <summary>Närmaste punkt till centralkroppen, i AU.</summary>
    public double PeriapsisAu => SemiMajorAu * (1.0 - Eccentricity);

    /// <summary>Centralkroppens gravitationsparameter i AU³/dygn².</summary>
    public double Mu { get; }

    /// <summary>
    /// Omloppstiden i dygn, eller null för hyperbler – de har ingen.
    /// </summary>
    public double? PeriodDays => IsHyperbolic
        ? null
        : 2.0 * Math.PI * Math.Sqrt(Math.Pow(SemiMajorAu, 3) / Mu);

    // Banplanets bas: riktningen mot periapsis och den vinkelräta åt det håll
    // kroppen rör sig när den passerar där.
    readonly Vector3 _periDir;
    readonly Vector3 _sideDir;

    readonly double _epochDay;
    readonly double _meanAnomalyAtEpoch;
    readonly double _meanMotion;         // radianer per dygn

    Conic(double semiMajorAu, double eccentricity, double mu, Vector3 periDir, Vector3 sideDir,
        double epochDay, double meanAnomalyAtEpoch, double meanMotion)
    {
        SemiMajorAu = semiMajorAu;
        Eccentricity = eccentricity;
        Mu = mu;
        _periDir = periDir;
        _sideDir = sideDir;
        _epochDay = epochDay;
        _meanAnomalyAtEpoch = meanAnomalyAtEpoch;
        _meanMotion = meanMotion;
    }

    /// <summary>
    /// Bygger banan ur ett tillstånd: läget i AU och hastigheten i AU/dygn vid
    /// den givna tidpunkten. Returnerar null för tillstånd som inte beskriver
    /// någon bana alls, till exempel ett läge rakt i centrum.
    ///
    /// Gången är den klassiska: energin ger halva storaxeln, rörelsemängds-
    /// momentet r×v ger banplanet, och excentricitetsvektorn ger både banans
    /// form och åt vilket håll periapsis ligger.
    /// </summary>
    public static Conic? FromState(Vec3 positionAu, Vec3 velocityAuPerDay,
        double epochDay, double mu)
    {
        double r = positionAu.Length;
        double v2 = velocityAuPerDay.LengthSquared;
        if (r < 1e-12 || mu <= 0)
            return null;

        // Vis-viva baklänges: energin bestämmer halva storaxeln. Uttrycket blir
        // negativt när farten överstiger flykthastigheten, och då är banan en
        // hyperbel.
        double alpha = 2.0 / r - v2 / mu;      // = 1/a
        if (Math.Abs(alpha) < 1e-12)
            return null;                        // parabel – oändligt lång bana
        double a = 1.0 / alpha;

        var h = Vec3.Cross(positionAu, velocityAuPerDay);
        if (h.Length < 1e-14)
            return null;                        // rakt in mot centrum, ingen bana

        // Excentricitetsvektorn pekar mot periapsis och har banans excentricitet
        // som längd.
        var eVec = Vec3.Cross(velocityAuPerDay, h) / mu - positionAu / r;
        double e = eVec.Length;
        if (double.IsNaN(e) || e >= 1.0 && a > 0)
            return null;                        // motsägelsefullt tillstånd

        var periVec = e > 1e-8
            ? eVec.Normalized()
            : positionAu.Normalized();          // cirkelbana: välj läget som utgångspunkt
        var sideVec = Vec3.Cross(h.Normalized(), periVec).Normalized();

        // Sanna anomalin vid epoken: vinkeln från periapsis, mätt i banplanet.
        // Räknas i dubbel precision; först därefter går riktningarna ned till
        // enkel, där de bara används för att peka ut banplanet vid ritning.
        double trueAnomaly = Math.Atan2(
            Vec3.Dot(positionAu, sideVec),
            Vec3.Dot(positionAu, periVec));
        var periDir = periVec.ToVector3();
        var sideDir = sideVec.ToVector3();

        double meanAnomaly, meanMotion;
        if (e < 1.0)
        {
            double ecc = 2.0 * Math.Atan2(
                Math.Sqrt(1.0 - e) * Math.Sin(trueAnomaly * 0.5),
                Math.Sqrt(1.0 + e) * Math.Cos(trueAnomaly * 0.5));
            meanAnomaly = ecc - e * Math.Sin(ecc);
            meanMotion = Math.Sqrt(mu / (a * a * a));
        }
        else
        {
            // Samma omräkning som för ellipsen, men med hyperbolfunktioner.
            // Argumentet till artanh håller sig under 1 så länge kroppen är
            // innanför asymptoterna, alltså alltid.
            double t = Math.Sqrt((e - 1.0) / (e + 1.0)) * Math.Tan(trueAnomaly * 0.5);
            if (Math.Abs(t) >= 1.0)
                return null;
            double hyp = 2.0 * Math.Atanh(t);
            meanAnomaly = e * Math.Sinh(hyp) - hyp;
            meanMotion = Math.Sqrt(mu / (-a * -a * -a));
        }

        return new Conic(a, e, mu, periDir, sideDir, epochDay, meanAnomaly, meanMotion);
    }

    /// <summary>
    /// Bygger banan ur dess periapsis: riktningen dit, riktningen kroppen rör
    /// sig när den passerar där, avståndet och excentriciteten. Tidpunkten som
    /// anges är själva periapsispassagen.
    ///
    /// Den här vägen in är exakt. FromState måste räkna fram energin som en
    /// liten skillnad mellan två stora tal, och tappar då precision; här är
    /// banan redan given i de storheter formlerna behöver.
    /// </summary>
    public static Conic? FromPeriapsis(Vector3 periapsisDir, Vector3 motionDir,
        double periapsisAu, double eccentricity, double periapsisDay, double mu)
    {
        if (periapsisAu <= 0 || mu <= 0 || Math.Abs(eccentricity - 1.0) < 1e-9)
            return null;                        // parabeln har ingen storaxel

        // Negativ för hyperbler, vilket är precis vad formlerna vill ha.
        double a = periapsisAu / (1.0 - eccentricity);
        double meanMotion = Math.Sqrt(mu / Math.Abs(a * a * a));

        return new Conic(a, eccentricity, mu,
            Vector3.Normalize(periapsisDir), Vector3.Normalize(motionDir),
            periapsisDay, 0.0, meanMotion);
    }

    /// <summary>
    /// Bygger banan ur klassiska banelement i stället, på samma sätt som
    /// planeterna beskrivs. Används för sonder som kretsar kring en planet, där
    /// banans form är känd men inte något enskilt tillstånd.
    /// </summary>
    public static Conic FromElements(double semiMajorAu, double eccentricity,
        double inclinationDeg, double ascNodeDeg, double argPeriapsisDeg,
        double periapsisDay, double mu)
    {
        double w = argPeriapsisDeg * Math.PI / 180.0;
        double o = ascNodeDeg * Math.PI / 180.0;
        double i = inclinationDeg * Math.PI / 180.0;
        double cw = Math.Cos(w), sw = Math.Sin(w);
        double co = Math.Cos(o), so = Math.Sin(o);
        double ci = Math.Cos(i), si = Math.Sin(i);

        // Samma rotation banplan -> ekliptika som CelestialBody använder, och
        // samma byte till appens koordinater (Y = norr om ekliptikan).
        static Vector3 ToWorld(double x, double y, double z)
            => new((float)x, (float)z, (float)-y);

        var periDir = ToWorld(
            cw * co - sw * so * ci,
            cw * so + sw * co * ci,
            sw * si);
        var sideDir = ToWorld(
            -sw * co - cw * so * ci,
            -sw * so + cw * co * ci,
            cw * si);

        return FromPeriapsis(periDir, sideDir, semiMajorAu * (1.0 - eccentricity),
            eccentricity, periapsisDay, mu)!;
    }

    /// <summary>Läget vid en given tid, i förhållande till centralkroppen.</summary>
    public Vector3 PositionAt(double day, float unitsPerAu)
    {
        double m = _meanAnomalyAtEpoch + _meanMotion * (day - _epochDay);
        double e = Eccentricity;

        double x, y;
        if (e < 1.0)
        {
            double ecc = Kepler.Elliptic(m, e);
            x = SemiMajorAu * (Math.Cos(ecc) - e);
            y = SemiMajorAu * Math.Sqrt(1.0 - e * e) * Math.Sin(ecc);
        }
        else
        {
            // Halva storaxeln är negativ här, vilket gör att x blir positivt vid
            // periapsis och y växer åt rörelsehållet – precis som i ellipsfallet.
            double hyp = Kepler.Hyperbolic(m, e);
            x = SemiMajorAu * (Math.Cosh(hyp) - e);
            y = -SemiMajorAu * Math.Sqrt(e * e - 1.0) * Math.Sinh(hyp);
        }

        return (_periDir * (float)x + _sideDir * (float)y) * unitsPerAu;
    }

    /// <summary>Avståndet till centralkroppen i AU vid en given tid.</summary>
    public double DistanceAu(double day) => PositionAt(day, 1f).Length();

    /// <summary>
    /// Farten i km/s vid en given tid, ur vis-viva-ekvationen. Uttrycket gäller
    /// oförändrat för hyperbler: där är halva storaxeln negativ, så -1/a blir ett
    /// positivt tillskott. Det är just det tillskottet som är flyktenergin – den
    /// fart sonden har kvar när den kommit oändligt långt bort.
    /// </summary>
    public double SpeedKmPerSecond(double day)
    {
        double r = DistanceAu(day);
        if (r < 1e-12)
            return 0;

        double v = Math.Sqrt(Math.Max(0.0, Mu * (2.0 / r - 1.0 / SemiMajorAu)));
        return v * SolarSystemData.AuKm / 86_400.0;
    }
}
