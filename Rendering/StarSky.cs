using System.Numerics;
using Solarsystem.Simulation;

namespace Solarsystem.Rendering;

/// <summary>
/// Stjärnhimlen: verkliga stjärnor ur katalogen, ett svagare bakgrundsbrus av
/// ljussvaga stjärnor och Vintergatans band längs det galaktiska planet.
/// Allt ritas "på oändligt avstånd", så himlen står stilla när kameran flyttas
/// mellan planeterna – precis som i verkligheten.
/// </summary>
public sealed class StarSky
{
    /// <summary>
    /// Stjärnor ljusare än så här får sitt namn utskrivet. Gränsen är satt så
    /// att Polstjärnan (magnitud 1,98) kommer med – den är inte särskilt ljus,
    /// men desto viktigare att kunna peka ut.
    /// </summary>
    const double NameMagnitudeLimit = 2.10;

    readonly record struct RenderStar(Vector3 Dir, Color Color, float Radius, bool Glow, string? Name);
    readonly record struct FaintStar(Vector3 Dir, float Radius, float Alpha);
    readonly record struct MilkyBlob(Vector3 Dir, float Radius, float Alpha);

    readonly RenderStar[] _stars;
    readonly FaintStar[] _faint;
    readonly MilkyBlob[] _milkyWay;
    readonly (Vector3 A, Vector3 B)[] _lines;
    readonly (string Name, Vector3 Dir)[] _constellationLabels;

    static readonly RadialGradientPaint MilkyPaint = new(
    [
        new PaintGradientStop(0f, Color.FromRgba(0.74f, 0.78f, 0.92f, 0.055f)),
        new PaintGradientStop(0.55f, Color.FromRgba(0.66f, 0.70f, 0.90f, 0.022f)),
        new PaintGradientStop(1f, Colors.Transparent),
    ])
    { Center = new Point(0.5, 0.5), Radius = 0.5 };

    public StarSky()
    {
        _stars = [.. StarCatalog.Stars.Select(ToRenderStar)];
        _faint = CreateFaintStars();
        _milkyWay = CreateMilkyWay();

        var byId = StarCatalog.Stars.ToDictionary(s => s.Id);
        _lines = [.. StarCatalog.Constellations
            .SelectMany(c => c.Lines)
            .Select(l => (byId[l.A].Direction, byId[l.B].Direction))];

        _constellationLabels = [.. StarCatalog.Constellations.Select(c =>
        {
            var sum = c.Lines
                .SelectMany<(string A, string B), string>(l => [l.A, l.B])
                .Distinct()
                .Aggregate(Vector3.Zero, (acc, id) => acc + byId[id].Direction);
            return (c.Name, Vector3.Normalize(sum));
        })];
    }

    public void Draw(ICanvas canvas, OrbitCamera camera, bool showConstellations, bool showStarNames)
    {
        DrawMilkyWay(canvas, camera);

        foreach (var (dir, radius, alpha) in _faint)
        {
            if (!camera.ProjectDirection(dir, out float x, out float y))
                continue;
            canvas.FillColor = Colors.White.WithAlpha(alpha);
            canvas.FillCircle(x, y, radius);
        }

        if (showConstellations)
            DrawConstellations(canvas, camera);

        foreach (var star in _stars)
        {
            if (!camera.ProjectDirection(star.Dir, out float x, out float y))
                continue;

            if (star.Glow)
            {
                canvas.FillColor = star.Color.WithAlpha(0.16f);
                canvas.FillCircle(x, y, star.Radius * 2.6f);
            }
            canvas.FillColor = star.Color;
            canvas.FillCircle(x, y, star.Radius);
        }

        if (showStarNames)
            DrawStarNames(canvas, camera);
    }

    // ------------------------------------------------------------- Vintergatan

    void DrawMilkyWay(ICanvas canvas, OrbitCamera camera)
    {
        foreach (var (dir, radius, alpha) in _milkyWay)
        {
            if (!camera.ProjectDirection(dir, out float x, out float y))
                continue;
            // Blobbarnas storlek följer projektionen så att bandet håller ihop.
            float r = camera.ScreenRadius(radius, 1f);
            canvas.Alpha = alpha;
            canvas.SetFillPaint(MilkyPaint, new RectF(x - r, y - r, r * 2, r * 2));
            canvas.FillCircle(x, y, r);
        }
        canvas.Alpha = 1f;
    }

    static MilkyBlob[] CreateMilkyWay()
    {
        var rnd = new Random(7);
        var (u, v, pole) = GalacticBasis();
        var blobs = new List<MilkyBlob>(260);

        for (int i = 0; i < 260; i++)
        {
            double lon = rnd.NextDouble() * Math.PI * 2;
            float offset = Gauss(rnd) * 0.075f;
            var dir = Vector3.Normalize(
                u * (float)Math.Cos(lon) + v * (float)Math.Sin(lon) + pole * offset);

            // Bandet är ljusast mot galaktiska centrum (longitud 0) och tunnas
            // ut mot ytterkanterna, så att det blir ett band och inte moln.
            float towardCenter = (float)((Math.Cos(lon) + 1.0) * 0.5);
            float alpha = (0.30f + towardCenter * 0.70f) * MathF.Exp(-offset * offset * 45f);
            blobs.Add(new MilkyBlob(dir, 0.045f + (float)rnd.NextDouble() * 0.045f, alpha));
        }
        return [.. blobs];
    }

    /// <summary>Ortogonal bas där u pekar mot galaktiska centrum och pole mot galaktiska nordpolen.</summary>
    static (Vector3 U, Vector3 V, Vector3 Pole) GalacticBasis()
    {
        var pole = StarCatalog.GalacticNorthPole;
        var center = StarCatalog.GalacticCenter;
        var u = Vector3.Normalize(center - pole * Vector3.Dot(center, pole));
        return (u, Vector3.Cross(pole, u), pole);
    }

    // ---------------------------------------------------------- katalogstjärnor

    static RenderStar ToRenderStar(Star s)
    {
        // Ljusstarka stjärnor ritas större; skalan är komprimerad så att
        // Sirius inte blir en skiva medan magnitud 4 fortfarande syns.
        float radius = Math.Clamp((float)(2.6 - s.Magnitude * 0.42), 0.7f, 3.4f);
        return new RenderStar(
            s.Direction,
            ColorFromColorIndex(s.ColorIndex, s.Magnitude),
            radius,
            Glow: s.Magnitude < 1.6,
            s.ProperName);
    }

    /// <summary>
    /// Färgindex B-V -> ungefärlig yttemperatur -> RGB. Ger blåvita jättar som
    /// Rigel och röda som Betelgeuse, precis som på himlen.
    /// </summary>
    static Color ColorFromColorIndex(double bv, double magnitude)
    {
        double t = 4600.0 * (1.0 / (0.92 * bv + 1.7) + 1.0 / (0.92 * bv + 0.62));
        double k = Math.Clamp(t, 1500.0, 40000.0) / 100.0;

        double r = k <= 66 ? 255 : 329.698727446 * Math.Pow(k - 60, -0.1332047592);
        double g = k <= 66
            ? 99.4708025861 * Math.Log(k) - 161.1195681661
            : 288.1221695283 * Math.Pow(k - 60, -0.0755148492);
        double b = k >= 66 ? 255
            : k <= 19 ? 0
            : 138.5177312231 * Math.Log(k - 10) - 305.0447927307;

        var color = new Color(
            (float)Math.Clamp(r / 255.0, 0, 1),
            (float)Math.Clamp(g / 255.0, 0, 1),
            (float)Math.Clamp(b / 255.0, 0, 1));

        // Ljussvaga stjärnor tonas ner; ögat ser dem ändå nästan färglösa.
        float alpha = (float)Math.Clamp(1.15 - magnitude * 0.16, 0.42, 1.0);
        return color.WithAlpha(alpha);
    }

    // ------------------------------------------------------------ bakgrundsbrus

    static FaintStar[] CreateFaintStars()
    {
        var rnd = new Random(42);
        var (u, v, pole) = GalacticBasis();
        var list = new List<FaintStar>(2400);

        // Jämnt spridda ljussvaga stjärnor över hela himlen.
        for (int i = 0; i < 1500; i++)
        {
            list.Add(new FaintStar(
                RandomDirection(rnd),
                0.4f + (float)rnd.NextDouble() * 0.6f,
                0.12f + (float)rnd.NextDouble() * 0.38f));
        }

        // Extra täthet längs det galaktiska planet – där ligger de flesta stjärnorna.
        for (int i = 0; i < 1900; i++)
        {
            double lon = rnd.NextDouble() * Math.PI * 2;
            float offset = Gauss(rnd) * 0.07f;
            var dir = Vector3.Normalize(
                u * (float)Math.Cos(lon) + v * (float)Math.Sin(lon) + pole * offset);
            list.Add(new FaintStar(
                dir,
                0.35f + (float)rnd.NextDouble() * 0.5f,
                0.10f + (float)rnd.NextDouble() * 0.32f));
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

    // -------------------------------------------------------------- stjärnbilder

    void DrawConstellations(ICanvas canvas, OrbitCamera camera)
    {
        canvas.StrokeSize = 1f;
        canvas.StrokeColor = Color.FromRgba(0.45f, 0.65f, 0.95f, 0.30f);

        foreach (var (a, b) in _lines)
            DrawGreatCircleSegment(canvas, camera, a, b);

        canvas.FontSize = 11f;
        canvas.FontColor = Color.FromRgba(0.45f, 0.62f, 0.85f, 0.65f);
        foreach (var (name, dir) in _constellationLabels)
        {
            if (camera.ProjectDirection(dir, out float x, out float y))
                canvas.DrawString(name, x, y, HorizontalAlignment.Center);
        }
    }

    /// <summary>
    /// Ritar en linje mellan två himmelsriktningar. Bågen delas upp i steg så att
    /// långa linjer följer himmelssfären i stället för att skära rakt igenom den.
    /// </summary>
    static void DrawGreatCircleSegment(ICanvas canvas, OrbitCamera camera, Vector3 a, Vector3 b)
    {
        const int steps = 8;
        float px = 0, py = 0;
        bool hasPrev = false;

        for (int i = 0; i <= steps; i++)
        {
            var dir = Vector3.Normalize(Vector3.Lerp(a, b, i / (float)steps));
            if (camera.ProjectDirection(dir, out float x, out float y))
            {
                if (hasPrev)
                    canvas.DrawLine(px, py, x, y);
                px = x; py = y; hasPrev = true;
            }
            else
            {
                hasPrev = false;
            }
        }
    }

    void DrawStarNames(ICanvas canvas, OrbitCamera camera)
    {
        canvas.FontSize = 11f;
        canvas.FontColor = Color.FromRgba(0.80f, 0.86f, 0.95f, 0.75f);

        foreach (var star in StarCatalog.Stars)
        {
            if (star.ProperName is null || star.Magnitude > NameMagnitudeLimit)
                continue;
            if (camera.ProjectDirection(star.Direction, out float x, out float y))
                canvas.DrawString(star.ProperName, x + 7, y + 4, HorizontalAlignment.Left);
        }
    }
}
