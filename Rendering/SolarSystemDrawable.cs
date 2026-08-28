using System.Numerics;
using Solarsystem.Simulation;

namespace Solarsystem.Rendering;

/// <summary>
/// Ritar hela scenen: stjärnhimmel, banor, solen, planeter (djupsorterade,
/// skuggade mot solen), Saturnus ringar samt namnetiketter.
/// </summary>
public sealed class SolarSystemDrawable : IDrawable
{
    /// <summary>Världsskala: 1 AU = 60 enheter. Avstånden är alltid skalenliga.</summary>
    public const float UnitsPerAu = 60f;

    // Med helt verklig skala är planeterna mindre än en pixel, därför kan de
    // förstoras (inbördes fortfarande skalenliga). Solen förstoras mindre för
    // att inte sluka Merkurius bana.
    const float PlanetBoost = 1000f;
    const float SunBoost = 30f;

    const int OrbitSamples = 240;
    const float SaturnRingTiltDeg = 26.73f;

    public OrbitCamera Camera { get; } = new();
    public double DaysSinceJ2000 { get; set; }
    public bool ShowOrbits { get; set; } = true;
    public bool RealScale { get; set; }
    public bool ShowConstellations { get; set; } = true;
    public bool ShowStarNames { get; set; }

    /// <summary>Hur många stjärnor som ritas (inställning i appen).</summary>
    public StarDensity StarDensity
    {
        get => _sky.Density;
        set => _sky.Density = value;
    }

    readonly StarSky _sky = new();
    Vector3[][]? _orbitPaths;
    readonly List<(string Name, float X, float Y)> _labels = new(16);

    // Cache av banornas skärmfigurer – giltig tills kameran flyttas.
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
            if (rect.Width < 10 || rect.Height < 10)
                return;

            Camera.UpdateFrame(rect.Width, rect.Height);
            _labels.Clear();

            _sky.Draw(canvas, Camera, rect, ShowConstellations, ShowStarNames);
            if (ShowOrbits)
                DrawOrbits(canvas, rect);
            DrawBodies(canvas, rect);
            DrawLabels(canvas);
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "solarsystem-draw.log"),
                $"[{DateTime.Now:HH:mm:ss}] {ex}\n\n");
            throw;
        }
    }

    // ------------------------------------------------------------------- banor

    void DrawOrbits(ICanvas canvas, RectF rect)
    {
        _orbitPaths ??= [.. SolarSystemData.Planets
            .Select(p => p.OrbitPath(OrbitSamples, UnitsPerAu))];

        // Banorna ligger stilla i världen, så deras skärmfigurer behöver bara
        // byggas om när kameran faktiskt flyttas.
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
                // En delfigur får bara börja när minst två punkter i rad är
                // synliga – en ensam MoveTo utan LineTo är ogiltig i Win2D.
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

    // ------------------------------------------------------------ himlakroppar

    void DrawBodies(ICanvas canvas, RectF rect)
    {
        double t = DaysSinceJ2000;

        // Solens skärmposition behövs för planeternas ljussättning.
        bool sunVisible = Camera.Project(Vector3.Zero, out float sunX, out float sunY, out float sunDepth);

        var bodies = new List<(CelestialBody? Body, Vector3 Pos, float WorldRadius, float Depth, float Sx, float Sy)>(9);

        float sunWorldR = VisualRadius(SolarSystemData.SunRadiusKm, isSun: true);
        if (sunVisible)
            bodies.Add((null, Vector3.Zero, sunWorldR, sunDepth, sunX, sunY));

        foreach (var planet in SolarSystemData.Planets)
        {
            var pos = planet.PositionAt(t, UnitsPerAu);
            if (!Camera.Project(pos, out float sx, out float sy, out float depth))
                continue;
            bodies.Add((planet, pos, VisualRadius(planet.RadiusKm, isSun: false), depth, sx, sy));
        }

        // Måla bakifrån och fram (painter's algorithm).
        bodies.Sort((a, b) => b.Depth.CompareTo(a.Depth));

        foreach (var (body, pos, worldR, depth, sx, sy) in bodies)
        {
            float r = Camera.ScreenRadius(worldR, depth);
            if (body is null)
            {
                r = MathF.Max(r, RealScale ? 1.2f : 2.5f);
                DrawSun(canvas, sx, sy, r);
                _labels.Add(("Solen", sx, sy + r + 6));
            }
            else
            {
                r = MathF.Max(r, RealScale ? 0.45f : 1.1f);
                if (body.Name == "Saturnus")
                {
                    BuildSaturnRingPaths(pos, worldR, depth, out var farRing, out var nearRing);
                    FillRing(canvas, farRing);
                    DrawPlanet(canvas, body, sx, sy, r, sunX, sunY);
                    FillRing(canvas, nearRing);
                }
                else
                {
                    DrawPlanet(canvas, body, sx, sy, r, sunX, sunY);
                }
                _labels.Add((body.Name, sx, sy + r + 6));
            }
        }
    }

    float VisualRadius(double radiusKm, bool isSun)
    {
        float real = (float)(radiusKm / SolarSystemData.AuKm) * UnitsPerAu;
        if (RealScale)
            return real;
        return real * (isSun ? SunBoost : PlanetBoost);
    }

    static void DrawSun(ICanvas canvas, float x, float y, float r)
    {
        // Yttre glöd.
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

        // Solskivan.
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
    }

    static void DrawPlanet(ICanvas canvas, CelestialBody body, float x, float y, float r, float sunX, float sunY)
    {
        if (r < 1.6f)
        {
            // För liten för skuggning – rita en enkel punkt.
            canvas.FillColor = body.BodyColor;
            canvas.FillCircle(x, y, r);
            return;
        }

        // Ljusare på sidan som vetter mot solen.
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

    static readonly Color RingColor = Color.FromRgba(0.85f, 0.78f, 0.60f, 0.55f);

    static void FillRing(ICanvas canvas, PathF? ring)
    {
        if (ring is null)
            return;
        canvas.FillColor = RingColor;
        canvas.FillPath(ring);
    }

    /// <summary>
    /// Saturnus ringar som ett band i planetens ekvatorsplan, uppdelat i två
    /// figurer: den bortre halvan (ritas bakom planeten) och den främre (ritas
    /// framför). Segmenten samlas i två banor så att hela ringen fylls med två
    /// anrop i stället för ett per segment.
    /// </summary>
    void BuildSaturnRingPaths(Vector3 center, float worldR, float planetDepth,
        out PathF? farRing, out PathF? nearRing)
    {
        farRing = nearRing = null;

        float inner = worldR * 1.24f;
        float outer = worldR * 2.27f;
        if (Camera.ScreenRadius(outer, planetDepth) < 3f)
            return;

        float tilt = SaturnRingTiltDeg * MathF.PI / 180f;
        var normal = new Vector3(0, MathF.Cos(tilt), MathF.Sin(tilt));
        var u = Vector3.UnitX;
        var v = Vector3.Cross(normal, u);

        const int segments = 72;
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * MathF.PI * 2f / segments;
            float a1 = (i + 1.05f) * MathF.PI * 2f / segments; // liten överlappning mot skarvar
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

    // --------------------------------------------------------------- etiketter

    /// <summary>
    /// Ritar namnen. När planeterna trängs ihop (t.ex. det inre solsystemet sett
    /// på långt håll) staplas etiketterna nedåt i stället för att skriva över
    /// varandra, och den som flyttats får en tunn streckad linje till sin planet.
    /// </summary>
    static readonly Color LabelShadowColor = Colors.Black.WithAlpha(0.8f);
    static readonly Color LabelLineColor = Colors.White.WithAlpha(0.28f);

    void DrawLabels(ICanvas canvas)
    {
        const float lineHeight = 16f;
        const float minSeparation = 70f;

        canvas.FontSize = 13f;
        var placed = new List<(float X, float Y)>(_labels.Count);

        foreach (var (name, x, anchorY) in _labels.OrderBy(l => l.Y))
        {
            float y = anchorY;
            for (bool moved = true; moved;)
            {
                moved = false;
                foreach (var (px, py) in placed)
                {
                    if (Math.Abs(px - x) < minSeparation && Math.Abs(py - y) < lineHeight)
                    {
                        y = py + lineHeight;
                        moved = true;
                    }
                }
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
