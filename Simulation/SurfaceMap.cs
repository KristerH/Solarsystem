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
    public static readonly SurfaceMap Jupiter = BuildJupiter();

    static SurfaceMap BuildJupiter()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        // Ett band runt hela klotet ritas som en rad fyrhörningar. Ett enda
        // varvpolygon hade inte fungerat: bandet är en ring med hål i, och det
        // går inte att fylla. Rutorna överlappar en aning så att skarvarna inte
        // syns som hårstreck, samma knep som ringarna använder.
        void Band(Color fill, float south, float north)
        {
            const int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float a = i * 360f / segments;
                float b = (i + 1.02f) * 360f / segments;
                raw.Add((fill, [(north, a), (north, b), (south, b), (south, a)]));
            }
        }

        // Polarområdena går däremot att rita som en enkel kalott, precis som
        // jordens isar.
        void Cap(Color fill, float lat)
        {
            var pts = new (float Lat, float Lon)[12];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = (lat, i * 30f);
            raw.Add((fill, pts));
        }

        void Oval(Color fill, float lat, float lon, float halfLat, float halfLon)
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

        Band(JupiterEquator, -7, 7);        // ekvatorialzonen, den ljusaste
        Band(JupiterNorthBelt, 7, 17);      // norra ekvatorialbältet, det tydligaste
        Band(JupiterSouthBelt, -20, -7);    // södra ekvatorialbältet
        Band(JupiterBelt, 24, 31);          // norra tempererade bältet
        Band(JupiterBelt, -37, -27);        // södra tempererade bältet
        Band(JupiterFaintBelt, 40, 48);     // norra norra tempererade bältet
        Band(JupiterFaintBelt, -53, -45);   // södra södra tempererade bältet
        // Polarområdet i två steg. En enda kalott gav en hård grå kupol på
        // toppen; i verkligheten mörknar det gradvis från ungefär 50° och ut.
        Cap(JupiterPolarInner, 55);
        Cap(JupiterPolarInner, -55);
        Cap(JupiterPolar, 70);
        Cap(JupiterPolar, -70);

        Oval(GreatRedSpot, -22, 65, 4.9f, 7.3f);

        // Tre av de vita ovalerna på 41° syd – stormar som liknar den röda
        // fläcken men är yngre och mindre.
        Oval(JupiterOval, -41, 150, 2.5f, 4f);
        Oval(JupiterOval, -41, 210, 2.5f, 4f);
        Oval(JupiterOval, -41, 275, 2.5f, 4f);

        return new SurfaceMap(JupiterZone, [.. raw]);
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
