namespace Solarsystem.Simulation;

/// <summary>
/// A body's surface as (latitude, longitude) polygons. They're drawn as
/// filled shapes directly on the globe – no texture image needed.
///
/// The outlines are rough (schoolbook-poster level) but the features are
/// recognisable. Longitude is measured the way the body turns, i.e.
/// eastward for everything that rotates prograde.
/// </summary>
public sealed class SurfaceMap
{
    /// <param name="MeanSinLat">
    /// The sine of the region's centre latitude. Only needed for the Sun,
    /// whose regions turn at different speeds depending on where they sit.
    /// That the rate is taken per region rather than per vertex is
    /// deliberate: a sunspot group turns as one clump, and letting every
    /// vertex go at its own rate would smear the group out over a few years.
    /// Real groups never last that long – they die within weeks.
    /// </param>
    public sealed record Region(Color Fill, float[] SinLat, float[] CosLat, float[] LonRad,
        float MeanSinLat);

    /// <summary>The base colour under the polygons – Earth's oceans, Mars's desert.</summary>
    public Color BaseColor { get; }

    public Region[] Regions { get; }

    /// <param name="smooth">
    /// Rounds off the regions' corners before drawing. For bodies whose
    /// features have diffuse boundaries – Mars's dust fields – rather than
    /// sharp ones, like Earth's coastlines.
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
    /// Earth's landmasses. The prime meridian sits where it should:
    /// Greenwich at longitude zero, Africa just east of it.
    /// </summary>
    public static readonly SurfaceMap Earth = new(Ocean,
        [
            // Africa
            (Land, [(37,10),(33,22),(30,32),(12,43),(11,51),(2,46),(-5,39),(-15,40),(-26,33),
                    (-34,20),(-33,17),(-22,14),(-8,13),(4,9),(6,-1),(5,-8),(12,-17),(21,-17),
                    (28,-12),(33,-9),(36,-3)]),
            // Madagascar
            (Land, [(-12,49),(-17,50),(-25,47),(-22,43),(-16,44)]),
            // Eurasia
            (Land, [(43,-9),(36,-6),(38,0),(43,4),(38,16),(36,23),(38,27),(36,33),(31,35),
                    (29,35),(13,44),(16,53),(24,58),(27,50),(24,54),(25,61),(21,72),(8,77),
                    (13,80),(20,87),(22,91),(16,94),(9,98),(1,104),(8,105),(11,109),(21,108),
                    (23,117),(31,122),(37,123),(35,129),(43,132),(54,137),(59,143),(51,157),
                    (61,163),(64,177),(67,190),(70,180),(72,150),(76,113),(73,80),(68,55),
                    (68,40),(71,26),(65,12),(59,5),(55,8),(54,9),(52,4),(48,-2),(44,-2)]),
            // The British Isles
            (Land, [(58,-5),(56,-2),(53,0),(51,1),(50,-4),(53,-4),(54,-6),(56,-6)]),
            // North America
            (Land, [(60,-166),(66,-162),(71,-157),(70,-141),(69,-125),(72,-108),(72,-95),
                    (66,-84),(59,-78),(62,-72),(58,-68),(54,-57),(47,-52),(45,-64),(41,-70),
                    (35,-76),(31,-81),(25,-80),(29,-84),(29,-91),(26,-97),(21,-97),(18,-94),
                    (15,-92),(13,-87),(9,-81),(8,-78),(9,-84),(16,-95),(19,-105),(23,-110),
                    (28,-114),(33,-118),(38,-123),(46,-124),(54,-131),(59,-140),(59,-152),
                    (55,-162)]),
            // South America
            (Land, [(11,-72),(10,-62),(5,-52),(0,-50),(-5,-35),(-13,-38),(-23,-42),(-34,-53),
                    (-39,-62),(-47,-66),(-54,-68),(-53,-71),(-46,-74),(-37,-73),(-30,-71),
                    (-18,-70),(-5,-81),(1,-80),(7,-77)]),
            // Australia
            (Land, [(-12,131),(-11,136),(-12,142),(-19,147),(-27,153),(-34,151),(-38,147),
                    (-38,141),(-35,137),(-32,133),(-32,125),(-34,115),(-26,113),(-20,119),
                    (-14,126)]),
            // New Guinea
            (Land, [(-1,131),(-3,141),(-8,148),(-10,150),(-8,143),(-5,135)]),
            // Greenland (ice-covered)
            (Ice, [(83,-35),(81,-20),(76,-19),(70,-22),(60,-43),(65,-53),(72,-56),(77,-70),
                   (81,-62)]),
            // Arctic sea ice around the north pole
            (Ice, [(84,0),(84,30),(84,60),(84,90),(84,120),(84,150),(84,180),(84,210),
                   (84,240),(84,270),(84,300),(84,330)]),
            // Antarctica around the south pole
            (Ice, [(-70,0),(-68,30),(-66,60),(-66,90),(-66,120),(-68,150),(-71,180),
                   (-74,210),(-75,240),(-72,270),(-64,297),(-70,330)]),
        ]);

    static readonly Color MarsDust = Color.FromArgb("#C4663F");
    static readonly Color MarsBright = Color.FromArgb("#DB9160");
    static readonly Color MarsDark = Color.FromArgb("#7E5341");
    static readonly Color MarsCanyon = Color.FromArgb("#66412F");

    /// <summary>
    /// Mars's albedo map: the dark features seen through telescopes since the
    /// 1600s. They aren't seas but bare bedrock the wind has swept clean of
    /// light dust, which is why they slowly change shape between dust-storm
    /// seasons. The names still date from the time people thought otherwise –
    /// Mare Cimmerium, Mare Sirenum.
    ///
    /// Longitudes are east and measured from Airy-0, the small crater that
    /// defines Mars's prime meridian. Syrtis Major therefore lands around
    /// 70° east, where it should: that's the feature Christiaan Huygens
    /// drew in 1659 and timed to measure Mars's day.
    ///
    /// The polar caps are drawn with the same extent year-round. In reality
    /// they breathe with the seasons – the southern winter cap reaches down
    /// toward 50° south and then shrinks to a spot – but the app has no
    /// seasonal model for frost.
    /// </summary>
    public static readonly SurfaceMap Mars = new(MarsDust,
        [
            // Bright highlands: Hellas, the 2,300 km wide impact basin that
            // often lies dust-filled and glowing, and Tharsis with the Solar
            // System's largest volcanoes.
            (MarsBright, [(-28,55),(-30,80),(-42,92),(-55,80),(-56,58),(-45,47)]),
            (MarsBright, [(25,230),(20,260),(0,268),(-10,255),(-6,232),(10,222)]),

            // Syrtis Major: the dark triangle, widest in the north. Mars's
            // best-known feature and the first anyone saw on another planet's
            // surface.
            (MarsDark, [(20,62),(18,76),(8,78),(0,74),(-4,70),(2,66),(10,60)]),
            // Mare Acidalium – the large dark field on the northern hemisphere.
            (MarsDark, [(60,330),(55,350),(48,5),(38,10),(30,0),(32,340),(40,325),(52,318)]),
            // Sinus Sabaeus and Sinus Meridiani, the band along the equator
            // that runs straight through the prime meridian.
            (MarsDark, [(-2,318),(2,330),(0,345),(2,358),(0,8),(-6,6),(-8,350),(-10,335),(-8,322)]),
            // Mare Erythraeum
            (MarsDark, [(-14,300),(-12,320),(-18,340),(-28,345),(-34,330),(-32,310),(-24,298)]),
            // Mare Tyrrhenum
            (MarsDark, [(-12,82),(-14,100),(-20,115),(-28,110),(-30,95),(-24,82)]),
            // Mare Cimmerium
            (MarsDark, [(-14,140),(-16,160),(-22,185),(-32,188),(-34,165),(-26,142)]),
            // Mare Sirenum
            (MarsDark, [(-20,205),(-24,225),(-32,240),(-40,232),(-38,210),(-28,200)]),
            // Solis Lacus, "the eye of Mars", which blinks as dust storms pass over.
            (MarsDark, [(-22,262),(-24,275),(-32,278),(-36,268),(-30,259)]),
            // Boreosyrtis near Utopia
            (MarsDark, [(48,95),(44,120),(36,128),(34,108),(40,92)]),

            // Valles Marineris: 4,000 km long, ten times the Grand Canyon and
            // deep enough to fit Mount Everest standing up. Drawn as a streak.
            (MarsCanyon, [(-6,262),(-9,280),(-12,300),(-15,320),(-18,318),(-15,298),(-12,278),(-9,260)]),

            // The polar caps: water ice under a lid of frozen carbon dioxide.
            (Ice, [(76,0),(76,30),(76,60),(76,90),(76,120),(76,150),(76,180),(76,210),
                   (76,240),(76,270),(76,300),(76,330)]),
            (Ice, [(-74,0),(-74,30),(-74,60),(-74,90),(-74,120),(-74,150),(-74,180),
                   (-74,210),(-74,240),(-74,270),(-74,300),(-74,330)]),
        ], smooth: true);


    // The tones deliberately sit close together, and the belts fade out
    // toward the poles. Two things had to be corrected after the map was
    // seen rendered. First, the contrast was too hard – dark brown belts
    // against cream white gave a striped ball rather than a planet, since
    // in photographs the difference between a belt and a zone is
    // surprisingly small. Then all the belts were equally strong, giving a
    // beach ball: in reality the two equatorial belts dominate while the
    // temperate ones are barely visible. Hence five tones instead of two.
    static readonly Color JupiterZone = Color.FromArgb("#EDE4D3");
    static readonly Color JupiterEquator = Color.FromArgb("#EBE0CA");
    static readonly Color JupiterNorthBelt = Color.FromArgb("#C49A76");   // strongest
    static readonly Color JupiterSouthBelt = Color.FromArgb("#C9A182");
    static readonly Color JupiterBelt = Color.FromArgb("#D6BDA4");        // temperate
    static readonly Color JupiterFaintBelt = Color.FromArgb("#E0D2BF");   // barely there
    static readonly Color JupiterPolarInner = Color.FromArgb("#E2D6C4");
    static readonly Color JupiterPolar = Color.FromArgb("#D4C6B2");
    static readonly Color GreatRedSpot = Color.FromArgb("#D5793F");
    static readonly Color JupiterOval = Color.FromArgb("#F7F2E8");

    /// <summary>
    /// Jupiter: cloud bands in latitude and the Great Red Spot. The bright
    /// zones are rising ammonia ice, the dark belts sinking gas where you
    /// see deeper in – so it's not painted stripes but weather in
    /// cross-section. The boundaries below are the accepted ones: the North
    /// Equatorial Belt 7–17° north, the South 7–20° south, then temperate
    /// belts in pairs out toward the poles.
    ///
    /// The Great Red Spot sits at 22° south, in the South Tropical Zone just
    /// below its belt, and is drawn 14.5° wide and 9.8° tall – 16,000 by
    /// 12,000 km, wider than Earth. Its longitude, however, is chosen
    /// freely. In reality the Spot drifts westward relative to the planet's
    /// interior rotation, roughly one lap every four years, and the app
    /// doesn't follow that drift. Where it stands on a given date is
    /// therefore not accurate, but that it exists, how big it is, and how it
    /// passes around the limb is.
    /// </summary>

    // ---------------------------------------------------- gas giant building blocks
    //
    // The gas giants have no coastlines to draw. Their surfaces are bands in
    // latitude, caps around the poles and the occasional storm, and those
    // three shapes cover a lot of ground. They live here as shared helpers
    // since all four giants use them.

    /// <summary>
    /// A band around the whole globe, as a row of quadrilaterals. A single
    /// loop polygon wouldn't have worked: the band is a ring with a hole in
    /// it, and that can't be filled. The tiles overlap slightly so the
    /// seams don't show up as hairline gaps, the same trick the rings use.
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

    /// <summary>A cap around the pole, just like Earth's ice caps.</summary>
    static void Cap(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, float lat)
    {
        var pts = new (float Lat, float Lon)[12];
        for (int i = 0; i < pts.Length; i++)
            pts[i] = (lat, i * 30f);
        raw.Add((fill, pts));
    }

    /// <summary>A storm as an oval spot. The semi-axes are given in degrees.</summary>
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
    /// A regular polygon around the pole – Saturn's hexagon is the only use,
    /// and the only known shape of its kind in the Solar System.
    ///
    /// The edges must be computed in the plane seen straight down from the
    /// pole, not interpolated in latitude and longitude. Two vertices on the
    /// same latitude connected by a latitude line give a circular arc
    /// bulging the wrong way, and the figure becomes a circle instead of a
    /// polygon.
    /// </summary>
    static void PolarPolygon(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, int sides, float vertexLat, int perEdge = 4)
    {
        bool north = vertexLat > 0;
        float radius = 90f - Math.Abs(vertexLat);      // distance from the pole in degrees
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

        Band(raw, JupiterEquator, -7, 7);        // the equatorial zone, the brightest
        Band(raw, JupiterNorthBelt, 7, 17);      // the North Equatorial Belt, the clearest
        Band(raw, JupiterSouthBelt, -20, -7);    // the South Equatorial Belt
        Band(raw, JupiterBelt, 24, 31);          // the North Temperate Belt
        Band(raw, JupiterBelt, -37, -27);        // the South Temperate Belt
        Band(raw, JupiterFaintBelt, 40, 48);     // the North North Temperate Belt
        Band(raw, JupiterFaintBelt, -53, -45);   // the South South Temperate Belt
        // The polar region in two steps. A single cap gave a hard grey dome
        // on top; in reality it darkens gradually from about 50° outward.
        Cap(raw, JupiterPolarInner, 55);
        Cap(raw, JupiterPolarInner, -55);
        Cap(raw, JupiterPolar, 70);
        Cap(raw, JupiterPolar, -70);

        Oval(raw, GreatRedSpot, -22, 65, 4.9f, 7.3f);

        // Three of the white ovals at 41° south – storms resembling the Red
        // Spot but younger and smaller.
        Oval(raw, JupiterOval, -41, 150, 2.5f, 4f);
        Oval(raw, JupiterOval, -41, 210, 2.5f, 4f);
        Oval(raw, JupiterOval, -41, 275, 2.5f, 4f);

        return new SurfaceMap(JupiterZone, [.. raw]);
    }



    /// <summary>
    /// A narrow streak running out from a point in a given compass
    /// direction – a crater ray from Tycho, a crack in Europa's ice.
    ///
    /// The streak follows a great circle, not a line in the lat/lon grid.
    /// The difference is large: a ray running due north from 45° south for
    /// 2,000 km lands in a completely different spot depending on which one
    /// is used. The width tapers toward the end, the way rays do.
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
            // The width tapers linearly, but never to zero – a tip under a
            // tenth of a degree just gives coincident points.
            float half = Math.Max(0.1f, widthDeg * 0.5f * (1f - 0.8f * i / steps));
            left.Add(Offset(mid.Lat, mid.Lon, bearingDeg - 90f, half));
            right.Add(Offset(mid.Lat, mid.Lon, bearingDeg + 90f, half));
        }

        right.Reverse();
        raw.Add((fill, [.. left, .. right]));
    }

    /// <summary>
    /// The point <paramref name="distanceDeg"/> degrees away along a great
    /// circle in the direction <paramref name="bearingDeg"/>, measured from
    /// north and clockwise. Standard spherical trigonometry.
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


    /// <summary>
    /// A ring around a point on the surface, built from quadrilaterals for
    /// the same reason as the cloud bands: a ring has a hole in it and can't
    /// be filled as a polygon. Callisto's Valhalla is the ripples an impact
    /// left behind.
    /// </summary>
    static void Annulus(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        Color fill, float lat, float lon, float innerDeg, float outerDeg, int segments = 14)
    {
        for (int i = 0; i < segments; i++)
        {
            float a = i * 360f / segments;
            float b = (i + 1.05f) * 360f / segments;
            var p0 = Offset(lat, lon, a, innerDeg);
            var p1 = Offset(lat, lon, b, innerDeg);
            var p2 = Offset(lat, lon, b, outerDeg);
            var p3 = Offset(lat, lon, a, outerDeg);
            raw.Add((fill, [p0, p1, p2, p3]));
        }
    }

    // ------------------------------------------------- the large moons
    //
    // All four Galilean moons and Titan are tidally locked: they show the
    // same side to their planet, just like our Moon does to Earth. Longitude
    // is therefore measured from the point facing the planet. Zero faces the
    // planet, 180° is directly opposite, 270° is the centre of the leading
    // hemisphere – the one moving first in the orbit – and 90° the centre of
    // the trailing one. That matters: Jupiter's magnetic field sweeps past
    // the moons from behind and bakes sulphur into exactly the trailing
    // side.

    static readonly Color IoSulphur = Color.FromArgb("#DFC96E");
    static readonly Color IoFrost = Color.FromArgb("#F0E8C8");
    static readonly Color IoPatera = Color.FromArgb("#8A6A32");
    static readonly Color IoDeposit = Color.FromArgb("#C97445");

    /// <summary>
    /// Io is the most volcanically active body in the Solar System. Jupiter
    /// and the other moons knead it so hard that the bedrock pumps up and
    /// down a hundred metres every lap, and the heat from that keeps the
    /// whole moon molten inside. Four hundred volcanoes spew sulphur and
    /// silicate rock.
    ///
    /// The consequence is that Io has no craters at all. The surface is
    /// remade faster than impacts can leave marks – on average a centimetre
    /// of new material is laid down every year, which buries a crater within
    /// a few thousand years. No other solid surface in the Solar System is
    /// that young.
    ///
    /// The spots below are representative, not measured: the pattern of
    /// dark volcanic calderas and bright sulphur dioxide fields is Io's
    /// look, but which particular volcano sits where isn't taken from a map.
    /// </summary>
    public static readonly SurfaceMap Io = BuildIo();

    static SurfaceMap BuildIo()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();
        uint seed = 314159u;
        float Next()
        {
            seed = seed * 1664525u + 1013904223u;
            return (seed >> 8) / (float)(1 << 24);
        }

        void Spot(Color fill, float lat, float lon, float radiusDeg)
        {
            float widen = 1f / MathF.Max(0.3f, MathF.Cos(lat * MathF.PI / 180f));
            Oval(raw, fill, lat, lon, radiusDeg, radiusDeg * widen);
        }

        // Bright sulphur dioxide fields first, then the volcanoes on top of them.
        for (int i = 0; i < 14; i++)
            Spot(IoFrost, (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.85f,
                 Next() * 360f, 6f + Next() * 9f);
        for (int i = 0; i < 22; i++)
        {
            float lat = (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.85f;
            float lon = Next() * 360f;
            float r = 2f + Next() * 4f;
            // Orange-red ejecta surrounds the largest calderas.
            if (r > 4.5f) Spot(IoDeposit, lat, lon, r * 1.9f);
            Spot(IoPatera, lat, lon, r);
        }

        return new SurfaceMap(IoSulphur, [.. raw]);
    }

    static readonly Color EuropaIce = Color.FromArgb("#E2E5E8");
    static readonly Color EuropaTrailing = Color.FromArgb("#D8CDC4");
    static readonly Color EuropaChaos = Color.FromArgb("#C9BDB1");
    static readonly Color EuropaLinea = Color.FromArgb("#A9705A");

    /// <summary>
    /// Europa is the smoothest thing we know of: the highest point on the
    /// whole moon rises a few hundred metres. Under the ice lies an ocean
    /// with more water than all of Earth's oceans combined, and the cracks
    /// in the surface are where the ice crust has been pulled apart and
    /// filled in from below.
    ///
    /// The trailing hemisphere, around 90° in this map's reckoning, is
    /// darker and redder than the leading one. That's no coincidence:
    /// Jupiter's magnetic field rotates faster than Europa can keep up with,
    /// so it sweeps past from behind and bakes sulphur from Io's volcanoes
    /// into exactly that side.
    /// </summary>
    public static readonly SurfaceMap Europa = BuildEuropa();

    static SurfaceMap BuildEuropa()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();
        uint seed = 271828u;
        float Next()
        {
            seed = seed * 1664525u + 1013904223u;
            return (seed >> 8) / (float)(1 << 24);
        }

        // The trailing hemisphere, slightly darker.
        Oval(raw, EuropaTrailing, 0f, 90f, 70f, 88f);

        // Chaos terrain: fields where the ice has broken up and refrozen.
        for (int i = 0; i < 9; i++)
            Oval(raw, EuropaChaos, (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.8f,
                 Next() * 360f, 5f + Next() * 6f, 8f + Next() * 10f);

        // The cracks. They run for thousands of kilometres and cross each
        // other, so both the starting point and direction are randomised –
        // but wide enough to be visible.
        for (int i = 0; i < 26; i++)
        {
            float lat = (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.8f;
            float lon = Next() * 360f;
            Streak(raw, EuropaLinea, lat, lon, Next() * 360f, 40f + Next() * 70f, 2.2f);
        }

        return new SurfaceMap(EuropaIce, [.. raw]);
    }

    static readonly Color GanymedeBase = Color.FromArgb("#8E8377");
    static readonly Color GanymedeDark = Color.FromArgb("#6B6055");
    static readonly Color GanymedeGroove = Color.FromArgb("#A79C8E");
    static readonly Color GanymedeCrater = Color.FromArgb("#BDB3A5");

    /// <summary>
    /// Ganymede is the largest moon in the Solar System – bigger than
    /// Mercury, and the only moon with its own magnetic field. The surface
    /// is a patchwork of two kinds of terrain: dark, ancient and
    /// heavily-cratered regions, and lighter fields laced with parallel
    /// grooves where the ice crust has been pulled apart.
    /// </summary>
    public static readonly SurfaceMap Ganymede = BuildGanymede();

    static SurfaceMap BuildGanymede()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();
        uint seed = 161803u;
        float Next()
        {
            seed = seed * 1664525u + 1013904223u;
            return (seed >> 8) / (float)(1 << 24);
        }

        // The dark, old regions – roughly a third of the surface.
        for (int i = 0; i < 7; i++)
            Oval(raw, GanymedeDark, (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.85f,
                 Next() * 360f, 14f + Next() * 12f, 18f + Next() * 18f);

        // The bright grooves, in bundles of parallel streaks. Roughly half
        // of Ganymede's surface is this kind of terrain, so there need to be
        // many bundles.
        for (int i = 0; i < 18; i++)
        {
            float lat = (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.8f;
            float lon = Next() * 360f;
            float bearing = Next() * 360f;
            for (int k = -2; k <= 2; k++)
            {
                var start = Offset(lat, lon, bearing + 90f, k * 4f);
                Streak(raw, GanymedeGroove, start.Lat, start.Lon, bearing, 26f, 2.6f);
            }
        }

        // Bright, young craters.
        for (int i = 0; i < 12; i++)
        {
            float lat = (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.85f;
            float widen = 1f / MathF.Max(0.3f, MathF.Cos(lat * MathF.PI / 180f));
            float r = 1.4f + Next() * 2.4f;
            Oval(raw, GanymedeCrater, lat, Next() * 360f, r, r * widen);
        }

        return new SurfaceMap(GanymedeBase, [.. raw]);
    }

    // The tones deliberately sit close together. Valhalla's rings first
    // came out as a sharp target pattern; in reality they're faint ripples
    // you can just make out, not lines.
    static readonly Color CallistoBase = Color.FromArgb("#655C53");
    static readonly Color CallistoLeading = Color.FromArgb("#71685E");
    static readonly Color CallistoCrater = Color.FromArgb("#786F65");
    static readonly Color CallistoBright = Color.FromArgb("#867D72");
    static readonly Color CallistoRing = Color.FromArgb("#7C7368");

    /// <summary>
    /// Callisto has the oldest surface in the Solar System. It lies outside
    /// the Laplace resonance that kneads the three inner moons, so nothing
    /// has heated it – the surface is the same as four billion years ago,
    /// and so densely cratered that new impacts can only strike old ones.
    ///
    /// Valhalla is the scar left by the largest of them: a bright spot
    /// surrounded by ripples, together 3,800 km across. The impact tore up
    /// the ice like a stone in a pond, and it froze solid before the waves
    /// had time to settle.
    /// </summary>
    public static readonly SurfaceMap Callisto = BuildCallisto();

    static SurfaceMap BuildCallisto()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();
        uint seed = 577215u;
        float Next()
        {
            seed = seed * 1664525u + 1013904223u;
            return (seed >> 8) / (float)(1 << 24);
        }

        // The leading hemisphere is brighter – impacts there dig up clean ice.
        Oval(raw, CallistoLeading, 0f, 270f, 70f, 88f);

        // Valhalla, on the far hemisphere toward the leading side. The outer
        // ring reaches 45° from the centre, giving the measured 3,800 km
        // across.
        Annulus(raw, CallistoRing, 18f, 240f, 26f, 30f);
        Annulus(raw, CallistoRing, 18f, 240f, 41f, 45f);
        Oval(raw, CallistoBright, 18f, 240f, 13f, 14f);

        // Craters, and they sit densely: Callisto is the most heavily
        // cratered surface in the Solar System, saturated so new impacts can
        // only strike old ones.
        for (int i = 0; i < 110; i++)
        {
            float lat = (float)(Math.Asin(2 * Next() - 1) / Math.PI * 180) * 0.9f;
            float widen = 1f / MathF.Max(0.28f, MathF.Cos(lat * MathF.PI / 180f));
            float r = 1.3f + Next() * 3.4f;
            Oval(raw, Next() < 0.3f ? CallistoBright : CallistoCrater,
                 lat, Next() * 360f, r, r * widen);
        }

        return new SurfaceMap(CallistoBase, [.. raw]);
    }

    static readonly Color MoonHighland = Color.FromArgb("#A8A49E");
    static readonly Color MoonMare = Color.FromArgb("#6F6F72");
    static readonly Color MoonMareDark = Color.FromArgb("#67676A");
    static readonly Color MoonRay = Color.FromArgb("#B7B3AD");

    /// <summary>
    /// The Moon: bright highlands and dark maria. The maria aren't water but
    /// lava plains, poured out three and a half billion years ago in the
    /// largest impact basins, and they lie almost exclusively on the
    /// Earth-facing side – nobody knows for certain why.
    ///
    /// That the far side lacks maria falls out on its own here: every mare
    /// below sits at longitudes between 90° west and 90° east, i.e. on the
    /// near side, because that's where they are. The prime meridian points
    /// at Earth.
    ///
    /// Tycho is only 85 km wide but has the clearest ray system on the
    /// Moon, visible to the naked eye at full moon. The crater is young –
    /// a hundred million years, which on the Moon is recent – so the rays
    /// haven't had time to darken.
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

        // The maria, with their measured positions and sizes. Oceanus
        // Procellarum is by far the largest: 2,500 km across.
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
        Oval(raw, MoonMare, 56.0f, 1.4f, 4.5f, 46f); // Mare Frigoris, elongated

        // Tycho's ray system. The rays are narrow – a few tens of
        // kilometres wide but thousands long – and irregularly spaced.
        // Even spacing and blunt spikes give a cartoon star instead of a
        // crater, so both directions and lengths vary.
        float[] tychoBearing = [8, 41, 67, 96, 129, 158, 187, 214, 243, 271, 302, 334];
        float[] tychoLength = [52, 34, 47, 28, 55, 38, 44, 31, 50, 36, 45, 40];
        for (int i = 0; i < tychoBearing.Length; i++)
            Streak(raw, MoonRay, -43.3f, 348.8f, tychoBearing[i], tychoLength[i], 2.4f);
        Oval(raw, MoonRay, -43.3f, 348.8f, 2.2f, 3.0f);

        // Copernicus, the other crater with clear rays, shorter and fainter.
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
    /// Pluto as New Horizons saw it in July 2015 – the first images ever of
    /// its surface, after nine and a half years of travel.
    ///
    /// Tombaugh Regio is the heart: a bright field of frozen nitrogen, named
    /// after Clyde Tombaugh, who discovered Pluto in 1930. Its western lobe,
    /// Sputnik Planitia, is a plain without a single crater, meaning the
    /// surface renews itself – the nitrogen flows slowly like a glacier.
    /// Next to it lies Cthulhu Macula, a dark band of tholins, organic
    /// compounds sunlight has baked out of methane.
    /// </summary>
    public static readonly SurfaceMap Pluto = BuildPluto();

    static SurfaceMap BuildPluto()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        // Cthulhu Macula, the dark band along the equator west of the heart.
        Oval(raw, PlutoDark, -5f, 90f, 20f, 62f);
        // The bright north polar cap of methane ice.
        Cap(raw, PlutoIce, 68f);

        // The heart: two lobes and a point downward.
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
    /// Mercury is grey and full of craters, so similar to the Moon that
    /// photos are hard to tell apart. It has no atmosphere to have weathered
    /// them down, so the surface has stayed largely untouched since the
    /// heavy bombardment four billion years ago.
    ///
    /// The four named basins sit at their measured positions in east
    /// longitude. Caloris is by far the largest: 1,550 km across, a quarter
    /// of the whole planet's circumference, struck by something that nearly
    /// split it in two. The scattered craters, on the other hand, aren't
    /// real but randomised from a fixed seed, so the picture comes out the
    /// same every time without anyone having to draw forty craters by hand.
    /// </summary>
    public static readonly SurfaceMap Mercury = BuildMercury();

    static SurfaceMap BuildMercury()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        /// A crater is round on the globe, not in the lat/lon grid: degrees
        /// of longitude bunch up toward the poles, so the semi-axis in
        /// longitude must be widened by 1/cos(lat).
        void Crater(Color fill, float lat, float lon, float radiusDeg)
        {
            float widen = 1f / MathF.Max(0.25f, MathF.Cos(lat * MathF.PI / 180f));
            Oval(raw, fill, lat, lon, radiusDeg, radiusDeg * widen);
        }

        // The smooth plains in the north, laid down by lava.
        Cap(raw, MercuryPlains, 62);

        // Scattered craters from a fixed seed. Latitude is drawn through
        // arcsin so they're spread evenly over the globe rather than
        // clumping at the poles.
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

        // Caloris: 1,550 km across, the largest impact basin. The floor is
        // brighter than its surroundings since it's been filled with lava.
        Crater(MercuryLight, 30.5f, 189.8f, 18.2f);
        Crater(MercuryDark, 20.8f, 236.2f, 7.4f);    // Beethoven
        Crater(MercuryDark, -32.9f, 271.8f, 8.4f);   // Rembrandt
        Crater(MercuryDark, -16.3f, 195.1f, 4.6f);   // Tolstoy

        // Kuiper: small but with a bright ray system, young enough that the
        // rays haven't had time to darken.
        Crater(MercuryRay, -11.3f, 328.6f, 5.0f);
        Crater(MercuryRay, -33.0f, 12.0f, 4.0f);     // Debussy

        // No smoothing: the craters are already round via Oval, and
        // rounding them again would quadruple the point count without any
        // visible effect.
        return new SurfaceMap(MercuryBase, [.. raw]);
    }

    static readonly Color VenusBase = Color.FromArgb("#E6D3A4");
    static readonly Color VenusShade = Color.FromArgb("#DFCB99");
    static readonly Color VenusPolar = Color.FromArgb("#EEDDB2");

    /// <summary>
    /// Venus shows none of its surface. The sulphuric-acid cloud deck is
    /// completely opaque, and it took orbital radar to map the ground
    /// beneath it. In ordinary light the planet is an even yellow-white
    /// disc without features.
    ///
    /// The faint streaks below are the Y-shaped pattern visible in
    /// ultraviolet light, rendered so pale it's barely noticeable. They're
    /// included for a single reason: without something for the eye to
    /// follow, there's no way to see that the planet rotates, and that it
    /// rotates backwards is the whole point of Venus. A perfectly even disc
    /// would have been more honest to the eye but would have hidden the
    /// fact.
    ///
    /// That comes with a simplification worth knowing: the clouds in
    /// reality race around the planet in four days, while the ground
    /// beneath takes 243. The app lets the streaks follow the ground. What's
    /// shown is therefore the planet's rotation, not the clouds' – and it's
    /// the planet's rotation this stage is about.
    /// </summary>
    public static readonly SurfaceMap Venus = BuildVenus();

    static SurfaceMap BuildVenus()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        // The Y-pattern's stem along the equator and its two arms, plus the
        // lighter polar hoods. All with minimal contrast against the base
        // colour.
        Band(raw, VenusShade, -12, 12, 6);
        Oval(raw, VenusShade, 28, 60, 12f, 55f);
        Oval(raw, VenusShade, -30, 55, 12f, 55f);
        Oval(raw, VenusShade, 10, 210, 16f, 40f);
        Cap(raw, VenusPolar, 68);
        Cap(raw, VenusPolar, -68);

        // No smoothing: the band is made of quadrilaterals that must keep
        // their straight edges, otherwise gaps open up between them.
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
    /// Saturn has the same kind of bands as Jupiter but much fainter. Haze
    /// higher up in the atmosphere blurs them out, so the planet looks
    /// almost uniformly tan-yellow in a small telescope – the band pattern
    /// is there, but only shows on closer inspection.
    ///
    /// Around the north pole sits the hexagon: a jet stream holding six
    /// straight sides, nearly 30,000 km across, discovered by Voyager in
    /// 1980 and photographed again by Cassini. No other known shape in the
    /// Solar System behaves like that, and nobody knows for certain why it
    /// holds together.
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

        // The hexagon: vertices 14.3° from the pole, i.e. at 75.7° north,
        // which gives the 29,000 km across that Cassini measured.
        PolarPolygon(raw, SaturnHexagon, 6, 75.7f);

        return new SurfaceMap(SaturnZone, [.. raw]);
    }

    static readonly Color UranusBase = Color.FromArgb("#A9DCE0");
    static readonly Color UranusBand = Color.FromArgb("#9FD3D8");
    static readonly Color UranusPolar = Color.FromArgb("#B4E2E5");

    /// <summary>
    /// Uranus is the smooth one. Methane in the atmosphere swallows red
    /// light and leaves the blue-green, and where Jupiter and Saturn have
    /// band patterns, Uranus has almost nothing – Voyager 2 flew past in
    /// 1986 and found a planet so even the images looked like a painted
    /// ball.
    ///
    /// The two faint bands and the lighter polar cap are included for a
    /// reason beyond being real: without the slightest feature on the
    /// surface, there's no way to see that the planet rolls rather than
    /// spins, and that's the whole point of Uranus.
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
    /// Neptune is a deeper blue than Uranus despite an almost identical
    /// composition, and nobody really knows why. It also has weather, which
    /// Uranus barely does: the fastest winds in the Solar System, over
    /// 2,000 km/h.
    ///
    /// The Great Dark Spot is drawn as Voyager 2 saw it in 1989, at 22°
    /// south and roughly Earth-sized, with its white companion cloud. It
    /// isn't permanent like Jupiter's Red Spot, though: when Hubble checked
    /// in 1994 it was gone, and new dark spots have come and gone since.
    /// The app therefore shows a state, not a lasting feature.
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

        // The Great Dark Spot, roughly Earth-sized: 13,000 × 6,600 km,
        // which on Neptune comes to a third of the way around the planet in
        // longitude.
        Oval(raw, GreatDarkSpot, -22, 40, 7.7f, 16.3f);
        // "Scooter", the bright cloud that travelled with the spot.
        Oval(raw, NeptuneCloud, -33, 32, 2.5f, 7f);

        return new SurfaceMap(NeptuneBase, [.. raw]);
    }



    /// <summary>
    /// Rounds off a polygon with Chaikin's corner-cutting: every vertex is
    /// replaced by two points a quarter of the way along each edge. Two
    /// rounds are enough for a hexagon to read as a blob instead of a
    /// polygon.
    ///
    /// Only done where the boundary is genuinely diffuse, and that's a
    /// choice made per map. Earth's coastlines ARE jagged and shouldn't be
    /// smoothed. Jupiter's bands can't be rounded either – they're
    /// quadrilaterals that need straight edges, otherwise gaps open up
    /// between them. Mars's albedo features are dust boundaries without
    /// sharp edges and read as polygons if left as they are.
    ///
    /// The longitudes are unwrapped into a continuous sequence first.
    /// Without that, the average of 358° and 8° would come out as 183°,
    /// i.e. straight across on the far side of the globe, for any region
    /// crossing the prime meridian – and Sinus Meridiani does exactly that.
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
                    // The last edge closes the curve. Take the version of the
                    // starting point closest to the end point, otherwise a
                    // full turn around the globe folds up into nothing – the
                    // polar caps are like that.
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

    // ------------------------------------------------------------------ the Sun

    /// <summary>The photosphere, the surface we see. Drawn in the app with a gradient, see below.</summary>
    static readonly Color Photosphere = Color.FromArgb("#FFE08A");

    /// <summary>The spot's penumbra, roughly 1,000 degrees cooler than the surrounding surface.</summary>
    static readonly Color Penumbra = Color.FromArgb("#C98A3A");

    /// <summary>The umbra, 1,500 degrees cooler. Still glowing – just darker than everything else.</summary>
    static readonly Color Umbra = Color.FromArgb("#6B4116");

    /// <summary>
    /// A sunspot: a dark core in a lighter penumbra. The size is the
    /// penumbra's half-width in degrees – a large group spans ten degrees
    /// across, i.e. over a hundred thousand kilometres, more than ten
    /// Earths wide.
    ///
    /// The longitude semi-axis is divided by the cosine of the latitude. A
    /// round spot covers more degrees of longitude the farther from the
    /// equator it sits, just as a city at high latitude sits between more
    /// closely spaced meridians.
    /// </summary>
    static void Spot(List<(Color Fill, (float Lat, float Lon)[] Pts)> raw,
        float lat, float lon, float size)
    {
        float wide = size / MathF.Cos(lat * MathF.PI / 180f);
        Oval(raw, Penumbra, lat, lon, size, wide);
        Oval(raw, Umbra, lat, lon, size * 0.42f, wide * 0.42f);
    }

    /// <summary>
    /// The Sun's surface: a handful of sunspot groups.
    ///
    /// The spots don't sit just anywhere. They keep to two belts on either
    /// side of the equator, between five and thirty degrees of latitude, and
    /// that's no coincidence but a consequence of the Sun rotating at
    /// different speeds at different latitudes: the rotation stretches the
    /// magnetic field into bundles that eventually break through the
    /// surface in pairs. Where they break through, heat from below is
    /// blocked, and the spot ends up 1,500 degrees cooler than the
    /// surrounding surface. It looks black, but only by comparison – lift a
    /// spot out of the Sun and it would shine brighter than the full moon.
    ///
    /// The groups come in pairs, a leading and a following spot. That's the
    /// magnetic field's two ends, and they have opposite polarity – a fact
    /// invisible in the image but which explains why they always appear two
    /// by two.
    ///
    /// <b>Caveat:</b> the spots sit still here and exist forever. In
    /// reality a group lives a few weeks, and the count follows the
    /// eleven-year cycle: at minimum the Sun is completely smooth, at
    /// maximum full of spots. Their latitudes also migrate toward the
    /// equator over the course of the cycle – plotted on a diagram the
    /// pattern becomes the famous butterfly, and that picture has no room
    /// in a map that looks the same at every point in time. The map
    /// therefore shows how the Sun looks in a year near maximum.
    ///
    /// The faculae, the bright veils around the spots, are deliberately
    /// left out: they're visible almost only near the limb, where you're
    /// looking obliquely through the gas, and that's an effect a flat
    /// colour surface can't express.
    /// </summary>
    public static readonly SurfaceMap Sun = BuildSun();

    static SurfaceMap BuildSun()
    {
        var raw = new List<(Color Fill, (float Lat, float Lon)[] Pts)>();

        // The northern belt. Each pair is a group: leading spot, then following.
        Spot(raw, 14f, 40f, 3.2f);
        Spot(raw, 12f, 51f, 1.9f);

        Spot(raw, 22f, 152f, 2.4f);
        Spot(raw, 21f, 161f, 1.3f);

        Spot(raw, 8f, 248f, 3.6f);
        Spot(raw, 10f, 259f, 2.1f);

        // The southern belt.
        Spot(raw, -12f, 95f, 2.9f);
        Spot(raw, -13f, 105f, 1.7f);

        Spot(raw, -19f, 198f, 2.2f);
        Spot(raw, -18f, 207f, 1.2f);

        Spot(raw, -7f, 318f, 1.6f);

        // Individual pores: small spots without a penumbra, the kind that
        // come and go within a day. Drawn as umbra only.
        Oval(raw, Umbra, 17f, 88f, 0.7f, 0.75f);
        Oval(raw, Umbra, -25f, 271f, 0.6f, 0.66f);
        Oval(raw, Umbra, 6f, 175f, 0.5f, 0.5f);

        return new SurfaceMap(Photosphere, [.. raw]);
    }

    /// <summary>
    /// Splits long edges into steps of at most 5 degrees so coastlines
    /// follow the globe's curvature and clip cleanly against its limb.
    /// Sine/cosine for the latitude are precomputed – at render time only
    /// the longitude varies (the body's rotation).
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
            while (dLon > 180) dLon -= 360;   // the shortest way around the globe
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
            [.. lon.Select(v => v * d2r)],
            MathF.Sin(lat.Average() * d2r));
    }
}
