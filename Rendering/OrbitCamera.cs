using System.Numerics;

namespace Solarsystem.Rendering;

/// <summary>
/// Kamera som kretsar kring en målpunkt (yaw/pitch/avstånd) och
/// projicerar världskoordinater till skärmkoordinater med perspektiv.
/// </summary>
public sealed class OrbitCamera
{
    public const float MinDistance = 1.5f;
    // 40 000 enheter är 666 AU. Rymdsonderna ligger på 65 till 170 AU, och för
    // att se en av dem med solen kvar i bild måste kameran backa ett par gånger
    // sondens avstånd – därav takhöjden.
    public const float MaxDistance = 40_000f;
    public const float MaxPitch = 1.55f;

    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public float Distance { get; set; }
    public Vector3 Target { get; set; }
    public float VerticalFovDeg { get; set; } = 50f;

    Vector3 _pos, _right, _up, _fwd;
    float _focal, _cx, _cy;

    public Vector3 Position => _pos;

    /// <summary>Fokallängd i pixlar för senaste bildrutan (sätts i UpdateFrame).</summary>
    public float Focal => _focal;

    /// <summary>Kamerans högerriktning i världskoordinater (senaste bildrutan).</summary>
    public Vector3 RightAxis => _right;

    /// <summary>Kamerans uppriktning i världskoordinater (senaste bildrutan).</summary>
    public Vector3 UpAxis => _up;

    public OrbitCamera() => ResetView();

    public void ResetView()
    {
        Yaw = 0.6f;
        Pitch = 0.55f;
        Distance = 900f;
        Target = Vector3.Zero;
    }

    public void ZoomBy(float factor) =>
        Distance = Math.Clamp(Distance * factor, MinDistance, MaxDistance);

    public void Rotate(float dYaw, float dPitch)
    {
        Yaw += dYaw;
        Pitch = Math.Clamp(Pitch + dPitch, -MaxPitch, MaxPitch);
    }

    /// <summary>Räknar om kamerabas och fokallängd inför en ny bildruta.</summary>
    public void UpdateFrame(float width, float height)
    {
        float cp = MathF.Cos(Pitch);
        _pos = Target + Distance * new Vector3(
            cp * MathF.Sin(Yaw),
            MathF.Sin(Pitch),
            cp * MathF.Cos(Yaw));

        _fwd = Vector3.Normalize(Target - _pos);
        var worldUp = Vector3.UnitY;
        var right = Vector3.Cross(_fwd, worldUp);
        if (right.LengthSquared() < 1e-8f)
            right = Vector3.UnitX; // rakt uppifrån/nerifrån
        _right = Vector3.Normalize(right);
        _up = Vector3.Cross(_right, _fwd);

        _cx = width * 0.5f;
        _cy = height * 0.5f;
        _focal = (height * 0.5f) / MathF.Tan(VerticalFovDeg * MathF.PI / 360f);
    }

    /// <summary>Projicerar en världspunkt. Falskt om punkten ligger bakom kameran.</summary>
    public bool Project(Vector3 world, out float sx, out float sy, out float depth)
    {
        var v = world - _pos;
        depth = Vector3.Dot(v, _fwd);
        if (depth < 0.05f) { sx = sy = 0; return false; }
        float inv = _focal / depth;
        sx = _cx + Vector3.Dot(v, _right) * inv;
        sy = _cy - Vector3.Dot(v, _up) * inv;
        return true;
    }

    /// <summary>Projicerar en riktning "på oändligt avstånd" (för stjärnhimlen).</summary>
    public bool ProjectDirection(Vector3 dir, out float sx, out float sy)
    {
        float depth = Vector3.Dot(dir, _fwd);
        if (depth < 0.01f) { sx = sy = 0; return false; }
        float inv = _focal / depth;
        sx = _cx + Vector3.Dot(dir, _right) * inv;
        sy = _cy - Vector3.Dot(dir, _up) * inv;
        return true;
    }

    /// <summary>Skenbar radie i pixlar för en sfär med given världsradie på givet djup.</summary>
    public float ScreenRadius(float worldRadius, float depth) => worldRadius * _focal / depth;
}
