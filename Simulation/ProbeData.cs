namespace Solarsystem.Simulation;

/// <summary>
/// De verkliga rymdsonderna, med sina verkliga datum.
///
/// Datumen är passagerna av respektive planet, och lägena i dag kommer från
/// NASA:s uppgifter om var sonderna befinner sig: avstånd samt riktning på
/// himlen i rektascension och deklination. Avstånd och riktning är avrundade,
/// men det är de enda uppgifter appen behöver – banorna räknas fram ur dem.
/// </summary>
public static class ProbeData
{
    static CelestialBody Planet(string name)
        => SolarSystemData.Planets.First(p => p.Name == name);

    /// <summary>Dagens datum i modellen, som sondernas kända lägen anges för.</summary>
    static readonly DateTime Today = new(2026, 1, 1);

    /// <summary>
    /// Voyager 1: uppskjuten 5 september 1977, förbi Jupiter 5 mars 1979 och
    /// Saturnus 12 november 1980.
    ///
    /// Vid Saturnus valde man att svänga sonden brant uppåt, ut ur ekliptikan,
    /// för att komma nära månen Titan. Priset var att den aldrig kunde nå någon
    /// mer planet; vinsten blev den första närbilden av en måne med atmosfär.
    /// I dag är Voyager 1 det avlägsnaste föremål människan har byggt, drygt
    /// 167 AU bort i riktning mot Ormbäraren.
    /// </summary>
    public static readonly Probe Voyager1 = Probe.Build(
        "Voyager 1", Color.FromArgb("#F2D9A0"),
        Waypoint.At(Planet("Jorden"), new DateTime(1977, 9, 5)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1979, 3, 5)),
        Waypoint.At(Planet("Saturnus"), new DateTime(1980, 11, 12)),
        Waypoint.InSky("Voyager 1 i dag", Today, 167.0, 17.25, 12.3))
        .Crossing("Heliopausen", new DateTime(2012, 8, 25));

    /// <summary>
    /// Voyager 2: uppskjuten 20 augusti 1977 – två veckor före Voyager 1, trots
    /// namnet – och förbi Jupiter 9 juli 1979, Saturnus 25 augusti 1981, Uranus
    /// 24 januari 1986 och Neptunus 25 augusti 1989.
    ///
    /// Den är den enda farkost som besökt alla fyra jätteplaneterna. Det var
    /// möjligt tack vare en uppställning där planeterna stod på rad, vilket bara
    /// inträffar vart 176:e år. Vid Neptunus svängde den i stället brant nedåt,
    /// för att passera månen Triton, och lämnade därför ekliptikan åt motsatt
    /// håll mot sin tvilling.
    /// </summary>
    public static readonly Probe Voyager2 = Probe.Build(
        "Voyager 2", Color.FromArgb("#A8DCEC"),
        Waypoint.At(Planet("Jorden"), new DateTime(1977, 8, 20)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1979, 7, 9)),
        Waypoint.At(Planet("Saturnus"), new DateTime(1981, 8, 25)),
        Waypoint.At(Planet("Uranus"), new DateTime(1986, 1, 24)),
        Waypoint.At(Planet("Neptunus"), new DateTime(1989, 8, 25)),
        Waypoint.InSky("Voyager 2 i dag", Today, 140.0, 20.12, -59.5))
        .Crossing("Heliopausen", new DateTime(2018, 11, 5));

    /// <summary>
    /// Pioneer 10: uppskjuten 3 mars 1972, förbi Jupiter 4 december 1973. Den
    /// var först med allt – först genom asteroidbältet, först vid Jupiter och
    /// först ut ur planetsystemet. Radiokontakten tystnade 2003, så läget i dag
    /// är framräknat och inte mätt.
    ///
    /// Sonden lämnar solsystemet nästan längs ekliptikan, i riktning mot
    /// Aldebaran i Oxen. Dit tar det över två miljoner år.
    /// </summary>
    public static readonly Probe Pioneer10 = Probe.Build(
        "Pioneer 10", Color.FromArgb("#E4A98F"),
        Waypoint.At(Planet("Jorden"), new DateTime(1972, 3, 3)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1973, 12, 4)),
        Waypoint.InSky("Pioneer 10 i dag", Today, 140.0, 4.60, 16.5));

    /// <summary>
    /// Pioneer 11: uppskjuten 6 april 1973, förbi Jupiter 3 december 1974 och
    /// Saturnus 1 september 1979 – den första sond som besökte Saturnus.
    ///
    /// Vägen mellan de två var udda: Jupiter slungade sonden uppåt och tvärs
    /// över solsystemet, så att den mötte Saturnus på andra sidan solen. Det
    /// benet sveper därför mer än ett halvt varv, vilket Lambert-lösaren klarar
    /// eftersom den väljer den långa vägen när den korta skulle gå baklänges.
    /// </summary>
    public static readonly Probe Pioneer11 = Probe.Build(
        "Pioneer 11", Color.FromArgb("#C3CE9E"),
        Waypoint.At(Planet("Jorden"), new DateTime(1973, 4, 6)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1974, 12, 3)),
        Waypoint.At(Planet("Saturnus"), new DateTime(1979, 9, 1)),
        Waypoint.InSky("Pioneer 11 i dag", Today, 118.0, 18.50, -8.9));

    /// <summary>
    /// New Horizons: uppskjuten 19 januari 2006, förbi Jupiter 28 februari 2007
    /// och Pluto 14 juli 2015. Den snabbaste uppskjutningen någonsin – den
    /// passerade månens bana efter nio timmar, mot Apollos tre dygn.
    ///
    /// Pluto-passagen är ett bra prov på att banorna räknas i tre dimensioner:
    /// Plutos bana lutar 17 grader, så mötet skedde långt utanför ekliptikans
    /// plan.
    /// </summary>
    public static readonly Probe NewHorizons = Probe.Build(
        "New Horizons", Color.FromArgb("#D9AEE6"),
        Waypoint.At(Planet("Jorden"), new DateTime(2006, 1, 19)),
        Waypoint.At(Planet("Jupiter"), new DateTime(2007, 2, 28)),
        Waypoint.At(Planet("Pluto"), new DateTime(2015, 7, 14)),
        Waypoint.InSky("New Horizons i dag", Today, 63.0, 19.25, -20.5));

    /// <summary>
    /// Alla sonder appen ritar. Fem farkoster är på väg ut ur solsystemet, och
    /// det här är allihop.
    /// </summary>
    public static readonly Probe[] All =
        [Voyager1, Voyager2, Pioneer10, Pioneer11, NewHorizons];

    // ------------------------------------------------------- kretsande sonder

    /// <summary>
    /// Cassini vid Saturnus: i omloppsbana 1 juli 2004, och avslutad 15 september
    /// 2017 genom att styras rakt ned i Saturnus atmosfär – för att inte riskera
    /// att en obemannad sond med jordbakterier en dag kraschar på Enceladus, där
    /// det finns ett hav under isen.
    ///
    /// Varvet som visas är ett representativt av de nästan trehundra Cassini
    /// gjorde: periapsis knappt tre Saturnusradier ut, apoapsis knappt fyrtio,
    /// vilket ger sexton dygns omloppstid. Banan lutar tjugo grader mot
    /// ringplanet – Cassini växlade mellan att ligga i ringplanet och att luta
    /// upp till 75 grader för att se ringarna uppifrån.
    /// </summary>
    public static readonly Orbiter Cassini = Orbiter.Build(
        "Cassini", Color.FromArgb("#F2C86A"), Planet("Saturnus"),
        periapsisRadii: 2.70, apoapsisRadii: 39.36,
        inclinationToEquatorDeg: 20.0,
        argPeriapsisDeg: 60.0,
        arrival: new DateTime(2004, 7, 1),
        end: new DateTime(2017, 9, 15),
        ending: "Styrdes ned i Saturnus atmosfär")!;

    /// <summary>
    /// Juno vid Jupiter: i omloppsbana 5 juli 2016. Banan är extrem – den dyker
    /// ned till drygt en Jupiterradie, alltså bara några tusen kilometer över
    /// molntopparna, och ut igen till 116 radier, vilket är åtta miljoner
    /// kilometer. Ett varv tar 53 dygn.
    ///
    /// Banan går över polerna, till skillnad från månarnas som ligger i
    /// ekvatorsplanet. Det är själva poängen med Juno: den ska mäta Jupiters
    /// magnetfält och inre, och den dyker dessutom mellan planeten och de
    /// farligaste strålningsbältena, som ligger kring ekvatorn.
    ///
    /// Slutdatumet är den senaste bekräftade kontakten, inte ett uppdragsslut.
    /// Juno flög vidare långt förbi det förlängda uppdragets planerade slut den
    /// 30 september 2025 och skickade data hela våren 2026; den 1 maj 2026 tog
    /// den närbilder av den lilla månen Thebe med sin stjärnkamera. Därefter
    /// finns inget bekräftat, och risken är budgetär snarare än teknisk – sonden
    /// fungerar, men fanns med bland de uppdrag som föreslogs strykas.
    ///
    /// Appen ritar därför Juno fram till den kända kontakten och inte längre.
    /// Hellre missa en sond som flyger än visa en som inte finns.
    ///
    /// Till skillnad från Cassini kommer Juno inte att styras ned i planeten.
    /// Det var planen från början, av samma skäl: en sond som slår ned på Europa
    /// skulle kunna föra med sig jordbakterier till havet under isen. Men under
    /// åren i omloppsbana böjde månarnas dragningskraft banan så mycket att Juno
    /// till slut inte passerade i närheten av Europa alls, och då fanns inget
    /// kvar att skydda mot.
    /// </summary>
    public static readonly Orbiter Juno = Orbiter.Build(
        "Juno", Color.FromArgb("#9FD8F2"), Planet("Jupiter"),
        periapsisRadii: 1.08, apoapsisRadii: 115.90,
        inclinationToEquatorDeg: 90.0,
        argPeriapsisDeg: 0.0,
        arrival: new DateTime(2016, 7, 5),
        end: new DateTime(2026, 5, 1),
        ending: "Senast bekräftade kontakten")!;

    /// <summary>De sonder som kretsar kring en planet i stället för att lämna.</summary>
    public static readonly Orbiter[] Orbiters = [Cassini, Juno];
}
