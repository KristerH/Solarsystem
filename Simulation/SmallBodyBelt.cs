using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// Ett bälte av småkroppar kring solen – antingen asteroidbältet mellan Mars
/// och Jupiter eller Kuiperbältet bortom Neptunus. Kropparna är inte verkliga
/// individer utan slumpade banor med samma statistiska fördelning som det
/// bälte de föreställer.
/// </summary>
public sealed class SmallBodyBelt
{
    /// <summary>
    /// En småkropp vars rotation från banplan till världskoordinater redan är
    /// inbakad i två vektorer. Då kostar en position bara en Kepler-lösning och
    /// två vektormultiplikationer – ingen trigonometri för banplanet per bildruta.
    /// </summary>
    public readonly struct Body(
        Vector3 planeX, Vector3 planeY, float eccentricity,
        float meanMotionRad, float meanAnomalyRad, float alpha)
    {
        /// <summary>Halva storaxeln gånger banplanets första basvektor.</summary>
        public readonly Vector3 PlaneX = planeX;

        /// <summary>Halva lillaxeln gånger banplanets andra basvektor.</summary>
        public readonly Vector3 PlaneY = planeY;

        public readonly float Eccentricity = eccentricity;
        public readonly float MeanMotionRad = meanMotionRad;
        public readonly float MeanAnomalyRad = meanAnomalyRad;
        public readonly float Alpha = alpha;
    }

    public Body[] Bodies { get; }

    /// <summary>
    /// Ungefärlig förflyttning per dygn i världsenheter. Används för att avgöra
    /// hur ofta positionerna behöver räknas om – Kuiperbältets kroppar kryper
    /// fram långsammare än asteroidbältets och behöver räknas om mer sällan.
    /// </summary>
    public float DriftPerDay { get; }

    SmallBodyBelt(int count, float unitsPerAu, int seed, Func<Random, float, Body> factory)
    {
        var rnd = new Random(seed);
        Bodies = new Body[count];
        for (int i = 0; i < count; i++)
            Bodies[i] = factory(rnd, unitsPerAu);

        // Gruppera efter ljusstyrka så att ritkoden slipper byta färg per kropp.
        Array.Sort(Bodies, (x, y) => x.Alpha.CompareTo(y.Alpha));

        double drift = 0;
        foreach (ref readonly var b in Bodies.AsSpan())
            drift += b.PlaneX.Length() * b.MeanMotionRad;
        DriftPerDay = (float)(drift / Math.Max(1, count));
    }

    /// <summary>Asteroidbältet mellan Mars och Jupiter.</summary>
    public static SmallBodyBelt CreateAsteroidBelt(int count, float unitsPerAu, int seed = 20260901)
        => new(count, unitsPerAu, seed, MainBeltBody);

    /// <summary>Kuiperbältet bortom Neptunus.</summary>
    public static SmallBodyBelt CreateKuiperBelt(int count, float unitsPerAu, int seed = 20260902)
        => new(count, unitsPerAu, seed, KuiperBody);

    // -------------------------------------------------------- asteroidbältet

    /// <summary>Huvudbältets inner- och ytterkant i AU (4:1- och 2:1-resonanserna).</summary>
    public const double InnerAu = 2.06;
    public const double OuterAu = 3.27;

    /// <summary>Kirkwood-gapen: mittpunkt och halva bredden, i AU.</summary>
    static readonly (double Centre, double HalfWidth)[] KirkwoodGaps =
    [
        (2.502, 0.022),   // 3:1
        (2.825, 0.019),   // 5:2
        (2.958, 0.012),   // 7:3
    ];

    /// <summary>
    /// En asteroid i huvudbältet: halva storaxlar mellan 2,06 och 3,27 AU,
    /// medelexcentricitet omkring 0,14 och medelbanlutning omkring 9,5 grader.
    ///
    /// Kirkwood-gapen sparas ut. Det är avstånd där en asteroid skulle hinna ett
    /// helt antal varv medan Jupiter hinner ett annat – vid 2,50 AU tre varv per
    /// Jupiter-varv, vid 3,27 AU två. Jupiters upprepade knuffar i samma
    /// riktning har rensat bort nästan allt där.
    /// </summary>
    static Body MainBeltBody(Random rnd, float unitsPerAu)
    {
        double a = DrawSemiMajorAxis(rnd);
        // Excentricitet och banlutning fördelas som i det verkliga bältet:
        // många nästan cirkulära och flacka banor, en svans av avvikande.
        double e = Math.Min(0.35, Rayleigh(rnd, 0.1117));
        double inc = Math.Min(30.0, Rayleigh(rnd, 7.58)) * Math.PI / 180.0;
        return Build(rnd, unitsPerAu, a, e, inc);
    }

    /// <summary>Drar en halv storaxel i huvudbältet, utanför Kirkwood-gapen.</summary>
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
        return 2.35;   // nödutgång, ska i praktiken aldrig behövas
    }

    // ---------------------------------------------------------- Kuiperbältet

    /// <summary>Kuiperbältets inner- och ytterkant i AU.</summary>
    public const double KuiperInnerAu = 39.0;
    public const double KuiperOuterAu = 47.8;

    /// <summary>
    /// En kropp i Kuiperbältet, som består av två tydliga befolkningar.
    ///
    /// Plutinos ligger låsta i 3:2-resonans med Neptunus vid 39,4 AU – de hinner
    /// två varv medan Neptunus hinner tre. Pluto själv är en av dem, och det är
    /// just resonansen som gör att Plutos bana kan korsa Neptunus utan att de
    /// någonsin kommer i närheten av varandra.
    ///
    /// Det klassiska bältet ligger mellan 42 och 47,8 AU med lägre
    /// excentricitet. Vid 47,8 AU tar bältet abrupt slut – "Kuiperklippan".
    ///
    /// Banlutningarna är ungefär lika stora som i asteroidbältet, men eftersom
    /// bältet ligger sexton gånger längre bort blir det i absoluta mått mycket
    /// tjockare: flera AU upp och ner i stället för en bråkdel.
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
            a = 42.0 + rnd.NextDouble() * (KuiperOuterAu - 42.0);   // klassiska bältet
            e = Math.Min(0.25, Rayleigh(rnd, 0.056));
            incDeg = Math.Min(35.0, Rayleigh(rnd, 8.0));
        }
        return Build(rnd, unitsPerAu, a, e, incDeg * Math.PI / 180.0);
    }

    // ------------------------------------------------------------- gemensamt

    /// <summary>
    /// Bygger en kropp av given halv storaxel, excentricitet och banlutning.
    /// Banans orientering i planet slumpas jämnt.
    /// </summary>
    static Body Build(Random rnd, float unitsPerAu, double a, double e, double inc)
    {
        double node = rnd.NextDouble() * Math.PI * 2;
        double argPeri = rnd.NextDouble() * Math.PI * 2;
        double meanAnomaly = rnd.NextDouble() * Math.PI * 2;

        double cw = Math.Cos(argPeri), sw = Math.Sin(argPeri);
        double cO = Math.Cos(node), sO = Math.Sin(node);
        double ci = Math.Cos(inc), si = Math.Sin(inc);

        // Banplanets två basvektorer, omräknade till världskoordinater
        // (Y = norr om ekliptikan) på samma sätt som CelestialBody.ToWorld.
        var col1 = ToWorld(cw * cO - sw * sO * ci, cw * sO + sw * cO * ci, sw * si);
        var col2 = ToWorld(-sw * cO - cw * sO * ci, -sw * sO + cw * cO * ci, cw * si);

        float scale = (float)a * unitsPerAu;
        float minorFactor = (float)Math.Sqrt(1.0 - e * e);

        double periodDays = 365.256 * Math.Pow(a, 1.5);   // Keplers tredje lag
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
    /// Ljusstyrkan läggs i tre steg i stället för att vara helt slumpmässig.
    /// Tillsammans med att listan sorteras efter styrka behöver ritkoden bara
    /// byta färg tre gånger per bildruta i stället för en gång per kropp.
    /// </summary>
    static float QuantiseBrightness(double u) => u < 0.34 ? 0.32f : u < 0.70 ? 0.50f : 0.72f;

    static Vector3 ToWorld(double x, double y, double z) =>
        new((float)x, (float)z, (float)-y);

    /// <summary>
    /// Rayleigh-fördelning, som beskriver excentricitet och banlutning i ett
    /// bälte bättre än en likformig fördelning gör.
    /// </summary>
    static double Rayleigh(Random rnd, double sigma) =>
        sigma * Math.Sqrt(-2.0 * Math.Log(1.0 - rnd.NextDouble()));

    /// <summary>Normalfördelat slumptal, för plutinornas spridning kring resonansen.</summary>
    static double Gauss(Random rnd) =>
        Math.Sqrt(-2.0 * Math.Log(1.0 - rnd.NextDouble())) *
        Math.Cos(2.0 * Math.PI * rnd.NextDouble());

    /// <summary>Kroppens läge vid given tid, i världskoordinater.</summary>
    public static Vector3 PositionOf(in Body a, double daysSinceJ2000)
    {
        float m = (float)((a.MeanAnomalyRad + a.MeanMotionRad * daysSinceJ2000) % (Math.PI * 2));
        float e = a.Eccentricity;

        // Startgissningen E = M + e*sin M ligger så nära att fyra
        // Newton-steg räcker gott för bältenas måttliga excentriciteter.
        float ecc = m + e * MathF.Sin(m);
        for (int k = 0; k < 4; k++)
            ecc -= (ecc - e * MathF.Sin(ecc) - m) / (1f - e * MathF.Cos(ecc));

        return a.PlaneX * (MathF.Cos(ecc) - e) + a.PlaneY * MathF.Sin(ecc);
    }
}
