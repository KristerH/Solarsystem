using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// A body's rotation axis and its rotation around it.
///
/// The axis is described the same way as an orbital plane: the inclination
/// to the ecliptic and the ascending node's longitude. That's deliberate –
/// the planet's equatorial plane is exactly the plane its regular moons and
/// rings lie in, so the same two numbers describe both where the pole points
/// and how the moons' orbits are tilted.
///
/// <b>The north pole follows the right-hand rule</b>, i.e. the direction the
/// axis points when the body spins counterclockwise seen from above. For
/// Venus, Uranus and Pluto it points south of the ecliptic, and that's
/// exactly what it means for them to rotate backwards: the inclination comes
/// out greater than 90 degrees. The alternative would be to always let the
/// pole point north and instead give them a negative rotation period, but
/// then the moons' orbital planes would have needed flipping before they
/// could be used. As it stands, Miranda can read Uranus's axis directly.
/// </summary>
/// <param name="InclinationDeg">
/// The equatorial plane's inclination to the ecliptic. Above 90 degrees means
/// retrograde rotation. Note that this isn't the same as the axial tilt
/// usually quoted in tables – that's measured against the body's own orbit.
/// Mercury stands upright relative to its orbit (0.03°) but tilts 7.0° to the
/// ecliptic, because the orbit itself tilts 7.0°.
/// </param>
/// <param name="NodeDeg">
/// The longitude of the equator's ascending node on the ecliptic, i.e. which
/// way the body leans.
/// </param>
/// <param name="RotationDays">
/// One turn around the axis, in days. Always positive: the direction is
/// already encoded in the pole's position.
/// </param>
/// <param name="PrimeMeridianDeg">
/// The prime meridian's angle from the ascending node at epoch J2000,
/// measured the same way the body spins. This number determines which side
/// faces the Sun on a given date.
/// </param>
public sealed record BodyAxis(
    double InclinationDeg,
    double NodeDeg,
    double RotationDays,
    double PrimeMeridianDeg)
{
    /// <summary>
    /// How much slower the surface turns the farther from the equator you
    /// get, in degrees per day. Zero – the default – means the body rotates
    /// as one piece, which everything solid does.
    ///
    /// The Sun is the only body in the app where this number isn't zero, and
    /// that's a discovery worth something in itself: a body that rotates at
    /// different speeds at different latitudes cannot possibly be solid. The
    /// rate follows <c>ω(φ) = A + B·sin²φ</c>, where A is the equator's rate
    /// (360 degrees divided by <see cref="RotationDays"/>) and B is this
    /// number.
    /// </summary>
    public double DifferentialDegPerDay { get; init; }

    readonly double _sinI = Math.Sin(InclinationDeg * Math.PI / 180.0);
    readonly double _cosI = Math.Cos(InclinationDeg * Math.PI / 180.0);
    readonly double _sinNode = Math.Sin(NodeDeg * Math.PI / 180.0);
    readonly double _cosNode = Math.Cos(NodeDeg * Math.PI / 180.0);

    /// <summary>The rotation axis in world coordinates (Y = north of the ecliptic).</summary>
    public Vector3 NorthPole => new(
        (float)(_sinI * _sinNode), (float)_cosI, (float)(_sinI * _cosNode));

    /// <summary>
    /// The point on the equator at the ascending node. By definition it lies
    /// in the equatorial plane and perpendicular to the axis, so it works as
    /// the zero direction when the surface is drawn – and as the first basis
    /// vector for the rings.
    /// </summary>
    public Vector3 NodeAxis => new((float)_cosNode, 0f, (float)(-_sinNode));

    /// <summary>The equatorial plane's second basis vector, a quarter turn east of the node.</summary>
    public Vector3 EastAxis => new(
        (float)(-_cosI * _sinNode), (float)_sinI, (float)(-_cosI * _cosNode));

    /// <summary>
    /// How far the prime meridian has turned from the node at a given time,
    /// in radians. The angle isn't wrapped down to one turn: Jupiter racks up
    /// ninety million degrees in a century, which a double easily carries
    /// with margin to spare.
    /// </summary>
    public double SpinRadians(double daysSinceJ2000)
        => (PrimeMeridianDeg + 360.0 * daysSinceJ2000 / RotationDays) * Math.PI / 180.0;

    /// <summary>
    /// The rotation at a given latitude. Same result as the overload above
    /// for anything that rotates as one piece; for the Sun, this is the one
    /// that applies.
    /// </summary>
    public double SpinRadians(double daysSinceJ2000, double sinLat)
    {
        if (DifferentialDegPerDay == 0.0)
            return SpinRadians(daysSinceJ2000);

        double rate = 360.0 / RotationDays + DifferentialDegPerDay * sinLat * sinLat;
        return (PrimeMeridianDeg + rate * daysSinceJ2000) * Math.PI / 180.0;
    }

    /// <summary>
    /// The direction from the body's centre out to a point on the surface,
    /// expressed in world coordinates. Longitude is measured the way the
    /// surface turns, which for every prograde-rotating body is the same as
    /// east longitude.
    /// </summary>
    public Vector3 Direction(double sinLat, double cosLat, double lonRad, double spinRad)
    {
        double a = spinRad + lonRad;
        double u = cosLat * Math.Cos(a);   // along the node
        double v = cosLat * Math.Sin(a);   // eastward
        return new Vector3(
            (float)(u * _cosNode - v * _cosI * _sinNode + sinLat * _sinI * _sinNode),
            (float)(v * _sinI + sinLat * _cosI),
            (float)(-u * _sinNode - v * _cosI * _cosNode + sinLat * _sinI * _cosNode));
    }
}
