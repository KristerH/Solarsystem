using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En rymdfärd från en kropp till en annan, längs en överföringsbana som
/// farkosten följer utan att styra – precis som en verklig sond gör mellan
/// raketmotorns två korta brinntider.
///
/// Banan löses ur sina randvillkor med Lambert-lösaren: den ska starta vid
/// jordens läge på uppskjutningsdagen och nå målets läge på ankomstdagen. Den
/// enda kvarvarande frågan är alltså hur lång restiden ska vara, och den väljs
/// så att uppskjutningen blir så billig som möjligt – mätt som den fart
/// farkosten måste ha i förhållande till jorden när den lämnar. Det är samma
/// avvägning som verkliga uppdrag gör, och den ger omkring 3 km/s för Mars i de
/// bästa fönstren, vilket också är vad verkligheten kostar.
///
/// Banan är i grunden en Hohmann-överföring – en halv ellips från jordens bana
/// ut till målets, den energisnålaste vägen. Men exakt en halv ellips duger
/// inte: den kan bara nå punkter rakt mitt emot starten, och just där blir
/// banplanet obestämt och priset skenar. Mars ligger dessutom 1,85 grader ur
/// ekliptikan. Den billigaste lösningen landar därför en bit förbi ett halvt
/// varv, kring 200 graders svep.
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
    public double SemiMajorAu => _transfer.SemiMajorAu;

    /// <summary>Banans excentricitet.</summary>
    public double Eccentricity => _transfer.Eccentricity;

    /// <summary>
    /// Hur många grader farkosten sveper kring centralkroppen på vägen, räknat
    /// åt det håll planeterna går. En ren Hohmann-överföring sveper exakt 180
    /// grader; de billigaste verkliga banorna hamnar strax över, eftersom målet
    /// ska hinna fram till mötet.
    /// </summary>
    public double SweepDegrees { get; }

    /// <summary>
    /// Farten farkosten måste ha i förhållande till startkroppen i det ögonblick
    /// den lämnar den. Det är måttet på hur stor raket färden kräver, och det som
    /// avgör om ett startfönster är öppet: verkliga Mars-uppdrag ligger kring
    /// 3 km/s.
    /// </summary>
    public double DepartureSpeedKmS { get; }

    readonly Conic _transfer;

    Mission(string name, CelestialBody target, CelestialBody? center, Conic transfer,
        double launchDay, double arrivalDay, double sweepDegrees, double departureSpeedKmS)
    {
        Name = name;
        Target = target;
        Center = center;
        _transfer = transfer;
        LaunchDay = launchDay;
        ArrivalDay = arrivalDay;
        SweepDegrees = sweepDegrees;
        DepartureSpeedKmS = departureSpeedKmS;
    }

    // --------------------------------------------------- färd mellan planeter

    /// <summary>Kortaste restid som prövas när banan söks, i dygn.</summary>
    const double MinTravelDays = 120.0;

    /// <summary>Längsta restid som prövas när banan söks, i dygn.</summary>
    const double MaxTravelDays = 480.0;

    /// <summary>Antal stickprov över restiden i den grova sökningen.</summary>
    const int TravelSamples = 12;

    /// <summary>
    /// Planerar en färd från en kropp till en annan med uppskjutning en given
    /// dag, längs den billigaste bana som finns den dagen. Returnerar null när
    /// ingen bana går att räkna fram.
    /// </summary>
    public static Mission? Plan(string name, CelestialBody origin, CelestialBody target,
        double launchDay)
    {
        var (departureSpeed, travelDays) = CheapestDeparture(origin, target, launchDay);
        if (double.IsInfinity(departureSpeed))
            return null;

        // Allt räknas i AU; skalan till världsenheter läggs på först vid ritning.
        var r1 = origin.PositionAt(launchDay, 1f);
        var r2 = target.PositionAt(launchDay + travelDays, 1f);
        if (!Lambert.Solve(r1, r2, travelDays, SolarSystemData.SunMu, out var v1, out _))
            return null;

        var transfer = Conic.FromState(r1, v1, launchDay, SolarSystemData.SunMu);
        if (transfer is null)
            return null;

        return new Mission(name, target, center: null, transfer,
            launchDay, launchDay + travelDays, SweepBetween(r1, r2), departureSpeed);
    }

    /// <summary>
    /// Vinkeln mellan två lägen, räknad åt det håll planeterna går. Ligger målet
    /// mindre än ett halvt varv bort åt andra hållet, så har färden i själva
    /// verket svept mer än ett halvt varv.
    /// </summary>
    static double SweepBetween(Vector3 from, Vector3 to)
    {
        double cos = Math.Clamp(
            Vector3.Dot(from, to) / (from.Length() * to.Length()), -1.0, 1.0);
        double sweep = Math.Acos(cos) * 180.0 / Math.PI;
        return Vector3.Cross(from, to).Y < 0 ? 360.0 - sweep : sweep;
    }

    /// <summary>
    /// Den billigaste uppskjutningen en given dag: farten som krävs i
    /// förhållande till startkroppen, och restiden som ger den. Oändlig fart
    /// betyder att ingen bana alls hittades.
    ///
    /// Kostnaden växlar kraftigt med restiden. Korta färder kräver mycket fart,
    /// och mitt i intervallet ligger dessutom en topp: där hamnar målet rakt mitt
    /// emot starten sett från solen, och då är banplanet obestämt och priset
    /// skenar. Sökningen tar därför stickprov över hela intervallet innan den
    /// förfinar kring det bästa av dem.
    /// </summary>
    public static (double SpeedKmS, double TravelDays) CheapestDeparture(
        CelestialBody origin, CelestialBody target, double day, bool refine = true)
    {
        double bestSpeed = double.PositiveInfinity, bestTravel = 0;

        for (int i = 0; i <= TravelSamples; i++)
        {
            double travel = MinTravelDays + (MaxTravelDays - MinTravelDays) * i / TravelSamples;
            double speed = DepartureSpeed(origin, target, day, travel);
            if (speed < bestSpeed)
            {
                bestSpeed = speed;
                bestTravel = travel;
            }
        }

        if (!refine || double.IsInfinity(bestSpeed))
            return (bestSpeed, bestTravel);

        // Halvera steglängden och pröva åt båda hållen, tills restiden är
        // bestämd på ungefär ett dygn när.
        for (double step = (MaxTravelDays - MinTravelDays) / TravelSamples * 0.5;
             step > 0.5; step *= 0.5)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                double travel = bestTravel + side * step;
                if (travel < MinTravelDays || travel > MaxTravelDays)
                    continue;

                double speed = DepartureSpeed(origin, target, day, travel);
                if (speed < bestSpeed)
                {
                    bestSpeed = speed;
                    bestTravel = travel;
                }
            }
        }

        return (bestSpeed, bestTravel);
    }

    /// <summary>
    /// Vad en given restid kostar: farten farkosten måste ha i förhållande till
    /// startkroppen när den lämnar den.
    /// </summary>
    static double DepartureSpeed(CelestialBody origin, CelestialBody target,
        double day, double travelDays)
    {
        var r1 = origin.PositionAt(day, 1f);
        var r2 = target.PositionAt(day + travelDays, 1f);
        if (!Lambert.Solve(r1, r2, travelDays, SolarSystemData.SunMu, out var v1, out _))
            return double.PositiveInfinity;

        return (v1 - VelocityOf(origin, day)).Length() * SolarSystemData.AuKm / 86_400.0;
    }

    /// <summary>Kroppens egen hastighet i AU/dygn, ur lägena ett halvt dygn isär.</summary>
    static Vector3 VelocityOf(CelestialBody body, double day)
        => body.PositionAt(day + 0.5, 1f) - body.PositionAt(day - 0.5, 1f);

    // ------------------------------------------------------------------ lägen

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
        => _transfer.PositionAt(day, unitsPerAu);

    /// <summary>Sant när farkosten har nått fram.</summary>
    public bool HasArrived(double day) => day >= ArrivalDay;

    /// <summary>
    /// Hur långt farkosten har kvar till målet, i kilometer. Både farkost och mål
    /// mäts i banans eget system, så samma räkning gäller för en färd kring solen
    /// som för en kring en planet.
    /// </summary>
    public double DistanceToTargetKm(double day)
    {
        double d = Math.Clamp(day, LaunchDay, ArrivalDay);
        return (Target.PositionAt(d, 1f) - PositionAt(d, 1f)).Length() * SolarSystemData.AuKm;
    }

    /// <summary>
    /// Farkostens fart i km/s vid given tid, ur vis-viva-ekvationen. Farten är
    /// högst vid uppskjutningen och lägst vid ankomsten, precis som Keplers
    /// andra lag säger.
    /// </summary>
    public double SpeedKmPerSecond(double day)
        => _transfer.SpeedKmPerSecond(Math.Min(day, ArrivalDay));

    // ------------------------------------------------------------ startfönster

    /// <summary>
    /// Hur mycket dyrare än fönstrets allra bästa dag en uppskjutning får vara
    /// och ändå räknas som ett startfönster.
    ///
    /// Måttet är relativt i stället för en fast gräns i km/s, eftersom fönstren
    /// är olika bra: Mars excentriska bana gör att det billigaste tillfället
    /// växlar mellan 2,9 och 3,1 km/s beroende på var planeten står. En fast
    /// gräns hade gjort somliga fönster obefintliga och andra halvårslånga.
    /// </summary>
    public const double WindowMarginKmS = 0.1;

    /// <summary>Hur långt åt vardera hållet fönstrets bästa dag söks.</summary>
    const double WindowSearchDays = 180.0;

    /// <summary>
    /// Avståndet mellan stickproven när kostnadskurvan kartläggs.
    ///
    /// Rutnätet är fast, räknat från epoken, och inte lagt kring den dag frågan
    /// gäller. Det är avgörande: både "är fönstret öppet i dag?" och "när öppnar
    /// nästa?" mäter mot samma stickprov och kan därför aldrig ge motstridiga
    /// svar. Med varsitt rutnät hände det att knappen "Nästa startfönster"
    /// hoppade till en dag som knappen "Skjut upp" sedan vägrade släppa fram.
    /// </summary>
    const double CostStepDays = 5.0;

    /// <summary>
    /// Kostnaden i rutnätets punkter, undansparad. Fönsterkollen tittar ett
    /// halvår åt vardera hållet flera gånger i sekunden, och stickproven är då
    /// till nio tiondelar desamma som förra gången – medan varje enskilt
    /// stickprov kräver en egen sökning efter bästa restid. Planetbanorna ändrar
    /// sig aldrig, så ett sparat värde kan aldrig bli inaktuellt.
    /// </summary>
    static readonly Dictionary<(string Origin, string Target, long Index), double> GridCost = new();

    /// <summary>Kostnaden i en av rutnätets punkter.</summary>
    static double CostAtGridPoint(CelestialBody origin, CelestialBody target, long index)
    {
        var key = (origin.Name, target.Name, index);
        if (GridCost.TryGetValue(key, out double cached))
            return cached;

        double cost = CheapestDeparture(origin, target, index * CostStepDays, refine: false).SpeedKmS;

        // Rutnätet är glest – 8192 punkter räcker till mer än hundra år – men
        // den som spolar tiden riktigt långt ska inte samla på sig minne i
        // oändlighet.
        if (GridCost.Count > 8192)
            GridCost.Clear();

        GridCost[key] = cost;
        return cost;
    }

    /// <summary>Sant om en energisnål färd kan påbörjas just den dagen.</summary>
    public static bool IsLaunchWindow(CelestialBody origin, CelestialBody target, double day)
    {
        double here = CheapestDeparture(origin, target, day, refine: false).SpeedKmS;
        if (double.IsInfinity(here))
            return false;

        return here <= BestNearby(origin, target, day) + WindowMarginKmS;
    }

    /// <summary>
    /// Den billigaste uppskjutningen i närheten av en dag. Sökvidden är ett
    /// halvår åt vardera hållet: tillräckligt för att hitta botten i det fönster
    /// man befinner sig i, men klart kortare än de 780 dygn som skiljer fönstren
    /// åt, så att nästa fönsters botten inte råkar tas med.
    /// </summary>
    static double BestNearby(CelestialBody origin, CelestialBody target, double day)
    {
        long centre = (long)Math.Round(day / CostStepDays);
        int span = (int)(WindowSearchDays / CostStepDays);

        double best = double.PositiveInfinity;
        for (long k = centre - span; k <= centre + span; k++)
            best = Math.Min(best, CostAtGridPoint(origin, target, k));
        return best;
    }

    /// <summary>
    /// Letar upp nästa dag då en färd kan påbörjas. Kostnadskurvan kartläggs en
    /// gång över hela sökhorisonten – med ett halvårs marginal åt vardera hållet,
    /// eftersom varje punkt jämförs med det billigaste i sin omgivning – och
    /// första dagen som ryms inom marginalen är svaret. Returnerar null om inget
    /// fönster hittas inom horisonten, men eftersom varje fönster per definition
    /// innehåller sin egen bottenpunkt inträffar det bara för orimligt korta
    /// horisonter.
    /// </summary>
    public static double? NextLaunchWindow(CelestialBody origin, CelestialBody target,
        double fromDay, double horizonDays = 900.0)
    {
        int span = (int)(WindowSearchDays / CostStepDays);
        long first = (long)Math.Round(fromDay / CostStepDays) - span;
        long last = (long)Math.Round((fromDay + horizonDays) / CostStepDays) + span;

        // Kartlägg kostnaden en gång; varje stickprov kräver en egen sökning
        // efter bästa restid, så de ska inte räknas om i onödan.
        var cost = new double[last - first + 1];
        for (int i = 0; i < cost.Length; i++)
            cost[i] = CostAtGridPoint(origin, target, first + i);

        for (long k = (long)Math.Round(fromDay / CostStepDays); k <= last - span; k++)
        {
            int i = (int)(k - first);

            double best = double.PositiveInfinity;
            for (int j = i - span; j <= i + span; j++)
                best = Math.Min(best, cost[j]);

            if (cost[i] > best + WindowMarginKmS)
                continue;

            // Stega bakåt till fönstrets första dag. Marginalen mäts mot samma
            // omgivning hela vägen, så länge dagarna hör till samma rutpunkt.
            double day = k * CostStepDays;
            while (day - 1.0 >= fromDay &&
                   (long)Math.Round((day - 1.0) / CostStepDays) == k &&
                   CheapestDeparture(origin, target, day - 1.0, refine: false).SpeedKmS
                       <= best + WindowMarginKmS)
                day -= 1.0;

            return Math.Max(day, fromDay);
        }

        return null;
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

        // Uppskjutningen sker i perigeum, så banan byggs därifrån direkt – den
        // vägen in tappar ingen precision.
        var transfer = Conic.FromPeriapsis(periDir, sideDir, rp, e, launchDay, mu);
        if (transfer is null)
            return null;

        return new Mission(name, moon, planet, transfer, launchDay, arrivalDay,
            sweep * 180.0 / Math.PI, transfer.SpeedKmPerSecond(launchDay));
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
}
