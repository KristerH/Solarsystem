namespace Solarsystem.Simulation;

/// <summary>
/// Lambert's problem: which orbit goes from one position to another in
/// exactly a given time?
///
/// That question is what lets the spacecraft be built from real dates.
/// Voyager 1 launched on 5 September 1977 and was at Jupiter on 5 March 1979.
/// Where Earth and Jupiter stood on those dates the app already knows, and
/// the travel time is known too – so the orbit is determined, without a
/// single orbital element needing to be entered.
///
/// The solution uses Bate, Mueller and White's universal variables. The idea
/// is to describe every conic section with the same formulas through the
/// variable z, which is positive for ellipses, negative for hyperbolas and
/// zero for the parabola in between. Travel time grows monotonically with z,
/// so the right orbit can be pinned down by bisection.
///
/// The method handles trips of less than one full lap, which covers every
/// probe leg in the app.
/// </summary>
public static class Lambert
{
    /// <summary>
    /// Solves the orbit between two positions (AU) in a given time (days).
    /// The velocities are returned in AU/day at departure and at arrival.
    /// False when no orbit is found – for example when the two positions lie
    /// directly opposite each other seen from the centre, where the orbital
    /// plane can't be determined.
    /// </summary>
    public static bool Solve(Vec3 from, Vec3 to, double travelDays, double mu,
        out Vec3 departureVelocity, out Vec3 arrivalVelocity)
    {
        departureVelocity = arrivalVelocity = default;

        double r1 = from.Length, r2 = to.Length;
        if (r1 < 1e-12 || r2 < 1e-12 || travelDays <= 0 || mu <= 0)
            return false;

        double cosSweep = Math.Clamp(Vec3.Dot(from, to) / (r1 * r2), -1.0, 1.0);
        if (1.0 - cosSweep < 1e-12)
            return false;                       // same direction: no orbit

        double sweep = Math.Acos(cosSweep);

        // All probes travel prograde, i.e. counterclockwise seen from the
        // north. In the app's coordinates, Y points north, so a
        // counterclockwise lap has r1×r2 pointing up. If it points down, the
        // trip has swept more than half a turn – the "long way round".
        if (Vec3.Cross(from, to).Y < 0)
            sweep = Math.PI * 2 - sweep;

        double a = Math.Sin(sweep) * Math.Sqrt(r1 * r2 / (1.0 - cosSweep));
        if (Math.Abs(a) < 1e-14)
            return false;

        // Pin down z. Travel time grows monotonically with z: far down in
        // negative territory sit the fast hyperbolas, at zero the parabola,
        // and upward increasingly slow ellipses, until travel time goes to
        // infinity at a full lap.
        //
        // Below a certain limit there's no orbit at all – the conic section
        // simply doesn't reach, which y reveals by going negative. Travel
        // time approaches zero as that limit is neared from below, so the
        // impossible z values register as "faster than everything else".
        // That keeps the search monotonic, and bisection can't get stuck in
        // the impossible region.
        double lo = -4096.0, hi = 4.0 * Math.PI * Math.PI - 1e-9;
        // 60 bisections shrink the interval from four thousand down to well
        // below what floating point can tell apart; more laps would only
        // cost time, and this search runs hundreds of times when a launch
        // window is being found.
        for (int k = 0; k < 60; k++)
        {
            double mid = 0.5 * (lo + hi);
            if ((TimeOfFlight(mid, a, r1, r2, mu) ?? 0.0) < travelDays)
                lo = mid;
            else
                hi = mid;
        }

        double z = 0.5 * (lo + hi);
        if (YOf(z, a, r1, r2) is not { } y || y <= 0)
            return false;

        // Lagrange's f and g: they tie the two positions together with the
        // velocities.
        double f = 1.0 - y / r1;
        double g = a * Math.Sqrt(y / mu);
        double gDot = 1.0 - y / r2;
        if (Math.Abs(g) < 1e-14)
            return false;

        // The subtractions below remove nearly equal numbers, and every
        // digit matters there: computed in float, the velocity would be off
        // enough that a probe misses Jupiter by a couple of planet radii
        // after a two-year flight. So they're kept in double precision all
        // the way out of the solver too – whatever builds the orbit from
        // them needs every digit.
        departureVelocity = Combine(to, 1.0 / g, from, -f / g);
        arrivalVelocity = Combine(to, gDot / g, from, -1.0 / g);
        return true;
    }

    /// <summary>Computes u·a + v·b component-wise.</summary>
    static Vec3 Combine(Vec3 a, double u, Vec3 b, double v) => new(
        a.X * u + b.X * v,
        a.Y * u + b.Y * v,
        a.Z * u + b.Z * v);

    /// <summary>
    /// The helper quantity y: how far the conic section reaches for a given
    /// z. A negative y means the orbit doesn't reach, in which case there is
    /// no travel time.
    /// </summary>
    static double? YOf(double z, double a, double r1, double r2)
    {
        double c = StumpffC(z);
        if (c <= 0)
            return null;

        double y = r1 + r2 + a * (z * StumpffS(z) - 1.0) / Math.Sqrt(c);
        return y > 0 ? y : null;
    }

    /// <summary>Travel time in days for a given z, or null when the orbit doesn't reach.</summary>
    static double? TimeOfFlight(double z, double a, double r1, double r2, double mu)
    {
        if (YOf(z, a, r1, r2) is not { } y)
            return null;

        double x = Math.Sqrt(y / StumpffC(z));
        return (x * x * x * StumpffS(z) + a * Math.Sqrt(y)) / Math.Sqrt(mu);
    }

    // Stumpff's functions C and S. They're the same series expansion
    // throughout, but written with cosine and sine for ellipses (z > 0),
    // with hyperbolic functions for hyperbolas (z < 0), and as a series near
    // zero, where both forms would otherwise be zero divided by zero.
    static double StumpffC(double z)
    {
        if (z > 1e-6)
        {
            double s = Math.Sqrt(z);
            return (1.0 - Math.Cos(s)) / z;
        }
        if (z < -1e-6)
        {
            double s = Math.Sqrt(-z);
            return (Math.Cosh(s) - 1.0) / -z;
        }
        return 0.5 - z / 24.0 + z * z / 720.0;
    }

    static double StumpffS(double z)
    {
        if (z > 1e-6)
        {
            double s = Math.Sqrt(z);
            return (s - Math.Sin(s)) / (s * s * s);
        }
        if (z < -1e-6)
        {
            double s = Math.Sqrt(-z);
            return (Math.Sinh(s) - s) / (s * s * s);
        }
        return 1.0 / 6.0 - z / 120.0 + z * z / 5040.0;
    }
}
