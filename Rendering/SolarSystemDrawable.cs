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

    Vector3[][]? _orbitPaths;
    (Vector3 Dir, float Size, float Alpha)[]? _stars;
    readonly List<(string Name, float X, float Y)> _labels = new(16);

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(rect);
        if (rect.Width < 10 || rect.Height < 10)
            return;

        Camera.UpdateFrame(rect.Width, rect.Height);
        _labels.Clear();

        DrawStars(canvas);
        if (ShowOrbits)
            DrawOrbits(canvas);
        DrawBodies(canvas, rect);
        DrawLabels(canvas);
    }

    // ---------------------------------------------------------------- stjärnor

    void DrawStars(ICanvas canvas)
    {
        _stars ??= CreateStars();
        foreach (var (dir, size, alpha) in _stars)
        {
            if (!Camera.ProjectDirection(dir, out float sx, out float sy))
                continue;
            canvas.FillColor = Colors.White.WithAlpha(alpha);
            canvas.FillCircle(sx, sy, size);
        }
    }

    static (Vector3, float, float)[] CreateStars()
    {
        var rnd = new Random(42);
        var list = new List<(Vector3, float, float)>(3200);

        // Jämnt spridda stjärnor över hela himlen.
        for (int i = 0; i < 1700; i++)
        {
            list.Add((RandomDirection(rnd),
                      0.5f + (float)rnd.NextDouble() * 1.1f,
                      0.25f + (float)rnd.NextDouble() * 0.75f));
        }

        // Ett tätare, svagare band av stjärnor som antyder Vintergatan
        // (galaktiska planet lutar ca 60° mot ekliptikan).
        var pole = Vector3.Normalize(new Vector3(0.55f, 0.50f, 0.67f));
        var u = Vector3.Normalize(Vector3.Cross(pole, Vector3.UnitY));
        var v = Vector3.Cross(pole, u);
        for (int i = 0; i < 1500; i++)
        {
            double ang = rnd.NextDouble() * Math.PI * 2;
            float off = Gauss(rnd) * 0.13f;
            var dir = Vector3.Normalize(
                u * MathF.Cos((float)ang) + v * MathF.Sin((float)ang) + pole * off);
            list.Add((dir,
                      0.4f + (float)rnd.NextDouble() * 0.8f,
                      0.10f + (float)rnd.NextDouble() * 0.45f));
        }
        return [.. list];
    }

    static Vector3 RandomDirection(Random rnd)
    {
        double z = rnd.NextDouble() * 2 - 1;
        double t = rnd.NextDouble() * Math.PI * 2;
        double r = Math.Sqrt(1 - z * z);
        return new Vector3((float)(r * Math.Cos(t)), (float)z, (float)(r * Math.Sin(t)));
    }

    static float Gauss(Random rnd) =>
        (float)(Math.Sqrt(-2.0 * Math.Log(1 - rnd.NextDouble())) *
                Math.Cos(2.0 * Math.PI * rnd.NextDouble()));

    // ------------------------------------------------------------------- banor

    void DrawOrbits(ICanvas canvas)
    {
        _orbitPaths ??= [.. SolarSystemData.Planets
            .Select(p => p.OrbitPath(OrbitSamples, UnitsPerAu))];

        canvas.StrokeSize = 1f;
        for (int i = 0; i < _orbitPaths.Length; i++)
        {
            canvas.StrokeColor = SolarSystemData.Planets[i].BodyColor.WithAlpha(0.35f);
            var pts = _orbitPaths[i];
            var path = new PathF();
            bool started = false;
            for (int k = 0; k <= pts.Length; k++)
            {
                var p = pts[k % pts.Length];
                if (Camera.Project(p, out float sx, out float sy, out _))
                {
                    if (!started) { path.MoveTo(sx, sy); started = true; }
                    else path.LineTo(sx, sy);
                }
                else
                {
                    started = false;
                }
            }
            canvas.DrawPath(path);
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
                    DrawSaturnRing(canvas, pos, worldR, depth, nearHalf: false);
                DrawPlanet(canvas, body, sx, sy, r, sunX, sunY);
                if (body.Name == "Saturnus")
                    DrawSaturnRing(canvas, pos, worldR, depth, nearHalf: true);
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

    /// <summary>
    /// Saturnus ringar som ett band i planetens ekvatorsplan. Ritas i två halvor:
    /// den bortre bakom planeten och den främre framför.
    /// </summary>
    void DrawSaturnRing(ICanvas canvas, Vector3 center, float worldR, float planetDepth, bool nearHalf)
    {
        float inner = worldR * 1.24f;
        float outer = worldR * 2.27f;
        if (Camera.ScreenRadius(outer, planetDepth) < 3f)
            return;

        float tilt = SaturnRingTiltDeg * MathF.PI / 180f;
        var normal = new Vector3(0, MathF.Cos(tilt), MathF.Sin(tilt));
        var u = Vector3.UnitX;
        var v = Vector3.Cross(normal, u);

        const int segments = 72;
        canvas.FillColor = Color.FromRgba(0.85f, 0.78f, 0.60f, 0.55f);

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
            if ((segDepth < planetDepth) != nearHalf)
                continue;

            var path = new PathF();
            path.MoveTo(x00, y00);
            path.LineTo(x01, y01);
            path.LineTo(x11, y11);
            path.LineTo(x10, y10);
            path.Close();
            canvas.FillPath(path);
        }
    }

    // --------------------------------------------------------------- etiketter

    void DrawLabels(ICanvas canvas)
    {
        canvas.FontSize = 13f;
        foreach (var (name, x, y) in _labels)
        {
            canvas.FontColor = Colors.Black.WithAlpha(0.8f);
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
