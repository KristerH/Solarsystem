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
///
/// Samma klass beskriver också färder som kretsar kring en planet i stället
/// för kring solen, som resan till månen – se PlanToMoon. Skillnaden ligger
/// bara i vilken kropp banan räknas kring och hur hårt den drar (µ); ellipsen
/// och Keplers ekvation är desamma.
/// </summary>
public sealed class Mission
{
    /// <summary>Farkostens namn, visas vid pricken.</summary>
    public string Name { get; }

    /// <summary>Målet som farkosten är på väg mot.</summary>
    public CelestialBody Target { get; }

    /// <summary>
    /// Kroppen banan kretsar kring: null för solen, eller planeten när färden
    /// går till en av dess månar. Farkostens lägen räknas i förhållande till
    /// den, precis som månarnas banelement är planetcentriska.
    /// </summary>
    public CelestialBody? Center { get; }

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

    /// <summary>
    /// Hur många grader farkosten sveper kring centralkroppen på vägen. En ren
    /// Hohmann-överföring – den energisnålaste – sveper exakt 180 grader. Ju
    /// längre ifrån det, desto mer bränsle skulle färden kräva, så vinkeln
    /// duger som mått på hur bra ett uppskjutningstillfälle är.
    /// </summary>
    public double SweepDegrees { get; }

    // Banplanets bas: periheliets riktning och den vinkelräta i rörelseriktningen.
    readonly Vector3 _periDir;
    readonly Vector3 _sideDir;
    readonly double _periodDays;

    // Centralkroppens gravitationsparameter, i AU och dygn.
    readonly double _mu;

    Mission(string name, CelestialBody target, double launchDay, double arrivalDay,
        double semiMajorAu, double eccentricity, double periodDays,
        double sweepDegrees, Vector3 periDir, Vector3 sideDir,
        CelestialBody? center, double mu)
    {
        Name = name;
        Target = target;
        Center = center;
        _mu = mu;
        LaunchDay = launchDay;
        ArrivalDay = arrivalDay;
        SemiMajorAu = semiMajorAu;
        Eccentricity = eccentricity;
        SweepDegrees = sweepDegrees;
        _periodDays = periodDays;
        _periDir = periDir;
        _sideDir = sideDir;
    }

    /// <summary>Omloppstiden för en ellips med halva storaxeln a, ur Keplers tredje lag.</summary>
    static double PeriodOf(double semiMajorAu, double mu)
        => 2.0 * Math.PI * Math.Sqrt(semiMajorAu * semiMajorAu * semiMajorAu / mu);

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
            solution.SweepDegrees, r1Dir, Vector3.Normalize(sideDir),
            center: null, SolarSystemData.SunMu);
    }

    // ---------------------------------------------------------------- månfärd

    /// <summary>Höjden över jordytan där färden börjar: en låg parkeringsbana.</summary>
    public const double ParkingOrbitAltitudeKm = 400.0;

    /// <summary>
    /// Restiden till månen i dygn. Apollo 11 var framme efter 76 timmar, alltså
    /// drygt tre dygn.
    /// </summary>
    public const double MoonTravelDays = 3.0;

    /// <summary>
    /// Planerar en färd till en av planetens egna månar. Här kretsar farkosten
    /// kring planeten i stället för kring solen, och skillnaden mot Mars-färden
    /// är större än den ser ut.
    ///
    /// Uppskjutningen sker från en låg omloppsbana, och därifrån kan farkosten
    /// lämna åt vilket håll som helst. Startpunkten är alltså inte given på
    /// förhand, som jordens läge är vid en Mars-färd, utan väljs så att banan
    /// möter månen. Det är därför en månfärd kan påbörjas nästan vilken dag som
    /// helst medan Mars kräver ett startfönster vartannat år.
    ///
    /// Restiden bestäms i stället i förväg, och banan söks fram till den. En ren
    /// Hohmann-bana ut till månen tar nästan fem dygn; för att hinna på tre måste
    /// farkosten skjutas upp med mer fart, så att banans bortre ände hamnar en
    /// bra bit bortom månen – 440 000 till 630 000 km beroende på var månen står
    /// – och månen alltså hinns ikapp på vägen ut, före vändpunkten. Det var
    /// precis så Apollo flög.
    /// </summary>
    public static Mission? PlanToMoon(string name, CelestialBody planet, CelestialBody moon,
        double launchDay, double travelDays = MoonTravelDays)
    {
        double mu = planet.Mu;
        if (mu <= 0 || travelDays <= 0)
            return null;

        // Parkeringsbanan blir banans perigeum, punkten närmast planeten.
        double rp = (planet.RadiusKm + ParkingOrbitAltitudeKm) / SolarSystemData.AuKm;

        double arrivalDay = launchDay + travelDays;
        var r2 = moon.PositionAt(arrivalDay, 1f);   // planetcentriskt läge vid ankomsten
        double r2Len = r2.Length();
        if (r2Len <= rp)
            return null;
        var r2Dir = r2 / (float)r2Len;

        // Sök halva storaxeln så att restiden blir den önskade. Restiden avtar
        // när banan görs större – mer fart vid uppskjutningen – så en
        // intervallhalvering hittar rätt utan omvägar. Undre gränsen är den
        // energisnålaste ellipsen som överhuvudtaget når ut till målet, alltså
        // den långsammaste; åtta gånger den är gott och väl snabbare än tre dygn.
        double lo = 0.5 * (rp + r2Len);
        double hi = lo * 8.0;
        if (TimeToRadius(lo, rp, r2Len, mu) < travelDays ||
            TimeToRadius(hi, rp, r2Len, mu) > travelDays)
            return null;    // den önskade restiden går inte att uppfylla

        for (int k = 0; k < 80; k++)
        {
            double mid = 0.5 * (lo + hi);
            if (TimeToRadius(mid, rp, r2Len, mu) > travelDays)
                lo = mid;   // för långsam – banan behöver göras större
            else
                hi = mid;
        }

        double a = 0.5 * (lo + hi);
        double e = 1.0 - rp / a;

        // Sveptvinkeln: hur långt runt planeten farkosten hinner på vägen ut.
        double cosSweep = Math.Clamp((a * (1.0 - e * e) / r2Len - 1.0) / e, -1.0, 1.0);
        double sweep = Math.Acos(cosSweep);

        // Banplanet läggs i månens eget banplan, precis som Apollo-färderna gjorde.
        // Normalen fås ur månens läge ett halvt dygn före och efter ankomsten och
        // pekar då åt samma håll som månens eget rörelsemängdsmoment, så att
        // farkosten går åt samma håll som månen.
        var normalRaw = Vector3.Cross(
            moon.PositionAt(arrivalDay - 0.5, 1f),
            moon.PositionAt(arrivalDay + 0.5, 1f));
        if (normalRaw.Length() < 1e-12f)
            return null;
        var normal = Vector3.Normalize(normalRaw);

        // Uppskjutningspunkten fås genom att vrida ankomstriktningen tillbaka
        // sveptvinkeln, alltså baklänges längs banan.
        var forward = Vector3.Cross(normal, r2Dir);
        var periDir = Vector3.Normalize(
            r2Dir * (float)Math.Cos(sweep) - forward * (float)Math.Sin(sweep));
        var sideDir = Vector3.Normalize(Vector3.Cross(normal, periDir));

        return new Mission(name, moon, launchDay, arrivalDay, a, e,
            PeriodOf(a, mu), sweep * 180.0 / Math.PI, periDir, sideDir, planet, mu);
    }

    /// <summary>
    /// Restiden från perigeum ut till en given radie, för ellipsen med halva
    /// storaxeln a och perigeum rp. Keplers ekvation baklänges mot hur den annars
    /// används: först excentrisk anomali ur radien, sedan tiden ur den.
    /// </summary>
    static double TimeToRadius(double a, double rp, double r, double mu)
    {
        double e = 1.0 - rp / a;
        double ecc = Math.Acos(Math.Clamp((1.0 - r / a) / e, -1.0, 1.0));
        double meanAnomaly = ecc - e * Math.Sin(ecc);
        return meanAnomaly / Math.Sqrt(mu / (a * a * a));
    }

    readonly record struct Solution(
        double SemiMajor, double Eccentricity, double Period,
        Vector3 TargetDir, double SweepDegrees);

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
        double period = PeriodOf(a, SolarSystemData.SunMu);

        // Restid ur Keplers ekvation: från perihelium fram till sveptvinkeln.
        double halfSweep = sweep * 0.5;
        double ecc = 2.0 * Math.Atan2(
            Math.Sqrt(1.0 - e) * Math.Sin(halfSweep),
            Math.Sqrt(1.0 + e) * Math.Cos(halfSweep));
        double computed = (ecc - e * Math.Sin(ecc)) / (Math.PI * 2) * period;

        residual = computed - travel;
        solution = new Solution(a, e, period, r2Dir, sweep * 180.0 / Math.PI);
        return true;
    }

    /// <summary>
    /// Farkostens läge vid given tid, räknat från centralkroppen – solen, eller
    /// planeten när färden går till en måne. Efter ankomsten
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
        double e = Eccentricity;
        double meanAnomaly = (day - LaunchDay) / _periodDays * Math.PI * 2;
        double ecc = SolveKepler(meanAnomaly, e);

        double x = SemiMajorAu * (Math.Cos(ecc) - e);
        double y = SemiMajorAu * Math.Sqrt(1.0 - e * e) * Math.Sin(ecc);

        return (_periDir * (float)x + _sideDir * (float)y) * unitsPerAu;
    }

    /// <summary>
    /// Löser Keplers ekvation E - e·sin E = M. Överföringsbanor är långt mer
    /// utdragna än planetbanorna – månfärdens excentricitet ligger kring 0,97
    /// mot jordens 0,017 – och där kan Newtons metod skena i väg, eftersom
    /// nämnaren 1 - e·cos E går mot noll nära perigeum. Intervallhalvering tar
    /// fler varv men kan inte missa: lösningen ligger alltid mellan M och M + e.
    /// </summary>
    static double SolveKepler(double meanAnomaly, double e)
    {
        double m = meanAnomaly % (Math.PI * 2);
        if (m < 0)
            m += Math.PI * 2;

        // Ekvationen är spegelsymmetrisk kring M = π, så andra halvan av varvet
        // löses som den första och speglas tillbaka.
        bool mirrored = m > Math.PI;
        if (mirrored)
            m = Math.PI * 2 - m;

        double lo = m, hi = m + e;
        for (int k = 0; k < 40; k++)
        {
            double mid = 0.5 * (lo + hi);
            if (mid - e * Math.Sin(mid) < m)
                lo = mid;
            else
                hi = mid;
        }

        double ecc = 0.5 * (lo + hi);
        return mirrored ? Math.PI * 2 - ecc : ecc;
    }

    // ------------------------------------------------------------ startfönster

    /// <summary>
    /// Hur nära en halv ellips banan måste ligga för att räknas som ett
    /// startfönster. Sveper farkosten mycket mindre än 180 grader behöver den
    /// en snabbare och mycket bränsletörstigare bana.
    ///
    /// Värdet är valt så att fönstren blir ungefär tre veckor långa, vilket är
    /// vad verkliga Mars-uppdrag har att röra sig med. En lösare gräns på 150
    /// grader gav fönster på över hundra dygn, alltså mest banor som i praktiken
    /// vore alldeles för bränslekrävande.
    /// </summary>
    public const double WindowSweepDegrees = 170.0;

    /// <summary>Sant om en energisnål färd kan påbörjas just den dagen.</summary>
    public static bool IsLaunchWindow(CelestialBody origin, CelestialBody target, double day)
        => Plan("", origin, target, day) is { } m && m.SweepDegrees >= WindowSweepDegrees;

    /// <summary>
    /// Letar upp nästa dag då en färd kan påbörjas. Söker först grovt i
    /// femdygnssteg och finjusterar sedan dag för dag, eftersom varje prövning
    /// kräver att en hel bana räknas fram. Returnerar null om inget fönster
    /// hittas inom sökhorisonten.
    /// </summary>
    public static double? NextLaunchWindow(CelestialBody origin, CelestialBody target,
        double fromDay, double horizonDays = 900.0)
    {
        for (double coarse = 0; coarse <= horizonDays; coarse += 5.0)
        {
            if (!IsLaunchWindow(origin, target, fromDay + coarse))
                continue;

            // Backa dag för dag till fönstrets första dag.
            double day = fromDay + coarse;
            for (int back = 0; back < 5; back++)
            {
                if (day - 1.0 < fromDay || !IsLaunchWindow(origin, target, day - 1.0))
                    break;
                day -= 1.0;
            }
            return day;
        }
        return null;
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

        double v = Math.Sqrt(Math.Max(0.0, _mu * (2.0 / r - 1.0 / SemiMajorAu)));
        return v * SolarSystemData.AuKm / 86_400.0;
    }
}
