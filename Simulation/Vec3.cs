namespace Solarsystem.Simulation;

/// <summary>
/// En vektor i dubbel precision, för de räkningar där <c>Vector3</c> inte
/// räcker till.
///
/// Ritningen klarar sig gott med enkel precision – en pixel är många tusen
/// kilometer – men banbyggandet gör det inte. När en bana byggs ur ett
/// tillstånd räknas energin som 2/r − v²/µ, och för en nästan parabolisk bana
/// är de två termerna nästan lika stora. Skillnaden mellan dem blir då liten
/// mot talen själva, och varje siffra som saknas i indata förstoras hundrafalt
/// i svaret. Voyagers och Pioneers ben ligger just där.
///
/// Bara det som banmatematiken behöver finns här. Allt annat sköts av
/// <c>Vector3</c> som förut.
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

    /// <summary>Ned till enkel precision, för ritning.</summary>
    public System.Numerics.Vector3 ToVector3() => new((float)X, (float)Y, (float)Z);

    public static Vec3 From(System.Numerics.Vector3 v) => new(v.X, v.Y, v.Z);
}
