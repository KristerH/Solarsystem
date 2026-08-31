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
        Waypoint.InSky("Voyager 1 i dag", Today, 167.0, 17.25, 12.3));

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
        Waypoint.InSky("Voyager 2 i dag", Today, 140.0, 20.12, -59.5));

    /// <summary>Alla sonder appen ritar.</summary>
    public static readonly Probe[] All = [Voyager1, Voyager2];
}
