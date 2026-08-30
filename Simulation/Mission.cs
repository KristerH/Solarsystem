using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En rymdfärd från jorden till en annan planet, längs en överföringsbana som
/// farkosten följer utan att styra – precis som en verklig sond gör mellan
/// raketmotorns två korta brinntider.
///
/// Banan är i grunden en Hohmann-överföring: en halv ellips med perihelium vid
/// jordens bana och aphelium vid målets. Den energisnålaste vägen, men den kan
/// bara nå punkter som ligger exakt 180 grader bort. Mars bana lutar 1,85 grader
/// mot ekliptikan och planeten är därför nästan aldrig exakt antiparallell med
/// jorden vid uppskjutningen. Skulle man ändå tvinga fram en halv ellips landar
/// farkosten flera miljoner kilometer vid sidan av.
///
/// Därför löses banan i stället ur sina randvillkor: den ska starta vid jordens
/// läge och nå målets läge efter en sveptvinkel som är ungefär, men inte exakt,
/// 180 grader. Med start i perihelium ger det
///
///     e = (r2 - r1) / (r1 - r2 * cos(sveptvinkel))
///     a = r1 / (1 - e)
///
/// vilket för sveptvinkeln 180 grader övergår i den vanliga Hohmann-formeln.
/// Restiden följer sedan av Keplers ekvation. Eftersom målets läge i sin tur
/// beror på restiden söks den fram som ett nollställe – se Plan nedan.
/// </summary>
public sealed class Mission
{
    /// <summary>Farkostens namn, visas vid pricken.</summary>
    public string Name { get; }

    /// <summary>Målet som farkosten är på väg mot.</summary>
    public CelestialBody Target { get; }

    /// <summary>Uppskjutningsögonblicket, i dygn sedan J2000.</summary>
    public double LaunchDay { get; }

    /// <summary>Ankomstögonblicket, i dygn sedan J2000.</summary>
    public double ArrivalDay { get; }

    /// <summary>Restidens längd i dygn.</summary>
    public double TravelDays => ArrivalDay - LaunchDay;

    /// <summary>Halva storaxeln i AU.</summary>
    public double SemiMajorAu { get; }

    /// <summary>Banans excentricitet.</summary>
    public double Eccentricity { get; }

    // Banplanets bas: periheliets riktning och den vinkelräta i rörelseriktningen.
    readonly Vector3 _periDir;
    readonly Vector3 _sideDir;
    readonly double _periodDays;

    Mission(string name, CelestialBody target, double launchDay, double arrivalDay,
        double semiMajorAu, double eccentricity, double periodDays,
        Vector3 periDir, Vector3 sideDir)
    {
        Name = name;
        Target = target;
        LaunchDay = launchDay;
        ArrivalDay = arrivalDay;
        SemiMajorAu = semiMajorAu;
        Eccentricity = eccentricity;
        _periodDays = periodDays;
        _periDir = periDir;
        _sideDir = sideDir;
    }

    /// <summary>
    /// Planerar en färd från en kropp till en annan med uppskjutning vid given
    /// tid. Returnerar null när ingen ellips klarar båda randvillkoren – det
    /// inträffar för stora delar av året och är själva skälet till att
    /// uppskjutningar bara kan ske under vissa startfönster.
    /// </summary>
    public static Mission? Plan(string name, CelestialBody origin, CelestialBody target,
        double launchDay)
    {
        // Allt räknas i AU; skalan till världsenheter läggs på först vid ritning.
        var r1 = origin.PositionAt(launchDay, 1f);
        double r1Len = r1.Length();
        if (r1Len < 1e-6)
            return null;
        var r1Dir = r1 / (float)r1Len;

        // Målets läge beror på restiden, som i sin tur följer av banan. Att bara
        // mata tillbaka den beräknade restiden om och om igen fungerar inte – den
        // itereringen svänger fram och tillbaka utan att närma sig svaret. I
        // stället söks nollstället till "beräknad restid minus antagen restid",
        // först grovt över ett intervall och sedan med intervallhalvering.
        const double minTravel = 40.0;
        const double maxTravel = 560.0;
        const int samples = 60;

        double lo = 0, hi = 0, loValue = 0;
        bool bracketed = false;
        double prevTravel = 0, prevValue = 0;
        bool hasPrev = false;

        for (int i = 0; i <= samples; i++)
        {
            double travel = minTravel + (maxTravel - minTravel) * i / samples;
            if (!Residual(target, launchDay, travel, r1Len, r1Dir, out double value, out _))
            {
                hasPrev = false;
                continue;
            }
            if (hasPrev && prevValue * value < 0)
            {
                lo = prevTravel;
                hi = travel;
                loValue = prevValue;
                bracketed = true;
                break;
            }
            prevTravel = travel;
            prevValue = value;
            hasPrev = true;
        }

        if (!bracketed)
            return null;

        for (int k = 0; k < 60; k++)
        {
            double mid = 0.5 * (lo + hi);
            if (!Residual(target, launchDay, mid, r1Len, r1Dir, out double value, out _))
                return null;
            if (loValue * value <= 0)
            {
                hi = mid;
            }
            else
            {
                lo = mid;
                loValue = value;
            }
        }

        double travelDays = 0.5 * (lo + hi);
        if (!Residual(target, launchDay, travelDays, r1Len, r1Dir, out _, out var solution))
            return null;

        // Banplanet spänns av start- och målriktningen.
        var normal = Vector3.Cross(r1Dir, solution.TargetDir);
        var planeNormal = normal.Length() > 1e-6f
            ? Vector3.Normalize(normal)
            : Vector3.UnitY;

        // Sidoriktningen pekar dit farkosten rör sig när den lämnar periheliet.
        var sideDir = Vector3.Cross(planeNormal, r1Dir);
        if (sideDir.Length() < 1e-6f)
            return null;

        return new Mission(name, target, launchDay, launchDay + travelDays,
            solution.SemiMajor, solution.Eccentricity, solution.Period,
            r1Dir, Vector3.Normalize(sideDir));
    }

    readonly record struct Solution(
        double SemiMajor, double Eccentricity, double Period, Vector3 TargetDir);

    /// <summary>
    /// För en antagen restid: bygg den ellips som både startar vid r1 och når
    /// målets läge, och ge skillnaden mellan dess verkliga restid och den antagna.
    /// Med start i perihelium följer excentriciteten direkt ur randvillkoren.
    /// </summary>
    static bool Residual(CelestialBody target, double launchDay, double travel,
        double r1Len, Vector3 r1Dir, out double residual, out Solution solution)
    {
        residual = 0;
        solution = default;

        var r2 = target.PositionAt(launchDay + travel, 1f);
        double r2Len = r2.Length();
        if (r2Len < 1e-6)
            return false;
        var r2Dir = r2 / (float)r2Len;

        double cosSweep = Math.Clamp(Vector3.Dot(r1Dir, r2Dir), -1.0, 1.0);
        double sweep = Math.Acos(cosSweep);

        // Nämnaren går mot noll när målet ligger rakt bakom start sett från solen.
        double denominator = r1Len - r2Len * cosSweep;
        if (Math.Abs(denominator) < 1e-9)
            return false;

        double e = (r2Len - r1Len) / denominator;
        if (e is < 0 or >= 1 || double.IsNaN(e))
            return false;   // ingen ellips klarar båda randvillkoren

        double a = r1Len / (1.0 - e);
        double period = 365.256 * Math.Pow(a, 1.5);

        // Restid ur Keplers ekvation: från perihelium fram till sveptvinkeln.
        double halfSweep = sweep * 0.5;
        double ecc = 2.0 * Math.Atan2(
            Math.Sqrt(1.0 - e) * Math.Sin(halfSweep),
            Math.Sqrt(1.0 + e) * Math.Cos(halfSweep));
        double computed = (ecc - e * Math.Sin(ecc)) / (Math.PI * 2) * period;

        residual = computed - travel;
        solution = new Solution(a, e, period, r2Dir);
        return true;
    }

    /// <summary>
    /// Farkostens läge vid given tid, i världskoordinater. Efter ankomsten
    /// följer den med målet: en verklig sond går in i omloppsbana eller landar,
    /// och blir alltså kvar vid planeten. Utan det blir farkosten stående still
    /// i tomma rymden medan planeten åker vidare – efter ett halvt år ligger de
    /// nästan 400 miljoner kilometer isär.
    /// </summary>
    public Vector3 PositionAt(double day, float unitsPerAu)
    {
        if (day >= ArrivalDay)
            return Target.PositionAt(day, unitsPerAu);
        return TransferPositionAt(day, unitsPerAu);
    }

    /// <summary>
    /// Läget på själva överföringsbanan, oavsett om farkosten redan kommit fram.
    /// Används för att rita banan.
    /// </summary>
    public Vector3 TransferPositionAt(double day, float unitsPerAu)
    {
        double meanAnomaly = (day - LaunchDay) / _periodDays * Math.PI * 2;

        double e = Eccentricity;
        double ecc = meanAnomaly;
        for (int k = 0; k < 12; k++)
            ecc -= (ecc - e * Math.Sin(ecc) - meanAnomaly) / (1.0 - e * Math.Cos(ecc));

        double x = SemiMajorAu * (Math.Cos(ecc) - e);
        double y = SemiMajorAu * Math.Sqrt(1.0 - e * e) * Math.Sin(ecc);

        return (_periDir * (float)x + _sideDir * (float)y) * unitsPerAu;
    }

    /// <summary>Sant när farkosten har nått fram.</summary>
    public bool HasArrived(double day) => day >= ArrivalDay;

    /// <summary>
    /// Farkostens fart i km/s vid given tid, ur vis-viva-ekvationen. Farten är
    /// högst vid uppskjutningen och lägst vid ankomsten, precis som Keplers
    /// andra lag säger.
    /// </summary>
    public double SpeedKmPerSecond(double day)
    {
        double r = TransferPositionAt(Math.Min(day, ArrivalDay), 1f).Length();
        if (r < 1e-9)
            return 0;

        // Solens gravitationsparameter uttryckt i AU och dygn.
        const double muAu = 2.959122082855911e-4;
        double v = Math.Sqrt(Math.Max(0.0, muAu * (2.0 / r - 1.0 / SemiMajorAu)));
        return v * SolarSystemData.AuKm / 86_400.0;
    }
}
