using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// A probe that orbits a planet instead of flying past it: Cassini at Saturn
/// and Juno at Jupiter. A simpler case than the five probes that left the
/// Solar System – here the orbit is a plain ellipse, just like the moons'.
///
/// One important difference from Voyager and the others: those orbits are
/// reconstructed from real dates, so the probes stand in the right place on
/// the right day. Here that's not possible. Cassini flew nearly three hundred
/// different laps around Saturn over thirteen years, with periods from a week
/// to four months and inclinations to the ring plane ranging up to 75
/// degrees. What's shown is therefore a representative lap: the size, shape,
/// period and orbital plane are real, but where the probe sits in the orbit
/// on a given date is not.
/// </summary>
public sealed class Orbiter
{
    /// <summary>The probe's name, shown at the dot.</summary>
    public string Name { get; }

    /// <summary>The colour the probe and its orbit are drawn in.</summary>
    public Color Color { get; }

    /// <summary>The planet the probe orbits.</summary>
    public CelestialBody Center { get; }

    /// <summary>The orbit, computed around the planet rather than the Sun.</summary>
    public Conic Path { get; }

    /// <summary>The day the probe entered orbit.</summary>
    public double ArrivalDay { get; }

    /// <summary>The day the mission ended, or null while it's ongoing.</summary>
    public double? EndDay { get; }

    /// <summary>What happened at the end, for the panel. Empty string while the mission is ongoing.</summary>
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
    /// Builds the probe from the orbit's size and shape expressed in planet
    /// radii, which is the unit these orbits are normally reported in.
    ///
    /// The orbital plane is given relative to the planet's equator, not the
    /// ecliptic: a polar orbit is 90 degrees to the equator regardless of how
    /// the planet itself is tilted. The inclination is therefore added to the
    /// planet's own, taken from its rotation axis, with the same ascending
    /// node – rotating the equatorial plane a quarter turn around the node
    /// line gives exactly a plane through both poles.
    /// </summary>
    public static Orbiter? Build(string name, Color color, CelestialBody center,
        double periapsisRadii, double apoapsisRadii, double inclinationToEquatorDeg,
        double argPeriapsisDeg, DateTime arrival, DateTime? end = null, string ending = "")
    {
        if (center.Mu <= 0 || periapsisRadii <= 0 || apoapsisRadii < periapsisRadii)
            return null;
        if (center.Axis is not BodyAxis equator)
            return null;   // without a known equator, the orbital plane can't be laid out

        double rp = periapsisRadii * center.RadiusKm / SolarSystemData.AuKm;
        double ra = apoapsisRadii * center.RadiusKm / SolarSystemData.AuKm;
        double semiMajorAu = 0.5 * (rp + ra);
        double eccentricity = (ra - rp) / (ra + rp);

        double arrivalDay = (arrival - SolarSystemData.EpochJ2000).TotalDays;

        var path = Conic.FromElements(semiMajorAu, eccentricity,
            equator.InclinationDeg + inclinationToEquatorDeg, equator.NodeDeg,
            argPeriapsisDeg, arrivalDay, center.Mu);

        return new Orbiter(name, color, center, path, arrivalDay,
            end is { } e ? (e - SolarSystemData.EpochJ2000).TotalDays : null, ending);
    }

    /// <summary>True while the probe is orbiting the planet on the given day.</summary>
    public bool Exists(double day)
        => day >= ArrivalDay && (EndDay is not double end || day <= end);

    /// <summary>The orbital period in days.</summary>
    public double PeriodDays => Path.PeriodDays ?? 0.0;

    /// <summary>The probe's position relative to the planet.</summary>
    public Vector3 PositionAt(double day, float unitsPerAu)
        => Path.PositionAt(day, unitsPerAu);

    /// <summary>The probe's speed in km/s. Greatest at periapsis, as for everything else.</summary>
    public double SpeedKmPerSecond(double day) => Path.SpeedKmPerSecond(day);

    /// <summary>
    /// The whole orbital ellipse as a list of points, planet-centric. One lap
    /// is enough – the orbit is closed.
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
