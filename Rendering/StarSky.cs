using System.Numerics;
using Solarsystem.Simulation;

namespace Solarsystem.Rendering;

/// <summary>Hur många stjärnor som ritas – en inställning i appen.</summary>
public enum StarDensity
{
    /// <summary>
    /// Ingen stjärnhimmel alls – helt svart bakgrund. Bra när man vill att
    /// eleverna ska titta enbart på solsystemet utan att distraheras.
    /// </summary>
    None = 0,
    /// <summary>Endast katalogens riktiga, namngivna stjärnor.</summary>
    Low = 1,
    /// <summary>Riktiga stjärnor plus ett måttligt bakgrundsbrus och Vintergatan.</summary>
    Medium = 2,
    /// <summary>Full stjärnhimmel.</summary>
    High = 3,
}

/// <summary>
/// Stjärnhimlen: verkliga stjärnor ur katalogen, ett svagare bakgrundsbrus och
/// Vintergatans band längs det galaktiska planet.
///
/// Prestanda: himlen ligger "på oändligt avstånd", så skärmpositionerna beror
/// bara på kamerans riktning och vyns storlek – inte på zoom, panorering mot
/// mål eller planeternas rörelse. Alla positioner cachas därför och räknas om
/// först när riktningen faktiskt ändras. Alla färger är förberäknade och allt
/// som hamnar utanför skärmen hoppas över.
/// </summary>
public sealed class StarSky
{
    /// <summary>
    /// Stjärnor ljusare än så här får sitt namn utskrivet. Gränsen är satt så
    /// att Polstjärnan (magnitud 1,98) kommer med – den är inte särskilt ljus,
    /// men desto viktigare att kunna peka ut.
    /// </summary>
    const double NameMagnitudeLimit = 2.10;

    /// <summary>Marginal utanför skärmkanten innan något kastas bort.</summary>
    const float Margin = 80f;

    /// <summary>
    /// En katalogstjärna färdig att rita. <c>NameKey</c> är en nyckel och inte en
    /// färdig text: namnet slås upp när stjärnan ritas, så att ett språkbyte
    /// syns direkt utan att hela himlen byggs om.
    /// </summary>
    readonly record struct CatalogEntry(
        Vector3 Dir, Color Color, Color GlowColor, float Radius, bool Glow, string? NameKey);
    readonly record struct PointEntry(Vector3 Dir, Color Color, float Radius);
    readonly record struct BlobEntry(Vector3 Dir, Color Color, float WorldRadius);

    readonly CatalogEntry[] _catalog;
    readonly PointEntry[] _uniform;   // jämnt spridda svaga stjärnor
    readonly PointEntry[] _band;      // svaga stjärnor längs Vintergatan
    readonly BlobEntry[] _blobs;      // Vintergatans mjuka glöd
    readonly (Vector3 A, Vector3 B)[] _lines;
    readonly (string Key, Vector3 Dir)[] _constellationLabels;

    public StarDensity Density { get; set; } = StarDensity.Medium;

    // ---------------- positionscache (ogiltig först när kameran roteras) ----
    float _sigYaw = float.NaN, _sigPitch, _sigFocal, _sigW, _sigH;
    readonly PointF[] _catalogPos;
    readonly bool[] _catalogVis;
    readonly PointF[] _uniformPos;
    readonly bool[] _uniformVis;
    readonly PointF[] _bandPos;
    readonly bool[] _bandVis;
    readonly PointF[] _blobPos;
    readonly bool[] _blobVis;
    readonly float[] _blobRadius;
    readonly PointF[] _labelPos;
    readonly bool[] _labelVis;
    PathF _linesPath = new();
    bool _linesAny;

    static readonly Color LineColor = Color.FromRgba(0.45f, 0.65f, 0.95f, 0.30f);
    static readonly Color ConstellationLabelColor = Color.FromRgba(0.45f, 0.62f, 0.85f, 0.65f);
    static readonly Color StarNameColor = Color.FromRgba(0.80f, 0.86f, 0.95f, 0.75f);

    public StarSky()
    {
        _catalog = [.. StarCatalog.Stars
            .OrderBy(s => s.Magnitude)
            .Select(ToCatalogEntry)];
        (_uniform, _band) = CreateFaintStars();
        _blobs = CreateMilkyWay();

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
            return (c.Key, Vector3.Normalize(sum));
        })];

        _catalogPos = new PointF[_catalog.Length];
        _catalogVis = new bool[_catalog.Length];
        _uniformPos = new PointF[_uniform.Length];
        _uniformVis = new bool[_uniform.Length];
        _bandPos = new PointF[_band.Length];
        _bandVis = new bool[_band.Length];
        _blobPos = new PointF[_blobs.Length];
        _blobVis = new bool[_blobs.Length];
        _blobRadius = new float[_blobs.Length];
        _labelPos = new PointF[_constellationLabels.Length];
        _labelVis = new bool[_constellationLabels.Length];
    }

    public void Draw(ICanvas canvas, OrbitCamera camera, RectF rect,
        bool showConstellations, bool showStarNames)
    {
        // Vid "Inga" ritas ingenting alls – varken stjärnor, stjärnbilder eller
        // Vintergata. Linjer mellan osynliga stjärnor vore ändå meningslösa.
        if (Density == StarDensity.None)
            return;

        RefreshCache(camera, rect);

        (int uniformCount, int bandCount, int blobCount) = Density switch
        {
            StarDensity.Low => (0, 0, 0),
            StarDensity.High => (_uniform.Length, _band.Length, _blobs.Length),
            _ => (Math.Min(900, _uniform.Length), Math.Min(800, _band.Length),
                  Math.Min(40, _blobs.Length)),
        };

        // Vintergatans glöd (längst bak). Tre koncentriska, mycket svaga cirklar
        // per fläck ger en mjuk avtoning utan dyra gradientpenslar.
        for (int i = 0; i < blobCount; i++)
        {
            if (!_blobVis[i])
                continue;
            float x = _blobPos[i].X, y = _blobPos[i].Y, r = _blobRadius[i];
            canvas.FillColor = _blobs[i].Color;
            canvas.FillCircle(x, y, r);
            canvas.FillCircle(x, y, r * 0.68f);
            canvas.FillCircle(x, y, r * 0.42f);
        }

        // Svaga bakgrundsstjärnor.
        for (int i = 0; i < uniformCount; i++)
        {
            if (!_uniformVis[i])
                continue;
            canvas.FillColor = _uniform[i].Color;
            canvas.FillCircle(_uniformPos[i].X, _uniformPos[i].Y, _uniform[i].Radius);
        }
        for (int i = 0; i < bandCount; i++)
        {
            if (!_bandVis[i])
                continue;
            canvas.FillColor = _band[i].Color;
            canvas.FillCircle(_bandPos[i].X, _bandPos[i].Y, _band[i].Radius);
        }

        if (showConstellations)
        {
            if (_linesAny)
            {
                canvas.StrokeSize = 1f;
                canvas.StrokeColor = LineColor;
                canvas.DrawPath(_linesPath);
            }
            canvas.FontSize = 11f;
            canvas.FontColor = ConstellationLabelColor;
            for (int i = 0; i < _constellationLabels.Length; i++)
            {
                if (_labelVis[i])
                    canvas.DrawString(Strings.Name(_constellationLabels[i].Key),
                        _labelPos[i].X, _labelPos[i].Y, HorizontalAlignment.Center);
            }
        }

        // Katalogens riktiga stjärnor (alltid alla – de är få och billiga).
        for (int i = 0; i < _catalog.Length; i++)
        {
            if (!_catalogVis[i])
                continue;
            var star = _catalog[i];
            var p = _catalogPos[i];
            if (star.Glow)
            {
                canvas.FillColor = star.GlowColor;
                canvas.FillCircle(p.X, p.Y, star.Radius * 2.6f);
            }
            canvas.FillColor = star.Color;
            canvas.FillCircle(p.X, p.Y, star.Radius);
        }

        if (showStarNames)
        {
            canvas.FontSize = 11f;
            canvas.FontColor = StarNameColor;
            for (int i = 0; i < _catalog.Length; i++)
            {
                // De flesta stjärnnamn är internationella – Betelgeuse heter så
                // överallt – så uppslagningen faller tillbaka på nyckeln själv.
                // Bara Plejaderna och Polstjärnan står i resursfilen.
                if (_catalogVis[i] && _catalog[i].NameKey is string key)
                    canvas.DrawString(Strings.Name(key),
                        _catalogPos[i].X + 7, _catalogPos[i].Y + 4, HorizontalAlignment.Left);
            }
        }
    }

    // --------------------------------------------------------------- cachning

    void RefreshCache(OrbitCamera camera, RectF rect)
    {
        if (camera.Yaw == _sigYaw && camera.Pitch == _sigPitch &&
            camera.Focal == _sigFocal && rect.Width == _sigW && rect.Height == _sigH)
            return;

        _sigYaw = camera.Yaw;
        _sigPitch = camera.Pitch;
        _sigFocal = camera.Focal;
        _sigW = rect.Width;
        _sigH = rect.Height;

        float maxX = rect.Width + Margin;
        float maxY = rect.Height + Margin;

        for (int i = 0; i < _catalog.Length; i++)
        {
            _catalogVis[i] = ProjectVisible(camera, _catalog[i].Dir, maxX, maxY, out var p);
            _catalogPos[i] = p;
        }
        for (int i = 0; i < _uniform.Length; i++)
        {
            _uniformVis[i] = ProjectVisible(camera, _uniform[i].Dir, maxX, maxY, out var p);
            _uniformPos[i] = p;
        }
        for (int i = 0; i < _band.Length; i++)
        {
            _bandVis[i] = ProjectVisible(camera, _band[i].Dir, maxX, maxY, out var p);
            _bandPos[i] = p;
        }
        for (int i = 0; i < _blobs.Length; i++)
        {
            float r = _blobs[i].WorldRadius * camera.Focal;
            _blobRadius[i] = r;
            bool ok = camera.ProjectDirection(_blobs[i].Dir, out float x, out float y);
            _blobVis[i] = ok && x > -Margin - r && x < maxX + r && y > -Margin - r && y < maxY + r;
            _blobPos[i] = new PointF(x, y);
        }
        for (int i = 0; i < _constellationLabels.Length; i++)
        {
            _labelVis[i] = ProjectVisible(camera, _constellationLabels[i].Dir, maxX, maxY, out var p);
            _labelPos[i] = p;
        }

        RebuildLinesPath(camera, maxX, maxY);
    }

    static bool ProjectVisible(OrbitCamera camera, Vector3 dir, float maxX, float maxY, out PointF p)
    {
        bool ok = camera.ProjectDirection(dir, out float x, out float y);
        p = new PointF(x, y);
        return ok && x > -Margin && x < maxX && y > -Margin && y < maxY;
    }

    /// <summary>
    /// Bygger alla stjärnbildslinjer som en enda figur. Varje linje delas upp i
    /// steg så att långa linjer följer himmelssfären i stället för att skära
    /// rakt igenom den.
    /// </summary>
    void RebuildLinesPath(OrbitCamera camera, float maxX, float maxY)
    {
        const int steps = 8;
        var path = new PathF();
        _linesAny = false;

        foreach (var (a, b) in _lines)
        {
            // En delfigur får bara börja när minst två punkter i rad är synliga –
            // en ensam MoveTo utan efterföljande LineTo är ogiltig i Win2D.
            bool started = false, hasPrev = false;
            float px = 0, py = 0;
            for (int i = 0; i <= steps; i++)
            {
                var dir = Vector3.Normalize(Vector3.Lerp(a, b, i / (float)steps));
                if (camera.ProjectDirection(dir, out float x, out float y) &&
                    x > -Margin && x < maxX && y > -Margin && y < maxY)
                {
                    if (hasPrev)
                    {
                        if (!started) { path.MoveTo(px, py); started = true; }
                        path.LineTo(x, y);
                        _linesAny = true;
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
        }
        _linesPath = path;
    }

    // ---------------------------------------------------------- uppbyggnad

    static CatalogEntry ToCatalogEntry(Star s)
    {
        // Ljusstarka stjärnor ritas större; skalan är komprimerad så att
        // Sirius inte blir en skiva medan magnitud 4 fortfarande syns.
        float radius = Math.Clamp((float)(2.6 - s.Magnitude * 0.42), 0.7f, 3.4f);
        var color = ColorFromColorIndex(s.ColorIndex, s.Magnitude);
        string? key = s.Magnitude <= NameMagnitudeLimit ? s.NameKey : null;
        return new CatalogEntry(s.Direction, color, color.WithAlpha(0.16f),
            radius, Glow: s.Magnitude < 1.6, key);
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

    static (PointEntry[] Uniform, PointEntry[] Band) CreateFaintStars()
    {
        var rnd = new Random(42);
        var (u, v, pole) = GalacticBasis();

        var uniform = new PointEntry[1500];
        for (int i = 0; i < uniform.Length; i++)
        {
            uniform[i] = new PointEntry(
                RandomDirection(rnd),
                Colors.White.WithAlpha(0.12f + (float)rnd.NextDouble() * 0.38f),
                0.4f + (float)rnd.NextDouble() * 0.6f);
        }

        // Extra täthet längs det galaktiska planet – där ligger de flesta stjärnorna.
        var band = new PointEntry[1900];
        for (int i = 0; i < band.Length; i++)
        {
            double lon = rnd.NextDouble() * Math.PI * 2;
            float offset = Gauss(rnd) * 0.07f;
            var dir = Vector3.Normalize(
                u * (float)Math.Cos(lon) + v * (float)Math.Sin(lon) + pole * offset);
            band[i] = new PointEntry(
                dir,
                Colors.White.WithAlpha(0.10f + (float)rnd.NextDouble() * 0.32f),
                0.35f + (float)rnd.NextDouble() * 0.5f);
        }
        return (uniform, band);
    }

    static BlobEntry[] CreateMilkyWay()
    {
        var rnd = new Random(7);
        var (u, v, pole) = GalacticBasis();
        var blobs = new BlobEntry[80];

        for (int i = 0; i < blobs.Length; i++)
        {
            double lon = rnd.NextDouble() * Math.PI * 2;
            float offset = Gauss(rnd) * 0.075f;
            var dir = Vector3.Normalize(
                u * (float)Math.Cos(lon) + v * (float)Math.Sin(lon) + pole * offset);

            // Bandet är ljusast mot galaktiska centrum (longitud 0) och tunnas
            // ut mot kanterna. Mjukheten kommer av att många mycket svaga
            // cirklar överlappar – inga gradienter behövs.
            float towardCenter = (float)((Math.Cos(lon) + 1.0) * 0.5);
            float alpha = (0.010f + towardCenter * 0.014f) *
                          MathF.Exp(-offset * offset * 45f);
            blobs[i] = new BlobEntry(
                dir,
                Color.FromRgba(0.72f, 0.76f, 0.92f, MathF.Max(alpha, 0.004f)),
                0.05f + (float)rnd.NextDouble() * 0.05f);
        }
        return blobs;
    }

    /// <summary>Ortogonal bas där u pekar mot galaktiska centrum och pole mot galaktiska nordpolen.</summary>
    static (Vector3 U, Vector3 V, Vector3 Pole) GalacticBasis()
    {
        var pole = StarCatalog.GalacticNorthPole;
        var center = StarCatalog.GalacticCenter;
        var u = Vector3.Normalize(center - pole * Vector3.Dot(center, pole));
        return (u, Vector3.Cross(pole, u), pole);
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
}
