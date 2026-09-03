using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// A celestial body with Keplerian orbital elements (epoch J2000).
/// Angles in degrees: inclination i, ascending node longitude Ω,
/// perihelion longitude ϖ, and mean longitude L0 at the epoch.
/// </summary>
/// <summary>
/// A ring system around a planet. Radii are given in planet radii, and the
/// plane is taken directly from the planet's rotation axis, so the rings
/// end up in the same plane as the moons without the inclination needing to
/// be written a second time.
/// </summary>
/// <param name="MinScreenRadius">
/// The ring is drawn only once it reaches this many pixels on screen.
/// Saturn's rings are visible to the naked eye in a small telescope and so
/// get a low value; the other three are so faint they were only discovered
/// by space probes, and so only appear once zoomed in substantially.
/// </param>
public sealed record PlanetRing(
    float InnerRadii,
    float OuterRadii,
    BodyAxis Axis,
    Color Color,
    float MinScreenRadius);

/// <param name="Key">
/// The body's language-neutral key, e.g. <c>Earth</c> or <c>Ganymede</c>.
/// It's not a name to display: the name is looked up in the resource files,
/// which have one row per language. The key is used both as an identity in
/// the code and as a lookup term, and it's in English for the same reason
/// all other code is.
/// </param>
public sealed record CelestialBody(
    string Key,
    Color BodyColor,
    double RadiusKm,
    double SemiMajorAu,
    double Eccentricity,
    double InclinationDeg,
    double AscNodeDeg,
    double PerihelionLonDeg,
    double MeanLonJ2000Deg,
    double OrbitalPeriodDays)
{
    /// <summary>
    /// Moons orbiting this body. Their orbital elements are planet-centric –
    /// PositionAt then gives the offset from the planet, not from the Sun.
    /// </summary>
    public CelestialBody[] Moons { get; init; } = [];

    /// <summary>
    /// The moon's share of the system's total mass. Controls how much the
    /// parent body itself wobbles around their common centre of mass. Zero
    /// (the default) means the parent body stands still, which is enough for
    /// every lightweight moon.
    /// </summary>
    public double MassFraction { get; init; }

    /// <summary>The planet's ring system, or null for bodies without rings.</summary>
    public PlanetRing? Ring { get; init; }

    /// <summary>
    /// The body's rotation axis and rotation period, or null for bodies
    /// where it doesn't matter. Moons and rings in the equatorial plane read
    /// their orbital plane from here, so the same two numbers describe both
    /// the pole and their orbits.
    /// </summary>
    public BodyAxis? Axis { get; init; }

    /// <summary>
    /// Surface map, for the bodies drawn as a globe once zoomed in enough.
    /// Null means the body is drawn as a shaded disc – either because it's
    /// too small to matter, or because there's no surface to show (Venus's
    /// clouds, Titan's haze).
    /// </summary>
    public SurfaceMap? Surface { get; init; }

    /// <summary>
    /// How fast the orbit's ascending node drifts, in degrees per day.
    /// Negative means backward, which is the usual case.
    ///
    /// An orbital plane doesn't sit still. The Moon's node completes a full
    /// backward turn in 18.6 years, and it's that motion that makes eclipse
    /// seasons drift nineteen days earlier each year instead of falling on
    /// the same date. With the node locked in place, that would be
    /// impossible to show.
    ///
    /// Zero for everything with no known or noticeable precession.
    /// </summary>
    public double AscNodeRateDegPerDay { get; init; }

    /// <summary>
    /// How fast perihelion drifts, in degrees per day. Positive means
    /// forward.
    ///
    /// Tied to the node and must be set together with it. The reason is
    /// that the mean anomaly is computed as mean longitude minus perihelion
    /// longitude: letting perihelion stand still while the node moves gives
    /// the orbit the right plane but the wrong position within it. The
    /// Moon's perigee advances a full turn in 8.85 years, and that motion is
    /// also what separates the anomalistic lunar month (27.55 days) from the
    /// sidereal one (27.32).
    /// </summary>
    public double PerihelionRateDegPerDay { get; init; }

    /// <summary>
    /// The body's gravitational parameter G·M in AU³/day² – the measure of
    /// how hard it pulls on whatever orbits it. Orbital periods follow from
    /// it: one lap at semi-major axis a takes 2π·√(a³/µ). Only needed for
    /// the bodies something actually orbits in the app, and zero for the
    /// rest.
    /// </summary>
    public double Mu { get; init; }

    /// <summary>Position in world coordinates (Y = north of the ecliptic) at a given time.</summary>
    public Vector3 PositionAt(double daysSinceJ2000, float unitsPerAu)
    {
        var p = PositionAuAt(daysSinceJ2000);
        return new Vector3(
            (float)(p.X * unitsPerAu), (float)(p.Y * unitsPerAu), (float)(p.Z * unitsPerAu));
    }

    /// <summary>
    /// The same position in AU, but without dropping to single precision.
    ///
    /// Rendering doesn't need this, but building an orbit does: an orbit
    /// computed from a position and a velocity otherwise loses digits before
    /// the computation even starts. See <see cref="Vec3"/> for why that hits
    /// so hard right there.
    /// </summary>
    public Vec3 PositionAuAt(double daysSinceJ2000)
    {
        double meanMotion = 360.0 / OrbitalPeriodDays;
        // The mean anomaly is measured against the drifting perihelion.
        // Subtracting its motion here is also what makes the mean longitude
        // still complete its sidereal lap – the two effects cancel out, just
        // as they do in reality.
        double mDeg = MeanLonJ2000Deg + meanMotion * daysSinceJ2000
                      - PerihelionAt(daysSinceJ2000);
        // Wrapped down to one turn before converting to radians. Fast moons
        // (Phobos manages three laps a day) would otherwise produce millions
        // of degrees, which eats into floating-point precision. The orbital
        // position is unchanged.
        double M = DegToRad(mDeg % 360.0);
        double E = SolveKepler(M, Eccentricity);
        return ToWorldAu(E, daysSinceJ2000);
    }

    /// <summary>The ascending node's longitude at a given time.</summary>
    public double AscNodeAt(double daysSinceJ2000)
        => AscNodeDeg + AscNodeRateDegPerDay * daysSinceJ2000;

    /// <summary>The perihelion longitude at a given time.</summary>
    public double PerihelionAt(double daysSinceJ2000)
        => PerihelionLonDeg + PerihelionRateDegPerDay * daysSinceJ2000;

    /// <summary>The whole orbital ellipse as a list of points (closed curve, evenly sampled in eccentric anomaly).</summary>
    /// <param name="daysSinceJ2000">
    /// Which day the orbit should be drawn for. Only matters for bodies
    /// whose plane rotates; for the rest the orbit looks the same at every
    /// time.
    /// </param>
    public Vector3[] OrbitPath(int samples, float unitsPerAu, double daysSinceJ2000 = 0)
    {
        var pts = new Vector3[samples];
        for (int k = 0; k < samples; k++)
        {
            var p = ToWorldAu(2.0 * Math.PI * k / samples, daysSinceJ2000);
            pts[k] = new Vector3(
                (float)(p.X * unitsPerAu), (float)(p.Y * unitsPerAu), (float)(p.Z * unitsPerAu));
        }
        return pts;
    }

    Vec3 ToWorldAu(double E, double daysSinceJ2000)
    {
        double e = Eccentricity;
        // Position in the orbital plane (focus = Sun).
        double xv = SemiMajorAu * (Math.Cos(E) - e);
        double yv = SemiMajorAu * Math.Sqrt(1.0 - e * e) * Math.Sin(E);

        double node = AscNodeAt(daysSinceJ2000);
        double w = DegToRad(PerihelionAt(daysSinceJ2000) - node); // argument of perihelion
        double O = DegToRad(node);
        double i = DegToRad(InclinationDeg);
        double cw = Math.Cos(w), sw = Math.Sin(w);
        double cO = Math.Cos(O), sO = Math.Sin(O);
        double ci = Math.Cos(i), si = Math.Sin(i);

        // Rotation orbital plane -> ecliptic coordinates.
        double x = (cw * cO - sw * sO * ci) * xv + (-sw * cO - cw * sO * ci) * yv;
        double y = (cw * sO + sw * cO * ci) * xv + (-sw * sO + cw * cO * ci) * yv;
        double z = (sw * si) * xv + (cw * si) * yv;

        // The ecliptic's plane is laid horizontal; north (+z) points up (+Y).
        return new Vec3(x, z, -y);
    }

    static double SolveKepler(double M, double e)
    {
        // Newton-Raphson on Kepler's equation E - e·sin E = M.
        double E = M;
        for (int k = 0; k < 12; k++)
            E -= (E - e * Math.Sin(E) - M) / (1.0 - e * Math.Cos(E));
        return E;
    }

    static double DegToRad(double d) => d * Math.PI / 180.0;
}

public static class SolarSystemData
{
    public const double AuKm = 149_597_870.7;

    /// <summary>
    /// The Sun's gravitational parameter G·M in AU³/day². That Earth's orbit
    /// with a = 1 AU takes 365.26 days is exactly this number written
    /// backward.
    /// </summary>
    public const double SunMu = 2.959122082855911e-4;

    /// <summary>
    /// Earth and the Moon's combined gravitational parameter, 403,503 km³/s²
    /// converted to AU and days. The Sun's is 330,000 times larger – which
    /// is why the Moon's lap around Earth takes 27 days while Earth's lap
    /// around the Sun takes a year, even though the distances differ by
    /// nearly 400 times.
    /// </summary>
    public const double EarthMu = 8.9971e-10;

    /// <summary>
    /// Jupiter's gravitational parameter (126,687,000 km³/s²), for probes
    /// orbiting the planet. That it's accurate shows up in the moons: the
    /// number gives Io a lap of 1.770 days, against a measured 1.769.
    /// </summary>
    public const double JupiterMu = 2.8248e-7;

    /// <summary>
    /// Saturn's gravitational parameter (37,931,000 km³/s²). Gives Titan a
    /// lap of 15.95 days, exactly its measured orbital period.
    /// </summary>
    public const double SaturnMu = 8.4573e-8;

    public const double SunRadiusKm = 696_340.0;
    public static readonly DateTime EpochJ2000 = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    // ---------------------------------------------------------- rotation axes
    //
    // The axes are computed from the IAU's pole directions (right ascension
    // and declination of the north pole, plus the prime meridian's position
    // W0) and converted to ecliptic coordinates. The north pole follows the
    // right-hand rule, so inclinations above 90 degrees mean retrograde
    // rotation – see BodyAxis for why the convention looks that way.
    //
    // That the numbers check out can be tested against more than
    // themselves: Mercury's axis lands 7.0 degrees from the ecliptic with
    // node 48.2, matching its own orbital inclination and node exactly – the
    // planet stands bolt upright relative to its own orbit, exactly as
    // measured. Earth's data gives a sub-solar point 0.8 degrees east of
    // Greenwich at J2000.0 (noon in Greenwich on 1 January 2000, the
    // equation of time included) and a declination of -23.0 degrees, i.e.
    // the middle of winter.

    /// <summary>
    /// The Sun's own rotation. That it has one isn't obvious – Galileo showed
    /// it in 1613 by following sunspots across the disc, and it was the same
    /// observation that revealed the spots sit on the Sun and aren't small
    /// bodies passing in front of it.
    ///
    /// The equator tilts 7.25 degrees to the ecliptic. The consequence can
    /// be seen in the app: we see more of the Sun's north pole in September
    /// and more of the south pole in March.
    ///
    /// The rotation period of 25.03 days applies <b>at the equator</b> and
    /// is measured on sunspots, i.e. on exactly what's drawn. The number is
    /// sidereal – one turn against the stars. Seen from Earth, which itself
    /// moves a bit along its orbit in the meantime, the same lap takes just
    /// under 27 days, and that's the figure sunspot observers have always
    /// quoted.
    ///
    /// The prime meridian is Carrington's, and that's a pure convention: the
    /// Sun has no lasting features to measure from. There's no crater and no
    /// coastline – just gas being replaced.
    /// </summary>
    public static readonly BodyAxis SunAxis = new(7.252, 75.77, 25.03, 23.075)
    {
        // The Sun rotates more slowly the farther from the equator you get.
        // It's the only place in the app where this number isn't zero, and
        // what makes it interesting is what it proves: a solid body can't do
        // that. The Sun is gas all the way through.
        //
        // The number is Newton and Nunn's 1951 sunspot measurement,
        // ω(φ) = 14.38 − 2.96·sin²φ degrees per day. At the equator it gives
        // 25.0 days, at 30 degrees latitude 26.4.
        //
        // **Caveat:** the law is measured on spots and so only applies where
        // spots occur, i.e. within ±35 degrees. Stretched to the pole it
        // gives 31.5 days against a measured 34. The difference doesn't
        // matter here, since only spots are drawn – but it would be wrong to
        // hide it.
        DifferentialDegPerDay = -2.96,
    };

    /// <summary>
    /// Mercury: 58.6 days, exactly two-thirds of its year – a 3:2 resonance
    /// with the Sun. Its tilt relative to its own orbit is only 0.03
    /// degrees, the smallest in the Solar System; the number below still
    /// comes out at 7.0 because the orbit itself tilts 7.0.
    /// </summary>
    public static readonly BodyAxis MercuryAxis = new(7.037, 48.24, 58.6461459, 291.274);

    /// <summary>
    /// Venus rotates backwards: the pole points almost due south of the
    /// ecliptic (178.8 degrees). One turn takes 243 days, longer than its
    /// 225-day year – the day is therefore longer than the year. Why it
    /// ended up flipped upside down, nobody knows for certain.
    /// </summary>
    public static readonly BodyAxis VenusAxis = new(178.761, 300.19, 243.0184840, 137.449);

    /// <summary>
    /// Earth: a 23.44-degree tilt – the whole explanation for the seasons –
    /// and one turn per sidereal day, 23h 56m 4.1s. That's four minutes
    /// shorter than a solar day, because Earth manages to move a bit along
    /// its orbit in the meantime. The prime meridian is taken from the same
    /// sidereal-time expression the night sky uses, so the sky and the
    /// globe turn in step.
    /// </summary>
    public static readonly BodyAxis EarthAxis = new(23.4392911, 180.0, 0.9972695663290739, 100.46061837);

    /// <summary>
    /// The Moon is tidally locked: one turn on its axis per orbit, hence the
    /// same side always facing us. The axis tilts only 1.5 degrees to the
    /// ecliptic but 6.7 degrees to the Moon's own orbit, and it always
    /// points the opposite way from the orbital pole (Cassini's second law).
    /// The orbit being elliptical still makes the Moon wobble six degrees
    /// back and forth in longitude – the libration that shows us a bit more
    /// than half the Moon.
    /// </summary>
    public static readonly BodyAxis MoonAxis = new(1.5424, 305.045, 27.32166, 93.293);

    /// <summary>Mars's day is nearly Earth's: 24h 37m. The 25.4-degree tilt gives it seasons.</summary>
    public static readonly BodyAxis MarsAxis = new(25.404, 84.84, 1.0259568, 133.120);

    /// <summary>
    /// Jupiter spins fastest of all: 9h 55m, despite being the largest. The
    /// axis stands nearly upright (2.2 degrees), so Jupiter has no seasons.
    /// </summary>
    public static readonly BodyAxis JupiterAxis = new(2.217, 337.82, 0.4135383, 305.363);

    /// <summary>Saturn: 10h 39m and a 28.1-degree tilt – that tilt is what the rings show off.</summary>
    public static readonly BodyAxis SaturnAxis = new(28.052, 169.53, 0.4440093, 358.934);

    /// <summary>
    /// Uranus lies on its side and rolls: 97.7 degrees means the axis lies
    /// almost in the orbital plane and the rotation is retrograde. Over its
    /// 84-year orbit, first one pole then the other points at the Sun, with
    /// forty years of day and forty years of night.
    /// </summary>
    public static readonly BodyAxis UranusAxis = new(97.722, 167.65, 0.7183333, 331.131);

    /// <summary>Neptune: 16h 06m and a 28.0-degree tilt, almost the same as Saturn.</summary>
    public static readonly BodyAxis NeptuneAxis = new(28.026, 49.24, 0.6712500, 228.657);

    /// <summary>
    /// Pluto rotates backwards (inclination 112.8 degrees) and is tidally
    /// locked to Charon: 6.387 days is both Pluto's day and Charon's orbital
    /// period. The two therefore constantly show the same face to each
    /// other – the only pair in the Solar System that does.
    /// </summary>
    public static readonly BodyAxis PlutoAxis = new(112.816, 227.35, 6.3872230, 319.809);

    /// <summary>
    /// The Moon with geocentric mean orbital elements (J2000): the orbit is
    /// computed around Earth instead of the Sun, with the same Kepler math.
    /// One lap takes 27.3 days (a sidereal month).
    /// </summary>
    public static readonly CelestialBody Moon = new(
        "Moon", Color.FromArgb("#BEBEB6"), 1_737.4,
        0.0025696 /* = 384,399 km */, 0.0549, 5.145, 125.045, 83.353, 218.316, 27.32166)
    {
        Axis = MoonAxis,
        Surface = SurfaceMap.Moon,
        // The node completes a backward turn in 18.6 years, perigee a
        // forward turn in 8.85. The two numbers are the oldest in the whole
        // app: the Babylonians knew the node cycle as early as the 6th
        // century BC and could predict eclipses with it.
        AscNodeRateDegPerDay = -0.0529539,
        PerihelionRateDegPerDay = 0.1114041,
    };

    // Mars's two small, irregular moons – probably captured asteroids. They
    // orbit very close to Mars: Phobos at just 2.8 Mars radii (compare the
    // Moon's 60 Earth radii). Their orbits lie in Mars's equatorial plane,
    // which tilts 25.4° to the ecliptic – that's why the orbital
    // inclination isn't near zero. The plane is read from MarsAxis, so it's
    // written in only one place.
    // Note: the phase angles (mean longitudes) are approximate. With
    // orbital periods of hours, even a very small period error accumulates
    // into whole laps over the decades the app can simulate, so the moons'
    // exact position in orbit on a given date still can't be trusted.
    // Distance, size, orbital plane and period are real, though.
    public static readonly CelestialBody Phobos = new(
        "Phobos", Color.FromArgb("#A89684"), 11.267,
        9_376.0 / AuKm, 0.0151, MarsAxis.InclinationDeg, MarsAxis.NodeDeg,
        0.0, 0.0, 0.3189100);

    public static readonly CelestialBody Deimos = new(
        "Deimos", Color.FromArgb("#B8A794"), 6.2,
        23_463.2 / AuKm, 0.00033, MarsAxis.InclinationDeg, MarsAxis.NodeDeg,
        0.0, 180.0, 1.2624407);

    // Jupiter's four large moons, the ones Galileo saw in 1610 and that
    // revealed not everything orbits Earth. Their orbits lie in Jupiter's
    // equatorial plane, which tilts only 2.2° to the ecliptic (Jupiter
    // stands nearly upright).
    //
    // The phase angles are chosen so the Laplace resonance holds:
    //     meanLongitude(Io) - 3*meanLongitude(Europa) + 2*meanLongitude(Ganymede) = 180°
    // Since the orbital periods already satisfy the resonance in mean
    // motion, the condition holds over time. The consequence is that the
    // three inner moons can never line up at once – whenever Io and Europa
    // meet, Ganymede is always 90° away. The four Galilean moons and Titan
    // are tidally locked: one turn on the axis per orbit, so the same side
    // always faces the planet. The axis stands perpendicular to the
    // orbital plane, i.e. the same inclination and node as the orbit, and
    // the rotation period is the orbit's own. The prime meridians are
    // computed so longitude zero points at the planet at the epoch, using
    // the moon's MEAN position as the anchor – using the true position
    // instead would land the zero at a libration extreme and make the
    // wobble asymmetric.

    public static readonly CelestialBody Io = new(
        "Io", Color.FromArgb("#E0C96A"), 1_821.6,
        421_800.0 / AuKm, 0.0041, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 0.0, 1.769138)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 1.769138, 202.180),
        Surface = SurfaceMap.Io,
    };

    public static readonly CelestialBody Europa = new(
        "Europa", Color.FromArgb("#DCD3C4"), 1_560.8,
        671_100.0 / AuKm, 0.0094, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 0.0, 3.551181)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 3.551181, 202.180),
        Surface = SurfaceMap.Europa,
    };

    public static readonly CelestialBody Ganymedes = new(
        "Ganymede", Color.FromArgb("#9E8E7C"), 2_634.1,
        1_070_400.0 / AuKm, 0.0013, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 90.0, 7.154553)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 7.154553, 292.180),
        Surface = SurfaceMap.Ganymede,
    };

    public static readonly CelestialBody Callisto = new(
        "Callisto", Color.FromArgb("#7C7065"), 2_410.3,
        1_882_700.0 / AuKm, 0.0074, JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg,
        0.0, 180.0, 16.689018)
    {
        Axis = new BodyAxis(JupiterAxis.InclinationDeg, JupiterAxis.NodeDeg, 16.689018, 22.180),
        Surface = SurfaceMap.Callisto,
    };

    // Charon, Pluto's large companion. With half Pluto's diameter and an
    // eighth of its mass, the pair is nearly a double planet: their common
    // centre of mass sits 2,126 km from Pluto's centre, i.e. outside Pluto
    // itself (which has a radius of 1,188 km). Pluto therefore visibly
    // wobbles around that centre instead of standing still.
    //
    // The orbit lies in Pluto's equatorial plane. Since Pluto rotates
    // retrograde, that plane tilts more than 90 degrees to the ecliptic –
    // Charon therefore travels "backwards" compared to most moons. The two
    // are also fully tidally locked: they constantly show the same face to
    // each other.
    //
    // The mean longitude isn't taken from an ephemeris but set so Charon
    // sits above Pluto's prime meridian, which is exactly what tidal
    // locking means – the IAU defines Pluto's prime meridian as the one
    // pointing at Charon. The consequence is that Sputnik Planitia, which
    // sits around 175 degrees east, faces away from Charon. That's really
    // how it looks, and it's probably no coincidence: the plain is heavy
    // enough to have rotated the whole of Pluto into place.
    public static readonly CelestialBody Charon = new(
        "Charon", Color.FromArgb("#9A9188"), 606.0,
        19_591.4 / AuKm, 0.0002, PlutoAxis.InclinationDeg, PlutoAxis.NodeDeg,
        0.0, 188.4, 6.387230)
    {
        MassFraction = 0.1085,
    };

    // Saturn's three best-known moons. Their orbits lie in Saturn's
    // equatorial plane, the same plane the rings tilt in (28.0 degrees to
    // the ecliptic). Enceladus is the brightest body in the Solar System –
    // an ice-white moon with geysers spraying water from an ocean under the
    // ice. Titan is larger than Mercury and the only moon with a thick
    // atmosphere, with lakes of liquid methane.
    public static readonly CelestialBody Enceladus = new(
        "Enceladus", Color.FromArgb("#F0F4F5"), 252.1,
        237_948.0 / AuKm, 0.0047, SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg,
        0.0, 0.0, 1.370218);

    public static readonly CelestialBody Rhea = new(
        "Rhea", Color.FromArgb("#C8C6C0"), 763.8,
        527_108.0 / AuKm, 0.0010, SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg,
        0.0, 120.0, 4.518212);

    public static readonly CelestialBody Titan = new(
        "Titan", Color.FromArgb("#D9A05B"), 2_574.7,
        1_221_870.0 / AuKm, 0.0288, SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg,
        0.0, 240.0, 15.945421)
    {
        // Titan gets no surface map, and that's the answer to the puzzle
        // rather than a gap: the haze is opaque and no surface is visible.
        // The moon is drawn as an evenly lit and shaded orange disc, just
        // like Venus. The axis is still included, since the tidal lock is
        // true whether or not it's visible.
        Axis = new BodyAxis(SaturnAxis.InclinationDeg, SaturnAxis.NodeDeg, 15.945421, 250.470),
    };

    // Uranus's moons, named after characters from Shakespeare and Pope.
    // Since Uranus lies on its side, the whole moon system stands nearly on
    // edge relative to the ecliptic: the inclination read from UranusAxis
    // is 97.7 degrees, i.e. above 90. Uranus rotates retrograde and the
    // moons follow their planet's rotation. The same plane with an
    // inclination of 82.3 degrees would have given the right plane but the
    // wrong direction of travel.

    public static readonly CelestialBody Miranda = new(
        "Miranda", Color.FromArgb("#A8A5A0"), 235.8,
        129_390.0 / AuKm, 0.0013, UranusAxis.InclinationDeg, UranusAxis.NodeDeg,
        0.0, 0.0, 1.413479);

    public static readonly CelestialBody Titania = new(
        "Titania", Color.FromArgb("#B5AAA0"), 788.4,
        435_910.0 / AuKm, 0.0011, UranusAxis.InclinationDeg, UranusAxis.NodeDeg,
        0.0, 140.0, 8.706234);

    public static readonly CelestialBody Oberon = new(
        "Oberon", Color.FromArgb("#9C9088"), 761.4,
        583_520.0 / AuKm, 0.0014, UranusAxis.InclinationDeg, UranusAxis.NodeDeg,
        0.0, 260.0, 13.463234);

    // Triton is the Solar System's great exception: it orbits RETROGRADE,
    // i.e. opposite to Neptune's rotation and to everything else in this
    // app. A moon formed together with its planet can't do that – Triton is
    // therefore almost certainly a captured dwarf planet from the Kuiper
    // belt. The inclination above 90 degrees is exactly what makes the
    // motion retrograde.
    // Note: the orbital plane precesses with a period of about 640 years,
    // so the orientation below is the position at the epoch rather than a
    // lasting property.
    // Triton has its own orbital plane since it doesn't follow the
    // equator; the rings read theirs from NeptuneAxis.
    public static readonly CelestialBody Triton = new(
        "Triton", Color.FromArgb("#D8CFC8"), 1_353.4,
        354_759.0 / AuKm, 0.000016, 130.0, 240.93, 0.0, 0.0, 5.876854)
    {
        // One turn in about 640 years, i.e. 0.0015 degrees per day. The
        // direction follows from the orbit being retrograde: Neptune's
        // oblateness turns the node at a rate that goes as the cosine of the
        // orbital inclination, and that inclination is above 90 degrees.
        // Where most moons have their node dragged backward, Triton's
        // therefore moves forward.
        AscNodeRateDegPerDay = 360.0 / (640.0 * 365.25),
    };

    // The ring systems. All four giant planets have rings – not just
    // Saturn. The radii are real, expressed in planet radii:
    //   Jupiter   122,500 – 129,000 km  (the thin main ring of dust)
    //   Saturn     74,700 – 136,800 km  (the C ring out to the A ring's outer edge)
    //   Uranus     38,000 –  51,150 km  (the narrow, coal-dark rings)
    //   Neptune    41,900 –  62,933 km  (out to the Adams ring)
    // Saturn's rings are bright ice particles; the other three are so dark
    // they weren't discovered until the 1970s and 80s.
    static readonly PlanetRing JupiterRing = new(
        1.75f, 1.85f, JupiterAxis, Color.FromRgba(0.58f, 0.45f, 0.36f, 0.30f), 25f);

    static readonly PlanetRing SaturnRing = new(
        1.283f, 2.349f, SaturnAxis, Color.FromRgba(0.85f, 0.78f, 0.60f, 0.55f), 3f);

    static readonly PlanetRing UranusRing = new(
        1.50f, 2.02f, UranusAxis, Color.FromRgba(0.55f, 0.63f, 0.70f, 0.30f), 25f);

    static readonly PlanetRing NeptuneRing = new(
        1.70f, 2.56f, NeptuneAxis, Color.FromRgba(0.50f, 0.58f, 0.74f, 0.26f), 25f);

    /// <summary>
    /// The dwarf planet Ceres, by far the largest body in the asteroid belt
    /// – it alone holds about a quarter of the whole belt's mass. Lies
    /// outside the planet list and is drawn together with the belt.
    /// </summary>
    public static readonly CelestialBody Ceres = new(
        "Ceres", Color.FromArgb("#A79C90"), 469.7,
        2.7675, 0.0758, 10.593, 80.393, 153.990, 249.979, 1_681.63);

    /// <summary>
    /// Halley's Comet: the only bright comet with a short enough period that
    /// a person can see it twice in a lifetime.
    ///
    /// The orbit is everything the planets' aren't. The eccentricity of
    /// 0.967 draws it out so far that the comet swings in inside Venus's
    /// orbit at perihelion (0.586 AU) and out past Neptune at aphelion (35.1
    /// AU) – sixty times farther out at most than at least. The speed
    /// follows from that, via Kepler's second law: 54 km/s at perihelion and
    /// under 1 km/s at aphelion. The comet therefore spends nearly its whole
    /// lap far out in the cold and rushes through the inner Solar System in
    /// a matter of months.
    ///
    /// The 162-degree inclination means it travels RETROGRADE, against the
    /// planets' direction of travel. That's why the 1986 encounter with
    /// Earth was so brief: the two bodies were coming toward each other
    /// instead of racing in the same direction.
    ///
    /// The elements are anchored to two known perihelia, 9 February 1986
    /// and 28 July 2061. That gives an orbital period of 27,563 days (75.5
    /// years), from which the semi-major axis follows.
    ///
    /// The radius refers to the nucleus, a potato-shaped lump roughly 15 x 8
    /// x 8 km and blacker than coal – the light doesn't come from it but
    /// from the gas and dust around it, which is also what the colour below
    /// describes.
    ///
    /// **Caveat:** a fixed Kepler orbit can't hit every perihelion. Halley's
    /// real orbital period varies between 74 and 79 years, since Jupiter and
    /// Saturn tug at the comet on every lap and jets of gas from the heated
    /// nucleus give it a further nudge, like a weak rocket. The model
    /// therefore places the 1910 perihelion in late August instead of 20
    /// April – four months off just one lap back. Around 1986 and 2061 it's
    /// accurate, and that's where it's used.
    /// </summary>
    public static readonly CelestialBody Halley = new(
        "Halley", Color.FromArgb("#CFEDE8"), 5.5,
        17.85745, 0.9671858, 162.262, 58.420, 169.753, 236.026, 27_562.54);

    // Orbital elements at J2000 (NASA/JPL, mean values). Accurate enough
    // that the planets' positions roughly match reality for a given date.
    public static readonly CelestialBody[] Planets =
    [
        new("Mercury", Color.FromArgb("#B5A79B"),  2_439.7, 0.38710, 0.20563, 7.005,  48.331,  77.456, 252.251,    87.969) { Axis = MercuryAxis, Surface = SurfaceMap.Mercury },
        new("Venus",     Color.FromArgb("#E8CDA0"),  6_051.8, 0.72333, 0.00677, 3.395,  76.680, 131.564, 181.980,   224.701) { Axis = VenusAxis, Surface = SurfaceMap.Venus },
        new("Earth",     Color.FromArgb("#4C8CE8"),  6_371.0, 1.00000, 0.01671, 0.000, -11.261, 102.947, 100.464,   365.256) { Moons = [Moon], Mu = EarthMu, Axis = EarthAxis, Surface = SurfaceMap.Earth },
        new("Mars",      Color.FromArgb("#D96C4A"),  3_389.5, 1.52371, 0.09339, 1.850,  49.559, 336.041, 355.445,   686.980) { Axis = MarsAxis, Moons = [Phobos, Deimos], Surface = SurfaceMap.Mars },
        new("Jupiter",   Color.FromArgb("#D8B48A"), 69_911.0, 5.20289, 0.04839, 1.304, 100.474,  14.728,  34.397, 4_332.59) { Axis = JupiterAxis, Moons = [Io, Europa, Ganymedes, Callisto], Ring = JupiterRing, Mu = JupiterMu, Surface = SurfaceMap.Jupiter },
        new("Saturn",    Color.FromArgb("#E8D5A8"), 58_232.0, 9.53668, 0.05386, 2.486, 113.662,  92.599,  49.954, 10_759.22) { Axis = SaturnAxis, Moons = [Enceladus, Rhea, Titan], Ring = SaturnRing, Mu = SaturnMu, Surface = SurfaceMap.Saturn },
        new("Uranus",    Color.FromArgb("#9BD4E4"), 25_362.0, 19.18916, 0.04726, 0.773, 74.017, 170.954, 313.238, 30_688.5) { Axis = UranusAxis, Moons = [Miranda, Titania, Oberon], Ring = UranusRing, Surface = SurfaceMap.Uranus },
        new("Neptune",   Color.FromArgb("#5A78E8"), 24_622.0, 30.06992, 0.00859, 1.770, 131.784,  44.965, 304.880, 60_182.0) { Axis = NeptuneAxis, Moons = [Triton], Ring = NeptuneRing, Surface = SurfaceMap.Neptune },
        // The dwarf planet Pluto: strongly tilted (17°) and an eccentric
        // orbit that dips inside Neptune's at times. One lap takes nearly
        // 248 years.
        new("Pluto",     Color.FromArgb("#C4AB94"),  1_188.3, 39.48212, 0.24883, 17.140, 110.304, 224.069, 238.929, 90_560.0) { Axis = PlutoAxis, Moons = [Charon], Surface = SurfaceMap.Pluto },
    ];
}
