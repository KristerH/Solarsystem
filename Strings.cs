using System.Globalization;
using System.Resources;

namespace Solarsystem;

/// <summary>
/// Appens texter, hämtade ur resursfilerna i <c>Resources/Strings</c>.
///
/// Engelska ligger i <c>AppStrings.resx</c> och är grundspråket – det som
/// används när inget annat passar. Svenska ligger i <c>AppStrings.sv.resx</c>.
/// Ett nytt språk är en fil till, <c>AppStrings.xx.resx</c>, och ingen kodändring:
/// byggsystemet gör en satellitassembly av den av sig självt.
///
/// Klassen är skriven för hand i stället för genererad av Visual Studios
/// resursverktyg. Skälet är att den genereraren bara kör inne i IDE:t – bygger
/// man med <c>dotnet build</c> från kommandoraden blir den genererade filen
/// aldrig till. Innehållet är detsamma som verktyget hade producerat, med den
/// skillnaden att uppslagningen går via <see cref="Culture"/> så att språket kan
/// bytas medan appen kör.
/// </summary>
public static class Strings
{
    static readonly ResourceManager Manager =
        new("Solarsystem.Resources.Strings.AppStrings", typeof(Strings).Assembly);

    /// <summary>Språket som gäller just nu. Byts med <see cref="Use"/>.</summary>
    public static CultureInfo Culture { get; private set; } = CultureInfo.CurrentUICulture;

    /// <summary>
    /// Byter språk. Både texterna och talformaten följer med: datum, decimaltecken
    /// och tusentalsavgränsare hämtas ur samma kultur, vilket är hela poängen med
    /// att sätta <see cref="CultureInfo.CurrentCulture"/> och inte bara texterna.
    /// </summary>
    public static void Use(CultureInfo culture)
    {
        Culture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    /// <summary>
    /// Texten för en nyckel. Saknas den ges nyckeln själv tillbaka – en app som
    /// visar "ui.pause" i en knapp är trasig på ett sätt som syns, vilket är
    /// bättre än en tom knapp.
    /// </summary>
    public static string Get(string key) => Manager.GetString(key, Culture) ?? key;

    /// <summary>
    /// Namnet på en himlakropp, stjärnbild, stjärna eller milstolpe.
    ///
    /// Saknas nyckeln ges den tillbaka som den är, och det är med flit: de
    /// flesta stjärnnamn är internationella (Betelgeuse, Sirius, Vega) och
    /// behöver ingen översättning. Bara de som verkligen skiljer sig mellan
    /// språken – Plejaderna, Polstjärnan – står i resursfilen.
    /// </summary>
    public static string Name(string key) => Manager.GetString("name." + key, Culture) ?? key;

    /// <summary>Texten för en nyckel, ifylld med värden.</summary>
    public static string Format(string key, params object?[] args)
        => string.Format(Culture, Get(key), args);

    /// <summary>⏸ Pause</summary>
    public static string Pause => Get("ui.pause");
    /// <summary>▶ Start</summary>
    public static string Start => Get("ui.start");
    /// <summary>Speed:</summary>
    public static string Speed => Get("ui.speed");
    /// <summary>Show orbits</summary>
    public static string ShowOrbits => Get("ui.showOrbits");
    /// <summary>Show moons</summary>
    public static string ShowMoons => Get("ui.showMoons");
    /// <summary>Real size</summary>
    public static string RealScale => Get("ui.realScale");
    /// <summary>Asteroid belt</summary>
    public static string AsteroidBelt => Get("ui.asteroidBelt");
    /// <summary>Kuiper belt</summary>
    public static string KuiperBelt => Get("ui.kuiperBelt");
    /// <summary>Halley's Comet</summary>
    public static string Halley => Get("ui.halley");
    /// <summary>Constellations</summary>
    public static string Constellations => Get("ui.constellations");
    /// <summary>Star names</summary>
    public static string StarNames => Get("ui.starNames");
    /// <summary>Stars:</summary>
    public static string Stars => Get("ui.stars");
    /// <summary>Focus:</summary>
    public static string Focus => Get("ui.focus");
    /// <summary>Reset view</summary>
    public static string ResetView => Get("ui.resetView");
    /// <summary>Date:</summary>
    public static string Date => Get("ui.date");
    /// <summary>YYYY-MM-DD</summary>
    public static string DatePlaceholder => Get("ui.datePlaceholder");
    /// <summary>Go there</summary>
    public static string GoToDate => Get("ui.goToDate");
    /// <summary>‹‹ Year</summary>
    public static string StepYearBack => Get("ui.stepYearBack");
    /// <summary>‹ Month</summary>
    public static string StepMonthBack => Get("ui.stepMonthBack");
    /// <summary>‹ Day</summary>
    public static string StepDayBack => Get("ui.stepDayBack");
    /// <summary>Today</summary>
    public static string TodayButton => Get("ui.todayButton");
    /// <summary>Day ›</summary>
    public static string StepDayForward => Get("ui.stepDayFwd");
    /// <summary>Month ›</summary>
    public static string StepMonthForward => Get("ui.stepMonthFwd");
    /// <summary>Year ››</summary>
    public static string StepYearForward => Get("ui.stepYearFwd");
    /// <summary>Mission:</summary>
    public static string Mission => Get("ui.mission");
    /// <summary>Launch to Mars</summary>
    public static string LaunchMars => Get("ui.launchMars");
    /// <summary>Launch to the Moon</summary>
    public static string LaunchMoon => Get("ui.launchMoon");
    /// <summary>Next launch window</summary>
    public static string NextWindow => Get("ui.nextWindow");
    /// <summary>Abort the mission</summary>
    public static string AbortMission => Get("ui.abortMission");
    /// <summary>Meetings:</summary>
    public static string Meetings => Get("ui.meetings");
    /// <summary>Go to next</summary>
    public static string GoToNext => Get("ui.goToNext");
    /// <summary>The Moon's orbit</summary>
    public static string MoonOrbit => Get("ui.moonOrbit");
    /// <summary>Show probes</summary>
    public static string ShowProbes => Get("ui.showProbes");
    /// <summary>All</summary>
    public static string All => Get("ui.all");
    /// <summary>None</summary>
    public static string None => Get("ui.none");
    /// <summary>▾  Hide controls</summary>
    public static string HidePanel => Get("ui.hidePanel");
    /// <summary>▴  Show controls</summary>
    public static string ShowPanel => Get("ui.showPanel");
    /// <summary>Language:</summary>
    public static string Language => Get("ui.language");
    /// <summary>None</summary>
    public static string StarsNone => Get("ui.starsNone");
    /// <summary>Few</summary>
    public static string StarsFew => Get("ui.starsFew");
    /// <summary>Normal</summary>
    public static string StarsNormal => Get("ui.starsNormal");
    /// <summary>Many</summary>
    public static string StarsMany => Get("ui.starsMany");
    /// <summary>Follow the system</summary>
    public static string FollowSystem => Get("ui.followSystem");
    /// <summary>Drag with the mouse = rotate • Scroll wheel = zoom • Arrow keys = rotate • W/S = zoom • Space = start/pause • R = reset view • M = hide the menu</summary>
    public static string Help => Get("ui.help");
    /// <summary>The Solar System in 3D</summary>
    public static string WindowTitle => Get("ui.windowTitle");
    /// <summary>Elapsed</summary>
    public static string Elapsed => Get("msg.elapsed");
    /// <summary>Back</summary>
    public static string Back => Get("msg.back");
    /// <summary>stopped</summary>
    public static string SpeedStopped => Get("msg.speedStopped");
    /// <summary> backwards</summary>
    public static string SpeedBackwards => Get("msg.speedBackwards");
    /// <summary>Closed</summary>
    public static string WindowClosed => Get("msg.windowClosed");
    /// <summary>Wind time forward to follow the journey</summary>
    public static string ProbeWindTime => Get("msg.probeWindTime");
    /// <summary>No more flybys – on its way out of the Solar System</summary>
    public static string ProbeNoMore => Get("msg.probeNoMore");
    /// <summary>gained</summary>
    public static string Gained => Get("msg.gained");
    /// <summary>cost</summary>
    public static string Cost => Get("msg.cost");
    /// <summary>No trajectory could be computed just now</summary>
    public static string CraftNoPath => Get("msg.craftNoPath");
    /// <summary>No such meeting found</summary>
    public static string NoMeeting => Get("msg.noMeeting");
    /// <summary>Solar eclipse</summary>
    public static string ChoiceSolarEclipse => Get("msg.choiceSolarEclipse");
    /// <summary>Lunar eclipse</summary>
    public static string ChoiceLunarEclipse => Get("msg.choiceLunarEclipse");
    /// <summary>Halley at perihelion</summary>
    public static string ChoicePerihelion => Get("msg.choicePerihelion");
    /// <summary>dddd d MMMM yyyy, HH:mm</summary>
    public static string DateFormat => Get("msg.dateFormat");
}
