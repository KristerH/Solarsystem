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

    /// <summary>
    /// Namnen på de rymdsonder som ska ritas – både de fem som lämnat
    /// solsystemet och de två som kretsar kring en planet. Sonder som inte finns
    /// i mängden ritas inte alls: varken prick, spår eller milstolpar. Tom mängd
    /// släcker allihop.
    ///
    /// Namnen duger som nyckel eftersom de är unika och redan används för att
    /// identifiera kroppar på andra håll i appen, till exempel i fokusväljaren.
    ///
    /// Tom från start: sonderna är en fördjupning och deras spår korsar hela
    /// vyn, så översikten ska vara ren tills någon bockar i dem i väljaren.
    /// </summary>
    public HashSet<string> VisibleProbes { get; } = [];

    /// <summary>
    /// Ritar Halleys komet med sin bana och sina svansar. Av som standard: den
    /// är borta 74 år av 75, och dess bana är så utdragen att den skymmer
    /// planeternas när den ligger kvar i bilden.
    /// </summary>
    public bool ShowHalley { get; set; }

    /// <summary>
    /// Ritar månbanan mot ekliptikan med noderna utmärkta. Av som standard –
    /// det är en fördjupning och inte något som hör hemma i översiktsvyn.
    /// </summary>
    public bool ShowMoonOrbit { get; set; }

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
            Diagnostics.Log($"ritfel: {ex}\n");
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
                    DrawBody(canvas, body, pos, sx, sy, r, sunX, sunY);
                    FillRing(canvas, ring, nearRing);
                }
                else
                {
                    DrawBody(canvas, body, pos, sx, sy, r, sunX, sunY);
                }
                // Månar ritas utan namnetikett – de känns igen på sin plats vid planeten.
                if (!isMoon)
                    _labels.Add((body.Name, sx, sy + r + 6));
            }
        }
    }

    /// <summary>
    /// Heliopausens avstånd från solen. Där möter solvinden det interstellära
    /// mediet och solens välde tar slut – det närmaste solsystemet har en kant.
    ///
    /// Att rita den som en kula är en förenkling. I verkligheten buktar den:
    /// solsystemet far genom det interstellära mediet i 25 km/s och får en
    /// stötvåg framför sig, så gränsen ligger närmare åt det håll vi är på väg
    /// och dras ut till en svans bakåt. Voyagersonderna korsade den på 121,6
    /// respektive 119,0 AU, och att de två talen inte är lika är just det.
    /// </summary>
    public const double HeliopauseAu = 120.0;

    static readonly Color HeliopauseFill = Color.FromRgba(0.30f, 0.50f, 0.85f, 0.045f);
    static readonly Color HeliopauseRim = Color.FromRgba(0.55f, 0.75f, 1.00f, 0.30f);

    /// <summary>
    /// Ritar heliopausen som en cirkel, men bara när kameran står utanför den.
    /// Innanför skulle sfären fylla hela bilden och bara vara en blå slöja.
    ///
    /// En kula sedd utifrån projiceras till en cirkel med vinkelradien
    /// arcsin(R/d), vilket på skärmen blir R·f/√(d²−R²). Det är alltså inte
    /// samma sak som att projicera kulans kant rakt av.
    /// </summary>
    void DrawHeliopause(ICanvas canvas, RectF rect)
    {
        float radius = (float)HeliopauseAu * UnitsPerAu;
        float distance = Camera.Position.Length();

        // Innanför, eller precis på gränsen där uttrycket spårar ur.
        if (distance <= radius * 1.02f)
            return;
        if (!Camera.Project(Vector3.Zero, out float sx, out float sy, out _))
            return;

        float screen = Camera.Focal * radius
                       / MathF.Sqrt(distance * distance - radius * radius);
        // Cirkeln ska rymmas i bild. Är den större än så ser man ingen kant alls,
        // bara en blå slöja över allt – och då är det bättre att inte rita den.
        if (screen < 8f || screen > MathF.Min(rect.Width, rect.Height) * 0.55f)
            return;

        canvas.FillColor = HeliopauseFill;
        canvas.FillCircle(sx, sy, screen);
        canvas.StrokeSize = 1.2f;
        canvas.StrokeColor = HeliopauseRim;
        canvas.DrawCircle(sx, sy, screen);

        _labels.Add(("Heliopausen", sx, sy - screen - 4));
    }


    // ---------------------------------------------------- Halleys komet

    /// <summary>
    /// Bortom det här avståndet från solen har kometen ingen svans. Gränsen är
    /// inte vald för att det ser bra ut: svansen finns bara så länge isen i
    /// kärnan ångar, och vattenis börjar ånga först när solen värmt den nog,
    /// vilket sker kring tre astronomiska enheter.
    ///
    /// Följden är värd att se i appen. Av sina 27 563 dygn tillbringar Halley
    /// 368 innanför gränsen – ett år av sjuttiofem. Resten av tiden är den en
    /// mörk isklump som ingen kan se.
    /// </summary>
    const double TailLimitAu = 3.0;

    /// <summary>
    /// Svansens längd på en astronomisk enhets avstånd. Den växer som 1/r², i
    /// takt med solljusets styrka, vilket ger 0,29 AU i periheliet och några
    /// hundradelar vid gränsen ovan.
    /// </summary>
    const double TailScaleAu = 0.10;

    /// <summary>Tak för längden, så att den inte skenar i väg innanför periheliet.</summary>
    const double MaxTailAu = 0.30;

    /// <summary>Hur lång dammsvansen är i förhållande till jonsvansen.</summary>
    const double DustLengthShare = 0.65;

    /// <summary>Hur hårt dammsvansen böjs bakåt i banan vid sin spets.</summary>
    const double DustCurve = 0.5;

    /// <summary>Antal punkter varje svans byggs av.</summary>
    const int TailSteps = 16;

    /// <summary>
    /// Kortaste svans som ritas, i bildpunkter. En svans på 0,3 AU är stor i
    /// jämförelse med jordens bana men liten i jämförelse med Halleys egen, så
    /// utzoomat vore den bara några pixlar lång. Den sträcks då ut till den här
    /// längden med riktningen behållen – samma sorts förstoring som planeterna
    /// får för att alls synas.
    /// </summary>
    const float MinTailPixels = 24f;

    const int HalleyOrbitSamples = 360;

    static readonly Color HalleyOrbitColor = Color.FromRgba(0.62f, 0.88f, 0.84f, 0.40f);
    static readonly Color IonTailColor = Color.FromRgba(0.62f, 0.82f, 1.00f, 0.85f);
    static readonly Color DustTailColor = Color.FromRgba(1.00f, 0.93f, 0.74f, 0.55f);
    static readonly Color ComaGlow = Color.FromRgba(0.80f, 0.96f, 0.94f, 0.22f);

    Vector3[]? _halleyOrbit;

    /// <summary>
    /// Halleys bana. Den ritas inte tillsammans med planeternas, dels för att
    /// den bara hör hemma i bilden när kometen är framme, dels för att den är
    /// sextio gånger längre än den är bred och därför inte tål att buntas ihop
    /// med cirklarna.
    /// </summary>
    void DrawHalleyOrbit(ICanvas canvas)
    {
        // Banan ligger stilla, så punkterna räknas fram en gång.
        _halleyOrbit ??= SolarSystemData.Halley.OrbitPath(HalleyOrbitSamples, UnitsPerAu);

        // Samma regel som för planetbanorna: en delfigur får börja först när två
        // punkter i rad är synliga, annars blir det en MoveTo utan LineTo.
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
    /// Kometen med sina två svansar.
    ///
    /// Det som är lättast att ha fel för sig om är åt vilket håll de pekar. En
    /// svans ligger inte bakom kometen i färdriktningen, som ett bloss efter en
    /// raket, utan bort från solen – och på vägen ut från periheliet går
    /// kometen alltså med svansen före.
    ///
    /// Att det finns två svansar, och varför de skiljer sig åt, är samma sak
    /// sedd närmare. Jonsvansen är gas som solljuset laddat elektriskt;
    /// solvinden blåser i 400 km/s och river med den rakt bort från solen utan
    /// att bry sig om vart kometen är på väg. Dammsvansen är korn som är för
    /// tunga för att ryckas med: de får en knuff av ljuset men behåller
    /// kometens egen fart, hamnar i egna banor kring solen och blir därför
    /// kortare, bredare och böjda bakåt.
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
            // Solen ligger i origo, så riktningen bort från den är läget självt.
            var away = posAu.Normalized();
            var motion = (comet.PositionAuAt(t + 0.5) - comet.PositionAuAt(t - 0.5)).Normalized();

            var ion = new Vector3[TailSteps + 1];
            var dust = new Vector3[TailSteps + 1];
            for (int k = 0; k <= TailSteps; k++)
            {
                double s = (double)k / TailSteps;
                ion[k] = ((posAu + away * (tailAu * s)) * UnitsPerAu).ToVector3();

                // Dammet släpar efter i banan, och eftersläpningen växer med
                // avståndet från kärnan eftersom de kornen släpptes tidigare.
                double along = tailAu * DustLengthShare * s;
                dust[k] = ((posAu + away * along - motion * (along * DustCurve * s))
                           * UnitsPerAu).ToVector3();
            }

            float stretch = TailStretch(ion[TailSteps], hx, hy);
            DrawTail(canvas, dust, hx, hy, stretch, DustTailColor, 4.0f);
            DrawTail(canvas, ion, hx, hy, stretch, IonTailColor, 1.6f);
        }

        // Kärnan är fem kilometer bred och skulle aldrig gå att se. Det man ser
        // är kaman: gasmolnet omkring kärnan, som nära solen blir hundratusen
        // kilometer brett och alltså större än solen själv. Prickens storlek
        // följer därför aktiviteten och inte kroppens mått.
        float coma = 2.4f + 4.6f * (float)(tailAu / MaxTailAu);
        canvas.FillColor = ComaGlow;
        canvas.FillCircle(hx, hy, coma * 2.4f);
        canvas.FillColor = comet.BodyColor;
        canvas.FillCircle(hx, hy, coma);

        _labels.Add((comet.Name, hx, hy + coma * 2.4f + 6f));
    }

    /// <summary>
    /// Hur mycket svansen behöver sträckas för att synas. Ett medgivande till
    /// kameran och inte till fysiken: riktningen är den uträknade, det är bara
    /// längden som är tilltagen.
    /// </summary>
    float TailStretch(Vector3 tip, float hx, float hy)
    {
        if (!Camera.Project(tip, out float tx, out float ty, out _))
            return 1f;
        float length = MathF.Sqrt((tx - hx) * (tx - hx) + (ty - hy) * (ty - hy));
        return length >= MinTailPixels || length <= 0.01f ? 1f : MinTailPixels / length;
    }

    /// <summary>
    /// Ritar en svans som en rad streck som blir bredare och blekare utåt –
    /// tätast och ljusast vid kärnan, precis som en riktig svans.
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

    // -------------------------------------------------- månbanan och noderna

    static readonly Color MoonOrbitColor = Color.FromRgba(0.70f, 0.75f, 0.85f, 0.55f);
    static readonly Color EclipticRingColor = Color.FromRgba(0.95f, 0.82f, 0.45f, 0.40f);
    static readonly Color NodeLineColor = Color.FromRgba(0.95f, 0.82f, 0.45f, 0.75f);

    /// <summary>
    /// Ritar månbanan mot ekliptikans plan och märker ut de två noderna – de
    /// punkter där banan korsar ekliptikan.
    ///
    /// Det är dem hela förmörkelsefrågan hänger på. Månen går varv efter varv
    /// utan att någon förmörkelse blir av, eftersom banan lutar 5,1 grader och
    /// månen därför passerar ovanför eller under solen. Bara när solen råkar stå
    /// nära nodlinjen kan de tre hamna på rad. Nodlinjen vrider sig dessutom ett
    /// varv baklänges på 18,6 år, vilket är varför förmörkelsesäsongerna glider
    /// bakåt genom kalendern i stället för att ligga still.
    ///
    /// Banan ritas med samma komprimering som månen själv, annars hade kurvan
    /// hamnat någon annanstans än månen.
    /// </summary>
    void DrawMoonOrbit(ICanvas canvas, double day)
    {
        var earth = SolarSystemData.Planets.FirstOrDefault(p => p.Name == "Jorden");
        if (earth is null || earth.Moons.Length == 0)
            return;

        var moon = earth.Moons[0];
        var centre = earth.PositionAt(day, UnitsPerAu);
        float scale = MoonDisplayScale(earth) * (1f - (float)moon.MassFraction);

        // Under den här storleken är banan bara en kringla kring en prick.
        if (!Camera.Project(centre, out _, out _, out float depth))
            return;
        float radius = (float)(moon.SemiMajorAu * UnitsPerAu) * scale;
        if (Camera.ScreenRadius(radius, depth) < 30f)
            return;

        // Själva banan, med den precession den har just den här dagen.
        const int samples = 96;
        var path = moon.OrbitPath(samples, UnitsPerAu, day);
        DrawClosedCurve(canvas, centre, path, MoonOrbitColor, 1.4f);

        // Ekliptikans plan som en cirkel med samma radie, att jämföra mot.
        var flat = new Vector3[samples];
        for (int i = 0; i < samples; i++)
        {
            double a = i * Math.PI * 2 / samples;
            flat[i] = new Vector3((float)(Math.Cos(a) * radius), 0f, (float)(Math.Sin(a) * radius));
        }
        DrawClosedCurve(canvas, centre, flat, EclipticRingColor, 1.0f);

        // Nodlinjen: där de två planen skär varandra. Riktningen följer av
        // nodens longitud, och den vrider sig med tiden.
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
            canvas.DrawString("Uppstigande nod", ax + 6, ay - 4, HorizontalAlignment.Left);
            canvas.DrawString("Nedstigande nod", bx + 6, by - 4, HorizontalAlignment.Left);
            canvas.FillColor = NodeLineColor;
            canvas.FillCircle(ax, ay, 3f);
            canvas.FillCircle(bx, by, 3f);
        }

        // En liten pil vinkelrätt mot ekliptikan, så att lutningen syns.
        if (Camera.Project(centre, out float cx, out float cy, out _) &&
            Camera.Project(centre + up * radius * 0.4f, out float ux, out float uy, out _))
        {
            canvas.StrokeSize = 1.0f;
            canvas.StrokeColor = EclipticRingColor;
            canvas.DrawLine(cx, cy, ux, uy);
        }
    }

    /// <summary>Ritar en sluten kurva av världspunkter kring en medelpunkt.</summary>
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
    /// <summary>
    /// Var en måne faktiskt hamnar på skärmen: planetens läge plus månens
    /// banläge, med samma komprimering som ritningen använder.
    ///
    /// Kameran måste sikta på det ritade läget och inte på det verkliga. Månarna
    /// dras in mot sin planet för att inte hamna utanför bild – vår egen måne
    /// ligger på 60 jordradier och skulle annars försvinna – och siktar man på
    /// den verkliga positionen pekar kameran långt bredvid.
    /// </summary>
    public Vector3 MoonPosition(CelestialBody planet, CelestialBody moon, double day)
        => planet.PositionAt(day, UnitsPerAu)
           + moon.PositionAt(day, UnitsPerAu)
             * MoonDisplayScale(planet) * (1f - (float)moon.MassFraction);

    /// <summary>
    /// Kameraavstånd när en måne väljs: samma bildvinkel som en planet får, så
    /// att klotet fyller lika mycket av rutan. Utan det hamnar man antingen
    /// inuti månen eller så långt bort att den bara blir en prick.
    /// </summary>
    public float SuggestedMoonDistance(CelestialBody moon)
        => MathF.Max(VisualRadius(moon.RadiusKm, isSun: false) * 12f,
                     OrbitCamera.AbsoluteMinDistance);

    public float SuggestedFocusDistance(CelestialBody planet)
    {
        // Kometen har ingen storlek att tala om – kärnan är fem kilometer – så
        // avståndet ramar in svansen i stället för kroppen.
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
        // Golvet var tidigare ett fast tal, vilket gjorde läget "Verklig storlek"
        // omöjligt att zooma in i: där är jordens radie 0,0026 enheter och åtta
        // enheter är tretusen jordradier bort. Nu följer det kroppen.
        return MathF.Max(distance, MathF.Max(visR * 2f, OrbitCamera.AbsoluteMinDistance));
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

    /// <summary>
    /// Kroppens ritade radie i världsenheter. Publik därför att kameran behöver
    /// den: hur nära man får komma beror på hur stor kroppen ritas.
    /// </summary>
    public float VisualRadius(double radiusKm, bool isSun)
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

    /// <summary>
    /// Ytan ritas först när klotet nått så här många pixlar i radie. Under det
    /// blir världsdelarna ändå bara ett par suddiga fläckar, och en skiva med
    /// ljus och skugga ser bättre ut.
    /// </summary>
    const float GlobeMinRadius = 14f;

    readonly List<Vector3> _globeDirs = new(256);
    readonly List<Vector3> _globeClipped = new(256);

    /// <summary>
    /// Ritar en kropp: som glob med yta om den har en ytkarta och är stor nog i
    /// bild, annars som en skiva med ljus och skugga.
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
    /// En kropp som glob med sin yta och sin verkliga rotation, synlig när man
    /// zoomat in. Varje ytpunkts riktning byggs ur kroppens rotationsaxel: därmed
    /// lutar jordaxeln 23,4° mot Polstjärnan och rätt kontinent är vänd mot solen
    /// vid rätt klockslag, med ett varv per stjärndygn (23 h 56 min).
    /// </summary>
    void DrawGlobe(ICanvas canvas, SurfaceMap surface, BodyAxis axis, Vector3 center,
        float sx, float sy, float r, float sunX, float sunY)
    {
        canvas.FillColor = surface.BaseColor;
        canvas.FillCircle(sx, sy, r);

        double spin = axis.SpinRadians(DaysSinceJ2000);

        // Ytpunkterna ritas med ortografisk projektion inom den ritade cirkeln:
        // skärmläget ges av riktningens komposanter längs kamerans höger- och
        // uppaxlar gånger cirkelns radie. Då kan land aldrig hamna utanför
        // globen (perspektivprojektion av randpunkter gjorde precis det när
        // kameran var nära). Synligt är det halvklot som vetter mot kameran.
        var toCam = Vector3.Normalize(Camera.Position - center);
        const float cosLimb = 0.02f;

        foreach (var region in surface.Regions)
        {
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

        // Ringarna ligger i planetens ekvatorsplan, så basvektorerna kommer
        // färdiga ur dess rotationsaxel: nodlinjen som den ena och punkten ett
        // kvarts varv öster om den som den andra.
        var u = ring.Axis.NodeAxis;
        var v = ring.Axis.EastAxis;

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
            if (!VisibleProbes.Contains(probe.Name))
                continue;   // bortvald i sondväljaren
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
                string text = milestone switch
                {
                    { IsLaunch: true } => $"Uppskjuten {date:MMM yyyy}",
                    { IsBoundary: true } => $"{milestone.Name} {date:MMM yyyy}",
                    _ => string.Create(SwedishText,
                        $"{milestone.Name} {date:MMM yyyy}  {milestone.SpeedGainKmS:+0.0;-0.0} km/s"),
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
    /// Ritar sonderna som kretsar kring en planet: hela banellipsen och sondens
    /// läge på den. Banan trycks ihop med samma faktor som planetens månar, så
    /// att den hamnar i samma storleksordning som dem i stället för att
    /// försvinna inne i det förstorade klotet. Cassinis varv är ungefär lika
    /// stort som Titans bana, och det syns nu också i vyn.
    ///
    /// Ritas bara när man zoomat in så nära att banan täcker något tiotal
    /// pixlar – på håll skulle den ändå bara bli en prick ovanpå planeten.
    /// </summary>
    void DrawOrbiters(ICanvas canvas)
    {
        double now = DaysSinceJ2000;

        foreach (var orbiter in ProbeData.Orbiters)
        {
            if (!VisibleProbes.Contains(orbiter.Name))
                continue;   // bortvald i sondväljaren
            if (!orbiter.Exists(now))
                continue;   // ännu inte framme, eller uppdraget slut

            var center = orbiter.Center;
            float scale = MoonDisplayScale(center);
            var origin = center.PositionAt(now, UnitsPerAu);

            if (!Camera.Project(origin, out _, out _, out float depth))
                continue;

            float radius = (float)orbiter.Path.SemiMajorAu * UnitsPerAu * scale;
            if (Camera.ScreenRadius(radius, depth) < 10f)
                continue;   // för utzoomat

            DrawOrbiterPath(canvas, orbiter, origin, scale);

            var pos = origin + orbiter.PositionAt(now, UnitsPerAu) * scale;
            if (!Camera.Project(pos, out float sx, out float sy, out _))
                continue;

            canvas.FillColor = orbiter.Color;
            canvas.FillCircle(sx, sy, 2.5f);
            _labels.Add((orbiter.Name, sx, sy + 9f));
        }
    }

    /// <summary>Ritar banellipsen som en sluten polylinje.</summary>
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
