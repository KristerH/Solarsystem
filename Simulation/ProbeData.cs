namespace Solarsystem.Simulation;

/// <summary>
/// The real spacecraft, with their real dates.
///
/// The dates are the flybys of each planet, and today's positions come from
/// NASA's data on where the probes are: distance and direction in the sky as
/// right ascension and declination. Distance and direction are rounded, but
/// that's the only data the app needs – the orbits are computed from it.
/// </summary>
public static class ProbeData
{
    static CelestialBody Planet(string key)
        => SolarSystemData.Planets.First(p => p.Key == key);

    /// <summary>Today's date in the model, the one the probes' known positions are given for.</summary>
    static readonly DateTime Today = new(2026, 1, 1);

    /// <summary>
    /// Voyager 1: launched 5 September 1977, past Jupiter on 5 March 1979 and
    /// Saturn on 12 November 1980.
    ///
    /// At Saturn, the choice was made to swing the probe sharply upward, out
    /// of the ecliptic, to get close to the moon Titan. The price was that it
    /// could never reach another planet; the gain was the first close-up of a
    /// moon with an atmosphere. Today Voyager 1 is the most distant object
    /// humanity has built, just over 167 AU out in the direction of
    /// Ophiuchus.
    /// </summary>
    public static readonly Probe Voyager1 = Probe.Build(
        "Voyager 1", Color.FromArgb("#F2D9A0"),
        Waypoint.At(Planet("Earth"), new DateTime(1977, 9, 5)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1979, 3, 5)),
        Waypoint.At(Planet("Saturn"), new DateTime(1980, 11, 12)),
        Waypoint.InSky("probeToday", Today, 167.0, 17.25, 12.3))
        .Crossing("Heliopause", new DateTime(2012, 8, 25));

    /// <summary>
    /// Voyager 2: launched 20 August 1977 – two weeks before Voyager 1,
    /// despite the name – and past Jupiter on 9 July 1979, Saturn on 25
    /// August 1981, Uranus on 24 January 1986 and Neptune on 25 August 1989.
    ///
    /// It's the only spacecraft to have visited all four giant planets. That
    /// was possible thanks to an alignment where the planets stood in a row,
    /// something that only happens once every 176 years. At Neptune it
    /// instead swung sharply downward to pass the moon Triton, and so left
    /// the ecliptic in the opposite direction from its twin.
    /// </summary>
    public static readonly Probe Voyager2 = Probe.Build(
        "Voyager 2", Color.FromArgb("#A8DCEC"),
        Waypoint.At(Planet("Earth"), new DateTime(1977, 8, 20)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1979, 7, 9)),
        Waypoint.At(Planet("Saturn"), new DateTime(1981, 8, 25)),
        Waypoint.At(Planet("Uranus"), new DateTime(1986, 1, 24)),
        Waypoint.At(Planet("Neptune"), new DateTime(1989, 8, 25)),
        Waypoint.InSky("probeToday", Today, 140.0, 20.12, -59.5))
        .Crossing("Heliopause", new DateTime(2018, 11, 5));

    /// <summary>
    /// Pioneer 10: launched 3 March 1972, past Jupiter on 4 December 1973. It
    /// was first at everything – first through the asteroid belt, first at
    /// Jupiter, and first out of the planetary system. Radio contact went
    /// silent in 2003, so today's position is calculated rather than
    /// measured.
    ///
    /// The probe is leaving the Solar System nearly along the ecliptic, in
    /// the direction of Aldebaran in Taurus. Getting there takes over two
    /// million years.
    /// </summary>
    public static readonly Probe Pioneer10 = Probe.Build(
        "Pioneer 10", Color.FromArgb("#E4A98F"),
        Waypoint.At(Planet("Earth"), new DateTime(1972, 3, 3)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1973, 12, 4)),
        Waypoint.InSky("probeToday", Today, 140.0, 4.60, 16.5));

    /// <summary>
    /// Pioneer 11: launched 6 April 1973, past Jupiter on 3 December 1974 and
    /// Saturn on 1 September 1979 – the first probe to visit Saturn.
    ///
    /// The path between the two was unusual: Jupiter slung the probe upward
    /// and across the Solar System, so it met Saturn on the far side of the
    /// Sun. That leg therefore sweeps more than half a turn, which the
    /// Lambert solver handles by choosing the long way whenever the short
    /// way would run backward.
    /// </summary>
    public static readonly Probe Pioneer11 = Probe.Build(
        "Pioneer 11", Color.FromArgb("#C3CE9E"),
        Waypoint.At(Planet("Earth"), new DateTime(1973, 4, 6)),
        Waypoint.At(Planet("Jupiter"), new DateTime(1974, 12, 3)),
        Waypoint.At(Planet("Saturn"), new DateTime(1979, 9, 1)),
        Waypoint.InSky("probeToday", Today, 118.0, 18.50, -8.9));

    /// <summary>
    /// New Horizons: launched 19 January 2006, past Jupiter on 28 February
    /// 2007 and Pluto on 14 July 2015. The fastest launch ever flown – it
    /// passed the Moon's orbit after nine hours, against Apollo's three days.
    ///
    /// The Pluto flyby is a good proof that the orbits are computed in three
    /// dimensions: Pluto's orbit tilts 17 degrees, so the encounter happened
    /// far outside the ecliptic plane.
    /// </summary>
    public static readonly Probe NewHorizons = Probe.Build(
        "New Horizons", Color.FromArgb("#D9AEE6"),
        Waypoint.At(Planet("Earth"), new DateTime(2006, 1, 19)),
        Waypoint.At(Planet("Jupiter"), new DateTime(2007, 2, 28)),
        Waypoint.At(Planet("Pluto"), new DateTime(2015, 7, 14)),
        Waypoint.InSky("probeToday", Today, 63.0, 19.25, -20.5));

    /// <summary>
    /// Every probe the app draws. Five spacecraft are on their way out of the
    /// Solar System, and this is all of them.
    /// </summary>
    public static readonly Probe[] All =
        [Voyager1, Voyager2, Pioneer10, Pioneer11, NewHorizons];

    // ------------------------------------------------------- orbiting probes

    /// <summary>
    /// Cassini at Saturn: entered orbit 1 July 2004, and ended 15 September
    /// 2017 by being steered straight down into Saturn's atmosphere – so that
    /// an uncrewed probe carrying Earth bacteria could never one day crash
    /// into Enceladus, which has an ocean under its ice.
    ///
    /// The lap shown is a representative one out of the nearly three hundred
    /// Cassini flew: periapsis just under three Saturn radii out, apoapsis
    /// just under forty, giving a sixteen-day period. The orbit tilts twenty
    /// degrees to the ring plane – Cassini alternated between lying in the
    /// ring plane and tilting up to 75 degrees to see the rings from above.
    /// </summary>
    public static readonly Orbiter Cassini = Orbiter.Build(
        "Cassini", Color.FromArgb("#F2C86A"), Planet("Saturn"),
        periapsisRadii: 2.70, apoapsisRadii: 39.36,
        inclinationToEquatorDeg: 20.0,
        argPeriapsisDeg: 60.0,
        arrival: new DateTime(2004, 7, 1),
        end: new DateTime(2017, 9, 15),
        ending: "burnedInSaturn")!;

    /// <summary>
    /// Juno at Jupiter: entered orbit 5 July 2016. The orbit is extreme – it
    /// dives down to just over one Jupiter radius, i.e. only a few thousand
    /// kilometres above the cloud tops, and back out to 116 radii, which is
    /// eight million kilometres. One lap takes 53 days.
    ///
    /// The orbit runs over the poles, unlike the moons', which lie in the
    /// equatorial plane. That's the whole point of Juno: it's meant to
    /// measure Jupiter's magnetic field and interior, and diving that way
    /// also keeps it between the planet and the most dangerous radiation
    /// belts, which sit around the equator.
    ///
    /// The end date is the last confirmed contact, not a mission end. Juno
    /// kept flying long past the extended mission's planned end on 30
    /// September 2025 and sent data throughout spring 2026; on 1 May 2026 it
    /// took close-up images of the small moon Thebe with its star tracker.
    /// Nothing is confirmed after that, and the risk is budgetary rather than
    /// technical – the probe works, but was among the missions proposed for
    /// cancellation.
    ///
    /// The app therefore draws Juno up to the last known contact and no
    /// further. Better to miss a probe that's still flying than to show one
    /// that no longer exists.
    ///
    /// Unlike Cassini, Juno will not be steered down into the planet. That
    /// was the original plan, for the same reason: a probe crashing into
    /// Europa could carry Earth bacteria into the ocean under its ice. But
    /// over the years in orbit, the moons' gravity bent the orbit so much
    /// that Juno eventually stopped passing anywhere near Europa at all, and
    /// so there was nothing left to protect.
    /// </summary>
    public static readonly Orbiter Juno = Orbiter.Build(
        "Juno", Color.FromArgb("#9FD8F2"), Planet("Jupiter"),
        periapsisRadii: 1.08, apoapsisRadii: 115.90,
        inclinationToEquatorDeg: 90.0,
        argPeriapsisDeg: 0.0,
        arrival: new DateTime(2016, 7, 5),
        end: new DateTime(2026, 5, 1),
        ending: "lastContact")!;

    /// <summary>The probes that orbit a planet instead of leaving.</summary>
    public static readonly Orbiter[] Orbiters = [Cassini, Juno];
}
