using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// Asteroidbältet mellan Mars och Jupiter. Kropparna är inte verkliga
/// individer utan slumpade banor med samma statistiska fördelning som det
/// riktiga bältet: halva storaxlar mellan 2,06 och 3,27 AU, medelexcentricitet
/// omkring 0,14 och medelbanlutning omkring 9,5 grader.
///
/// Kirkwood-gapen sparas ut. Det är avstånd där en asteroid skulle hinna ett
/// helt antal varv medan Jupiter hinner ett annat – vid 2,50 AU tre varv per
/// Jupiter-varv, vid 3,27 AU två. Jupiters upprepade knuffar i samma riktning
/// har rensat bort nästan allt där.
/// </summary>
public sealed class AsteroidBelt
{
    /// <summary>
    /// En asteroid vars rotation från banplan till världskoordinater redan är
    /// inbakad i två vektorer. Då kostar en position bara en Kepler-lösning och
    /// två vektormultiplikationer – ingen trigonometri för banplanet per bildruta.
    /// </summary>
    public readonly struct Asteroid(
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

    public Asteroid[] Bodies { get; }

    public AsteroidBelt(int count, float unitsPerAu, int seed = 20260901)
    {
        var rnd = new Random(seed);
        Bodies = new Asteroid[count];
        for (int i = 0; i < count; i++)
            Bodies[i] = Create(rnd, unitsPerAu);

        // Gruppera efter ljusstyrka så att ritkoden slipper byta färg per asteroid.
        Array.Sort(Bodies, (x, y) => x.Alpha.CompareTo(y.Alpha));
    }

    static Asteroid Create(Random rnd, float unitsPerAu)
    {
        double a = DrawSemiMajorAxis(rnd);
        // Excentricitet och banlutning fördelas som i det verkliga bältet:
        // många nästan cirkulära och flacka banor, en svans av avvikande.
        double e = Math.Min(0.35, Rayleigh(rnd, 0.1117));
        double inc = Math.Min(30.0, Rayleigh(rnd, 7.58)) * Math.PI / 180.0;

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

        return new Asteroid(
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
    /// byta färg tre gånger per bildruta i stället för en gång per asteroid.
    /// </summary>
    static float QuantiseBrightness(double u) => u < 0.34 ? 0.32f : u < 0.70 ? 0.50f : 0.72f;

    static Vector3 ToWorld(double x, double y, double z) =>
        new((float)x, (float)z, (float)-y);

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

    /// <summary>
    /// Rayleigh-fördelning, som beskriver excentricitet och banlutning i ett
    /// bälte bättre än en likformig fördelning gör.
    /// </summary>
    static double Rayleigh(Random rnd, double sigma) =>
        sigma * Math.Sqrt(-2.0 * Math.Log(1.0 - rnd.NextDouble()));

    /// <summary>Asteroidens läge vid given tid, i världskoordinater.</summary>
    public static Vector3 PositionOf(in Asteroid a, double daysSinceJ2000)
    {
        float m = (float)((a.MeanAnomalyRad + a.MeanMotionRad * daysSinceJ2000) % (Math.PI * 2));
        float e = a.Eccentricity;

        // Startgissningen E = M + e*sin M ligger så nära att fyra
        // Newton-steg räcker gott för bältets måttliga excentriciteter.
        float ecc = m + e * MathF.Sin(m);
        for (int k = 0; k < 4; k++)
            ecc -= (ecc - e * MathF.Sin(ecc) - m) / (1f - e * MathF.Cos(ecc));

        return a.PlaneX * (MathF.Cos(ecc) - e) + a.PlaneY * MathF.Sin(ecc);
    }
}
