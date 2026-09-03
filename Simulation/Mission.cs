using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// A space trip from one body to another, along a transfer orbit the craft
/// follows without steering – exactly as a real probe does between the
/// rocket engine's two brief burns.
///
/// The orbit is solved from its boundary conditions with the Lambert solver:
/// it must start at Earth's position on launch day and reach the target's
/// position on arrival day. The only remaining question is therefore how
/// long the travel time should be, and it's chosen to make the launch as
/// cheap as possible – measured as the speed the craft needs relative to
/// Earth when it departs. That's the same trade-off real missions make, and
/// it comes out to around 3 km/s for Mars in the best windows, which is also
/// what it costs in reality.
///
/// The orbit is basically a Hohmann transfer – half an ellipse from Earth's
/// orbit out to the target's, the most fuel-efficient path. But exactly half
/// an ellipse won't do: it can only reach points directly opposite the
/// start, and right there the orbital plane becomes undefined and the price
/// runs away. Mars also sits 1.85 degrees out of the ecliptic. The cheapest
/// solution therefore lands a bit past half a turn, around a 200-degree
/// sweep.
///
/// The same class also describes trips that orbit a planet instead of the
/// Sun, like the trip to the Moon – see PlanToMoon. The difference lies only
/// in which body the orbit is computed around and how hard it pulls (µ); the
/// ellipse and Kepler's equation are the same.
/// </summary>
public sealed class Mission
{
    /// <summary>
    /// The craft's language-neutral key. The name shown at the dot is looked
    /// up from it, the same way as the bodies'.
    /// </summary>
    public string Key { get; }

    /// <summary>The target the craft is heading toward.</summary>
    public CelestialBody Target { get; }

    /// <summary>
    /// The body the orbit is computed around: null for the Sun, or the
    /// planet when the trip goes to one of its moons. The craft's positions
    /// are computed relative to it, the same way the moons' orbital elements
    /// are planet-centric.
    /// </summary>
    public CelestialBody? Center { get; }

    /// <summary>The moment of launch, in days since J2000.</summary>
    public double LaunchDay { get; }

    /// <summary>The moment of arrival, in days since J2000.</summary>
    public double ArrivalDay { get; }

    /// <summary>The length of the travel time in days.</summary>
    public double TravelDays => ArrivalDay - LaunchDay;

    /// <summary>Semi-major axis in AU.</summary>
    public double SemiMajorAu => _transfer.SemiMajorAu;

    /// <summary>The orbit's eccentricity.</summary>
    public double Eccentricity => _transfer.Eccentricity;

    /// <summary>
    /// How many degrees the craft sweeps around the central body along the
    /// way, measured in the direction the planets travel. A pure Hohmann
    /// transfer sweeps exactly 180 degrees; the cheapest real orbits land
    /// just above that, since the target needs time to arrive at the
    /// rendezvous.
    /// </summary>
    public double SweepDegrees { get; }

    /// <summary>
    /// The speed the craft must have relative to the departure body at the
    /// moment it leaves it. This is the measure of how big a rocket the trip
    /// requires, and it's what determines whether a launch window is open:
    /// real Mars missions sit around 3 km/s.
    /// </summary>
    public double DepartureSpeedKmS { get; }

    readonly Conic _transfer;

    Mission(string key, CelestialBody target, CelestialBody? center, Conic transfer,
        double launchDay, double arrivalDay, double sweepDegrees, double departureSpeedKmS)
    {
        Key = key;
        Target = target;
        Center = center;
        _transfer = transfer;
        LaunchDay = launchDay;
        ArrivalDay = arrivalDay;
        SweepDegrees = sweepDegrees;
        DepartureSpeedKmS = departureSpeedKmS;
    }

    // --------------------------------------------------- trips between planets

    /// <summary>Shortest travel time tried when searching for an orbit, in days.</summary>
    const double MinTravelDays = 120.0;

    /// <summary>Longest travel time tried when searching for an orbit, in days.</summary>
    const double MaxTravelDays = 480.0;

    /// <summary>Number of samples over the travel time in the coarse search.</summary>
    const int TravelSamples = 12;

    /// <summary>
    /// Plans a trip from one body to another with launch on a given day,
    /// along the cheapest orbit available that day. Returns null when no
    /// orbit can be computed.
    /// </summary>
    public static Mission? Plan(string key, CelestialBody origin, CelestialBody target,
        double launchDay)
    {
        var (departureSpeed, travelDays) = CheapestDeparture(origin, target, launchDay);
        if (double.IsInfinity(departureSpeed))
            return null;

        // Everything is computed in AU and double precision; the scale to
        // world units is applied only when rendering.
        var r1 = origin.PositionAuAt(launchDay);
        var r2 = target.PositionAuAt(launchDay + travelDays);
        if (!Lambert.Solve(r1, r2, travelDays, SolarSystemData.SunMu, out var v1, out _))
            return null;

        var transfer = Conic.FromState(r1, v1, launchDay, SolarSystemData.SunMu);
        if (transfer is null)
            return null;

        return new Mission(key, target, center: null, transfer,
            launchDay, launchDay + travelDays, SweepBetween(r1, r2), departureSpeed);
    }

    /// <summary>
    /// The angle between two positions, measured in the direction the
    /// planets travel. If the target lies less than half a turn away the
    /// other way, the trip has actually swept more than half a turn.
    /// </summary>
    static double SweepBetween(Vec3 from, Vec3 to)
    {
        double cos = Math.Clamp(
            Vec3.Dot(from, to) / (from.Length * to.Length), -1.0, 1.0);
        double sweep = Math.Acos(cos) * 180.0 / Math.PI;
        return Vec3.Cross(from, to).Y < 0 ? 360.0 - sweep : sweep;
    }

    /// <summary>
    /// The cheapest launch on a given day: the speed required relative to
    /// the departure body, and the travel time that gives it. Infinite speed
    /// means no orbit at all was found.
    ///
    /// The cost swings sharply with travel time. Short trips need a lot of
    /// speed, and partway through the interval there's also a peak: there
    /// the target lands directly opposite the start as seen from the Sun,
    /// which leaves the orbital plane undefined and the price runs away. The
    /// search therefore samples across the whole interval before refining
    /// around the best of them.
    /// </summary>
    public static (double SpeedKmS, double TravelDays) CheapestDeparture(
        CelestialBody origin, CelestialBody target, double day, bool refine = true)
    {
        double bestSpeed = double.PositiveInfinity, bestTravel = 0;

        for (int i = 0; i <= TravelSamples; i++)
        {
            double travel = MinTravelDays + (MaxTravelDays - MinTravelDays) * i / TravelSamples;
            double speed = DepartureSpeed(origin, target, day, travel);
            if (speed < bestSpeed)
            {
                bestSpeed = speed;
                bestTravel = travel;
            }
        }

        if (!refine || double.IsInfinity(bestSpeed))
            return (bestSpeed, bestTravel);

        // Halve the step size and try both directions, until the travel time
        // is determined to within about a day.
        for (double step = (MaxTravelDays - MinTravelDays) / TravelSamples * 0.5;
             step > 0.5; step *= 0.5)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                double travel = bestTravel + side * step;
                if (travel < MinTravelDays || travel > MaxTravelDays)
                    continue;

                double speed = DepartureSpeed(origin, target, day, travel);
                if (speed < bestSpeed)
                {
                    bestSpeed = speed;
                    bestTravel = travel;
                }
            }
        }

        return (bestSpeed, bestTravel);
    }

    /// <summary>
    /// What a given travel time costs: the speed the craft must have
    /// relative to the departure body when it leaves it.
    /// </summary>
    static double DepartureSpeed(CelestialBody origin, CelestialBody target,
        double day, double travelDays)
    {
        var r1 = origin.PositionAuAt(day);
        var r2 = target.PositionAuAt(day + travelDays);
        if (!Lambert.Solve(r1, r2, travelDays, SolarSystemData.SunMu, out var v1, out _))
            return double.PositiveInfinity;

        return (v1 - VelocityOf(origin, day)).Length * SolarSystemData.AuKm / 86_400.0;
    }

    /// <summary>The body's own velocity in AU/day, from its positions half a day apart.</summary>
    static Vec3 VelocityOf(CelestialBody body, double day)
        => body.PositionAuAt(day + 0.5) - body.PositionAuAt(day - 0.5);

    // ------------------------------------------------------------------ positions

    /// <summary>
    /// The craft's position at a given time, measured from the central body
    /// – the Sun, or the planet when the trip goes to a moon. After arrival
    /// it travels along with the target: a real probe enters orbit or
    /// lands, and so stays with the planet. Without that, the craft would
    /// end up standing still in empty space while the planet moves on – after
    /// half a year they'd be nearly 400 million kilometres apart.
    /// </summary>
    public Vector3 PositionAt(double day, float unitsPerAu)
    {
        if (day >= ArrivalDay)
            return Target.PositionAt(day, unitsPerAu);
        return TransferPositionAt(day, unitsPerAu);
    }

    /// <summary>
    /// The position on the transfer orbit itself, whether or not the craft
    /// has already arrived. Used to draw the orbit.
    /// </summary>
    public Vector3 TransferPositionAt(double day, float unitsPerAu)
        => _transfer.PositionAt(day, unitsPerAu);

    /// <summary>True once the craft has arrived.</summary>
    public bool HasArrived(double day) => day >= ArrivalDay;

    /// <summary>
    /// How far the craft has left to the target, in kilometres. Both craft
    /// and target are measured in the orbit's own system, so the same
    /// computation applies to a trip around the Sun as to one around a
    /// planet.
    /// </summary>
    public double DistanceToTargetKm(double day)
    {
        double d = Math.Clamp(day, LaunchDay, ArrivalDay);
        return (Target.PositionAt(d, 1f) - PositionAt(d, 1f)).Length() * SolarSystemData.AuKm;
    }

    /// <summary>
    /// The craft's speed in km/s at a given time, from the vis-viva
    /// equation. Speed is highest at launch and lowest at arrival, exactly
    /// as Kepler's second law says.
    /// </summary>
    public double SpeedKmPerSecond(double day)
        => _transfer.SpeedKmPerSecond(Math.Min(day, ArrivalDay));

    // ------------------------------------------------------------ launch windows

    /// <summary>
    /// How much more expensive than the window's very best day a launch is
    /// allowed to be and still count as a launch window.
    ///
    /// The measure is relative rather than a fixed km/s limit, since windows
    /// vary in quality: Mars's eccentric orbit means the cheapest occasion
    /// swings between 2.9 and 3.1 km/s depending on where the planet stands.
    /// A fixed limit would have made some windows nonexistent and others
    /// half a year long.
    /// </summary>
    public const double WindowMarginKmS = 0.1;

    /// <summary>How far in each direction the window's best day is searched for.</summary>
    const double WindowSearchDays = 180.0;

    /// <summary>
    /// The spacing between samples when the cost curve is mapped out.
    ///
    /// The grid is fixed, measured from the epoch, and not laid out around
    /// the day the question concerns. That matters: both "is the window open
    /// today?" and "when does the next one open?" measure against the same
    /// samples and so can never give contradictory answers. With separate
    /// grids, it used to happen that the "Next launch window" button jumped
    /// to a day that the "Launch" button then refused to let through.
    /// </summary>
    const double CostStepDays = 5.0;

    /// <summary>
    /// The cost at the grid points, cached. The window check looks half a
    /// year in each direction several times a second, and the samples are
    /// then nine-tenths the same as last time – while each individual sample
    /// requires its own search for the best travel time. The planetary
    /// orbits never change, so a cached value can never go stale.
    /// </summary>
    static readonly Dictionary<(string Origin, string Target, long Index), double> GridCost = new();

    /// <summary>The cost at one of the grid points.</summary>
    static double CostAtGridPoint(CelestialBody origin, CelestialBody target, long index)
    {
        var key = (origin.Key, target.Key, index);
        if (GridCost.TryGetValue(key, out double cached))
            return cached;

        double cost = CheapestDeparture(origin, target, index * CostStepDays, refine: false).SpeedKmS;

        // The grid is sparse – 8192 points cover more than a hundred years –
        // but whoever winds time forward far enough shouldn't accumulate
        // memory forever.
        if (GridCost.Count > 8192)
            GridCost.Clear();

        GridCost[key] = cost;
        return cost;
    }

    /// <summary>True if a fuel-efficient trip can be started on that particular day.</summary>
    public static bool IsLaunchWindow(CelestialBody origin, CelestialBody target, double day)
    {
        double here = CheapestDeparture(origin, target, day, refine: false).SpeedKmS;
        if (double.IsInfinity(here))
            return false;

        return here <= BestNearby(origin, target, day) + WindowMarginKmS;
    }

    /// <summary>
    /// The cheapest launch near a given day. The search range is half a year
    /// in each direction: enough to find the bottom of the window you're
    /// currently in, but clearly shorter than the 780 days separating
    /// windows, so the next window's bottom doesn't accidentally get
    /// included.
    /// </summary>
    static double BestNearby(CelestialBody origin, CelestialBody target, double day)
    {
        long centre = (long)Math.Round(day / CostStepDays);
        int span = (int)(WindowSearchDays / CostStepDays);

        double best = double.PositiveInfinity;
        for (long k = centre - span; k <= centre + span; k++)
            best = Math.Min(best, CostAtGridPoint(origin, target, k));
        return best;
    }

    /// <summary>
    /// Finds the next day a trip can be started. The cost curve is mapped
    /// out once over the whole search horizon – with half a year of margin
    /// in each direction, since every point is compared against the
    /// cheapest in its neighbourhood – and the first day that falls within
    /// the margin is the answer. Returns null if no window is found within
    /// the horizon, but since every window by definition contains its own
    /// bottom point, that only happens for unreasonably short horizons.
    /// </summary>
    public static double? NextLaunchWindow(CelestialBody origin, CelestialBody target,
        double fromDay, double horizonDays = 900.0)
    {
        int span = (int)(WindowSearchDays / CostStepDays);
        long first = (long)Math.Round(fromDay / CostStepDays) - span;
        long last = (long)Math.Round((fromDay + horizonDays) / CostStepDays) + span;

        // Map out the cost once; every sample requires its own search for
        // the best travel time, so they shouldn't be recomputed
        // unnecessarily.
        var cost = new double[last - first + 1];
        for (int i = 0; i < cost.Length; i++)
            cost[i] = CostAtGridPoint(origin, target, first + i);

        for (long k = (long)Math.Round(fromDay / CostStepDays); k <= last - span; k++)
        {
            int i = (int)(k - first);

            double best = double.PositiveInfinity;
            for (int j = i - span; j <= i + span; j++)
                best = Math.Min(best, cost[j]);

            if (cost[i] > best + WindowMarginKmS)
                continue;

            // Step backward to the window's first day. The margin is
            // measured against the same neighbourhood throughout, as long
            // as the days belong to the same grid point.
            double day = k * CostStepDays;
            while (day - 1.0 >= fromDay &&
                   (long)Math.Round((day - 1.0) / CostStepDays) == k &&
                   CheapestDeparture(origin, target, day - 1.0, refine: false).SpeedKmS
                       <= best + WindowMarginKmS)
                day -= 1.0;

            return Math.Max(day, fromDay);
        }

        return null;
    }

    // ---------------------------------------------------------------- trip to the Moon

    /// <summary>Altitude above Earth's surface where the trip begins: a low parking orbit.</summary>
    public const double ParkingOrbitAltitudeKm = 400.0;

    /// <summary>
    /// Travel time to the Moon in days. Apollo 11 arrived after 76 hours,
    /// just over three days.
    /// </summary>
    public const double MoonTravelDays = 3.0;

    /// <summary>
    /// Plans a trip to one of the planet's own moons. Here the craft orbits
    /// the planet instead of the Sun, and the difference from the Mars trip
    /// is bigger than it looks.
    ///
    /// Launch happens from a low orbit, and from there the craft can leave
    /// in any direction. The starting point therefore isn't given in
    /// advance, the way Earth's position is for a Mars trip, but is instead
    /// chosen so the orbit meets the Moon. That's why a lunar trip can start
    /// on almost any day while Mars needs a launch window every couple of
    /// years.
    ///
    /// The travel time is instead fixed in advance, and the orbit is solved
    /// to match it. A pure Hohmann orbit out to the Moon takes nearly five
    /// days; to make it in three, the craft must be launched with more
    /// speed, so the orbit's far end lands well beyond the Moon –
    /// 440,000 to 630,000 km depending on where the Moon stands – and the
    /// Moon is caught up with on the way out, before the turning point.
    /// That's exactly how Apollo flew.
    /// </summary>
    public static Mission? PlanToMoon(string key, CelestialBody planet, CelestialBody moon,
        double launchDay, double travelDays = MoonTravelDays)
    {
        double mu = planet.Mu;
        if (mu <= 0 || travelDays <= 0)
            return null;

        // The parking orbit becomes the orbit's perigee, the point closest to the planet.
        double rp = (planet.RadiusKm + ParkingOrbitAltitudeKm) / SolarSystemData.AuKm;

        double arrivalDay = launchDay + travelDays;
        var r2 = moon.PositionAt(arrivalDay, 1f);   // planet-centric position at arrival
        double r2Len = r2.Length();
        if (r2Len <= rp)
            return null;
        var r2Dir = r2 / (float)r2Len;

        // Search for the semi-major axis that gives the desired travel time.
        // Travel time decreases as the orbit is made larger – more speed at
        // launch – so bisection finds the right one directly. The lower
        // bound is the most fuel-efficient ellipse that reaches the target
        // at all, i.e. the slowest; eight times that is comfortably faster
        // than three days.
        double lo = 0.5 * (rp + r2Len);
        double hi = lo * 8.0;
        if (TimeToRadius(lo, rp, r2Len, mu) < travelDays ||
            TimeToRadius(hi, rp, r2Len, mu) > travelDays)
            return null;    // the desired travel time can't be met

        for (int k = 0; k < 80; k++)
        {
            double mid = 0.5 * (lo + hi);
            if (TimeToRadius(mid, rp, r2Len, mu) > travelDays)
                lo = mid;   // too slow – the orbit needs to be made larger
            else
                hi = mid;
        }

        double a = 0.5 * (lo + hi);
        double e = 1.0 - rp / a;

        // The sweep angle: how far around the planet the craft travels on the way out.
        double cosSweep = Math.Clamp((a * (1.0 - e * e) / r2Len - 1.0) / e, -1.0, 1.0);
        double sweep = Math.Acos(cosSweep);

        // The orbital plane is laid in the Moon's own orbital plane, exactly
        // as the Apollo missions did. The normal is taken from the Moon's
        // position half a day before and after arrival, and points the same
        // way as the Moon's own angular momentum, so the craft travels the
        // same direction as the Moon.
        var normalRaw = Vector3.Cross(
            moon.PositionAt(arrivalDay - 0.5, 1f),
            moon.PositionAt(arrivalDay + 0.5, 1f));
        if (normalRaw.Length() < 1e-12f)
            return null;
        var normal = Vector3.Normalize(normalRaw);

        // The launch point is found by rotating the arrival direction back
        // by the sweep angle, i.e. backward along the orbit.
        var forward = Vector3.Cross(normal, r2Dir);
        var periDir = Vector3.Normalize(
            r2Dir * (float)Math.Cos(sweep) - forward * (float)Math.Sin(sweep));
        var sideDir = Vector3.Normalize(Vector3.Cross(normal, periDir));

        // Launch happens at perigee, so the orbit is built from there
        // directly – that entry point loses no precision.
        var transfer = Conic.FromPeriapsis(periDir, sideDir, rp, e, launchDay, mu);
        if (transfer is null)
            return null;

        return new Mission(key, moon, planet, transfer, launchDay, arrivalDay,
            sweep * 180.0 / Math.PI, transfer.SpeedKmPerSecond(launchDay));
    }

    /// <summary>
    /// The travel time from perigee out to a given radius, for the ellipse
    /// with semi-major axis a and perigee rp. Kepler's equation run
    /// backward from how it's otherwise used: first the eccentric anomaly
    /// from the radius, then the time from that.
    /// </summary>
    static double TimeToRadius(double a, double rp, double r, double mu)
    {
        double e = 1.0 - rp / a;
        double ecc = Math.Acos(Math.Clamp((1.0 - r / a) / e, -1.0, 1.0));
        double meanAnomaly = ecc - e * Math.Sin(ecc);
        return meanAnomaly / Math.Sqrt(mu / (a * a * a));
    }
}
