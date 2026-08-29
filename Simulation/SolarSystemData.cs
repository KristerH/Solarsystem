using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En himlakropp med keplerska banelement (epok J2000).
/// Vinklar i grader: banlutning i, uppstigande nodens longitud Ω,
/// perihelielongitud ϖ samt medellongitud L0 vid epoken.
/// </summary>
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

    /// <summary>Position i världskoordinater (Y = norr om ekliptikan) vid given tid.</summary>
    public Vector3 PositionAt(double daysSinceJ2000, float unitsPerAu)
    {
        double meanMotion = 360.0 / OrbitalPeriodDays;
        double mDeg = MeanLonJ2000Deg + meanMotion * daysSinceJ2000 - PerihelionLonDeg;
        double M = DegToRad(mDeg);
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
        0.0025696, 0.0549, 5.145, 125.045, 83.353, 218.316, 27.32166);

    // Banelement vid J2000 (NASA/JPL, medelvärden). Tillräckligt noggranna för att
    // planeternas positioner ungefär ska stämma med verkligheten för ett givet datum.
    public static readonly CelestialBody[] Planets =
    [
        new("Merkurius", Color.FromArgb("#B5A79B"),  2_439.7, 0.38710, 0.20563, 7.005,  48.331,  77.456, 252.251,    87.969),
        new("Venus",     Color.FromArgb("#E8CDA0"),  6_051.8, 0.72333, 0.00677, 3.395,  76.680, 131.564, 181.980,   224.701),
        new("Jorden",    Color.FromArgb("#4C8CE8"),  6_371.0, 1.00000, 0.01671, 0.000, -11.261, 102.947, 100.464,   365.256) { Moons = [Moon] },
        new("Mars",      Color.FromArgb("#D96C4A"),  3_389.5, 1.52371, 0.09339, 1.850,  49.559, 336.041, 355.445,   686.980),
        new("Jupiter",   Color.FromArgb("#D8B48A"), 69_911.0, 5.20289, 0.04839, 1.304, 100.474,  14.728,  34.397, 4_332.59),
        new("Saturnus",  Color.FromArgb("#E8D5A8"), 58_232.0, 9.53668, 0.05386, 2.486, 113.662,  92.599,  49.954, 10_759.22),
        new("Uranus",    Color.FromArgb("#9BD4E4"), 25_362.0, 19.18916, 0.04726, 0.773, 74.017, 170.954, 313.238, 30_688.5),
        new("Neptunus",  Color.FromArgb("#5A78E8"), 24_622.0, 30.06992, 0.00859, 1.770, 131.784,  44.965, 304.880, 60_182.0),
        // Dvärgplaneten Pluto: kraftigt lutande (17°) och excentrisk bana som
        // tidvis går innanför Neptunus. Ett varv tar nästan 248 år.
        new("Pluto",     Color.FromArgb("#C4AB94"),  1_188.3, 39.48212, 0.24883, 17.140, 110.304, 224.069, 238.929, 90_560.0),
    ];
}
