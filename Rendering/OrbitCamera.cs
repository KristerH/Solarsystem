using System.Numerics;

namespace Solarsystem.Rendering;

/// <summary>
/// A camera that orbits a target point (yaw/pitch/distance) and projects
/// world coordinates to screen coordinates with perspective.
/// </summary>
public sealed class OrbitCamera
{
    /// <summary>
    /// The absolute closest the camera can get. The limit is set by floating
    /// point, not by any body: world coordinates are single precision and
    /// reach 2,400 units out at Neptune, where the gap between two adjacent
    /// numbers is a few ten-thousandths of a unit. Closer than that has no
    /// meaning.
    /// </summary>
    public const float AbsoluteMinDistance = 1e-3f;
    // 40,000 units is 666 AU. The spacecraft sit between 65 and 170 AU, and
    // to see one of them with the Sun still in frame the camera has to back
    // off a couple of times the probe's distance – hence the headroom.
    public const float MaxDistance = 40_000f;
    public const float MaxPitch = 1.55f;

    /// <summary>The camera distance in the overview, which "Reset view" returns to.</summary>
    public const float DefaultDistance = 900f;

    public float Yaw { get; set; }
    public float Pitch { get; set; }

    /// <summary>
    /// How close the camera is allowed to get to the target. Has to be
    /// changeable at runtime rather than a constant: in magnified mode Earth
    /// has a radius of 2.6 units, in real scale 0.0026. A fixed floor that
    /// suits one keeps the camera a thousand Earth radii away in the other,
    /// where nothing of the surface would show.
    ///
    /// Whatever picks what the camera is looking at also sets this limit,
    /// since that's where the body's size is known. The distance is clamped
    /// immediately when the limit changes, so the camera never ends up stuck
    /// inside a planet after a focus change.
    /// </summary>
    public float MinDistance
    {
        get => _minDistance;
        set
        {
            _minDistance = MathF.Max(value, AbsoluteMinDistance);
            Distance = _distance;
        }
    }

    /// <summary>Distance to the target. Always kept within its limits.</summary>
    public float Distance
    {
        get => _distance;
        set => _distance = Math.Clamp(value, _minDistance, MaxDistance);
    }

    public Vector3 Target { get; set; }
    public float VerticalFovDeg { get; set; } = 50f;

    Vector3 _pos, _right, _up, _fwd;
    float _focal, _cx, _cy;
    float _distance = DefaultDistance;
    float _minDistance = AbsoluteMinDistance;

    public Vector3 Position => _pos;

    /// <summary>Focal length in pixels for the most recent frame (set in UpdateFrame).</summary>
    public float Focal => _focal;

    /// <summary>The camera's right direction in world coordinates (most recent frame).</summary>
    public Vector3 RightAxis => _right;

    /// <summary>The camera's up direction in world coordinates (most recent frame).</summary>
    public Vector3 UpAxis => _up;

    public OrbitCamera() => ResetView();

    public void ResetView()
    {
        Yaw = 0.6f;
        Pitch = 0.55f;
        Distance = DefaultDistance;
        Target = Vector3.Zero;
    }

    public void ZoomBy(float factor) => Distance *= factor;

    public void Rotate(float dYaw, float dPitch)
    {
        Yaw += dYaw;
        Pitch = Math.Clamp(Pitch + dPitch, -MaxPitch, MaxPitch);
    }

    /// <summary>Recomputes the camera basis and focal length ahead of a new frame.</summary>
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
            right = Vector3.UnitX; // straight from above or below
        _right = Vector3.Normalize(right);
        _up = Vector3.Cross(_right, _fwd);

        _cx = width * 0.5f;
        _cy = height * 0.5f;
        _focal = (height * 0.5f) / MathF.Tan(VerticalFovDeg * MathF.PI / 360f);
    }

    /// <summary>Projects a world point. False if the point is behind the camera.</summary>
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

    /// <summary>Projects a direction "at infinite distance" (for the night sky).</summary>
    public bool ProjectDirection(Vector3 dir, out float sx, out float sy)
    {
        float depth = Vector3.Dot(dir, _fwd);
        if (depth < 0.01f) { sx = sy = 0; return false; }
        float inv = _focal / depth;
        sx = _cx + Vector3.Dot(dir, _right) * inv;
        sy = _cy - Vector3.Dot(dir, _up) * inv;
        return true;
    }

    /// <summary>Apparent radius in pixels for a sphere with a given world radius at a given depth.</summary>
    public float ScreenRadius(float worldRadius, float depth) => worldRadius * _focal / depth;
}
