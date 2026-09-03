using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// A point where a probe demonstrably was: a planet on a given date, or a
/// known location in space on a given date. Launch counts as Earth.
/// </summary>
public sealed class Waypoint
{
    /// <summary>
    /// The point's language-neutral key – a body's key for a planetary
    /// flyby, otherwise one of its own (<c>probeToday</c>,
    /// <c>lastContact</c>). The name to display is looked up from it, see the
    /// resource files.
    /// </summary>
    public string Key { get; }

    /// <summary>The date the probe was there.</summary>
    public DateTime Date { get; }

    /// <summary>The same date in days since J2000.</summary>
    public double Day => (Date - SolarSystemData.EpochJ2000).TotalDays;

    readonly CelestialBody? _body;
    readonly Vec3 _fixedAu;

    Waypoint(string key, DateTime date, CelestialBody? body, Vec3 fixedAu)
    {
        Key = key;
        Date = date;
        _body = body;
        _fixedAu = fixedAu;
    }

    /// <summary>The probe was at a planet on this date.</summary>
    public static Waypoint At(CelestialBody body, DateTime date)
        => new(body.Key, date, body, default);

    /// <summary>
    /// The probe was at a known distance in a known direction in the sky.
    /// This is how the probes' current positions are given, and it's this
    /// data that determines how the final stretch out of the Solar System is
    /// tilted: the orbit there is computed from the distance and direction,
    /// rather than the tilt being entered directly.
    /// </summary>
    public static Waypoint InSky(string key, DateTime date, double distanceAu,
        double raHours, double decDeg)
        => new(key, date, null,
            StarCatalog.EquatorialToWorldAu(raHours, decDeg) * distanceAu);

    /// <summary>The point's position in AU, Sun-centred and in double precision.</summary>
    public Vec3 PositionAu() => _body?.PositionAuAt(Day) ?? _fixedAu;
}

/// <summary>
/// One leg of a probe's journey: the orbit from one point to the next. The
/// orbit is the one that genuinely goes between the two positions in exactly
/// the time that elapsed between the dates, and it's usually a hyperbola –
/// the probes have enough speed to never come back.
/// </summary>
public sealed record ProbeLeg(string From, string To, double StartDay, double EndDay, Conic Path)
{
    /// <summary>How long the leg lasted, in days.</summary>
    public double Days => EndDay - StartDay;
}

/// <summary>
/// A milestone along the journey: the launch, or a planetary flyby.
///
/// The speed before and after is taken from the two legs that meet at that
/// point. They meet at the same position but with different velocities, and
/// the difference is the gravity assist: the probe borrows speed from the
/// planet's motion around the Sun. At launch there's no leg before, so the
/// speed before is zero.
/// </summary>
public sealed record Milestone(
    string Key, double Day, Vector3 PositionAu, double SpeedBeforeKmS, double SpeedAfterKmS)
{
    /// <summary>True for the launch, which isn't a flyby.</summary>
    public bool IsLaunch => SpeedBeforeKmS <= 0;

    /// <summary>
    /// True for a boundary the probe passed without anything happening to
    /// its speed – the heliopause is the only one so far. It belongs among
    /// the milestones since it's a place the probe demonstrably passed on a
    /// known date, but it isn't a flyby and shouldn't be described as one.
    /// </summary>
    public bool IsBoundary { get; init; }

    /// <summary>How much speed the planet gave – or took, which also happens.</summary>
    public double SpeedGainKmS => IsLaunch || IsBoundary ? 0 : SpeedAfterKmS - SpeedBeforeKmS;
}

/// <summary>
/// A real spacecraft, built from the dates it actually passed the planets.
///
/// No orbital elements are entered, then. Instead, each leg of the journey
/// is allowed to be the orbit that goes from one planet to the next in
/// exactly the time the flybys took, computed from the app's own planet
/// positions with the Lambert solver. Two things follow from that: the probe
/// lands at the right planet on the right day on its own, and the speed
/// jumps at every flyby without anyone entering the jump – that's the
/// gravity assist, and it's the whole reason the probes could reach so far.
///
/// The final leg runs out to the probe's position today, given as a
/// distance and direction in the sky. That makes its inclination out of the
/// ecliptic a result too, rather than an input.
/// </summary>
public sealed class Probe
{
    /// <summary>The probe's name, shown at the dot.</summary>
    public string Name { get; }

    /// <summary>The colour the probe and its trail are drawn in.</summary>
    public Color Color { get; }

    /// <summary>The journey's legs, in time order.</summary>
    public IReadOnlyList<ProbeLeg> Legs { get; }

    /// <summary>Launch and the planetary flybys, in time order.</summary>
    public IReadOnlyList<Milestone> Milestones { get; private set; }

    /// <summary>The launch day, in days since J2000.</summary>
    public double LaunchDay { get; }

    Probe(string name, Color color, IReadOnlyList<ProbeLeg> legs)
    {
        Name = name;
        Color = color;
        Legs = legs;
        LaunchDay = legs.Count > 0 ? legs[0].StartDay : 0;
        Milestones = BuildMilestones(legs);
    }

    /// <summary>
    /// The milestones fall out of the legs: each leg starts at one, and the
    /// speed jump is the difference between the speed of the leg that ends
    /// there and the leg that begins there, both evaluated at that point.
    /// </summary>
    static Milestone[] BuildMilestones(IReadOnlyList<ProbeLeg> legs)
    {
        var milestones = new Milestone[legs.Count];

        for (int i = 0; i < legs.Count; i++)
        {
            double day = legs[i].StartDay;
            milestones[i] = new Milestone(
                legs[i].From, day, legs[i].Path.PositionAt(day, 1f),
                i == 0 ? 0.0 : legs[i - 1].Path.SpeedKmPerSecond(day),
                legs[i].Path.SpeedKmPerSecond(day));
        }

        return milestones;
    }

    /// <summary>
    /// Adds a boundary the probe passed on a known date, without its speed
    /// changing. The position and speed are taken from the orbit the probe
    /// was following at that moment, so the boundary lands where the probe
    /// genuinely was – not where someone entered that it was.
    ///
    /// Written after Build, since Build takes its points as params and has
    /// no room left for other kinds of data.
    /// </summary>
    public Probe Crossing(string key, DateTime date)
    {
        double day = (date - SolarSystemData.EpochJ2000).TotalDays;
        if (LegAt(day) is not { } leg)
            return this;

        double speed = leg.Path.SpeedKmPerSecond(day);
        var crossing = new Milestone(key, day, leg.Path.PositionAt(day, 1f), speed, speed)
        {
            IsBoundary = true,
        };

        Milestones = [.. Milestones.Append(crossing).OrderBy(m => m.Day)];
        return this;
    }

    /// <summary>The most recently passed milestone, or null before launch.</summary>
    public Milestone? LastMilestone(double day)
    {
        Milestone? last = null;
        foreach (var milestone in Milestones)
        {
            if (milestone.Day > day)
                break;
            last = milestone;
        }
        return last;
    }

    /// <summary>The next milestone the probe is heading toward, or null once all have passed.</summary>
    public Milestone? NextMilestone(double day)
    {
        foreach (var milestone in Milestones)
            if (milestone.Day > day)
                return milestone;
        return null;
    }

    /// <summary>
    /// Builds the probe from the points it passed. A leg that can't be
    /// solved is skipped; that shows up as Legs being shorter than expected,
    /// and is best checked outside the app.
    /// </summary>
    /// <summary>
    /// Legs that couldn't be built, with the probe and the flybys written
    /// out. An empty list means all the probe data went through.
    ///
    /// The probe is built from the legs that work, since a probe with a gap
    /// is better than no probe at all, and since `Build` is called from
    /// static fields – an exception there would take down the whole app at
    /// startup over one bad piece of data. But it must not pass unnoticed:
    /// it used to be that the orbit silently got a jump, and whoever had
    /// entered an impossible pair of dates had nothing to go on. Now it's
    /// written to the log and kept here to query, so the test programs
    /// outside the app can require the list to be empty.
    /// </summary>
    public static IReadOnlyList<string> SkippedLegs => Skipped;

    static readonly List<string> Skipped = [];

    public static Probe Build(string name, Color color, params Waypoint[] waypoints)
    {
        var legs = new List<ProbeLeg>(waypoints.Length);

        for (int i = 0; i + 1 < waypoints.Length; i++)
        {
            Waypoint from = waypoints[i], to = waypoints[i + 1];
            double travelDays = to.Day - from.Day;
            if (travelDays <= 0)
            {
                Skip(name, from, to, "the flybys aren't in time order");
                continue;
            }

            var r1 = from.PositionAu();
            var r2 = to.PositionAu();
            if (!Lambert.Solve(r1, r2, travelDays, SolarSystemData.SunMu, out var v1, out _))
            {
                Skip(name, from, to, "Lambert found no orbit for that time");
                continue;
            }

            if (Conic.FromState(r1, v1, from.Day, SolarSystemData.SunMu) is { } path)
                legs.Add(new ProbeLeg(from.Key, to.Key, from.Day, to.Day, path));
            else
                Skip(name, from, to, "the orbit couldn't be built from position and velocity");
        }

        return new Probe(name, color, legs);
    }

    static void Skip(string probe, Waypoint from, Waypoint to, string why)
    {
        double days = to.Day - from.Day;
        string message = string.Create(System.Globalization.CultureInfo.InvariantCulture,
            $"{probe}: leg {from.Key} -> {to.Key} skipped ({days:0.#} days) - {why}.");
        Skipped.Add(message);
        Diagnostics.Log(message);
    }

    /// <summary>True once the probe has launched and so exists to be drawn.</summary>
    public bool Exists(double day) => Legs.Count > 0 && day >= LaunchDay;

    /// <summary>
    /// The leg the probe is currently on. After the last point it continues
    /// on its last leg – the orbit still applies further out, the probe is
    /// still on its way outward.
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

    /// <summary>The probe's position, or null before it has launched.</summary>
    public Vector3? PositionAt(double day, float unitsPerAu)
        => LegAt(day)?.Path.PositionAt(day, unitsPerAu);

    /// <summary>The probe's distance from the Sun in AU, or zero before launch.</summary>
    public double DistanceAu(double day)
        => LegAt(day)?.Path.DistanceAu(day) ?? 0.0;

    /// <summary>
    /// The probe's speed in km/s. The speed jumps at every planetary flyby,
    /// since the legs meet at the same position but with different
    /// velocities – that's the gravity assist.
    /// </summary>
    public double SpeedKmPerSecond(double day)
        => LegAt(day)?.Path.SpeedKmPerSecond(day) ?? 0.0;
}
