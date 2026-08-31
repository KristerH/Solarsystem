using System.Globalization;
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

    public OrbitCamera Camera { get; } = new();
    public double DaysSinceJ2000 { get; set; }
    public bool ShowOrbits { get; set; } = true;
    public bool RealScale { get; set; }
    public bool ShowConstellations { get; set; } = true;
    public bool ShowStarNames { get; set; }

    /// <summary>Om planeternas månar ska ritas alls.</summary>
    public bool ShowMoons { get; set; } = true;

    /// <summary>Om asteroidbältet mellan Mars och Jupiter ska ritas.</summary>
    public bool ShowAsteroidBelt { get; set; }

    /// <summary>Pågående rymdfärd, eller null när ingen farkost är på väg.</summary>
    public Mission? Mission { get; set; }

    /// <summary>Om Kuiperbältet bortom Neptunus ska ritas.</summary>
    public bool ShowKuiperBelt { get; set; }

    /// <summary>Om de verkliga rymdsonderna och deras spår ska ritas.</summary>
    public bool ShowProbes { get; set; } = true;

    /// <summary>
    /// Sonden som är vald i fokusväljaren, eller null. Dess milstolpar skrivs
    /// ut med planetnamn och datum; de övrigas markeras bara med årtal, annars
    /// blir vyn full av text.
    /// </summary>
    public Probe? FocusedProbe { get; set; }

    /// <summary>
    /// Sant medan fönstret håller på att ändra storlek. Då ritas bara svart –
    /// plattformen ritar om vid varje storlekssteg, och att projicera om hela
    /// scenen för varje sådant steg är det som annars fryser fönsterhanteraren.
    /// </summary>
    public bool Suspended { get; set; }

    /// <summary>Hur många stjärnor som ritas (inställning i appen).</summary>
    public StarDensity StarDensity
    {
        get => _sky.Density;
        set => _sky.Density = value;
    }

    readonly StarSky _sky = new();

    // Bältet byggs först när det efterfrågas, så att appen startar lika snabbt
    // som förut för den som aldrig slår på det.
    SmallBodyBelt? _asteroids, _kuiper;
    Vector3[]? _asteroidPositions, _kuiperPositions;
    double _asteroidTime = double.NaN, _kuiperTime = double.NaN;

    // Asteroider är steniga och gråbruna; Kuiperkropparna är isiga och kallare i tonen.
    static readonly Color AsteroidColor = Color.FromArgb("#B4A794");
    static readonly Color KuiperColor = Color.FromArgb("#A9BCC8");
    static readonly Color CeresLabelColor = Color.FromRgba(0.82f, 0.78f, 0.72f, 0.85f);
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
            // Negerad jämförelse så att även NaN-mått (mitt under en pågående
            // storleksändring) stoppas här.
            if (Suspended || !(rect.Width >= 10 && rect.Height >= 10))
                return;

            Camera.UpdateFrame(rect.Width, rect.Height);
            _labels.Clear();
            _milestoneText.Clear();

            _sky.Draw(canvas, Camera, rect, ShowConstellations, ShowStarNames);
            if (ShowOrbits)
                DrawOrbits(canvas, rect);
            if (ShowKuiperBelt)
            {
                _kuiper ??= SmallBodyBelt.CreateKuiperBelt(KuiperCount, UnitsPerAu);
                _kuiperPositions ??= new Vector3[_kuiper.Bodies.Length];
                DrawBelt(canvas, rect, _kuiper, _kuiperPositions, KuiperColor,
                    ref _kuiperTime, 1.2f);
            }
            if (ShowAsteroidBelt)
                DrawAsteroidBelt(canvas, rect);
            DrawBodies(canvas, rect);
            if (ShowProbes)
                DrawProbes(canvas);
            if (Mission is not null)
                DrawMission(canvas, Mission);
            DrawLabels(canvas, rect);
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

    // ------------------------------------------------------------- bältena

    /// <summary>Hur många kroppar som ritas i respektive bälte.</summary>
    const int AsteroidCount = 1400;
    const int KuiperCount = 1100;

    void DrawAsteroidBelt(ICanvas canvas, RectF rect)
    {
        _asteroids ??= SmallBodyBelt.CreateAsteroidBelt(AsteroidCount, UnitsPerAu);
        _asteroidPositions ??= new Vector3[_asteroids.Bodies.Length];
        DrawBelt(canvas, rect, _asteroids, _asteroidPositions, AsteroidColor,
            ref _asteroidTime, 1.1f);

        // Ceres är så mycket större än allt annat i bältet att den får namn.
        var ceres = SolarSystemData.Ceres.PositionAt(DaysSinceJ2000, UnitsPerAu);
        if (Camera.Project(ceres, out float cx, out float cy, out _))
        {
            canvas.FillColor = SolarSystemData.Ceres.BodyColor;
            canvas.FillCircle(cx, cy, 2.6f);
            canvas.FontSize = 11f;
            canvas.FontColor = CeresLabelColor;
            canvas.DrawString("Ceres", cx, cy + 16f, HorizontalAlignment.Center);
        }
    }

    /// <summary>
    /// Ritar ett bälte som ett fint stoft av prickar. Varje kropp får sin plats
    /// ur en egen Kepler-bana, så att bältet roterar med inre varv snabbare än
    /// yttre precis som i verkligheten. Prickar utanför bildkanten hoppas över.
    /// </summary>
    void DrawBelt(ICanvas canvas, RectF rect, SmallBodyBelt belt, Vector3[] positions,
        Color colour, ref double cachedTime, float dotRadius)
    {
        // Kropparna kryper framåt i sina banor – ett varv tar år till århundraden.
        // Positionerna behöver därför inte lösas ur Keplers ekvation varje bildruta,
        // utan först när rörelsen hunnit bli en pixel på skärmen. Toleransen följer
        // både zoomen och hur snabbt just det här bältet rör sig.
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

            // Listan är sorterad efter ljusstyrka, så detta slår till tre gånger.
            float alpha = belt.Bodies[i].Alpha;
            if (alpha != currentAlpha)
            {
                currentAlpha = alpha;
                canvas.FillColor = colour.WithAlpha(alpha);
            }
            canvas.FillCircle(sx, sy, dotRadius);
        }
    }

    // ------------------------------------------------------------ himlakroppar

    void DrawBodies(ICanvas canvas, RectF rect)
    {
        double t = DaysSinceJ2000;

        // Solens skärmposition behövs för planeternas ljussättning.
        bool sunVisible = Camera.Project(Vector3.Zero, out float sunX, out float sunY, out float sunDepth);

        var bodies = new List<(CelestialBody? Body, bool IsMoon, Vector3 Pos, float WorldRadius, float Depth, float Sx, float Sy)>(16);

        float sunWorldR = VisualRadius(SolarSystemData.SunRadiusKm, isSun: true);
        if (sunVisible)
            bodies.Add((null, false, Vector3.Zero, sunWorldR, sunDepth, sunX, sunY));

        foreach (var planet in SolarSystemData.Planets)
        {
            // Kepler-banan beskriver egentligen systemets tyngdpunkt. Med en så
            // tung måne som Charon ligger den utanför moderkroppen, som då
            // vaggar synligt kring den i stället för att stå stilla.
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

        // Måla bakifrån och fram (painter's algorithm).
        bodies.Sort((a, b) => b.Depth.CompareTo(a.Depth));

        foreach (var (body, isMoon, pos, worldR, depth, sx, sy) in bodies)
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
                // Småmånar som Phobos (11 km radie) blir försvinnande små även
                // förstorade, så de garanteras en synlig prick.
                r = MathF.Max(r, RealScale ? 0.45f : isMoon ? 2.5f : 1.1f);
                if (body.Ring is PlanetRing ring)
                {
                    // Den bortre ringhalvan målas före planeten och den främre efter,
                    // så att ringen ser ut att gå bakom och framför klotet.
                    BuildRingPaths(ring, pos, worldR, depth, out var farRing, out var nearRing);
                    FillRing(canvas, ring, farRing);
                    DrawPlanet(canvas, body, sx, sy, r, sunX, sunY);
                    FillRing(canvas, ring, nearRing);
                }
                else if (ReferenceEquals(body, Earth) && r >= 14f)
                {
                    DrawEarthGlobe(canvas, pos, sx, sy, r, sunX, sunY);
                }
                else
                {
                    DrawPlanet(canvas, body, sx, sy, r, sunX, sunY);
                }
                // Månar ritas utan namnetikett – de känns igen på sin plats vid planeten.
                if (!isMoon)
                    _labels.Add((body.Name, sx, sy + r + 6));
            }
        }
    }

    static readonly CelestialBody Earth =
        SolarSystemData.Planets.First(p => p.Name == "Jorden");

    /// <summary>Innersta månen får som mest hamna så här långt ut (planetradier).</summary>
    const float MoonInnerMaxRadii = 3f;

    /// <summary>Yttersta månen får som mest hamna så här långt ut (planetradier).</summary>
    const float MoonOuterMaxRadii = 10f;

    /// <summary>
    /// Innersta månen får aldrig tryckas närmare än så här (planetradier).
    /// Utan den här spärren skulle Titans stora bana pressa in Enceladus i
    /// Saturnus ringar, som når ut till 2,3 planetradier.
    /// </summary>
    const float MoonInnerMinRadii = 2.5f;

    /// <summary>
    /// Hur månbanorna skalas i förhållande till planeten. Utgångspunkten är att
    /// följa planeternas egen förstoring, så att systemets geometri blir exakt
    /// rätt (Mars månar ligger verkligen så nära som de ser ut). Systemet
    /// komprimeras först när det behövs: när innersta månen skulle hamna
    /// längre ut än 3 planetradier (vår egen måne ligger på 60) eller yttersta
    /// månen längre ut än 10 (Callisto ligger på 27 jupiterradier).
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

        // ... men aldrig så hårt att innersta månen hamnar inne i planeten
        // eller dess ringar.
        return MathF.Max(scale,
            parentVisR * MoonInnerMinRadii / (float)(innerAu * UnitsPerAu));
    }

    /// <summary>
    /// Lämpligt kameraavstånd när en kropp väljs i fokusväljaren: nära nog för
    /// att se planeten, men tillräckligt långt bort för att hela månsystemet
    /// ska rymmas i bild.
    /// </summary>
    public float SuggestedFocusDistance(CelestialBody planet)
    {
        float visR = VisualRadius(planet.RadiusKm, isSun: false);
        float distance = visR * 12f;

        if (planet.Moons.Length > 0)
        {
            float outer = (float)(planet.Moons.Max(m => m.SemiMajorAu) * UnitsPerAu)
                          * MoonDisplayScale(planet);
            distance = MathF.Max(distance, outer * 2.7f);
        }
        return MathF.Max(distance, 8f);
    }

    /// <summary>
    /// Lägger till en planets månar. Varje måne kretsar med sina riktiga
    /// banelement kring planeten. I förstorat läge vore de verkliga avstånden
    /// missvisande stora (jordens måne ligger på 60 jordradier), så där
    /// komprimeras systemet: den innersta månen hamnar på 3 x planetens
    /// visuella radie och de övriga behåller sina inbördes avståndsproportioner.
    /// Banornas form, riktning och fart är fortfarande korrekta. Månarna ritas
    /// bara när man zoomat in så nära att banan täcker något tiotal pixlar.
    /// </summary>
    void AddMoons(List<(CelestialBody? Body, bool IsMoon, Vector3 Pos, float WorldRadius, float Depth, float Sx, float Sy)> bodies,
        CelestialBody planet, Vector3 barycentre, float displayScale, double t)
    {
        foreach (var moon in planet.Moons)
        {
            // Separationen mäts från moderkroppen; månen placeras på sin sida om
            // tyngdpunkten så att avståndet mellan kropparna blir det rätta.
            var offset = moon.PositionAt(t, UnitsPerAu) * displayScale; // planetcentriskt
            var moonPos = barycentre + offset * (1f - (float)moon.MassFraction);

            if (!Camera.Project(moonPos, out float sx, out float sy, out float depth))
                continue;
            float orbitRadius = (float)(moon.SemiMajorAu * UnitsPerAu) * displayScale;
            if (Camera.ScreenRadius(orbitRadius, depth) < 10f)
                continue; // för utzoomat – månen skulle bara smeta ihop med planeten

            bodies.Add((moon, true, moonPos,
                VisualRadius(moon.RadiusKm, isSun: false), depth, sx, sy));
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

    // ------------------------------------------------------------ jordgloben

    static readonly Color OceanColor = Color.FromArgb("#2C5D9E");
    const double ObliquityRad = 23.4392911 * Math.PI / 180.0;

    readonly List<Vector3> _globeDirs = new(256);
    readonly List<Vector3> _globeClipped = new(256);

    /// <summary>
    /// Jorden som glob med världsdelar, polarisar och verklig rotation, synlig
    /// när man zoomat in (samma zoomnivå som gör månen synlig). Varje ytpunkts
    /// riktning beräknas ur stjärntiden (GMST): därmed lutar jordaxeln 23,4°
    /// mot Polstjärnan och rätt kontinent är vänd mot solen vid rätt klockslag,
    /// med ett varv per stjärndygn (23 h 56 min).
    /// </summary>
    void DrawEarthGlobe(ICanvas canvas, Vector3 center,
        float sx, float sy, float r, float sunX, float sunY)
    {
        canvas.FillColor = OceanColor;
        canvas.FillCircle(sx, sy, r);

        double gmst = (280.46061837 + 360.98564736629 * DaysSinceJ2000) * Math.PI / 180.0;
        double cosE = Math.Cos(ObliquityRad), sinE = Math.Sin(ObliquityRad);

        // Ytpunkterna ritas med ortografisk projektion inom den ritade cirkeln:
        // skärmläget ges av riktningens komposanter längs kamerans höger- och
        // uppaxlar gånger cirkelns radie. Då kan land aldrig hamna utanför
        // globen (perspektivprojektion av randpunkter gjorde precis det när
        // kameran var nära). Synligt är det halvklot som vetter mot kameran.
        var toCam = Vector3.Normalize(Camera.Position - center);
        const float cosLimb = 0.02f;

        foreach (var region in EarthMap.Regions)
        {
            _globeDirs.Clear();
            for (int i = 0; i < region.LonRad.Length; i++)
            {
                // RA = GMST + longitud, deklination = latitud -> ekvatorial
                // riktning, sedan samma rotation till världskoordinater som
                // för stjärnorna.
                double ra = gmst + region.LonRad[i];
                double xq = region.CosLat[i] * Math.Cos(ra);
                double yq = region.CosLat[i] * Math.Sin(ra);
                double zq = region.SinLat[i];
                double ye = yq * cosE + zq * sinE;
                double ze = -yq * sinE + zq * cosE;
                _globeDirs.Add(new Vector3((float)xq, (float)ze, (float)-ye));
            }

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

        // Dag/natt: ljus ton mot solsidan, mörk skugga på nattsidan.
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
    /// Sutherland-Hodgman-klippning av en polygon på klotytan mot den synliga
    /// kalotten (dot(riktning, mot kameran) >= cosLimb).
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
                // Kanten korsar randen – interpolera fram skärningspunkten.
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
    /// Saturnus ringar som ett band i planetens ekvatorsplan, uppdelat i två
    /// figurer: den bortre halvan (ritas bakom planeten) och den främre (ritas
    /// framför). Segmenten samlas i två banor så att hela ringen fylls med två
    /// anrop i stället för ett per segment.
    /// </summary>
    void BuildRingPaths(PlanetRing ring, Vector3 center, float worldR, float planetDepth,
        out PathF? farRing, out PathF? nearRing)
    {
        farRing = nearRing = null;

        float inner = worldR * ring.InnerRadii;
        float outer = worldR * ring.OuterRadii;
        if (Camera.ScreenRadius(outer, planetDepth) < ring.MinScreenRadius)
            return;

        // Ringplanets normal ur lutning och nod, samma formel som för en banpol,
        // omräknad till världskoordinater (Y = norr om ekliptikan).
        double incl = ring.InclinationDeg * Math.PI / 180.0;
        double node = ring.AscNodeDeg * Math.PI / 180.0;
        float si = (float)Math.Sin(incl), ci = (float)Math.Cos(incl);
        float sO = (float)Math.Sin(node), cO = (float)Math.Cos(node);

        var normal = new Vector3(si * sO, ci, si * cO);
        // Nodlinjen ligger per definition i ringplanet och är vinkelrät mot
        // normalen, så den duger som första basvektor.
        var u = new Vector3(cO, 0f, -sO);
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

    // --------------------------------------------------------------- rymdsonder

    /// <summary>Antal punkter varje ben av en sondbana ritas med.</summary>
    const int ProbeTrailSteps = 70;

    /// <summary>
    /// Ritar de verkliga sonderna med spåret efter sig. Spåret räknas fram ur
    /// banan i stället för att sparas undan bildruta för bildruta, så det ser
    /// likadant ut även om man hoppar i tiden eller spelar den baklänges.
    ///
    /// Sonderna är i dag över 150 AU bort, alltså fyra gånger längre ut än
    /// Neptunus. I översiktsvyn syns därför bara spårets innersta del, och man
    /// får zooma ut rejält för att se hela – vilket i sig är poängen med hur
    /// långt de har kommit.
    /// </summary>
    void DrawProbes(ICanvas canvas)
    {
        double now = DaysSinceJ2000;

        foreach (var probe in ProbeData.All)
        {
            if (!probe.Exists(now))
                continue;   // ännu inte uppskjuten

            canvas.StrokeSize = 1.4f;
            canvas.StrokeColor = probe.Color.WithAlpha(0.55f);
            foreach (var leg in probe.Legs)
            {
                if (leg.StartDay >= now)
                    break;  // benen ligger i tidsordning, resten är framtid
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
    /// Ritar milstolparna – uppskjutningen och planetpassagerna – som ringar
    /// längs spåret. Den valda sondens milstolpar får planetnamn och datum,
    /// de övriga bara årtalet.
    ///
    /// Årtalen ritas direkt i stället för att gå via etikettstaplingen, som är
    /// gjord för himlakroppar: elva passager som staplas nedåt hade blivit en
    /// textpelare tvärs över vyn. I stället hoppas ett årtal över när det skulle
    /// hamna ovanpå ett som redan skrivits.
    /// </summary>
    void DrawMilestones(ICanvas canvas, Probe probe, double now)
    {
        bool focused = ReferenceEquals(probe, FocusedProbe);
        canvas.FontSize = 11f;

        foreach (var milestone in probe.Milestones)
        {
            if (milestone.Day > now)
                break;      // milstolparna ligger i tidsordning

            if (!Camera.Project(milestone.PositionAu * UnitsPerAu,
                    out float x, out float y, out _))
                continue;

            canvas.StrokeSize = 1.4f;
            canvas.StrokeColor = probe.Color.WithAlpha(0.9f);
            canvas.DrawCircle(x, y, 3.5f);

            var date = SolarSystemData.EpochJ2000.AddDays(milestone.Day);
            if (focused)
            {
                // Den valda sonden får hela historien utskriven.
                string text = milestone.IsLaunch
                    ? $"Uppskjuten {date:MMM yyyy}"
                    : string.Create(SwedishText,
                        $"{milestone.Name} {date:MMM yyyy}  {milestone.SpeedGainKmS:+0.0;-0.0} km/s");
                DrawMilestoneText(canvas, text, x, y, probe.Color, minSeparation: 0f);
            }
            else
            {
                DrawMilestoneText(canvas, date.Year.ToString(), x, y,
                    probe.Color.WithAlpha(0.7f), minSeparation: 26f);
            }
        }
    }

    /// <summary>Var milstolpstexterna hamnade den här bildrutan.</summary>
    readonly List<(float X, float Y)> _milestoneText = new(16);

    static readonly CultureInfo SwedishText = new("sv-SE");

    void DrawMilestoneText(ICanvas canvas, string text, float x, float y, Color color,
        float minSeparation)
    {
        foreach (var (px, py) in _milestoneText)
            if (Math.Abs(px - x) < minSeparation && Math.Abs(py - y) < minSeparation)
                return;     // krockar med en text som redan står där

        _milestoneText.Add((x, y));

        canvas.FontColor = LabelShadowColor;
        canvas.DrawString(text, x + 7f, y - 4f, HorizontalAlignment.Left);
        canvas.FontColor = color;
        canvas.DrawString(text, x + 6f, y - 5f, HorizontalAlignment.Left);
    }

    /// <summary>Ritar ett ben av en sondbana fram till en given dag.</summary>
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

    // ---------------------------------------------------------------- rymdfärd

    static readonly Color CraftTrailColor = Color.FromRgba(0.55f, 0.85f, 1.00f, 0.75f);
    static readonly Color CraftFutureColor = Color.FromRgba(0.45f, 0.70f, 0.90f, 0.28f);
    static readonly Color CraftColor = Color.FromArgb("#E8F4FF");

    /// <summary>
    /// Ritar farkostens bana och dess nuvarande läge. Spåret räknas fram ur
    /// banan i stället för att sparas undan bildruta för bildruta – då ser det
    /// likadant ut även om man hoppar i tiden eller spelar den baklänges.
    /// </summary>
    void DrawMission(ICanvas canvas, Mission mission)
    {
        double now = Math.Clamp(DaysSinceJ2000, mission.LaunchDay, mission.ArrivalDay);
        var origin = MissionOrigin(mission, out float scale);

        canvas.StrokeSize = 1.4f;
        canvas.StrokeColor = CraftFutureColor;
        DrawMissionArc(canvas, mission, now, mission.ArrivalDay, origin, scale);   // vad som återstår

        canvas.StrokeSize = 1.8f;
        canvas.StrokeColor = CraftTrailColor;
        DrawMissionArc(canvas, mission, mission.LaunchDay, now, origin, scale);    // det tillryggalagda

        if (DaysSinceJ2000 < mission.LaunchDay)
            return;

        // Efter ankomsten följer farkosten med målet i stället för att bli stående
        // kvar där planeten råkade vara – en sond går ju in i omloppsbana eller landar.
        var pos = origin + mission.PositionAt(DaysSinceJ2000, UnitsPerAu) * scale;
        if (!Camera.Project(pos, out float sx, out float sy, out _))
            return;

        canvas.FillColor = CraftColor;
        canvas.FillCircle(sx, sy, 3f);
        _labels.Add((mission.HasArrived(DaysSinceJ2000)
            ? $"{mission.Name} framme"
            : mission.Name, sx, sy + 9f));
    }

    /// <summary>
    /// Var farkostens bana hör hemma i världen. En färd kring solen ritas som
    /// den är, medan en månfärd måste följa med planeten och tryckas ihop på
    /// exakt samma sätt som månbanorna – annars skulle hela banan försvinna inne
    /// i det kraftigt förstorade jordklotet. Den ritas dessutom kring planetens
    /// nuvarande läge, alltså sedd från jorden precis som månarna, i stället för
    /// att släpa efter längs jordens egen färd kring solen.
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
    /// Farkostens läge i världen, precis som det ritas – med planetens läge och
    /// månsystemets hoptryckning inräknade. Null när ingen färd pågår. Kameran
    /// använder det för att följa med farkosten ner till målet vid ankomsten.
    /// </summary>
    public Vector3? CraftPosition()
    {
        if (Mission is not { } mission)
            return null;

        var origin = MissionOrigin(mission, out float scale);
        double day = Math.Max(DaysSinceJ2000, mission.LaunchDay);
        return origin + mission.PositionAt(day, UnitsPerAu) * scale;
    }

    /// <summary>Ritar en del av banan som en polylinje.</summary>
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

    // --------------------------------------------------------------- etiketter

    /// <summary>
    /// Ritar namnen. När planeterna trängs ihop (t.ex. det inre solsystemet sett
    /// på långt håll) staplas etiketterna nedåt i stället för att skriva över
    /// varandra, och den som flyttats får en tunn streckad linje till sin planet.
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
            // Etiketter långt utanför skärmen behöver varken staplas eller ritas.
            if (!(x > -200 && x < rect.Width + 200 && anchorY > -200 && anchorY < rect.Height + 200))
                continue;

            float y = anchorY;
            // Hårt tak på antalet omflyttningsvarv: varje varv ska flytta
            // etiketten strikt nedåt, men flyttalsavrundning i extremfall får
            // aldrig kunna låsa UI-tråden i en evig loop igen.
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
