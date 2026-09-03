using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// A conic section around a central body, built from a state: a position and
/// a velocity at a given point in time.
///
/// The planets are described by fixed orbital elements that never change,
/// but a spacecraft switches orbit at every planetary flyby. There, the
/// state is the natural way to describe the trip: the velocity right after
/// the flyby determines the entire following orbit.
///
/// The class handles both ellipses and hyperbolas. The difference lies in
/// the speed. Below the local escape velocity, the orbit is a closed
/// ellipse; above it, an open hyperbola, and the body never comes back. In
/// the formulas this shows up as the semi-major axis going negative – which
/// is exactly what they need to keep holding.
///
/// Everything is computed in AU and days, with the gravitational parameter µ
/// in AU³/day².
/// </summary>
public sealed class Conic
{
    /// <summary>Semi-major axis in AU. Negative for hyperbolas.</summary>
    public double SemiMajorAu { get; }

    /// <summary>Eccentricity. Above 1 means the orbit is open.</summary>
    public double Eccentricity { get; }

    /// <summary>True when the orbit is open and the body never comes back.</summary>
    public bool IsHyperbolic => Eccentricity > 1.0;

    /// <summary>Closest point to the central body, in AU.</summary>
    public double PeriapsisAu => SemiMajorAu * (1.0 - Eccentricity);

    /// <summary>The central body's gravitational parameter in AU³/day².</summary>
    public double Mu { get; }

    /// <summary>
    /// The orbital period in days, or null for hyperbolas – they don't have one.
    /// </summary>
    public double? PeriodDays => IsHyperbolic
        ? null
        : 2.0 * Math.PI * Math.Sqrt(Math.Pow(SemiMajorAu, 3) / Mu);

    // The orbital plane's basis: the direction to periapsis, and the
    // perpendicular direction the body moves when passing through it.
    readonly Vector3 _periDir;
    readonly Vector3 _sideDir;

    readonly double _epochDay;
    readonly double _meanAnomalyAtEpoch;
    readonly double _meanMotion;         // radians per day

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
    /// Builds the orbit from a state: the position in AU and the velocity in
    /// AU/day at the given point in time. Returns null for states that don't
    /// describe any orbit at all, for example a position right at the
    /// centre.
    ///
    /// The approach is the classical one: the energy gives the semi-major
    /// axis, the angular momentum r×v gives the orbital plane, and the
    /// eccentricity vector gives both the orbit's shape and which direction
    /// periapsis lies in.
    /// </summary>
    public static Conic? FromState(Vec3 positionAu, Vec3 velocityAuPerDay,
        double epochDay, double mu)
    {
        double r = positionAu.Length;
        double v2 = velocityAuPerDay.LengthSquared;
        if (r < 1e-12 || mu <= 0)
            return null;

        // Vis-viva backward: the energy determines the semi-major axis. The
        // expression goes negative once speed exceeds escape velocity, and
        // then the orbit is a hyperbola.
        double alpha = 2.0 / r - v2 / mu;      // = 1/a
        if (Math.Abs(alpha) < 1e-12)
            return null;                        // parabola – infinitely long orbit
        double a = 1.0 / alpha;

        var h = Vec3.Cross(positionAu, velocityAuPerDay);
        if (h.Length < 1e-14)
            return null;                        // straight into the centre, no orbit

        // The eccentricity vector points toward periapsis and has the
        // orbit's eccentricity as its length.
        var eVec = Vec3.Cross(velocityAuPerDay, h) / mu - positionAu / r;
        double e = eVec.Length;
        if (double.IsNaN(e) || e >= 1.0 && a > 0)
            return null;                        // contradictory state

        var periVec = e > 1e-8
            ? eVec.Normalized()
            : positionAu.Normalized();          // circular orbit: use the position as the reference
        var sideVec = Vec3.Cross(h.Normalized(), periVec).Normalized();

        // The true anomaly at the epoch: the angle from periapsis, measured
        // in the orbital plane. Computed in double precision; only afterward
        // do the directions drop to single, where they're only used to point
        // out the orbital plane when rendering.
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
            // The same conversion as for the ellipse, but with hyperbolic
            // functions. The argument to artanh stays under 1 as long as the
            // body is inside the asymptotes, which is always.
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
    /// Builds the orbit from its periapsis instead: the direction to it, the
    /// direction the body moves when passing through it, the distance and
    /// the eccentricity. The time given is the periapsis passage itself.
    ///
    /// This entry point is exact. FromState has to compute the energy as a
    /// small difference between two large numbers and loses precision doing
    /// so; here the orbit is already given in the quantities the formulas
    /// need.
    /// </summary>
    public static Conic? FromPeriapsis(Vector3 periapsisDir, Vector3 motionDir,
        double periapsisAu, double eccentricity, double periapsisDay, double mu)
    {
        if (periapsisAu <= 0 || mu <= 0 || Math.Abs(eccentricity - 1.0) < 1e-9)
            return null;                        // the parabola has no semi-major axis

        // Negative for hyperbolas, which is exactly what the formulas want.
        double a = periapsisAu / (1.0 - eccentricity);
        double meanMotion = Math.Sqrt(mu / Math.Abs(a * a * a));

        return new Conic(a, eccentricity, mu,
            Vector3.Normalize(periapsisDir), Vector3.Normalize(motionDir),
            periapsisDay, 0.0, meanMotion);
    }

    /// <summary>
    /// Builds the orbit from classical orbital elements instead, the same
    /// way the planets are described. Used for probes orbiting a planet,
    /// where the orbit's shape is known but not any particular state.
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

        // The same orbital-plane-to-ecliptic rotation that CelestialBody
        // uses, and the same conversion to the app's coordinates (Y = north
        // of the ecliptic).
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

    /// <summary>The position at a given time, relative to the central body.</summary>
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
            // The semi-major axis is negative here, which makes x come out
            // positive at periapsis and y grow in the direction of motion –
            // exactly as in the elliptical case.
            double hyp = Kepler.Hyperbolic(m, e);
            x = SemiMajorAu * (Math.Cosh(hyp) - e);
            y = -SemiMajorAu * Math.Sqrt(e * e - 1.0) * Math.Sinh(hyp);
        }

        return (_periDir * (float)x + _sideDir * (float)y) * unitsPerAu;
    }

    /// <summary>The distance to the central body in AU at a given time.</summary>
    public double DistanceAu(double day) => PositionAt(day, 1f).Length();

    /// <summary>
    /// The speed in km/s at a given time, from the vis-viva equation. The
    /// expression holds unchanged for hyperbolas: there, the semi-major axis
    /// is negative, so -1/a becomes a positive addition. That addition is
    /// exactly the excess hyperbolic energy – the speed the probe still has
    /// left once it's travelled infinitely far away.
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
