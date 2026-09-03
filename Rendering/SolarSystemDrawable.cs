using System.Globalization;
using System.Numerics;
using Solarsystem.Simulation;

namespace Solarsystem.Rendering;

/// <summary>
/// Draws the whole scene: the night sky, orbits, the Sun, planets
/// (depth-sorted, shaded against the Sun), Saturn's rings, and name labels.
/// </summary>
public sealed class SolarSystemDrawable : IDrawable
{
    /// <summary>World scale: 1 AU = 60 units. Distances are always to scale.</summary>
    public const float UnitsPerAu = 60f;

    // At fully real scale the planets are smaller than a pixel, so they're
    // enlarged (still to scale relative to each other). The Sun is enlarged
    // less, so it doesn't swallow Mercury's orbit.
    const float PlanetBoost = 1000f;
    const float SunBoost = 30f;

    const int OrbitSamples = 240;

    public OrbitCamera Camera { get; } = new();
    public double DaysSinceJ2000 { get; set; }
    public bool ShowOrbits { get; set; } = true;
    public bool RealScale { get; set; }
    public bool ShowConstellations { get; set; } = true;
    public bool ShowStarNames { get; set; }

    /// <summary>Whether the planets' moons should be drawn at all.</summary>
    public bool ShowMoons { get; set; } = true;

    /// <summary>Whether the asteroid belt between Mars and Jupiter should be drawn.</summary>
    public bool ShowAsteroidBelt { get; set; }

    /// <summary>The ongoing space mission, or null when no craft is under way.</summary>
    public Mission? Mission { get; set; }

    /// <summary>Whether the Kuiper belt beyond Neptune should be drawn.</summary>
    public bool ShowKuiperBelt { get; set; }

    /// <summary>
    /// The names of the spacecraft that should be drawn – both the five that
    /// left the Solar System and the two orbiting a planet. Probes not in
    /// the set aren't drawn at all: no dot, trail or milestones. An empty
    /// set turns everything off.
    ///
    /// The names work as a key since they're unique and already used to
    /// identify bodies elsewhere in the app, for instance in the focus
    /// selector.
    ///
    /// Empty from the start: the probes are a deeper layer and their trails
    /// cross the whole view, so the overview should stay clean until someone
    /// checks them in the selector.
    /// </summary>
    public HashSet<string> VisibleProbes { get; } = [];

    /// <summary>
    /// Draws Halley's Comet with its orbit and its tails. Off by default:
    /// it's absent 74 years out of 75, and its orbit is so elongated that it
    /// obscures the planets' orbits when left in the picture.
    /// </summary>
    public bool ShowHalley { get; set; }

    /// <summary>
    /// Draws the Moon's orbit against the ecliptic with the nodes marked.
    /// Off by default – it's a deeper layer and not something that belongs
    /// in the overview.
    /// </summary>
    public bool ShowMoonOrbit { get; set; }

    /// <summary>
    /// The probe selected in the focus selector, or null. Its milestones are
    /// printed with planet name and date; the others' are marked with just
    /// the year, otherwise the view fills up with text.
    /// </summary>
    public Probe? FocusedProbe { get; set; }

    /// <summary>
    /// True while the window is being resized. Then only black is drawn –
    /// the platform redraws on every resize step, and reprojecting the
    /// whole scene for every such step is what would otherwise freeze the
    /// window manager.
    /// </summary>
    public bool Suspended { get; set; }

    /// <summary>How many stars are drawn (a setting in the app).</summary>
    public StarDensity StarDensity
    {
        get => _sky.Density;
        set => _sky.Density = value;
    }

    readonly StarSky _sky = new();

    // The belt is built only when requested, so the app starts just as fast
    // as before for anyone who never turns it on.
    SmallBodyBelt? _asteroids, _kuiper;
    Vector3[]? _asteroidPositions, _kuiperPositions;
    double _asteroidTime = double.NaN, _kuiperTime = double.NaN;

    // Asteroids are rocky and grey-brown; Kuiper bodies are icy and cooler in tone.
    static readonly Color AsteroidColor = Color.FromArgb("#B4A794");
    static readonly Color KuiperColor = Color.FromArgb("#A9BCC8");
    static readonly Color CeresLabelColor = Color.FromRgba(0.82f, 0.78f, 0.72f, 0.85f);
    Vector3[][]? _orbitPaths;
    readonly List<(string Name, float X, float Y)> _labels = new(16);

    // Cache of the orbits' screen shapes – valid until the camera moves.
    float _orbYaw = float.NaN, _orbPitch, _orbDist, _orbW, _orbH;
    Vector3 _orbTarget;
    PathF[] _orbitScreenPaths = [];
    static readonly Color[] OrbitColors =
        [.. SolarSystemData.Planets.Select(p => p.BodyColor.WithAlpha(0.35f))];

    public void Draw(ICanvas canvas, RectF rect)
    {
        try
        {
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(rect);
            // A negated comparison so NaN dimensions (mid-resize) are also
            // caught here.
            if (Suspended || !(rect.Width >= 10 && rect.Height >= 10))
                return;

            Camera.UpdateFrame(rect.Width, rect.Height);
            _labels.Clear();
            _milestoneText.Clear();

            _sky.Draw(canvas, Camera, rect, ShowConstellations, ShowStarNames);
            DrawHeliopause(canvas, rect);
            if (ShowOrbits)
                DrawOrbits(canvas, rect);
            if (ShowHalley && ShowOrbits)
                DrawHalleyOrbit(canvas);
            if (ShowKuiperBelt)
            {
                _kuiper ??= SmallBodyBelt.CreateKuiperBelt(KuiperCount, UnitsPerAu);
                _kuiperPositions ??= new Vector3[_kuiper.Bodies.Length];
                DrawBelt(canvas, rect, _kuiper, _kuiperPositions, KuiperColor,
                    ref _kuiperTime, 1.2f);
            }
            if (ShowAsteroidBelt)
                DrawAsteroidBelt(canvas, rect);
            if (ShowMoonOrbit)
                DrawMoonOrbit(canvas, DaysSinceJ2000);
            DrawBodies(canvas, rect);
            if (ShowHalley)
                DrawHalley(canvas);
            if (VisibleProbes.Count > 0)
            {
                DrawProbes(canvas);
                DrawOrbiters(canvas);
            }
            if (Mission is not null)
                DrawMission(canvas, Mission);
            DrawLabels(canvas, rect);
        }
        catch (Exception ex)
        {
            Diagnostics.Log($"draw error: {ex}\n");
            throw;
        }
    }

    // ------------------------------------------------------------------- orbits

    void DrawOrbits(ICanvas canvas, RectF rect)
    {
        _orbitPaths ??= [.. SolarSystemData.Planets
            .Select(p => p.OrbitPath(OrbitSamples, UnitsPerAu))];

        // The orbits sit still in the world, so their screen shapes only
        // need rebuilding when the camera actually moves.
        if (Camera.Yaw != _orbYaw || Camera.Pitch != _orbPitch ||
            Camera.Distance != _orbDist || Camera.Target != _orbTarget ||
            rect.Width != _orbW || rect.Height != _orbH)
        {
            _orbYaw = Camera.Yaw;
            _orbPitch = Camera.Pitch;
            _orbDist = Camera.Distance;
            _orbTarget = Camera.Target;
            _orbW = rect.Width;
            _orbH = rect.Height;

            if (_orbitScreenPaths.Length != _orbitPaths.Length)
                _orbitScreenPaths = new PathF[_orbitPaths.Length];

            for (int i = 0; i < _orbitPaths.Length; i++)
            {
                var pts = _orbitPaths[i];
                var path = new PathF();
                // A subpath may only start once at least two points in a row
                // are visible – a lone MoveTo with no LineTo is invalid in Win2D.
                bool started = false, hasPrev = false;
                float px = 0, py = 0;
                for (int k = 0; k <= pts.Length; k++)
                {
                    var p = pts[k % pts.Length];
                    if (Camera.Project(p, out float sx, out float sy, out _))
                    {
                        if (hasPrev)
                        {
                            if (!started) { path.MoveTo(px, py); started = true; }
                            path.LineTo(sx, sy);
                        }
                        hasPrev = true;
                        px = sx;
                        py = sy;
                    }
                    else
                    {
                        hasPrev = false;
                        started = false;
                    }
                }
                _orbitScreenPaths[i] = path;
            }
        }

        canvas.StrokeSize = 1f;
        for (int i = 0; i < _orbitScreenPaths.Length; i++)
        {
            canvas.StrokeColor = OrbitColors[i];
            canvas.DrawPath(_orbitScreenPaths[i]);
        }
    }

    // ------------------------------------------------------------- the belts

    /// <summary>How many bodies are drawn in each belt.</summary>
    const int AsteroidCount = 1400;
    const int KuiperCount = 1100;

    void DrawAsteroidBelt(ICanvas canvas, RectF rect)
    {
        _asteroids ??= SmallBodyBelt.CreateAsteroidBelt(AsteroidCount, UnitsPerAu);
        _asteroidPositions ??= new Vector3[_asteroids.Bodies.Length];
        DrawBelt(canvas, rect, _asteroids, _asteroidPositions, AsteroidColor,
            ref _asteroidTime, 1.1f);

        // Ceres is so much larger than everything else in the belt that it gets a name.
        var ceres = SolarSystemData.Ceres.PositionAt(DaysSinceJ2000, UnitsPerAu);
        if (Camera.Project(ceres, out float cx, out float cy, out _))
        {
            canvas.FillColor = SolarSystemData.Ceres.BodyColor;
            canvas.FillCircle(cx, cy, 2.6f);
            canvas.FontSize = 11f;
            canvas.FontColor = CeresLabelColor;
            canvas.DrawString(Strings.Name("Ceres"), cx, cy + 16f, HorizontalAlignment.Center);
        }
    }

    /// <summary>
    /// Draws a belt as a fine dust of dots. Each body gets its position from
    /// its own Kepler orbit, so the belt rotates with the inner laps faster
    /// than the outer, exactly as in reality. Dots outside the screen edge
    /// are skipped.
    /// </summary>
    void DrawBelt(ICanvas canvas, RectF rect, SmallBodyBelt belt, Vector3[] positions,
        Color colour, ref double cachedTime, float dotRadius)
    {
        // The bodies creep forward in their orbits – one lap takes years to
        // centuries. The positions therefore don't need to be solved from
        // Kepler's equation every frame, only once the motion has become a
        // pixel on screen. The tolerance follows both the zoom and how fast
        // this particular belt moves.
        float tolerance = MathF.Max(0.02f, Camera.Distance / (belt.DriftPerDay * Camera.Focal));
        if (double.IsNaN(cachedTime) || Math.Abs(DaysSinceJ2000 - cachedTime) > tolerance)
        {
            cachedTime = DaysSinceJ2000;
            var bodies = belt.Bodies;
            for (int i = 0; i < bodies.Length; i++)
                positions[i] = SmallBodyBelt.PositionOf(bodies[i], DaysSinceJ2000);
        }

        float maxX = rect.Width + 40f, maxY = rect.Height + 40f;
        float currentAlpha = -1f;
        for (int i = 0; i < positions.Length; i++)
        {
            if (!Camera.Project(positions[i], out float sx, out float sy, out _))
                continue;
            if (sx < -40f || sx > maxX || sy < -40f || sy > maxY)
                continue;

            // The list is sorted by brightness, so this triggers three times.
            float alpha = belt.Bodies[i].Alpha;
            if (alpha != currentAlpha)
            {
                currentAlpha = alpha;
                canvas.FillColor = colour.WithAlpha(alpha);
            }
            canvas.FillCircle(sx, sy, dotRadius);
        }
    }

    // ------------------------------------------------------------ celestial bodies

    void DrawBodies(ICanvas canvas, RectF rect)
    {
        double t = DaysSinceJ2000;

        // The Sun's screen position is needed for the planets' lighting.
        bool sunVisible = Camera.Project(Vector3.Zero, out float sunX, out float sunY, out float sunDepth);

        var bodies = new List<(CelestialBody? Body, bool IsMoon, Vector3 Pos, float WorldRadius, float Depth, float Sx, float Sy)>(16);

        float sunWorldR = VisualRadius(SolarSystemData.SunRadiusKm, isSun: true);
        if (sunVisible)
            bodies.Add((null, false, Vector3.Zero, sunWorldR, sunDepth, sunX, sunY));

        foreach (var planet in SolarSystemData.Planets)
        {
            // The Kepler orbit really describes the system's centre of mass.
            // With a moon as heavy as Charon, that centre lies outside the
            // parent body, which then visibly wobbles around it instead of
            // standing still.
            var barycentre = planet.PositionAt(t, UnitsPerAu);
            var pos = barycentre;
            float moonScale = 0f;
            if (ShowMoons && planet.Moons.Length > 0)
            {
                moonScale = MoonDisplayScale(planet);
                foreach (var moon in planet.Moons)
                {
                    if (moon.MassFraction > 0)
                        pos -= moon.PositionAt(t, UnitsPerAu) * moonScale * (float)moon.MassFraction;
                }
            }

            if (Camera.Project(pos, out float sx, out float sy, out float depth))
                bodies.Add((planet, false, pos, VisualRadius(planet.RadiusKm, isSun: false), depth, sx, sy));
            if (ShowMoons && planet.Moons.Length > 0)
                AddMoons(bodies, planet, barycentre, moonScale, t);
        }

        // Paint back to front (painter's algorithm).
        bodies.Sort((a, b) => b.Depth.CompareTo(a.Depth));

        foreach (var (body, isMoon, pos, worldR, depth, sx, sy) in bodies)
        {
            float r = Camera.ScreenRadius(worldR, depth);
            if (body is null)
            {
                r = MathF.Max(r, RealScale ? 1.2f : 2.5f);
                DrawSun(canvas, sx, sy, r);
                _labels.Add((Strings.Name("Sun"), sx, sy + r + 6));
            }
            else
            {
                // Tiny moons like Phobos (11 km radius) become vanishingly small
                // even magnified, so they're guaranteed a visible dot.
                r = MathF.Max(r, RealScale ? 0.45f : isMoon ? 2.5f : 1.1f);
                if (body.Ring is PlanetRing ring)
                {
                    // The far half of the ring is painted before the planet and the
                    // near half after, so the ring appears to pass behind and in
                    // front of the globe.
                    BuildRingPaths(ring, pos, worldR, depth, out var farRing, out var nearRing);
                    FillRing(canvas, ring, farRing);
                    DrawBody(canvas, body, pos, sx, sy, r, sunX, sunY);
                    FillRing(canvas, ring, nearRing);
                }
                else
                {
                    DrawBody(canvas, body, pos, sx, sy, r, sunX, sunY);
                }
                // Moons are drawn without a name label – they're recognised by their position next to the planet.
                if (!isMoon)
                    _labels.Add((Strings.Name(body.Key), sx, sy + r + 6));
            }
        }
    }

    /// <summary>
    /// The heliopause's distance from the Sun. There the solar wind meets
    /// the interstellar medium and the Sun's domain ends – the nearest thing
    /// the Solar System has to an edge.
    ///
    /// Drawing it as a sphere is a simplification. In reality it's not
    /// round: the Solar System travels through the interstellar medium at
    /// 25 km/s and gets a bow shock ahead of it, so the boundary sits closer
    /// in the direction we're heading and is drawn out into a tail behind.
    /// The Voyager probes crossed it at 121.6 and 119.0 AU respectively, and
    /// that the two numbers aren't equal is exactly why.
    /// </summary>
    public const double HeliopauseAu = 120.0;

    static readonly Color HeliopauseFill = Color.FromRgba(0.30f, 0.50f, 0.85f, 0.045f);
    static readonly Color HeliopauseRim = Color.FromRgba(0.55f, 0.75f, 1.00f, 0.30f);

    /// <summary>
    /// Draws the heliopause as a circle, but only while the camera stands
    /// outside it. From inside, the sphere would fill the whole frame and
    /// just be a blue haze.
    ///
    /// A sphere seen from outside projects to a circle with angular radius
    /// arcsin(R/d), which on screen becomes R·f/√(d²−R²). So it's not the
    /// same thing as projecting the sphere's edge directly.
    /// </summary>
    void DrawHeliopause(ICanvas canvas, RectF rect)
    {
        float radius = (float)HeliopauseAu * UnitsPerAu;
        float distance = Camera.Position.Length();

        // Inside, or right at the limit where the expression derails.
        if (distance <= radius * 1.02f)
            return;
        if (!Camera.Project(Vector3.Zero, out float sx, out float sy, out _))
            return;

        float screen = Camera.Focal * radius
                       / MathF.Sqrt(distance * distance - radius * radius);
        // The circle has to fit in frame. Bigger than that, no edge is
        // visible at all, just a blue haze over everything – and then it's
        // better not to draw it.
        if (screen < 8f || screen > MathF.Min(rect.Width, rect.Height) * 0.55f)
            return;

        canvas.FillColor = HeliopauseFill;
        canvas.FillCircle(sx, sy, screen);
        canvas.StrokeSize = 1.2f;
        canvas.StrokeColor = HeliopauseRim;
        canvas.DrawCircle(sx, sy, screen);

        _labels.Add((Strings.Name("Heliopause"), sx, sy - screen - 4));
    }


    // ---------------------------------------------------- Halley's Comet

    /// <summary>
    /// Beyond this distance from the Sun, the comet has no tail. The limit
    /// isn't chosen for looks: the tail exists only as long as the ice in
    /// the nucleus is vaporising, and water ice only starts vaporising once
    /// the Sun has warmed it enough, which happens around three astronomical
    /// units.
    ///
    /// The consequence is worth seeing in the app. Of its 27,563 days,
    /// Halley spends 368 inside the limit – one year out of seventy-five.
    /// The rest of the time it's a dark lump of ice nobody can see.
    /// </summary>
    const double TailLimitAu = 3.0;

    /// <summary>
    /// The tail's length at one astronomical unit of distance. It grows as
    /// 1/r², in step with the strength of sunlight, giving 0.29 AU at
    /// perihelion and a few hundredths at the limit above.
    /// </summary>
    const double TailScaleAu = 0.10;

    /// <summary>Cap on the length, so it doesn't run away inside perihelion.</summary>
    const double MaxTailAu = 0.30;

    /// <summary>How long the dust tail is relative to the ion tail.</summary>
    const double DustLengthShare = 0.65;

    /// <summary>How sharply the dust tail curves backward in the orbit toward its tip.</summary>
    const double DustCurve = 0.5;

    /// <summary>Number of points each tail is built from.</summary>
    const int TailSteps = 16;

    /// <summary>
    /// The shortest tail that's drawn, in pixels. A tail of 0.3 AU is large
    /// compared to Earth's orbit but small compared to Halley's own, so
    /// zoomed out it would only be a few pixels long. It's then stretched
    /// out to this length with its direction kept – the same kind of
    /// magnification the planets get to be visible at all.
    /// </summary>
    const float MinTailPixels = 24f;

    const int HalleyOrbitSamples = 360;

    static readonly Color HalleyOrbitColor = Color.FromRgba(0.62f, 0.88f, 0.84f, 0.40f);
    static readonly Color IonTailColor = Color.FromRgba(0.62f, 0.82f, 1.00f, 0.85f);
    static readonly Color DustTailColor = Color.FromRgba(1.00f, 0.93f, 0.74f, 0.55f);
    static readonly Color ComaGlow = Color.FromRgba(0.80f, 0.96f, 0.94f, 0.22f);

    Vector3[]? _halleyOrbit;

    /// <summary>
    /// Halley's orbit. It isn't drawn together with the planets' orbits,
    /// partly because it only belongs in the picture while the comet is
    /// present, partly because it's sixty times longer than it is wide and
    /// so can't be lumped in with the circles.
    /// </summary>
    void DrawHalleyOrbit(ICanvas canvas)
    {
        // The orbit sits still, so the points are computed once.
        _halleyOrbit ??= SolarSystemData.Halley.OrbitPath(HalleyOrbitSamples, UnitsPerAu);

        // The same rule as for the planetary orbits: a subpath may only
        // start once two points in a row are visible, otherwise it becomes a
        // MoveTo with no LineTo.
        var path = new PathF();
        bool started = false, hasPrev = false;
        float px = 0, py = 0;
        for (int k = 0; k <= _halleyOrbit.Length; k++)
        {
            if (Camera.Project(_halleyOrbit[k % _halleyOrbit.Length],
                    out float sx, out float sy, out _))
            {
                if (hasPrev)
                {
                    if (!started) { path.MoveTo(px, py); started = true; }
                    path.LineTo(sx, sy);
                }
                hasPrev = true;
                px = sx;
                py = sy;
            }
            else
            {
                hasPrev = false;
                started = false;
            }
        }

        canvas.StrokeSize = 1f;
        canvas.StrokeColor = HalleyOrbitColor;
        canvas.DrawPath(path);
    }

    /// <summary>
    /// The comet with its two tails.
    ///
    /// What's easiest to get wrong is which way they point. A tail doesn't
    /// trail behind the comet in its direction of travel, like a rocket's
    /// exhaust, but away from the Sun – and on the way out from perihelion
    /// the comet therefore travels tail-first.
    ///
    /// That there are two tails, and why they differ, is the same thing
    /// seen up close. The ion tail is gas that sunlight has electrically
    /// charged; the solar wind blows at 400 km/s and tears it straight away
    /// from the Sun without caring where the comet is headed. The dust tail
    /// is grains too heavy to be swept along: they get a push from the
    /// light but keep the comet's own speed, end up in their own orbits
    /// around the Sun, and so come out shorter, wider and curved backward.
    /// </summary>
    void DrawHalley(ICanvas canvas)
    {
        var comet = SolarSystemData.Halley;
        double t = DaysSinceJ2000;
        var posAu = comet.PositionAuAt(t);
        if (!Camera.Project((posAu * UnitsPerAu).ToVector3(),
                out float hx, out float hy, out _))
            return;

        double r = posAu.Length;
        double tailAu = r < TailLimitAu ? Math.Min(MaxTailAu, TailScaleAu / (r * r)) : 0.0;

        if (tailAu > 0)
        {
            // The Sun sits at the origin, so the direction away from it is the position itself.
            var away = posAu.Normalized();
            var motion = (comet.PositionAuAt(t + 0.5) - comet.PositionAuAt(t - 0.5)).Normalized();

            var ion = new Vector3[TailSteps + 1];
            var dust = new Vector3[TailSteps + 1];
            for (int k = 0; k <= TailSteps; k++)
            {
                double s = (double)k / TailSteps;
                ion[k] = ((posAu + away * (tailAu * s)) * UnitsPerAu).ToVector3();

                // The dust lags behind in the orbit, and the lag grows with
                // distance from the nucleus since those grains were released
                // earlier.
                double along = tailAu * DustLengthShare * s;
                dust[k] = ((posAu + away * along - motion * (along * DustCurve * s))
                           * UnitsPerAu).ToVector3();
            }

            float stretch = TailStretch(ion[TailSteps], hx, hy);
            DrawTail(canvas, dust, hx, hy, stretch, DustTailColor, 4.0f);
            DrawTail(canvas, ion, hx, hy, stretch, IonTailColor, 1.6f);
        }

        // The nucleus is five kilometres wide and would never be visible.
        // What's seen is the coma: the gas cloud around the nucleus, which
        // near the Sun becomes a hundred thousand kilometres wide and so
        // larger than the Sun itself. The dot's size therefore follows the
        // activity, not the body's actual measurements.
        float coma = 2.4f + 4.6f * (float)(tailAu / MaxTailAu);
        canvas.FillColor = ComaGlow;
        canvas.FillCircle(hx, hy, coma * 2.4f);
        canvas.FillColor = comet.BodyColor;
        canvas.FillCircle(hx, hy, coma);

        _labels.Add((Strings.Name(comet.Key), hx, hy + coma * 2.4f + 6f));
    }

    /// <summary>
    /// How much the tail needs to be stretched to be visible. A concession
    /// to the camera and not to the physics: the direction is the computed
    /// one, only the length is exaggerated.
    /// </summary>
    float TailStretch(Vector3 tip, float hx, float hy)
    {
        if (!Camera.Project(tip, out float tx, out float ty, out _))
            return 1f;
        float length = MathF.Sqrt((tx - hx) * (tx - hx) + (ty - hy) * (ty - hy));
        return length >= MinTailPixels || length <= 0.01f ? 1f : MinTailPixels / length;
    }

    /// <summary>
    /// Draws a tail as a row of strokes that grow wider and paler outward –
    /// densest and brightest at the nucleus, just like a real tail.
    /// </summary>
    void DrawTail(ICanvas canvas, Vector3[] points, float hx, float hy,
        float stretch, Color colour, float maxWidth)
    {
        float px = 0, py = 0;
        bool hasPrev = false;
        for (int k = 0; k <= TailSteps; k++)
        {
            if (!Camera.Project(points[k], out float sx, out float sy, out _))
            {
                hasPrev = false;
                continue;
            }
            sx = hx + (sx - hx) * stretch;
            sy = hy + (sy - hy) * stretch;

            if (hasPrev)
            {
                float s = (float)k / TailSteps;
                float fade = (1f - s) * (1f - s);
                canvas.StrokeSize = 0.8f + maxWidth * s;
                canvas.StrokeColor = colour.WithAlpha(colour.Alpha * fade);
                canvas.DrawLine(px, py, sx, sy);
            }
            px = sx;
            py = sy;
            hasPrev = true;
        }
    }

    // -------------------------------------------------- the Moon's orbit and its nodes

    static readonly Color MoonOrbitColor = Color.FromRgba(0.70f, 0.75f, 0.85f, 0.55f);
    static readonly Color EclipticRingColor = Color.FromRgba(0.95f, 0.82f, 0.45f, 0.40f);
    static readonly Color NodeLineColor = Color.FromRgba(0.95f, 0.82f, 0.45f, 0.75f);

    /// <summary>
    /// Draws the Moon's orbit against the ecliptic plane and marks the two
    /// nodes – the points where the orbit crosses the ecliptic.
    ///
    /// These are what the whole eclipse question hinges on. The Moon goes
    /// around lap after lap without any eclipse happening, because the orbit
    /// tilts 5.1 degrees and the Moon therefore passes above or below the
    /// Sun. Only when the Sun happens to stand near the node line can the
    /// three line up. The node line also turns backward once every 18.6
    /// years, which is why eclipse seasons drift backward through the
    /// calendar instead of staying put.
    ///
    /// The orbit is drawn with the same compression as the Moon itself,
    /// otherwise the curve would end up somewhere other than the Moon.
    /// </summary>
    void DrawMoonOrbit(ICanvas canvas, double day)
    {
        var earth = SolarSystemData.Planets.FirstOrDefault(p => p.Key == "Earth");
        if (earth is null || earth.Moons.Length == 0)
            return;

        var moon = earth.Moons[0];
        var centre = earth.PositionAt(day, UnitsPerAu);
        float scale = MoonDisplayScale(earth) * (1f - (float)moon.MassFraction);

        // Below this size, the orbit is just a squiggle around a dot.
        if (!Camera.Project(centre, out _, out _, out float depth))
            return;
        float radius = (float)(moon.SemiMajorAu * UnitsPerAu) * scale;
        if (Camera.ScreenRadius(radius, depth) < 30f)
            return;

        // The orbit itself, with the precession it has on this particular day.
        const int samples = 96;
        var path = moon.OrbitPath(samples, UnitsPerAu, day);
        DrawClosedCurve(canvas, centre, path, MoonOrbitColor, 1.4f);

        // The ecliptic's plane as a circle of the same radius, to compare against.
        var flat = new Vector3[samples];
        for (int i = 0; i < samples; i++)
        {
            double a = i * Math.PI * 2 / samples;
            flat[i] = new Vector3((float)(Math.Cos(a) * radius), 0f, (float)(Math.Sin(a) * radius));
        }
        DrawClosedCurve(canvas, centre, flat, EclipticRingColor, 1.0f);

        // The node line: where the two planes intersect. The direction
        // follows from the node's longitude, and it turns over time.
        double node = moon.AscNodeAt(day) * Math.PI / 180.0;
        var along = new Vector3((float)Math.Cos(node), 0f, (float)(-Math.Sin(node)));
        var up = new Vector3(0f, 1f, 0f);

        if (Camera.Project(centre + along * radius, out float ax, out float ay, out _) &&
            Camera.Project(centre - along * radius, out float bx, out float by, out _))
        {
            canvas.StrokeSize = 1.2f;
            canvas.StrokeColor = NodeLineColor;
            canvas.DrawLine(ax, ay, bx, by);

            canvas.FontSize = 11f;
            canvas.FontColor = NodeLineColor;
            canvas.DrawString(Strings.Name("ascendingNode"), ax + 6, ay - 4, HorizontalAlignment.Left);
            canvas.DrawString(Strings.Name("descendingNode"), bx + 6, by - 4, HorizontalAlignment.Left);
            canvas.FillColor = NodeLineColor;
            canvas.FillCircle(ax, ay, 3f);
            canvas.FillCircle(bx, by, 3f);
        }

        // A small arrow perpendicular to the ecliptic, so the tilt is visible.
        if (Camera.Project(centre, out float cx, out float cy, out _) &&
            Camera.Project(centre + up * radius * 0.4f, out float ux, out float uy, out _))
        {
            canvas.StrokeSize = 1.0f;
            canvas.StrokeColor = EclipticRingColor;
            canvas.DrawLine(cx, cy, ux, uy);
        }
    }

    /// <summary>Draws a closed curve of world points around a centre.</summary>
    void DrawClosedCurve(ICanvas canvas, Vector3 centre, Vector3[] points, Color color, float width)
    {
        var path = new PathF();
        int drawn = 0;
        foreach (var point in points)
        {
            if (!Camera.Project(centre + point, out float x, out float y, out _))
                continue;
            if (drawn++ == 0) path.MoveTo(x, y);
            else path.LineTo(x, y);
        }
        if (drawn < 3)
            return;

        path.Close();
        canvas.StrokeSize = width;
        canvas.StrokeColor = color;
        canvas.DrawPath(path);
    }

    /// <summary>The innermost moon may end up at most this far out (planet radii).</summary>
    const float MoonInnerMaxRadii = 3f;

    /// <summary>The outermost moon may end up at most this far out (planet radii).</summary>
    const float MoonOuterMaxRadii = 10f;

    /// <summary>
    /// The innermost moon may never be pushed closer than this (planet
    /// radii). Without this floor, Titan's large orbit would press
    /// Enceladus into Saturn's rings, which reach out to 2.3 planet radii.
    /// </summary>
    const float MoonInnerMinRadii = 2.5f;

    /// <summary>
    /// How the moon orbits are scaled relative to the planet. The starting
    /// point is to follow the planets' own magnification, so the system's
    /// geometry comes out exactly right (Mars's moons really do sit as close
    /// as they look). The system is only compressed when needed: when the
    /// innermost moon would land farther out than 3 planet radii (our own
    /// Moon sits at 60) or the outermost moon farther out than 10 (Callisto
    /// sits at 27 Jupiter radii).
    /// </summary>
    float MoonDisplayScale(CelestialBody planet)
    {
        if (RealScale || planet.Moons.Length == 0)
            return 1f;

        float parentVisR = VisualRadius(planet.RadiusKm, isSun: false);
        double innerAu = planet.Moons.Min(m => m.SemiMajorAu);
        double outerAu = planet.Moons.Max(m => m.SemiMajorAu);

        float scale = MathF.Min(PlanetBoost, MathF.Min(
            parentVisR * MoonInnerMaxRadii / (float)(innerAu * UnitsPerAu),
            parentVisR * MoonOuterMaxRadii / (float)(outerAu * UnitsPerAu)));

        // ... but never so hard that the innermost moon ends up inside the
        // planet or its rings.
        return MathF.Max(scale,
            parentVisR * MoonInnerMinRadii / (float)(innerAu * UnitsPerAu));
    }

    /// <summary>
    /// Suitable camera distance when a body is selected in the focus
    /// selector: close enough to see the planet, but far enough away for
    /// its whole moon system to fit in frame.
    /// </summary>
    /// <summary>
    /// Where a moon actually ends up on screen: the planet's position plus
    /// the moon's orbital position, with the same compression the rendering
    /// uses.
    ///
    /// The camera has to aim at the drawn position and not the real one.
    /// Moons are pulled in toward their planet so they don't end up outside
    /// the frame – our own Moon sits at 60 Earth radii and would otherwise
    /// disappear – and aiming at the real position would point the camera
    /// far off to the side.
    /// </summary>
    public Vector3 MoonPosition(CelestialBody planet, CelestialBody moon, double day)
        => planet.PositionAt(day, UnitsPerAu)
           + moon.PositionAt(day, UnitsPerAu)
             * MoonDisplayScale(planet) * (1f - (float)moon.MassFraction);

    /// <summary>
    /// Camera distance when a moon is selected: the same viewing angle a
    /// planet gets, so the globe fills the same share of the frame. Without
    /// this, you'd end up either inside the moon or so far away it's just a
    /// dot.
    /// </summary>
    public float SuggestedMoonDistance(CelestialBody moon)
        => MathF.Max(VisualRadius(moon.RadiusKm, isSun: false) * 12f,
                     OrbitCamera.AbsoluteMinDistance);

    public float SuggestedFocusDistance(CelestialBody planet)
    {
        // The comet has no size worth speaking of – the nucleus is five
        // kilometres – so the distance frames the tail instead of the body.
        if (ReferenceEquals(planet, SolarSystemData.Halley))
            return (float)(MaxTailAu * UnitsPerAu) * 3f;

        float visR = VisualRadius(planet.RadiusKm, isSun: false);
        float distance = visR * 12f;

        if (planet.Moons.Length > 0)
        {
            float outer = (float)(planet.Moons.Max(m => m.SemiMajorAu) * UnitsPerAu)
                          * MoonDisplayScale(planet);
            distance = MathF.Max(distance, outer * 2.7f);
        }
        // The floor used to be a fixed number, which made "Real size" mode
        // impossible to zoom into: there, Earth's radius is 0.0026 units and
        // eight units is three thousand Earth radii away. Now it follows
        // the body.
        return MathF.Max(distance, MathF.Max(visR * 2f, OrbitCamera.AbsoluteMinDistance));
    }

    /// <summary>
    /// Adds a planet's moons. Each moon orbits the planet with its real
    /// orbital elements. In magnified mode the real distances would be
    /// misleadingly large (Earth's Moon sits at 60 Earth radii), so the
    /// system is compressed there: the innermost moon lands at 3x the
    /// planet's visual radius, and the others keep their proportions
    /// relative to each other. The orbits' shape, direction and speed are
    /// still correct. Moons are only drawn once zoomed in close enough that
    /// the orbit spans a few tens of pixels.
    /// </summary>
    void AddMoons(List<(CelestialBody? Body, bool IsMoon, Vector3 Pos, float WorldRadius, float Depth, float Sx, float Sy)> bodies,
        CelestialBody planet, Vector3 barycentre, float displayScale, double t)
    {
        foreach (var moon in planet.Moons)
        {
            // The separation is measured from the parent body; the moon is
            // placed on its side of the centre of mass so the distance between
            // the bodies comes out right.
            var offset = moon.PositionAt(t, UnitsPerAu) * displayScale; // planetcentriskt
            var moonPos = barycentre + offset * (1f - (float)moon.MassFraction);

            if (!Camera.Project(moonPos, out float sx, out float sy, out float depth))
                continue;
            float orbitRadius = (float)(moon.SemiMajorAu * UnitsPerAu) * displayScale;
            if (Camera.ScreenRadius(orbitRadius, depth) < 10f)
                continue; // too zoomed out – the moon would just smear into the planet

            bodies.Add((moon, true, moonPos,
                VisualRadius(moon.RadiusKm, isSun: false), depth, sx, sy));
        }
    }

    /// <summary>
    /// The body's drawn radius in world units. Public because the camera
    /// needs it: how close you're allowed to get depends on how large the
    /// body is drawn.
    /// </summary>
    public float VisualRadius(double radiusKm, bool isSun)
    {
        float real = (float)(radiusKm / SolarSystemData.AuKm) * UnitsPerAu;
        if (RealScale)
            return real;
        return real * (isSun ? SunBoost : PlanetBoost);
    }

    /// <summary>
    /// The Sun: glow, disc, and – once zoomed in enough – sunspots.
    ///
    /// The disc is painted with a gradient instead of a flat colour, and
    /// that's not decoration but limb darkening. The Sun really is darker
    /// and redder at the edge than in the middle, since there you're looking
    /// obliquely into the gas and only reach the upper, cooler layers. The
    /// effect shows up in any solar telescope image.
    /// </summary>
    void DrawSun(ICanvas canvas, float x, float y, float r)
    {
        // Outer glow.
        float glow = MathF.Max(r * 3.2f, r + 12f);
        var glowRect = new RectF(x - glow, y - glow, glow * 2, glow * 2);
        var glowPaint = new RadialGradientPaint(
        [
            new PaintGradientStop(0f, Color.FromRgba(1f, 0.85f, 0.45f, 0.55f)),
            new PaintGradientStop(0.5f, Color.FromRgba(1f, 0.72f, 0.25f, 0.18f)),
            new PaintGradientStop(1f, Colors.Transparent),
        ])
        { Center = new Point(0.5, 0.5), Radius = 0.5 };
        canvas.SetFillPaint(glowPaint, glowRect);
        canvas.FillCircle(x, y, glow);

        // The solar disc.
        var coreRect = new RectF(x - r, y - r, r * 2, r * 2);
        var corePaint = new RadialGradientPaint(
        [
            new PaintGradientStop(0f, Color.FromArgb("#FFFFF3")),
            new PaintGradientStop(0.55f, Color.FromArgb("#FFE08A")),
            new PaintGradientStop(1f, Color.FromArgb("#FFA226")),
        ])
        { Center = new Point(0.5, 0.5), Radius = 0.5 };
        canvas.SetFillPaint(corePaint, coreRect);
        canvas.FillCircle(x, y, r);

        // Spots only once the disc is large enough. The threshold is higher
        // than the planets', which follows from how big the spots are: the
        // largest group spans seven degrees across, which on a disc of
        // radius r comes to 0.12·r. At the planets' fourteen pixels it would
        // be under a pixel.
        if (r >= SunSpotMinRadius)
            DrawSurfaceRegions(canvas, SurfaceMap.Sun, SolarSystemData.SunAxis,
                Vector3.Zero, x, y, r);
    }

    static void DrawPlanet(ICanvas canvas, CelestialBody body, float x, float y, float r, float sunX, float sunY)
    {
        if (r < 1.6f)
        {
            // Too small for shading – draw a simple dot.
            canvas.FillColor = body.BodyColor;
            canvas.FillCircle(x, y, r);
            return;
        }

        // Brighter on the side facing the Sun.
        float dx = sunX - x, dy = sunY - y;
        float len = MathF.Sqrt(dx * dx + dy * dy);
        if (len > 1e-3f) { dx /= len; dy /= len; }

        var lit = Mix(body.BodyColor, Colors.White, 0.40f);
        var dark = Mix(body.BodyColor, Colors.Black, 0.72f);
        var rectF = new RectF(x - r, y - r, r * 2, r * 2);
        var paint = new RadialGradientPaint(
        [
            new PaintGradientStop(0f, lit),
            new PaintGradientStop(0.55f, body.BodyColor),
            new PaintGradientStop(1f, dark),
        ])
        {
            Center = new Point(0.5 + dx * 0.30, 0.5 + dy * 0.30),
            Radius = 0.75,
        };
        canvas.SetFillPaint(paint, rectF);
        canvas.FillCircle(x, y, r);
    }

    // ------------------------------------------------------------ globe rendering

    /// <summary>
    /// The surface is only drawn once the globe has reached this many
    /// pixels in radius. Below that, the continents would just be a couple
    /// of blurry smudges anyway, and a shaded disc looks better.
    /// </summary>
    const float GlobeMinRadius = 14f;

    /// <summary>The Sun disc's minimum radius for spots to be drawn. See DrawSun.</summary>
    const float SunSpotMinRadius = 30f;

    readonly List<Vector3> _globeDirs = new(256);
    readonly List<Vector3> _globeClipped = new(256);

    /// <summary>
    /// Draws a body: as a globe with a surface if it has a surface map and
    /// is large enough in frame, otherwise as a shaded disc.
    /// </summary>
    void DrawBody(ICanvas canvas, CelestialBody body, Vector3 center,
        float sx, float sy, float r, float sunX, float sunY)
    {
        if (r >= GlobeMinRadius && body.Surface is SurfaceMap surface && body.Axis is BodyAxis axis)
            DrawGlobe(canvas, surface, axis, center, sx, sy, r, sunX, sunY);
        else
            DrawPlanet(canvas, body, sx, sy, r, sunX, sunY);
    }

    /// <summary>
    /// A body as a globe with its surface and its real rotation, visible
    /// once zoomed in. Each surface point's direction is built from the
    /// body's rotation axis: so Earth's axis tilts 23.4° toward Polaris and
    /// the right continent faces the Sun at the right time of day, with one
    /// turn per sidereal day (23h 56m).
    /// </summary>
    void DrawGlobe(ICanvas canvas, SurfaceMap surface, BodyAxis axis, Vector3 center,
        float sx, float sy, float r, float sunX, float sunY)
    {
        canvas.FillColor = surface.BaseColor;
        canvas.FillCircle(sx, sy, r);

        DrawSurfaceRegions(canvas, surface, axis, center, sx, sy, r);

        // Day/night: a light tint toward the sunlit side, a dark shadow on the night side.
        float dx = sunX - sx, dy = sunY - sy;
        float len = MathF.Sqrt(dx * dx + dy * dy);
        if (len > 1e-3f) { dx /= len; dy /= len; }
        var rectF = new RectF(sx - r, sy - r, r * 2, r * 2);
        var shade = new RadialGradientPaint(
        [
            new PaintGradientStop(0f, Color.FromRgba(1f, 1f, 0.95f, 0.16f)),
            new PaintGradientStop(0.55f, Colors.Transparent),
            new PaintGradientStop(1f, Color.FromRgba(0f, 0f, 0.02f, 0.62f)),
        ])
        {
            Center = new Point(0.5 + dx * 0.30, 0.5 + dy * 0.30),
            Radius = 0.75,
        };
        canvas.SetFillPaint(shade, rectF);
        canvas.FillCircle(sx, sy, r);
    }

    /// <summary>
    /// The surface map's polygons painted onto a globe disc already in
    /// place. Split out from the globe rendering since the Sun needs the
    /// same thing but nothing else: it has no day and night side to shade,
    /// and its disc is a gradient rather than a flat colour.
    ///
    /// The surface points are drawn with orthographic projection within the
    /// drawn circle: the screen position is given by the direction's
    /// components along the camera's right and up axes times the circle's
    /// radius. That way land can never end up outside the globe
    /// (perspective projection of limb points did exactly that when the
    /// camera was close). What's visible is the hemisphere facing the
    /// camera.
    ///
    /// The rotation is taken per region rather than per vertex. For
    /// anything solid it doesn't matter – the whole body turns the same
    /// amount – but the Sun rotates at different speeds at different
    /// latitudes, and there the region is the right level: a sunspot group
    /// follows along as one clump. See
    /// <see cref="SurfaceMap.Region.MeanSinLat"/>.
    /// </summary>
    void DrawSurfaceRegions(ICanvas canvas, SurfaceMap surface, BodyAxis axis,
        Vector3 center, float sx, float sy, float r)
    {
        double rigidSpin = axis.SpinRadians(DaysSinceJ2000);
        bool differential = axis.DifferentialDegPerDay != 0.0;

        var toCam = Vector3.Normalize(Camera.Position - center);
        const float cosLimb = 0.02f;

        foreach (var region in surface.Regions)
        {
            double spin = differential
                ? axis.SpinRadians(DaysSinceJ2000, region.MeanSinLat)
                : rigidSpin;

            _globeDirs.Clear();
            for (int i = 0; i < region.LonRad.Length; i++)
                _globeDirs.Add(axis.Direction(
                    region.SinLat[i], region.CosLat[i], region.LonRad[i], spin));

            ClipToVisibleCap(_globeDirs, _globeClipped, toCam, cosLimb);
            if (_globeClipped.Count < 3)
                continue;

            var path = new PathF();
            int drawn = 0;
            foreach (var dir in _globeClipped)
            {
                float px = sx + Vector3.Dot(dir, Camera.RightAxis) * r;
                float py = sy - Vector3.Dot(dir, Camera.UpAxis) * r;
                if (drawn == 0) path.MoveTo(px, py);
                else path.LineTo(px, py);
                drawn++;
            }
            if (drawn < 3)
                continue;
            path.Close();
            canvas.FillColor = region.Fill;
            canvas.FillPath(path);
        }
    }

    /// <summary>
    /// Sutherland-Hodgman clipping of a polygon on the globe's surface
    /// against the visible cap (dot(direction, toward camera) >= cosLimb).
    /// </summary>
    static void ClipToVisibleCap(List<Vector3> dirs, List<Vector3> result,
        Vector3 toCam, float cosLimb)
    {
        result.Clear();
        int n = dirs.Count;
        for (int i = 0; i < n; i++)
        {
            var a = dirs[i];
            var b = dirs[(i + 1) % n];
            float da = Vector3.Dot(a, toCam) - cosLimb;
            float db = Vector3.Dot(b, toCam) - cosLimb;

            if (da >= 0)
                result.Add(a);
            if (da >= 0 != db >= 0)
            {
                // The edge crosses the limb – interpolate the crossing point.
                float t = da / (da - db);
                result.Add(Vector3.Normalize(Vector3.Lerp(a, b, t)));
            }
        }
    }

    static void FillRing(ICanvas canvas, PlanetRing ring, PathF? path)
    {
        if (path is null)
            return;
        canvas.FillColor = ring.Color;
        canvas.FillPath(path);
    }

    /// <summary>
    /// Saturn's rings as a band in the planet's equatorial plane, split into
    /// two shapes: the far half (drawn behind the planet) and the near half
    /// (drawn in front). The segments are gathered into two paths so the
    /// whole ring is filled with two calls instead of one per segment.
    /// </summary>
    void BuildRingPaths(PlanetRing ring, Vector3 center, float worldR, float planetDepth,
        out PathF? farRing, out PathF? nearRing)
    {
        farRing = nearRing = null;

        float inner = worldR * ring.InnerRadii;
        float outer = worldR * ring.OuterRadii;
        if (Camera.ScreenRadius(outer, planetDepth) < ring.MinScreenRadius)
            return;

        // The rings lie in the planet's equatorial plane, so the basis
        // vectors come ready-made from its rotation axis: the node line as
        // one and the point a quarter turn east of it as the other.
        var u = ring.Axis.NodeAxis;
        var v = ring.Axis.EastAxis;

        const int segments = 72;
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * MathF.PI * 2f / segments;
            float a1 = (i + 1.05f) * MathF.PI * 2f / segments; // slight overlap against seams
            var d0 = u * MathF.Cos(a0) + v * MathF.Sin(a0);
            var d1 = u * MathF.Cos(a1) + v * MathF.Sin(a1);

            var p00 = center + d0 * inner;
            var p01 = center + d0 * outer;
            var p11 = center + d1 * outer;
            var p10 = center + d1 * inner;

            if (!Camera.Project(p00, out float x00, out float y00, out float z00) ||
                !Camera.Project(p01, out float x01, out float y01, out float z01) ||
                !Camera.Project(p11, out float x11, out float y11, out float z11) ||
                !Camera.Project(p10, out float x10, out float y10, out float z10))
                continue;

            float segDepth = (z00 + z01 + z11 + z10) * 0.25f;
            var path = segDepth < planetDepth
                ? nearRing ??= new PathF()
                : farRing ??= new PathF();

            path.MoveTo(x00, y00);
            path.LineTo(x01, y01);
            path.LineTo(x11, y11);
            path.LineTo(x10, y10);
            path.Close();
        }
    }

    // --------------------------------------------------------------- spacecraft

    /// <summary>Number of points each leg of a probe's orbit is drawn with.</summary>
    const int ProbeTrailSteps = 70;

    /// <summary>
    /// Draws the real spacecraft with their trails behind them. The trail
    /// is computed from the orbit rather than saved frame by frame, so it
    /// looks the same whether time is jumped or played backward.
    ///
    /// The probes are today over 150 AU out, four times farther than
    /// Neptune. In the overview only the trail's innermost part is
    /// therefore visible, and you have to zoom out a great deal to see the
    /// whole thing – which is itself the point of how far they've come.
    /// </summary>
    void DrawProbes(ICanvas canvas)
    {
        double now = DaysSinceJ2000;

        foreach (var probe in ProbeData.All)
        {
            if (!VisibleProbes.Contains(probe.Name))
                continue;   // deselected in the probe selector
            if (!probe.Exists(now))
                continue;   // not launched yet

            canvas.StrokeSize = 1.4f;
            canvas.StrokeColor = probe.Color.WithAlpha(0.55f);
            foreach (var leg in probe.Legs)
            {
                if (leg.StartDay >= now)
                    break;  // the legs are in time order, the rest is future
                DrawProbeTrail(canvas, leg, Math.Min(leg.EndDay, now));
            }

            DrawMilestones(canvas, probe, now);

            if (probe.PositionAt(now, UnitsPerAu) is not { } pos ||
                !Camera.Project(pos, out float sx, out float sy, out _))
                continue;

            canvas.FillColor = probe.Color;
            canvas.FillCircle(sx, sy, 2.5f);
            _labels.Add((probe.Name, sx, sy + 9f));
        }
    }

    /// <summary>
    /// Draws the milestones – launch and the planetary flybys – as rings
    /// along the trail. The selected probe's milestones get the planet's
    /// name and the date; the others just the year.
    ///
    /// The years are drawn directly instead of going through the label
    /// stacking made for celestial bodies: eleven flybys stacking downward
    /// would become a text column running clear across the view. Instead a
    /// year is skipped whenever it would land on top of one already
    /// written.
    /// </summary>
    /// <summary>
    /// What a milestone is called. The key is usually a body's – "Jupiter" –
    /// but can also be its own, with one needing the probe's name filled in:
    /// "Voyager 1 today". Always going through <c>Format</c> makes the two
    /// cases look the same at the call site; a text with no placeholder
    /// doesn't care about the argument.
    /// </summary>
    public static string MilestoneName(Probe probe, Milestone milestone)
        => string.Format(Strings.Culture, Strings.Name(milestone.Key), probe.Name);

    void DrawMilestones(ICanvas canvas, Probe probe, double now)
    {
        bool focused = ReferenceEquals(probe, FocusedProbe);
        canvas.FontSize = 11f;

        foreach (var milestone in probe.Milestones)
        {
            if (milestone.Day > now)
                break;      // the milestones are in time order

            if (!Camera.Project(milestone.PositionAu * UnitsPerAu,
                    out float x, out float y, out _))
                continue;

            canvas.StrokeSize = 1.4f;
            canvas.StrokeColor = probe.Color.WithAlpha(0.9f);
            canvas.DrawCircle(x, y, 3.5f);

            var date = SolarSystemData.EpochJ2000.AddDays(milestone.Day);
            if (focused)
            {
                // The selected probe gets its whole history written out.
                string name = MilestoneName(probe, milestone);
                string text = milestone switch
                {
                    { IsLaunch: true } => Strings.Format("msg.milestoneLaunched", date),
                    { IsBoundary: true } => Strings.Format("msg.milestonePass", name, date),
                    _ => Strings.Format("msg.milestoneBoost", name, date,
                        milestone.SpeedGainKmS),
                };
                DrawMilestoneText(canvas, text, x, y, probe.Color, minSeparation: 0f);
            }
            else
            {
                DrawMilestoneText(canvas, date.Year.ToString(), x, y,
                    probe.Color.WithAlpha(0.7f), minSeparation: 26f);
            }
        }
    }

    /// <summary>
    /// Draws the probes orbiting a planet: the whole orbital ellipse and the
    /// probe's position on it. The orbit is compressed by the same factor
    /// as the planet's moons, so it ends up on the same order of magnitude
    /// as them instead of disappearing inside the magnified globe. Cassini's
    /// lap is roughly the same size as Titan's orbit, and that now shows in
    /// the view too.
    ///
    /// Only drawn once zoomed in close enough that the orbit spans a few
    /// tens of pixels – from farther out it would just be a dot on top of
    /// the planet anyway.
    /// </summary>
    void DrawOrbiters(ICanvas canvas)
    {
        double now = DaysSinceJ2000;

        foreach (var orbiter in ProbeData.Orbiters)
        {
            if (!VisibleProbes.Contains(orbiter.Name))
                continue;   // deselected in the probe selector
            if (!orbiter.Exists(now))
                continue;   // not yet arrived, or mission over

            var center = orbiter.Center;
            float scale = MoonDisplayScale(center);
            var origin = center.PositionAt(now, UnitsPerAu);

            if (!Camera.Project(origin, out _, out _, out float depth))
                continue;

            float radius = (float)orbiter.Path.SemiMajorAu * UnitsPerAu * scale;
            if (Camera.ScreenRadius(radius, depth) < 10f)
                continue;   // too zoomed out

            DrawOrbiterPath(canvas, orbiter, origin, scale);

            var pos = origin + orbiter.PositionAt(now, UnitsPerAu) * scale;
            if (!Camera.Project(pos, out float sx, out float sy, out _))
                continue;

            canvas.FillColor = orbiter.Color;
            canvas.FillCircle(sx, sy, 2.5f);
            _labels.Add((orbiter.Name, sx, sy + 9f));
        }
    }

    /// <summary>Draws the orbital ellipse as a closed polyline.</summary>
    void DrawOrbiterPath(ICanvas canvas, Orbiter orbiter, Vector3 origin, float scale)
    {
        const int samples = 180;

        var path = new PathF();
        bool started = false;

        foreach (var point in orbiter.OrbitPath(samples, UnitsPerAu))
        {
            if (!Camera.Project(origin + point * scale, out float x, out float y, out _))
            {
                started = false;
                continue;
            }

            if (started)
                path.LineTo(x, y);
            else
                path.MoveTo(x, y);
            started = true;
        }

        canvas.StrokeSize = 1.2f;
        canvas.StrokeColor = orbiter.Color.WithAlpha(0.55f);
        canvas.DrawPath(path);
    }

    /// <summary>Where the milestone texts landed this frame.</summary>
    readonly List<(float X, float Y)> _milestoneText = new(16);

    void DrawMilestoneText(ICanvas canvas, string text, float x, float y, Color color,
        float minSeparation)
    {
        foreach (var (px, py) in _milestoneText)
            if (Math.Abs(px - x) < minSeparation && Math.Abs(py - y) < minSeparation)
                return;     // collides with a text already placed there

        _milestoneText.Add((x, y));

        canvas.FontColor = LabelShadowColor;
        canvas.DrawString(text, x + 7f, y - 4f, HorizontalAlignment.Left);
        canvas.FontColor = color;
        canvas.DrawString(text, x + 6f, y - 5f, HorizontalAlignment.Left);
    }

    /// <summary>Draws one leg of a probe's orbit up to a given day.</summary>
    void DrawProbeTrail(ICanvas canvas, ProbeLeg leg, double toDay)
    {
        if (toDay - leg.StartDay < 1e-6)
            return;

        var path = new PathF();
        bool started = false, hasPrev = false;
        float px = 0, py = 0;

        for (int i = 0; i <= ProbeTrailSteps; i++)
        {
            double day = leg.StartDay + (toDay - leg.StartDay) * i / ProbeTrailSteps;
            if (Camera.Project(leg.Path.PositionAt(day, UnitsPerAu),
                    out float x, out float y, out _))
            {
                if (hasPrev)
                {
                    if (!started) { path.MoveTo(px, py); started = true; }
                    path.LineTo(x, y);
                }
                hasPrev = true;
                px = x;
                py = y;
            }
            else
            {
                hasPrev = false;
                started = false;
            }
        }

        if (started)
            canvas.DrawPath(path);
    }

    // ---------------------------------------------------------------- space mission

    static readonly Color CraftTrailColor = Color.FromRgba(0.55f, 0.85f, 1.00f, 0.75f);
    static readonly Color CraftFutureColor = Color.FromRgba(0.45f, 0.70f, 0.90f, 0.28f);
    static readonly Color CraftColor = Color.FromArgb("#E8F4FF");

    /// <summary>
    /// Draws the craft's orbit and its current position. The trail is
    /// computed from the orbit rather than saved frame by frame – that way
    /// it looks the same whether time is jumped or played backward.
    /// </summary>
    void DrawMission(ICanvas canvas, Mission mission)
    {
        double now = Math.Clamp(DaysSinceJ2000, mission.LaunchDay, mission.ArrivalDay);
        var origin = MissionOrigin(mission, out float scale);

        canvas.StrokeSize = 1.4f;
        canvas.StrokeColor = CraftFutureColor;
        DrawMissionArc(canvas, mission, now, mission.ArrivalDay, origin, scale);   // what remains

        canvas.StrokeSize = 1.8f;
        canvas.StrokeColor = CraftTrailColor;
        DrawMissionArc(canvas, mission, mission.LaunchDay, now, origin, scale);    // the distance already covered

        if (DaysSinceJ2000 < mission.LaunchDay)
            return;

        // After arrival the craft travels along with the target instead of
        // staying put wherever the planet happened to be – a real probe does
        // enter orbit or land, after all.
        var pos = origin + mission.PositionAt(DaysSinceJ2000, UnitsPerAu) * scale;
        if (!Camera.Project(pos, out float sx, out float sy, out _))
            return;

        canvas.FillColor = CraftColor;
        canvas.FillCircle(sx, sy, 3f);
        _labels.Add((mission.HasArrived(DaysSinceJ2000)
            ? Strings.Format("msg.craftLabelArrived", Strings.Name(mission.Key))
            : Strings.Name(mission.Key), sx, sy + 9f));
    }

    /// <summary>
    /// Where the craft's orbit belongs in the world. A trip around the Sun
    /// is drawn as it is, while a lunar trip has to travel along with the
    /// planet and be compressed exactly like the moon orbits – otherwise the
    /// whole orbit would disappear inside the heavily magnified globe of
    /// Earth. It's also drawn around the planet's current position, i.e.
    /// seen from Earth just like the moons, rather than lagging behind along
    /// Earth's own trip around the Sun.
    /// </summary>
    Vector3 MissionOrigin(Mission mission, out float scale)
    {
        if (mission.Center is not { } center)
        {
            scale = 1f;
            return Vector3.Zero;
        }

        scale = MoonDisplayScale(center);
        return center.PositionAt(DaysSinceJ2000, UnitsPerAu);
    }

    /// <summary>
    /// The craft's position in the world, exactly as drawn – with the
    /// planet's position and the moon system's compression included. Null
    /// when no trip is under way. The camera uses this to follow the craft
    /// down to the target on arrival.
    /// </summary>
    public Vector3? CraftPosition()
    {
        if (Mission is not { } mission)
            return null;

        var origin = MissionOrigin(mission, out float scale);
        double day = Math.Max(DaysSinceJ2000, mission.LaunchDay);
        return origin + mission.PositionAt(day, UnitsPerAu) * scale;
    }

    /// <summary>Draws part of the orbit as a polyline.</summary>
    void DrawMissionArc(ICanvas canvas, Mission mission, double fromDay, double toDay,
        Vector3 origin, float scale)
    {
        const int steps = 160;
        if (toDay - fromDay < 1e-6)
            return;

        var path = new PathF();
        bool started = false, hasPrev = false;
        float px = 0, py = 0;
        for (int i = 0; i <= steps; i++)
        {
            double day = fromDay + (toDay - fromDay) * i / steps;
            if (Camera.Project(origin + mission.TransferPositionAt(day, UnitsPerAu) * scale,
                    out float x, out float y, out _))
            {
                if (hasPrev)
                {
                    if (!started) { path.MoveTo(px, py); started = true; }
                    path.LineTo(x, y);
                }
                hasPrev = true;
                px = x;
                py = y;
            }
            else
            {
                hasPrev = false;
                started = false;
            }
        }
        if (started)
            canvas.DrawPath(path);
    }

    // --------------------------------------------------------------- labels

    /// <summary>
    /// Draws the names. When planets crowd together (e.g. the inner Solar
    /// System seen from far away), labels stack downward instead of
    /// overwriting each other, and one that's been moved gets a thin line
    /// back to its planet.
    /// </summary>
    static readonly Color LabelShadowColor = Colors.Black.WithAlpha(0.8f);
    static readonly Color LabelLineColor = Colors.White.WithAlpha(0.28f);

    void DrawLabels(ICanvas canvas, RectF rect)
    {
        const float lineHeight = 16f;
        const float minSeparation = 70f;

        canvas.FontSize = 13f;
        var placed = new List<(float X, float Y)>(_labels.Count);

        foreach (var (name, x, anchorY) in _labels.OrderBy(l => l.Y))
        {
            // Labels far outside the screen need neither stacking nor drawing.
            if (!(x > -200 && x < rect.Width + 200 && anchorY > -200 && anchorY < rect.Height + 200))
                continue;

            float y = anchorY;
            // A hard cap on the number of relocation passes: every pass
            // should move the label strictly downward, but floating-point
            // rounding in extreme cases must never be able to lock the UI
            // thread in an infinite loop again.
            for (int pass = 0; pass < 16; pass++)
            {
                bool moved = false;
                foreach (var (px, py) in placed)
                {
                    if (Math.Abs(px - x) < minSeparation && Math.Abs(py - y) < lineHeight &&
                        py + lineHeight > y)
                    {
                        y = py + lineHeight;
                        moved = true;
                    }
                }
                if (!moved)
                    break;
            }
            placed.Add((x, y));

            if (y - anchorY > 2f)
            {
                canvas.StrokeSize = 1f;
                canvas.StrokeColor = LabelLineColor;
                canvas.DrawLine(x, anchorY - 5, x, y + 1);
            }

            canvas.FontColor = LabelShadowColor;
            canvas.DrawString(name, x + 1, y + 13, HorizontalAlignment.Center);
            canvas.FontColor = Colors.White;
            canvas.DrawString(name, x, y + 12, HorizontalAlignment.Center);
        }
    }

    static Color Mix(Color a, Color b, float t) => new(
        a.Red + (b.Red - a.Red) * t,
        a.Green + (b.Green - a.Green) * t,
        a.Blue + (b.Blue - a.Blue) * t,
        1f);
}
