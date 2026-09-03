namespace Solarsystem.Simulation;

/// <summary>
/// A double-precision vector, for the computations where <c>Vector3</c> isn't
/// enough.
///
/// Rendering does fine with single precision – a pixel is many thousand
/// kilometres – but building an orbit doesn't. When an orbit is built from a
/// state, the energy is computed as 2/r − v²/µ, and for a near-parabolic
/// orbit the two terms are nearly equal in size. The difference between them
/// then becomes small relative to the numbers themselves, and any missing
/// digit in the input is magnified a hundredfold in the answer. Voyager's and
/// Pioneer's legs sit right in that regime.
///
/// Only what the orbit math needs lives here. Everything else is still
/// handled by <c>Vector3</c> as before.
/// </summary>
public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 v, double k) => new(v.X * k, v.Y * k, v.Z * k);
    public static Vec3 operator /(Vec3 v, double k) => new(v.X / k, v.Y / k, v.Z / k);
    public static Vec3 operator -(Vec3 v) => new(-v.X, -v.Y, -v.Z);

    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => Math.Sqrt(LengthSquared);

    public static double Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vec3 Cross(Vec3 a, Vec3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public Vec3 Normalized()
    {
        double n = Length;
        return n > 0 ? this / n : this;
    }

    /// <summary>Down to single precision, for rendering.</summary>
    public System.Numerics.Vector3 ToVector3() => new((float)X, (float)Y, (float)Z);

    public static Vec3 From(System.Numerics.Vector3 v) => new(v.X, v.Y, v.Z);
}
