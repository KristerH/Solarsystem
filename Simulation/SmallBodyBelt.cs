using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// A belt of small bodies around the Sun – either the asteroid belt between
/// Mars and Jupiter or the Kuiper belt beyond Neptune. The bodies aren't real
/// individuals but randomised orbits with the same statistical distribution
/// as the belt they represent.
/// </summary>
public sealed class SmallBodyBelt
{
    /// <summary>
    /// A small body whose rotation from orbital plane to world coordinates is
    /// already baked into two vectors. That way a position costs only one
    /// Kepler solve and two vector multiplications – no orbital-plane
    /// trigonometry per frame.
    /// </summary>
    public readonly struct Body(
        Vector3 planeX, Vector3 planeY, float eccentricity,
        float meanMotionRad, float meanAnomalyRad, float alpha)
    {
        /// <summary>Semi-major axis times the orbital plane's first basis vector.</summary>
        public readonly Vector3 PlaneX = planeX;

        /// <summary>Semi-minor axis times the orbital plane's second basis vector.</summary>
        public readonly Vector3 PlaneY = planeY;

        public readonly float Eccentricity = eccentricity;
        public readonly float MeanMotionRad = meanMotionRad;
        public readonly float MeanAnomalyRad = meanAnomalyRad;
        public readonly float Alpha = alpha;
    }

    public Body[] Bodies { get; }

    /// <summary>
    /// Approximate movement per day in world units. Used to decide how often
    /// the positions need recomputing – the Kuiper belt's bodies creep along
    /// more slowly than the asteroid belt's and need recomputing less often.
    /// </summary>
    public float DriftPerDay { get; }

    SmallBodyBelt(int count, float unitsPerAu, int seed, Func<Random, float, Body> factory)
    {
        var rnd = new Random(seed);
        Bodies = new Body[count];
        for (int i = 0; i < count; i++)
            Bodies[i] = factory(rnd, unitsPerAu);

        // Grouped by brightness so the drawing code doesn't need to switch colour per body.
        Array.Sort(Bodies, (x, y) => x.Alpha.CompareTo(y.Alpha));

        double drift = 0;
        foreach (ref readonly var b in Bodies.AsSpan())
            drift += b.PlaneX.Length() * b.MeanMotionRad;
        DriftPerDay = (float)(drift / Math.Max(1, count));
    }

    /// <summary>The asteroid belt between Mars and Jupiter.</summary>
    public static SmallBodyBelt CreateAsteroidBelt(int count, float unitsPerAu, int seed = 20260901)
        => new(count, unitsPerAu, seed, MainBeltBody);

    /// <summary>The Kuiper belt beyond Neptune.</summary>
    public static SmallBodyBelt CreateKuiperBelt(int count, float unitsPerAu, int seed = 20260902)
        => new(count, unitsPerAu, seed, KuiperBody);

    // -------------------------------------------------------- asteroid belt

    /// <summary>The main belt's inner and outer edges in AU (the 4:1 and 2:1 resonances).</summary>
    public const double InnerAu = 2.06;
    public const double OuterAu = 3.27;

    /// <summary>The Kirkwood gaps: centre and half-width, in AU.</summary>
    static readonly (double Centre, double HalfWidth)[] KirkwoodGaps =
    [
        (2.502, 0.022),   // 3:1
        (2.825, 0.019),   // 5:2
        (2.958, 0.012),   // 7:3
    ];

    /// <summary>
    /// An asteroid in the main belt: semi-major axes between 2.06 and 3.27
    /// AU, mean eccentricity around 0.14 and mean inclination around 9.5
    /// degrees.
    ///
    /// The Kirkwood gaps are excluded. These are distances where an asteroid
    /// would complete a whole number of laps while Jupiter completes another
    /// whole number – at 2.50 AU three laps per Jupiter lap, at 3.27 AU two.
    /// Jupiter's repeated tugs in the same direction have swept away almost
    /// everything there.
    /// </summary>
    static Body MainBeltBody(Random rnd, float unitsPerAu)
    {
        double a = DrawSemiMajorAxis(rnd);
        // Eccentricity and inclination are distributed as in the real belt:
        // many nearly circular, shallow orbits, with a tail of outliers.
        double e = Math.Min(0.35, Rayleigh(rnd, 0.1117));
        double inc = Math.Min(30.0, Rayleigh(rnd, 7.58)) * Math.PI / 180.0;
        return Build(rnd, unitsPerAu, a, e, inc);
    }

    /// <summary>Draws a semi-major axis in the main belt, outside the Kirkwood gaps.</summary>
    static double DrawSemiMajorAxis(Random rnd)
    {
        for (int attempt = 0; attempt < 32; attempt++)
        {
            double a = InnerAu + rnd.NextDouble() * (OuterAu - InnerAu);
            bool inGap = false;
            foreach (var (centre, halfWidth) in KirkwoodGaps)
            {
                if (Math.Abs(a - centre) < halfWidth)
                {
                    inGap = true;
                    break;
                }
            }
            if (!inGap)
                return a;
        }
        return 2.35;   // emergency exit, should never actually be needed
    }

    // ---------------------------------------------------------- Kuiper belt

    /// <summary>The Kuiper belt's inner and outer edges in AU.</summary>
    public const double KuiperInnerAu = 39.0;
    public const double KuiperOuterAu = 47.8;

    /// <summary>
    /// A body in the Kuiper belt, which consists of two distinct populations.
    ///
    /// Plutinos are locked in 3:2 resonance with Neptune at 39.4 AU – they
    /// complete two laps while Neptune completes three. Pluto itself is one
    /// of them, and it's exactly this resonance that lets Pluto's orbit cross
    /// Neptune's without the two ever coming close to each other.
    ///
    /// The classical belt lies between 42 and 47.8 AU with lower
    /// eccentricity. At 47.8 AU the belt ends abruptly – the "Kuiper Cliff".
    ///
    /// The inclinations are roughly the same size as the asteroid belt's, but
    /// since the belt sits sixteen times farther out, it becomes much
    /// thicker in absolute terms: several AU up and down instead of a
    /// fraction.
    /// </summary>
    static Body KuiperBody(Random rnd, float unitsPerAu)
    {
        double a, e, incDeg;
        if (rnd.NextDouble() < 0.22)
        {
            a = 39.45 + Gauss(rnd) * 0.30;                          // plutinos
            e = Math.Min(0.34, Rayleigh(rnd, 0.16));
            incDeg = Math.Min(35.0, Rayleigh(rnd, 9.6));
        }
        else
        {
            a = 42.0 + rnd.NextDouble() * (KuiperOuterAu - 42.0);   // classical belt
            e = Math.Min(0.25, Rayleigh(rnd, 0.056));
            incDeg = Math.Min(35.0, Rayleigh(rnd, 8.0));
        }
        return Build(rnd, unitsPerAu, a, e, incDeg * Math.PI / 180.0);
    }

    // ------------------------------------------------------------- shared

    /// <summary>
    /// Builds a body from a given semi-major axis, eccentricity and
    /// inclination. The orbit's orientation in its plane is randomised
    /// uniformly.
    /// </summary>
    static Body Build(Random rnd, float unitsPerAu, double a, double e, double inc)
    {
        double node = rnd.NextDouble() * Math.PI * 2;
        double argPeri = rnd.NextDouble() * Math.PI * 2;
        double meanAnomaly = rnd.NextDouble() * Math.PI * 2;

        double cw = Math.Cos(argPeri), sw = Math.Sin(argPeri);
        double cO = Math.Cos(node), sO = Math.Sin(node);
        double ci = Math.Cos(inc), si = Math.Sin(inc);

        // The orbital plane's two basis vectors, converted to world
        // coordinates (Y = north of the ecliptic) the same way as
        // CelestialBody.ToWorld.
        var col1 = ToWorld(cw * cO - sw * sO * ci, cw * sO + sw * cO * ci, sw * si);
        var col2 = ToWorld(-sw * cO - cw * sO * ci, -sw * sO + cw * cO * ci, cw * si);

        float scale = (float)a * unitsPerAu;
        float minorFactor = (float)Math.Sqrt(1.0 - e * e);

        double periodDays = 365.256 * Math.Pow(a, 1.5);   // Kepler's third law
        float meanMotion = (float)(Math.PI * 2 / periodDays);

        return new Body(
            col1 * scale,
            col2 * (scale * minorFactor),
            (float)e,
            meanMotion,
            (float)meanAnomaly,
            QuantiseBrightness(rnd.NextDouble()));
    }

    /// <summary>
    /// Brightness is assigned in three steps instead of being fully random.
    /// Combined with sorting the list by brightness, the drawing code only
    /// needs to switch colour three times per frame instead of once per body.
    /// </summary>
    static float QuantiseBrightness(double u) => u < 0.34 ? 0.32f : u < 0.70 ? 0.50f : 0.72f;

    static Vector3 ToWorld(double x, double y, double z) =>
        new((float)x, (float)z, (float)-y);

    /// <summary>
    /// Rayleigh distribution, which describes eccentricity and inclination in
    /// a belt better than a uniform distribution does.
    /// </summary>
    static double Rayleigh(Random rnd, double sigma) =>
        sigma * Math.Sqrt(-2.0 * Math.Log(1.0 - rnd.NextDouble()));

    /// <summary>Normally distributed random number, for the plutinos' spread around the resonance.</summary>
    static double Gauss(Random rnd) =>
        Math.Sqrt(-2.0 * Math.Log(1.0 - rnd.NextDouble())) *
        Math.Cos(2.0 * Math.PI * rnd.NextDouble());

    /// <summary>The body's position at a given time, in world coordinates.</summary>
    public static Vector3 PositionOf(in Body a, double daysSinceJ2000)
    {
        float m = (float)((a.MeanAnomalyRad + a.MeanMotionRad * daysSinceJ2000) % (Math.PI * 2));
        float e = a.Eccentricity;

        // The starting guess E = M + e*sin M is close enough that four
        // Newton steps are plenty for the belts' modest eccentricities.
        float ecc = m + e * MathF.Sin(m);
        for (int k = 0; k < 4; k++)
            ecc -= (ecc - e * MathF.Sin(ecc) - m) / (1f - e * MathF.Cos(ecc));

        return a.PlaneX * (MathF.Cos(ecc) - e) + a.PlaneY * MathF.Sin(ecc);
    }
}
