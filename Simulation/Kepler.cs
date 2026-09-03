namespace Solarsystem.Simulation;

/// <summary>
/// Kepler's equation, in its two forms.
///
/// Planets, moons and transfer orbits travel in ellipses, where
/// E - e·sin E = M holds. Spacecraft picked up enough speed at their planetary
/// flybys that they never come back: their orbits are hyperbolas, where
/// instead e·sinh H - H = M holds. It's the same equation with the circular
/// functions swapped for hyperbolic ones, and the eccentric anomaly E
/// replaced by its hyperbolic counterpart H.
///
/// Neither can be solved by hand – that's exactly what made Kepler's equation
/// famous – so both are solved numerically.
/// </summary>
public static class Kepler
{
    /// <summary>
    /// The elliptical case, solved by bisection. Transfer orbits can reach an
    /// eccentricity of up to about 0.97, and there Newton's method can run
    /// away since the denominator 1 - e·cos E approaches zero near perihelion.
    /// Bisection takes more iterations but can't miss: the solution always
    /// lies between M and M + e.
    /// </summary>
    public static double Elliptic(double meanAnomaly, double e)
    {
        double m = meanAnomaly % (Math.PI * 2);
        if (m < 0)
            m += Math.PI * 2;

        // The equation is mirror-symmetric around M = π, so the second half of
        // the lap is solved the same way as the first and mirrored back.
        bool mirrored = m > Math.PI;
        if (mirrored)
            m = Math.PI * 2 - m;

        double lo = m, hi = m + e;
        for (int k = 0; k < 40; k++)
        {
            double mid = 0.5 * (lo + hi);
            if (mid - e * Math.Sin(mid) < m)
                lo = mid;
            else
                hi = mid;
        }

        double ecc = 0.5 * (lo + hi);
        return mirrored ? Math.PI * 2 - ecc : ecc;
    }

    /// <summary>
    /// The hyperbolic case: e·sinh H - H = M. Here Newton's method works well,
    /// because the derivative e·cosh H - 1 is always at least e - 1 and can
    /// never approach zero when e is greater than 1.
    ///
    /// The starting guess is arsinh(M/e), chosen with care. Since
    /// e·sinh H = M + H, sinh H is always greater than M/e, so the guess is
    /// guaranteed to sit below the answer – and since the function is
    /// increasing and convex, Newton then approaches the solution from below
    /// without ever overshooting. The obvious guess M/(e - 1) doesn't work:
    /// for orbits near parabolic, where e is just above 1, it becomes huge,
    /// and sinh of a large number blows up the floating point immediately.
    /// </summary>
    public static double Hyperbolic(double meanAnomaly, double e)
    {
        double m = Math.Abs(meanAnomaly);
        double h = Math.Asinh(m / e);

        for (int k = 0; k < 60; k++)
        {
            double step = (e * Math.Sinh(h) - h - m) / (e * Math.Cosh(h) - 1.0);
            if (double.IsNaN(step) || double.IsInfinity(step))
                break;
            h -= step;
            if (Math.Abs(step) < 1e-13)
                break;
        }

        // The equation is odd: H(-M) = -H(M). Before perihelion everything is
        // therefore mirrored, which is exactly what's needed to compute
        // backward in time.
        return meanAnomaly < 0 ? -h : h;
    }
}
