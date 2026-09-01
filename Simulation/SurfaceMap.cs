namespace Solarsystem.Simulation;

/// <summary>
/// En kropps yta som (latitud, longitud)-polygoner. De ritas som fyllda ytor
/// direkt på klotet – ingen texturbild behövs.
///
/// Konturerna är grova (skolplansch-nivå) men dragen känns igen. Longituden
/// räknas åt det håll kroppen vrider sig, alltså österut för allt som roterar
/// rättvänt.
/// </summary>
public sealed class SurfaceMap
{
    public sealed record Region(Color Fill, float[] SinLat, float[] CosLat, float[] LonRad);

    /// <summary>Grundfärgen under polygonerna – jordens hav, Mars öken.</summary>
    public Color BaseColor { get; }

    public Region[] Regions { get; }

    /// <param name="smooth">
    /// Rundar av ytornas hörn innan de ritas. För kroppar vars drag har diffusa
    /// gränser – Mars dammfält – i stället för skarpa, som jordens kustlinjer.
    /// </param>
    SurfaceMap(Color baseColor, (Color Fill, (float Lat, float Lon)[] Pts)[] raw,
        bool smooth = false)
    {
        BaseColor = baseColor;
        Regions = [.. raw.Select(r => Densify(r.Fill, smooth ? Smooth(r.Pts) : r.Pts))];
    }

    static readonly Color Ocean = Color.FromArgb("#2C5D9E");
    static readonly Color Land = Color.FromArgb("#6FA35C");
    static readonly Color Ice = Color.FromArgb("#E9EFF4");

    /// <summary>
    /// Jordens landmassor. Nollmeridianen ligger där den ska: Greenwich på
    /// longitud noll, Afrika strax öster om den.
    /// </summary>
    public static readonly SurfaceMap Earth = new(Ocean,
        [
            // Afrika
            (Land, [(37,10),(33,22),(30,32),(12,43),(11,51),(2,46),(-5,39),(-15,40),(-26,33),
                    (-34,20),(-33,17),(-22,14),(-8,13),(4,9),(6,-1),(5,-8),(12,-17),(21,-17),
                    (28,-12),(33,-9),(36,-3)]),
            // Madagaskar
            (Land, [(-12,49),(-17,50),(-25,47),(-22,43),(-16,44)]),
            // Eurasien
            (Land, [(43,-9),(36,-6),(38,0),(43,4),(38,16),(36,23),(38,27),(36,33),(31,35),
                    (29,35),(13,44),(16,53),(24,58),(27,50),(24,54),(25,61),(21,72),(8,77),
                    (13,80),(20,87),(22,91),(16,94),(9,98),(1,104),(8,105),(11,109),(21,108),
                    (23,117),(31,122),(37,123),(35,129),(43,132),(54,137),(59,143),(51,157),
                    (61,163),(64,177),(67,190),(70,180),(72,150),(76,113),(73,80),(68,55),
                    (68,40),(71,26),(65,12),(59,5),(55,8),(54,9),(52,4),(48,-2),(44,-2)]),
            // Brittiska öarna
            (Land, [(58,-5),(56,-2),(53,0),(51,1),(50,-4),(53,-4),(54,-6),(56,-6)]),
            // Nordamerika
            (Land, [(60,-166),(66,-162),(71,-157),(70,-141),(69,-125),(72,-108),(72,-95),
                    (66,-84),(59,-78),(62,-72),(58,-68),(54,-57),(47,-52),(45,-64),(41,-70),
                    (35,-76),(31,-81),(25,-80),(29,-84),(29,-91),(26,-97),(21,-97),(18,-94),
                    (15,-92),(13,-87),(9,-81),(8,-78),(9,-84),(16,-95),(19,-105),(23,-110),
                    (28,-114),(33,-118),(38,-123),(46,-124),(54,-131),(59,-140),(59,-152),
                    (55,-162)]),
            // Sydamerika
            (Land, [(11,-72),(10,-62),(5,-52),(0,-50),(-5,-35),(-13,-38),(-23,-42),(-34,-53),
                    (-39,-62),(-47,-66),(-54,-68),(-53,-71),(-46,-74),(-37,-73),(-30,-71),
                    (-18,-70),(-5,-81),(1,-80),(7,-77)]),
            // Australien
            (Land, [(-12,131),(-11,136),(-12,142),(-19,147),(-27,153),(-34,151),(-38,147),
                    (-38,141),(-35,137),(-32,133),(-32,125),(-34,115),(-26,113),(-20,119),
                    (-14,126)]),
            // Nya Guinea
            (Land, [(-1,131),(-3,141),(-8,148),(-10,150),(-8,143),(-5,135)]),
            // Grönland (istäckt)
            (Ice, [(83,-35),(81,-20),(76,-19),(70,-22),(60,-43),(65,-53),(72,-56),(77,-70),
                   (81,-62)]),
            // Arktiska havsisen kring nordpolen
            (Ice, [(84,0),(84,30),(84,60),(84,90),(84,120),(84,150),(84,180),(84,210),
                   (84,240),(84,270),(84,300),(84,330)]),
            // Antarktis kring sydpolen
            (Ice, [(-70,0),(-68,30),(-66,60),(-66,90),(-66,120),(-68,150),(-71,180),
                   (-74,210),(-75,240),(-72,270),(-64,297),(-70,330)]),
        ]);

    static readonly Color MarsDust = Color.FromArgb("#C4663F");
    static readonly Color MarsBright = Color.FromArgb("#DB9160");
    static readonly Color MarsDark = Color.FromArgb("#7E5341");
    static readonly Color MarsCanyon = Color.FromArgb("#66412F");

    /// <summary>
    /// Mars albedokarta: de mörka fälten som setts i teleskop sedan 1600-talet.
    /// De är inte hav utan bara berggrund som vinden sopat ren från ljust damm,
    /// vilket är varför de långsamt byter form mellan stormsäsongerna. Namnen är
    /// ändå kvar från den tid då man trodde annat – Mare Cimmerium, Mare Sirenum.
    ///
    /// Longituderna är östliga och räknade från Airy-0, den lilla krater som
    /// definierar Mars nollmeridian. Syrtis Major hamnar därför kring 70° öst,
    /// där den ska vara: det var den fläcken Christiaan Huygens ritade av 1659
    /// och tog tiden på för att mäta Mars dygn.
    ///
    /// Polarisarna ritas med samma utsträckning året om. I verkligheten andas de
    /// med årstiderna – den södra vinterkalotten når ned mot 50° syd och krymper
    /// sedan till en fläck – men appen har ingen årstidsmodell för frost.
    /// </summary>
    public static readonly SurfaceMap Mars = new(MarsDust,
        [
            // Ljusa högslätter: Hellas, den 2 300 km vida nedslagsbassängen som
            // ofta ligger dammfylld och lysande, och Tharsis med solsystemets
            // största vulkaner.
            (MarsBright, [(-28,55),(-30,80),(-42,92),(-55,80),(-56,58),(-45,47)]),
            (MarsBright, [(25,230),(20,260),(0,268),(-10,255),(-6,232),(10,222)]),

            // Syrtis Major: den mörka triangeln, bredast i norr. Mars mest kända
            // drag och det första någon sett på en annan planets yta.
            (MarsDark, [(20,62),(18,76),(8,78),(0,74),(-4,70),(2,66),(10,60)]),
            // Mare Acidalium – det stora mörka fältet på norra halvklotet.
            (MarsDark, [(60,330),(55,350),(48,5),(38,10),(30,0),(32,340),(40,325),(52,318)]),
            // Sinus Sabaeus och Sinus Meridiani, bandet längs ekvatorn som går
            // rakt genom nollmeridianen.
            (MarsDark, [(-2,318),(2,330),(0,345),(2,358),(0,8),(-6,6),(-8,350),(-10,335),(-8,322)]),
            // Mare Erythraeum
            (MarsDark, [(-14,300),(-12,320),(-18,340),(-28,345),(-34,330),(-32,310),(-24,298)]),
            // Mare Tyrrhenum
            (MarsDark, [(-12,82),(-14,100),(-20,115),(-28,110),(-30,95),(-24,82)]),
            // Mare Cimmerium
            (MarsDark, [(-14,140),(-16,160),(-22,185),(-32,188),(-34,165),(-26,142)]),
            // Mare Sirenum
            (MarsDark, [(-20,205),(-24,225),(-32,240),(-40,232),(-38,210),(-28,200)]),
            // Solis Lacus, "Mars öga", som blinkar när dammstormar drar över.
            (MarsDark, [(-22,262),(-24,275),(-32,278),(-36,268),(-30,259)]),
            // Boreosyrtis vid Utopia
            (MarsDark, [(48,95),(44,120),(36,128),(34,108),(40,92)]),

            // Valles Marineris: 4 000 km lång, tio gånger Grand Canyon och djup
            // nog att rymma Mount Everest stående. Den syns som ett streck.
            (MarsCanyon, [(-6,262),(-9,280),(-12,300),(-15,320),(-18,318),(-15,298),(-12,278),(-9,260)]),

            // Polarisarna: vattenis under ett lock av frusen koldioxid.
            (Ice, [(76,0),(76,30),(76,60),(76,90),(76,120),(76,150),(76,180),(76,210),
                   (76,240),(76,270),(76,300),(76,330)]),
            (Ice, [(-74,0),(-74,30),(-74,60),(-74,90),(-74,120),(-74,150),(-74,180),
                   (-74,210),(-74,240),(-74,270),(-74,300),(-74,330)]),
        ], smooth: true);


    // Tonerna ligger med flit nära varandra, och bältena tonar bort mot polerna.
    // Två saker fick rättas efter att kartan setts ritad. Först var kontrasten
    // för hård – mörkbruna bälten mot gräddvitt gav en randig boll snarare än en
    // planet, för på fotografier är skillnaden mellan bälte och zon
    // förvånansvärt liten. Sedan var alla bälten lika starka, vilket gav en
    // strandboll: i verkligheten dominerar de två ekvatorsbältena medan de
    // tempererade knappt syns. Därför fem toner i stället för två.
    static readonly Color JupiterZone = Color.FromArgb("#EDE4D3");
    static readonly Color JupiterEquator = Color.FromArgb("#EBE0CA");
    static readonly Color JupiterNorthBelt = Color.FromArgb("#C49A76");   // starkast
    static readonly Color JupiterSouthBelt = Color.FromArgb("#C9A182");
    static readonly Color JupiterBelt = Color.FromArgb("#D6BDA4");        // tempererade
    static readonly Color JupiterFaintBelt = Color.FromArgb("#E0D2BF");   // nätt och jämnt
    static readonly Color JupiterPolarInner = Color.FromArgb("#E2D6C4");
    static readonly Color JupiterPolar = Color.FromArgb("#D4C6B2");
    static readonly Color GreatRedSpot = Color.FromArgb("#D5793F");
    static readonly Color JupiterOval = Color.FromArgb("#F7F2E8");

    /// <summary>
    /// Jupiter: molnband i latitud och Stora röda fläcken. De ljusa zonerna är
    /// uppstigande ammoniakis, de mörka bältena nedsjunkande gas där man ser
    /// djupare in – det är alltså inte målade ränder utan väder i tvärsnitt.
    /// Gränserna nedan är de vedertagna: norra ekvatorialbältet 7–17° nord,
    /// södra 7–20° syd, sedan tempererade bälten i par ut mot polerna.
    ///
    /// Stora röda fläcken ligger på 22° syd, i södra tropiska zonen strax under
    /// sitt bälte, och är ritad 14,5° bred och 9,8° hög – 16 000 gånger
    /// 12 000 km, alltså bredare än jorden. Longituden är däremot vald på fri
    /// hand. Fläcken driver i verkligheten mot väster i förhållande till
    /// planetens inre rotation, ungefär ett varv på fyra år, och appen följer
    /// inte den driften. Var den står ett givet datum stämmer alltså inte, men
    /// att den finns, hur stor den är och hur den passerar runt kanten gör det.
    /// </summary>

    // ---------------------------------------------------- byggstenar för gaser
    //
    // Gasjättarna har inga kustlinjer att rita av. Deras ytor är band i latitud,
    // kalotter kring polerna och enstaka stormar, och de tre formerna räcker
    // långt. De ligger här som gemensamma hjälpare eftersom alla fyra jättar
    // använder dem.

    /// <summary>
    /// Ett band runt hela klotet, som en rad fyrhörningar. Ett enda varvpolygon
    /// hade inte fungerat: bandet är en ring med hål i, och det går inte att
    /// fylla. Rutorna överlappar en aning så att skarvarna inte syns som
    /// hårstreck, samma knep som ringarna använder.
    /// </summary>
    static void Band(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, float south, float north, int segments = 8)
    {
        for (int i = 0; i < segments; i++)
        {
            float a = i * 360f / segments;
            float b = (i + 1.02f) * 360f / segments;
            raw.Add((fill, [(north, a), (north, b), (south, b), (south, a)]));
        }
    }

    /// <summary>En kalott kring polen, precis som jordens isar.</summary>
    static void Cap(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, float lat)
    {
        var pts = new (float Lat, float Lon)[12];
        for (int i = 0; i < pts.Length; i++)
            pts[i] = (lat, i * 30f);
        raw.Add((fill, pts));
    }

    /// <summary>En storm som en oval fläck. Halvaxlarna anges i grader.</summary>
    static void Oval(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, float lat, float lon, float halfLat, float halfLon)
    {
        var pts = new (float Lat, float Lon)[16];
        for (int i = 0; i < pts.Length; i++)
        {
            double t = i * Math.PI * 2 / pts.Length;
            pts[i] = (lat + halfLat * (float)Math.Sin(t),
                      lon + halfLon * (float)Math.Cos(t));
        }
        raw.Add((fill, pts));
    }

    /// <summary>
    /// En regelbunden månghörning kring polen – Saturnus sexhörning är den enda
    /// användningen, och den enda kända formen av sitt slag i solsystemet.
    ///
    /// Kanterna måste räknas ut i planet sett rakt ovanifrån polen, inte
    /// interpoleras i latitud och longitud. Två hörn på samma breddgrad förbundna
    /// med en latitudlinje ger en cirkelbåge som buktar åt fel håll, och figuren
    /// blir en cirkel i stället för en månghörning.
    /// </summary>
    static void PolarPolygon(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, int sides, float vertexLat, int perEdge = 4)
    {
        bool north = vertexLat > 0;
        float radius = 90f - Math.Abs(vertexLat);      // avstånd från polen i grader
        var pts = new List<(float Lat, float Lon)>(sides * perEdge);
        for (int i = 0; i < sides; i++)
        {
            double a0 = i * Math.PI * 2 / sides, a1 = (i + 1) * Math.PI * 2 / sides;
            double x0 = radius * Math.Cos(a0), y0 = radius * Math.Sin(a0);
            double x1 = radius * Math.Cos(a1), y1 = radius * Math.Sin(a1);
            for (int k = 0; k < perEdge; k++)
            {
                double t = (double)k / perEdge;
                double x = x0 + (x1 - x0) * t, y = y0 + (y1 - y0) * t;
                double r = Math.Sqrt(x * x + y * y);
                double lon = Math.Atan2(y, x) * 180.0 / Math.PI;
                pts.Add(((float)(north ? 90.0 - r : r - 90.0), (float)lon));
            }
        }
        raw.Add((fill, [.. pts]));
    }

    public static readonly SurfaceMap Jupiter = BuildJupiter();

    static SurfaceMap BuildJupiter()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        Band(raw, JupiterEquator, -7, 7);        // ekvatorialzonen, den ljusaste
        Band(raw, JupiterNorthBelt, 7, 17);      // norra ekvatorialbältet, det tydligaste
        Band(raw, JupiterSouthBelt, -20, -7);    // södra ekvatorialbältet
        Band(raw, JupiterBelt, 24, 31);          // norra tempererade bältet
        Band(raw, JupiterBelt, -37, -27);        // södra tempererade bältet
        Band(raw, JupiterFaintBelt, 40, 48);     // norra norra tempererade bältet
        Band(raw, JupiterFaintBelt, -53, -45);   // södra södra tempererade bältet
        // Polarområdet i två steg. En enda kalott gav en hård grå kupol på
        // toppen; i verkligheten mörknar det gradvis från ungefär 50° och ut.
        Cap(raw, JupiterPolarInner, 55);
        Cap(raw, JupiterPolarInner, -55);
        Cap(raw, JupiterPolar, 70);
        Cap(raw, JupiterPolar, -70);

        Oval(raw, GreatRedSpot, -22, 65, 4.9f, 7.3f);

        // Tre av de vita ovalerna på 41° syd – stormar som liknar den röda
        // fläcken men är yngre och mindre.
        Oval(raw, JupiterOval, -41, 150, 2.5f, 4f);
        Oval(raw, JupiterOval, -41, 210, 2.5f, 4f);
        Oval(raw, JupiterOval, -41, 275, 2.5f, 4f);

        return new SurfaceMap(JupiterZone, [.. raw]);
    }



    /// <summary>
    /// Ett smalt streck som löper ut från en punkt i en given kompassriktning –
    /// en kraterstråle från Tycho, en spricka i Europas is.
    ///
    /// Strecket följer en storcirkel, inte en linje i gradnätet. Skillnaden är
    /// stor: en stråle som går rakt norrut från 45° syd och 2 000 km bort hamnar
    /// på helt olika ställen beroende på vilket man räknar. Bredden avtar mot
    /// slutet, som strålarna gör.
    /// </summary>
    static void Streak(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, float lat, float lon, float bearingDeg, float lengthDeg, float widthDeg)
    {
        const int steps = 7;
        var left = new List<(float Lat, float Lon)>(steps + 1);
        var right = new List<(float Lat, float Lon)>(steps + 1);

        for (int i = 0; i <= steps; i++)
        {
            float along = lengthDeg * i / steps;
            var mid = Offset(lat, lon, bearingDeg, along);
            // Bredden avtar linjärt, men aldrig till noll – en spets på under en
            // tiondels grad ger bara sammanfallande punkter.
            float half = Math.Max(0.1f, widthDeg * 0.5f * (1f - 0.8f * i / steps));
            left.Add(Offset(mid.Lat, mid.Lon, bearingDeg - 90f, half));
            right.Add(Offset(mid.Lat, mid.Lon, bearingDeg + 90f, half));
        }

        right.Reverse();
        raw.Add((fill, [.. left, .. right]));
    }

    /// <summary>
    /// Punkten som ligger <paramref name="distanceDeg"/> grader bort längs en
    /// storcirkel i riktningen <paramref name="bearingDeg"/>, räknad från norr
    /// och medurs. Vanlig sfärisk trigonometri.
    /// </summary>
    static (float Lat, float Lon) Offset(float lat, float lon, float bearingDeg, float distanceDeg)
    {
        double f1 = lat * Math.PI / 180.0, l1 = lon * Math.PI / 180.0;
        double b = bearingDeg * Math.PI / 180.0, d = distanceDeg * Math.PI / 180.0;
        double f2 = Math.Asin(Math.Sin(f1) * Math.Cos(d) + Math.Cos(f1) * Math.Sin(d) * Math.Cos(b));
        double l2 = l1 + Math.Atan2(Math.Sin(b) * Math.Sin(d) * Math.Cos(f1),
                                    Math.Cos(d) - Math.Sin(f1) * Math.Sin(f2));
        return ((float)(f2 * 180.0 / Math.PI), (float)(l2 * 180.0 / Math.PI));
    }

    static readonly Color MoonHighland = Color.FromArgb("#A8A49E");
    static readonly Color MoonMare = Color.FromArgb("#6F6F72");
    static readonly Color MoonMareDark = Color.FromArgb("#67676A");
    static readonly Color MoonRay = Color.FromArgb("#B7B3AD");

    /// <summary>
    /// Månen: ljusa högländer och mörka hav. Haven är inte vatten utan
    /// lavaslätter, utgjutna för tre och en halv miljard år sedan i de största
    /// nedslagsbassängerna, och de ligger nästan uteslutande på den sida som
    /// vetter mot jorden – varför vet ingen säkert.
    ///
    /// Att baksidan saknar hav faller ut av sig själv här: alla hav nedan ligger
    /// på longituder mellan 90° väst och 90° öst, alltså på framsidan, eftersom
    /// det är där de finns. Nollmeridianen pekar mot jorden.
    ///
    /// Tycho är bara 85 km bred men har det tydligaste strålsystemet på månen,
    /// synligt med blotta ögat vid fullmåne. Kratern är ung – hundra miljoner år,
    /// vilket på månen är nyss – så strålarna har inte hunnit mörkna.
    /// </summary>
    public static readonly SurfaceMap Moon = BuildMoon();

    static SurfaceMap BuildMoon()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        void Mare(Color fill, float lat, float lon, float radiusDeg)
        {
            float widen = 1f / MathF.Max(0.3f, MathF.Cos(lat * MathF.PI / 180f));
            Oval(raw, fill, lat, lon, radiusDeg, radiusDeg * widen);
        }

        // Haven, med sina uppmätta lägen och storlekar. Oceanus Procellarum är
        // det överlägset största: 2 500 km tvärs över.
        Mare(MoonMareDark, 18.4f, 302.6f, 42.4f);   // Oceanus Procellarum
        Mare(MoonMare, 32.8f, 344.4f, 18.9f);       // Mare Imbrium
        Mare(MoonMare, 28.0f, 17.5f, 11.7f);        // Mare Serenitatis
        Mare(MoonMare, 8.5f, 31.4f, 14.4f);         // Mare Tranquillitatis – Apollo 11
        Mare(MoonMare, 17.0f, 59.1f, 9.2f);         // Mare Crisium
        Mare(MoonMare, -7.8f, 51.3f, 15.0f);        // Mare Fecunditatis
        Mare(MoonMare, -15.2f, 35.5f, 5.5f);        // Mare Nectaris
        Mare(MoonMare, -21.3f, 343.4f, 11.8f);      // Mare Nubium
        Mare(MoonMare, -24.4f, 321.4f, 6.4f);       // Mare Humorum
        Mare(MoonMare, 13.3f, 3.6f, 4.0f);          // Mare Vaporum
        Oval(raw, MoonMare, 56.0f, 1.4f, 4.5f, 46f); // Mare Frigoris, långsträckt

        // Tychos strålsystem. Strålarna är smala – några tiotal kilometer breda
        // men tusentals långa – och sitter oregelbundet. Jämna mellanrum och
        // grova spikar ger en tecknad stjärna i stället för en krater, så både
        // riktningar och längder varierar.
        float[] tychoBearing = [8, 41, 67, 96, 129, 158, 187, 214, 243, 271, 302, 334];
        float[] tychoLength = [52, 34, 47, 28, 55, 38, 44, 31, 50, 36, 45, 40];
        for (int i = 0; i < tychoBearing.Length; i++)
            Streak(raw, MoonRay, -43.3f, 348.8f, tychoBearing[i], tychoLength[i], 2.4f);
        Oval(raw, MoonRay, -43.3f, 348.8f, 2.2f, 3.0f);

        // Copernicus, den andra kratern med tydliga strålar, kortare och svagare.
        float[] copBearing = [17, 63, 104, 148, 196, 241, 288, 331];
        for (int i = 0; i < copBearing.Length; i++)
            Streak(raw, MoonRay, 9.6f, 339.9f, copBearing[i], 14f + (i % 3) * 5f, 1.8f);
        Oval(raw, MoonRay, 9.6f, 339.9f, 1.6f, 1.7f);

        return new SurfaceMap(MoonHighland, [.. raw]);
    }

    static readonly Color PlutoBase = Color.FromArgb("#AF9174");
    static readonly Color PlutoIce = Color.FromArgb("#EAE3D6");
    static readonly Color PlutoDark = Color.FromArgb("#71503E");

    /// <summary>
    /// Pluto som New Horizons såg den i juli 2015 – de första bilderna någonsin
    /// av dess yta, efter nio och ett halvt års färd.
    ///
    /// Tombaugh Regio är hjärtat: ett ljust fält av frusen kväve, uppkallat
    /// efter Clyde Tombaugh som upptäckte Pluto 1930. Dess västra lob, Sputnik
    /// Planitia, är en slätt utan en enda krater, vilket betyder att ytan förnyas
    /// – kvävet flyter långsamt som en glaciär. Bredvid ligger Cthulhu Macula,
    /// ett mörkt band av tholiner, organiska ämnen som solljuset bakat fram ur
    /// metan.
    /// </summary>
    public static readonly SurfaceMap Pluto = BuildPluto();

    static SurfaceMap BuildPluto()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        // Cthulhu Macula, det mörka bandet längs ekvatorn väster om hjärtat.
        Oval(raw, PlutoDark, -5f, 90f, 20f, 62f);
        // Den ljusa nordpolskalotten av metanis.
        Cap(raw, PlutoIce, 68f);

        // Hjärtat: två lober och en spets nedåt.
        Oval(raw, PlutoIce, 20f, 175f, 22f, 27f);   // Sputnik Planitia
        Oval(raw, PlutoIce, 18f, 212f, 16f, 21f);
        raw.Add((PlutoIce, [(6f, 150f), (10f, 225f), (-8f, 214f), (-26f, 186f), (-8f, 160f)]));

        return new SurfaceMap(PlutoBase, [.. raw], smooth: true);
    }

    static readonly Color MercuryBase = Color.FromArgb("#8C8A87");
    static readonly Color MercuryPlains = Color.FromArgb("#807E7B");
    static readonly Color MercuryLight = Color.FromArgb("#9A9895");
    static readonly Color MercuryDark = Color.FromArgb("#7B7976");
    static readonly Color MercuryRay = Color.FromArgb("#ADABA8");

    /// <summary>
    /// Merkurius är grå och full av kratrar, så lik månen att bilderna är svåra
    /// att skilja åt. Den saknar atmosfär som kunnat vittra ned dem, så ytan har
    /// legat i stort sett orörd sedan det stora bombardemanget för fyra
    /// miljarder år sedan.
    ///
    /// De fyra namngivna bassängerna ligger på sina uppmätta lägen i östlig
    /// longitud. Caloris är den överlägset största: 1 550 km tvärs över, alltså
    /// en fjärdedel av hela planetens omkrets, slagen av något som nästan
    /// klöv den. De spridda kratrarna är däremot inte verkliga utan slumpade ur
    /// ett fast frö, så att bilden blir densamma varje gång utan att någon
    /// behöver rita in fyrtio kratrar för hand.
    /// </summary>
    public static readonly SurfaceMap Mercury = BuildMercury();

    static SurfaceMap BuildMercury()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        /// En krater är rund på klotet, inte i gradnätet: longitudgrader trängs
        /// ihop mot polerna, så halvaxeln i longitud måste vidgas med 1/cos(lat).
        void Crater(Color fill, float lat, float lon, float radiusDeg)
        {
            float widen = 1f / MathF.Max(0.25f, MathF.Cos(lat * MathF.PI / 180f));
            Oval(raw, fill, lat, lon, radiusDeg, radiusDeg * widen);
        }

        // De släta slätterna i norr, utlagda av lava.
        Cap(raw, MercuryPlains, 62);

        // Spridda kratrar ur ett fast frö. Latituden dras ur arcsin så att de
        // fördelas jämnt över klotet och inte klumpar ihop sig vid polerna.
        uint seed = 20260901;
        float Next()
        {
            seed = seed * 1664525u + 1013904223u;
            return (seed >> 8) / (float)(1 << 24);
        }
        for (int i = 0; i < 46; i++)
        {
            float lat = (float)(Math.Asin(2 * Next() - 1) * 180 / Math.PI) * 0.9f;
            float lon = Next() * 360f;
            float r = 1.6f + Next() * 4.6f;
            Crater(Next() < 0.45f ? MercuryLight : MercuryDark, lat, lon, r);
        }

        // Caloris: 1 550 km tvärs över, den största nedslagsbassängen. Golvet är
        // ljusare än omgivningen eftersom det fyllts av lava.
        Crater(MercuryLight, 30.5f, 189.8f, 18.2f);
        Crater(MercuryDark, 20.8f, 236.2f, 7.4f);    // Beethoven
        Crater(MercuryDark, -32.9f, 271.8f, 8.4f);   // Rembrandt
        Crater(MercuryDark, -16.3f, 195.1f, 4.6f);   // Tolstoj

        // Kuiper: liten men med ett ljust strålsystem, ung nog att strålarna
        // inte hunnit mörkna.
        Crater(MercuryRay, -11.3f, 328.6f, 5.0f);
        Crater(MercuryRay, -33.0f, 12.0f, 4.0f);     // Debussy

        // Ingen utjämning: kratrarna är redan runda från Oval, och att runda
        // dem en gång till skulle fyrdubbla antalet punkter utan att synas.
        return new SurfaceMap(MercuryBase, [.. raw]);
    }

    static readonly Color VenusBase = Color.FromArgb("#E6D3A4");
    static readonly Color VenusShade = Color.FromArgb("#DFCB99");
    static readonly Color VenusPolar = Color.FromArgb("#EEDDB2");

    /// <summary>
    /// Venus visar ingenting av sin yta. Molntäcket av svavelsyra är helt
    /// ogenomskinligt, och det tog radar från omloppsbana att kartlägga marken
    /// under. I vanligt ljus är planeten en jämn gulvit skiva utan drag.
    ///
    /// De svaga strimmorna nedan är det Y-formade mönster som syns i
    /// ultraviolett ljus, återgivet så blekt att det knappt märks. De finns med
    /// av ett enda skäl: utan något att följa med blicken går det inte att se
    /// att planeten roterar, och att den roterar baklänges är hela poängen med
    /// Venus. En helt jämn skiva hade varit ärligare mot ögat men dolt saken.
    ///
    /// Med det följer en förenkling värd att känna till: molnen i verkligheten
    /// far runt planeten på fyra dygn, medan marken under tar 243. Appen låter
    /// strimmorna följa marken. Det som visas är alltså planetens rotation, inte
    /// molnens – och det är planetens rotation etappen handlar om.
    /// </summary>
    public static readonly SurfaceMap Venus = BuildVenus();

    static SurfaceMap BuildVenus()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        // Y-mönstrets stam längs ekvatorn och dess två armar, plus de ljusare
        // polhättorna. Allt med minimal kontrast mot grundtonen.
        Band(raw, VenusShade, -12, 12, 6);
        Oval(raw, VenusShade, 28, 60, 12f, 55f);
        Oval(raw, VenusShade, -30, 55, 12f, 55f);
        Oval(raw, VenusShade, 10, 210, 16f, 40f);
        Cap(raw, VenusPolar, 68);
        Cap(raw, VenusPolar, -68);

        // Ingen utjämning: bandet består av fyrhörningar som måste behålla sina
        // raka kanter, annars glipar fogarna mellan dem.
        return new SurfaceMap(VenusBase, [.. raw]);
    }

    static readonly Color SaturnZone = Color.FromArgb("#EBDDB4");
    static readonly Color SaturnEquator = Color.FromArgb("#F0E5C2");
    static readonly Color SaturnBelt = Color.FromArgb("#DFCEA0");
    static readonly Color SaturnFaintBelt = Color.FromArgb("#E5D6AB");
    static readonly Color SaturnPolarInner = Color.FromArgb("#DCCFAE");
    static readonly Color SaturnPolar = Color.FromArgb("#C9C0AC");
    static readonly Color SaturnHexagon = Color.FromArgb("#BDB7A6");

    /// <summary>
    /// Saturnus har samma slags band som Jupiter men mycket svagare. Dimman
    /// högre upp i atmosfären suddar ut dem, så planeten ser nästan enfärgat
    /// gulbeige ut i ett litet teleskop – bandmönstret finns där, men det syns
    /// först vid närmare påseende.
    ///
    /// Kring nordpolen ligger sexhörningen: en jetström som håller sex raka
    /// sidor, nästan 30 000 km tvärs över, upptäckt av Voyager 1980 och
    /// fotograferad på nytt av Cassini. Ingen annan känd form i solsystemet
    /// beter sig så, och ingen vet säkert varför den håller ihop.
    /// </summary>
    public static readonly SurfaceMap Saturn = BuildSaturn();

    static SurfaceMap BuildSaturn()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        Band(raw, SaturnEquator, -8, 8);
        Band(raw, SaturnBelt, 8, 20);
        Band(raw, SaturnBelt, -22, -8);
        Band(raw, SaturnFaintBelt, 28, 36);
        Band(raw, SaturnFaintBelt, -38, -30);
        Band(raw, SaturnFaintBelt, 44, 52);
        Band(raw, SaturnFaintBelt, -54, -46);
        Cap(raw, SaturnPolarInner, 60);
        Cap(raw, SaturnPolarInner, -60);
        Cap(raw, SaturnPolar, 72);
        Cap(raw, SaturnPolar, -72);

        // Sexhörningen: hörnen 14,3° från polen, alltså på 75,7° nord, vilket
        // ger de 29 000 km tvärs över som Cassini mätte upp.
        PolarPolygon(raw, SaturnHexagon, 6, 75.7f);

        return new SurfaceMap(SaturnZone, [.. raw]);
    }

    static readonly Color UranusBase = Color.FromArgb("#A9DCE0");
    static readonly Color UranusBand = Color.FromArgb("#9FD3D8");
    static readonly Color UranusPolar = Color.FromArgb("#B4E2E5");

    /// <summary>
    /// Uranus är den släta. Metanet i atmosfären slukar rött ljus och lämnar
    /// det blågröna, och där Jupiter och Saturnus har bandmönster har Uranus
    /// nästan ingenting – Voyager 2 passerade 1986 och fann en planet så jämn
    /// att bilderna såg ut som en målad boll.
    ///
    /// De två svaga banden och den ljusare polkalotten finns med av ett skäl
    /// utöver att de är verkliga: utan minsta drag på ytan går det inte att se
    /// att planeten rullar i stället för att snurra, och det är hela poängen
    /// med Uranus.
    /// </summary>
    public static readonly SurfaceMap Uranus = BuildUranus();

    static SurfaceMap BuildUranus()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();
        Band(raw, UranusBand, -25, -10);
        Band(raw, UranusBand, 15, 28);
        Cap(raw, UranusPolar, 55);
        return new SurfaceMap(UranusBase, [.. raw]);
    }

    static readonly Color NeptuneBase = Color.FromArgb("#3F6FD0");
    static readonly Color NeptuneBand = Color.FromArgb("#3862BC");
    static readonly Color NeptuneZone = Color.FromArgb("#4C7ED8");
    static readonly Color GreatDarkSpot = Color.FromArgb("#27408A");
    static readonly Color NeptuneCloud = Color.FromArgb("#DCE8F5");

    /// <summary>
    /// Neptunus är djupare blå än Uranus trots nästan samma sammansättning, och
    /// ingen vet riktigt varför. Den har också väder, vilket Uranus knappt har:
    /// de snabbaste vindarna i solsystemet, över 2 000 km/h.
    ///
    /// Stora mörka fläcken är ritad som Voyager 2 såg den 1989, på 22° syd och
    /// ungefär lika stor som jorden, med sitt vita följeslagarmoln. Den är
    /// däremot inte permanent som Jupiters röda fläck: när Hubble tittade efter
    /// 1994 var den borta, och nya mörka fläckar har kommit och gått sedan dess.
    /// Appen visar alltså ett tillstånd, inte ett bestående drag.
    /// </summary>
    public static readonly SurfaceMap Neptune = BuildNeptune();

    static SurfaceMap BuildNeptune()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        Band(raw, NeptuneZone, -8, 8);
        Band(raw, NeptuneBand, 18, 32);
        Band(raw, NeptuneBand, -34, -20);
        Cap(raw, NeptuneZone, 62);
        Cap(raw, NeptuneZone, -62);

        // Stora mörka fläcken, ungefär jordstor: 13 000 × 6 600 km, vilket på
        // Neptunus blir en tredjedel av vägen runt planeten i longitud.
        Oval(raw, GreatDarkSpot, -22, 40, 7.7f, 16.3f);
        // "Scooter", det ljusa molnet som följde med fläcken.
        Oval(raw, NeptuneCloud, -33, 32, 2.5f, 7f);

        return new SurfaceMap(NeptuneBase, [.. raw]);
    }



    /// <summary>
    /// Rundar av en polygon med Chaikins hörnkapning: varje hörn ersätts av två
    /// punkter en fjärdedel in på vardera kanten. Två varv räcker för att en
    /// sexhörning ska läsas som en fläck i stället för som en polygon.
    ///
    /// Görs bara där gränsen verkligen är diffus, och det är ett val per karta.
    /// Jordens kustlinjer ÄR kantiga och ska inte jämnas ut. Jupiters band får
    /// heller inte rundas – de är fyrhörningar som ska ha raka kanter, annars
    /// öppnar sig glipor mellan dem. Mars albedofält är dammgränser utan skarpa
    /// kanter och blir polygoner om de lämnas som de är.
    ///
    /// Longituderna vecklas ut till en sammanhängande följd först. Utan det
    /// skulle medelvärdet av 358° och 8° bli 183°, alltså rakt över på andra
    /// sidan klotet, för varje yta som korsar nollmeridianen – och Sinus
    /// Meridiani gör just det.
    /// </summary>
    static (float Lat, float Lon)[] Smooth((float Lat, float Lon)[] pts, int rounds = 2)
    {
        var lat = pts.Select(p => p.Lat).ToList();
        var lon = new List<float>(pts.Length) { pts[0].Lon };
        for (int i = 1; i < pts.Length; i++)
        {
            float d = pts[i].Lon - lon[i - 1];
            while (d > 180) d -= 360;
            while (d < -180) d += 360;
            lon.Add(lon[i - 1] + d);
        }

        for (int r = 0; r < rounds; r++)
        {
            var nextLat = new List<float>(lat.Count * 2);
            var nextLon = new List<float>(lat.Count * 2);
            for (int i = 0; i < lat.Count; i++)
            {
                int j = (i + 1) % lat.Count;
                float lonJ = lon[j];
                if (j == 0)
                {
                    // Sista kanten sluter kurvan. Ta den variant av startpunkten
                    // som ligger närmast slutpunkten, annars viks ett helt varv
                    // runt klotet ihop till ingenting – polarisarna är sådana.
                    float d = lon[0] - lon[^1];
                    lonJ = lon[0] - 360f * MathF.Round(d / 360f);
                }
                nextLat.Add(0.75f * lat[i] + 0.25f * lat[j]);
                nextLon.Add(0.75f * lon[i] + 0.25f * lonJ);
                nextLat.Add(0.25f * lat[i] + 0.75f * lat[j]);
                nextLon.Add(0.25f * lon[i] + 0.75f * lonJ);
            }
            lat = nextLat;
            lon = nextLon;
        }

        return [.. lat.Select((v, i) => (v, lon[i]))];
    }

    /// <summary>
    /// Delar upp långa kanter i steg om högst 5 grader så att kustlinjerna
    /// följer klotets buktning och klipps snyggt mot dess rand. Sinus/cosinus
    /// för latituden förberäknas – vid ritning varierar bara longituden
    /// (kroppens rotation).
    /// </summary>
    static Region Densify(Color fill, (float Lat, float Lon)[] pts)
    {
        var lat = new List<float>(pts.Length * 4);
        var lon = new List<float>(pts.Length * 4);
        for (int i = 0; i < pts.Length; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Length];
            float dLon = b.Lon - a.Lon;
            while (dLon > 180) dLon -= 360;   // kortaste vägen runt klotet
            while (dLon < -180) dLon += 360;
            float dLat = b.Lat - a.Lat;
            int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(dLat), MathF.Abs(dLon)) / 5f));
            for (int s = 0; s < steps; s++)
            {
                lat.Add(a.Lat + dLat * s / steps);
                lon.Add(a.Lon + dLon * s / steps);
            }
        }
        const float d2r = MathF.PI / 180f;
        return new Region(fill,
            [.. lat.Select(v => MathF.Sin(v * d2r))],
            [.. lat.Select(v => MathF.Cos(v * d2r))],
            [.. lon.Select(v => v * d2r)]);
    }
}
