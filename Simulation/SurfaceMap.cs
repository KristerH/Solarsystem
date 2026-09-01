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

    SurfaceMap(Color baseColor, (Color Fill, (float Lat, float Lon)[] Pts)[] raw)
    {
        BaseColor = baseColor;
        Regions = [.. raw.Select(r => Densify(r.Fill, r.Pts))];
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
