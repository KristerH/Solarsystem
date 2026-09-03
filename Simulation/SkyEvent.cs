namespace Solarsystem.Simulation;

/// <summary>
/// Meetings in the sky: when two planets stand in the same direction seen
/// from Earth, or when a planet stands directly opposite the Sun.
///
/// The point of computing them <b>as seen from Earth</b> rather than from the
/// Sun isn't a detail. The great conjunction between Jupiter and Saturn in
/// 2020 lands on the right day, 21 December, if you ask from Earth – but
/// seven weeks off, 1 November, if you compare their heliocentric
/// longitudes. It's from Earth that they're observed, and it's Earth's own
/// position that determines when they appear to meet.
///
/// The search has the same shape as the launch windows in
/// <see cref="Mission"/>: step day by day, find a dip, refine. The difference
/// is that what's being minimized here is an angle in the sky instead of a
/// speed.
/// </summary>
public static class SkyEvent
{
    public enum Kind
    {
        /// <summary>Two bodies stand in the same direction seen from Earth.</summary>
        Conjunction,

        /// <summary>The planet stands directly opposite the Sun, i.e. closest to Earth and visible all night.</summary>
        Opposition,

        /// <summary>The Moon passes in front of the Sun: new moon near one of the Moon's orbital nodes.</summary>
        SolarEclipse,

        /// <summary>The Moon enters Earth's shadow: full moon near one of the nodes.</summary>
        LunarEclipse,

        /// <summary>
        /// The body stands closest to the Sun. Unlike the others, this isn't
        /// an event in the sky but in the orbit: it depends only on where the
        /// body itself is, not on where the observer stands.
        /// </summary>
        Perihelion,
    }

    /// <summary>A meeting: when it happens, how close in the sky, and how far away the body is.</summary>
    public sealed record Meeting(double Day, double SeparationDeg, double DistanceAu);

    /// <summary>
    /// A choice in the selector: what to search for and between which bodies.
    ///
    /// No label here. That depends on the language, and language doesn't
    /// belong in the simulation – the UI assembles the text from the kind and
    /// the bodies' keys.
    /// </summary>
    public sealed record Choice(Kind Kind, CelestialBody A, CelestialBody? B);

    const double CoarseStepDays = 1.0;

    /// <summary>How far ahead the search goes before giving up. Pluto needs the most: 248 years.</summary>
    const double MaxSearchDays = 300.0 * 365.25;

    /// <summary>
    /// How close two bodies must stand for it to count as a meeting. Five
    /// degrees is roughly a binoculars' field of view – standing farther
    /// apart than that, nobody would call it a conjunction, just an
    /// approach.
    /// </summary>
    const double ConjunctionLimitDeg = 5.0;

    /// <summary>
    /// How close to the Sun the Moon has to come for there to be a solar
    /// eclipse somewhere on Earth. The Sun and the Moon are both a bit over
    /// half a degree wide, and parallax from different places on the globe
    /// shifts the Moon by up to a degree – together the limit comes to just
    /// over a degree and a half measured from Earth's centre.
    /// </summary>
    const double SolarEclipseLimitDeg = 1.55;

    /// <summary>
    /// The equivalent for a lunar eclipse. Earth's umbra at the Moon's
    /// distance is about 0.7 degrees in radius and the Moon 0.26, so part of
    /// the Moon ends up in the shadow once its centre comes within a degree
    /// of the point directly opposite the Sun.
    /// </summary>
    const double LunarEclipseLimitDeg = 1.0;

    /// <summary>
    /// The angle between the Moon and the Sun as seen from Earth – or from
    /// the point directly opposite the Sun, which is where Earth's shadow
    /// falls.
    ///
    /// The Moon's orbital elements are geocentric: its position <b>is</b>
    /// already the direction from Earth. Subtracting Earth's position again,
    /// as for the planets, would have stacked Earth's orbit on top of the
    /// Moon's and given an answer ten degrees off.
    /// </summary>
    static double EclipseCost(Kind kind, double day)
    {
        var toSun = (-Earth.PositionAuAt(day)).Normalized();
        var toMoon = SolarSystemData.Moon.PositionAuAt(day).Normalized();
        double separation = Math.Acos(Math.Clamp(Vec3.Dot(toSun, toMoon), -1.0, 1.0))
                            * 180.0 / Math.PI;
        return kind == Kind.SolarEclipse ? separation : 180.0 - separation;
    }

    static CelestialBody Earth => SolarSystemData.Planets.First(p => p.Key == "Earth");

    /// <summary>
    /// The angle between two directions seen from Earth, in degrees. Null
    /// means the Sun, which sits at the origin.
    /// </summary>
    static double AngleFromEarth(CelestialBody? a, CelestialBody? b, double day)
    {
        var earth = Earth.PositionAuAt(day);
        var da = ((a?.PositionAuAt(day) ?? default) - earth).Normalized();
        var db = ((b?.PositionAuAt(day) ?? default) - earth).Normalized();
        return Math.Acos(Math.Clamp(Vec3.Dot(da, db), -1.0, 1.0)) * 180.0 / Math.PI;
    }

    /// <summary>
    /// The quantity to be minimized. For a conjunction it's the angular
    /// separation in the sky between the two bodies; for an opposition it's
    /// how far the planet is from standing directly opposite the Sun.
    /// </summary>
    static double Cost(Kind kind, CelestialBody a, CelestialBody? b, double day) => kind switch
    {
        Kind.Conjunction => AngleFromEarth(a, b, day),
        Kind.Opposition => 180.0 - AngleFromEarth(null, a, day),
        // Perihelion is simply the distance to the Sun. The search doesn't
        // care that this quantity has a different unit than the others – it
        // looks for a dip, and a distance has just as clear a dip as an
        // angle does. The advantage is that an orbit has exactly one
        // perihelion per lap, so it can't be missed.
        Kind.Perihelion => a.PositionAuAt(day).Length,
        _ => EclipseCost(kind, day),
    };

    /// <summary>How close the meeting has to be to count as a meeting at all.</summary>
    static double LimitFor(Kind kind) => kind switch
    {
        Kind.Conjunction => ConjunctionLimitDeg,
        Kind.SolarEclipse => SolarEclipseLimitDeg,
        Kind.LunarEclipse => LunarEclipseLimitDeg,
        // An opposition and a perihelion can't be "too poor". They happen
        // when they happen, and there's nothing to filter out.
        _ => double.PositiveInfinity,
    };

    /// <summary>
    /// The next meeting after the given day, or null if none is found within
    /// the search window. Called when clicked, so the search is allowed to
    /// cost something – a full century takes a few tenths of a second.
    /// </summary>
    public static Meeting? Next(Kind kind, CelestialBody a, CelestialBody? b, double fromDay)
    {
        double f0 = Cost(kind, a, b, fromDay);
        double f1 = Cost(kind, a, b, fromDay + CoarseStepDays);

        for (double d = fromDay + CoarseStepDays; d < fromDay + MaxSearchDays; d += CoarseStepDays)
        {
            double f2 = Cost(kind, a, b, d + CoarseStepDays);

            // A dip sits pinched between the three samples.
            if (f1 < f0 && f1 <= f2)
            {
                double day = Refine(kind, a, b, d - CoarseStepDays, d + CoarseStepDays);

                // A thirty-degree approach isn't a conjunction, and a new
                // moon far from the node isn't an eclipse. Keep searching.
                if (Cost(kind, a, b, day) > LimitFor(kind))
                {
                    f0 = f1; f1 = f2;
                    continue;
                }

                if (kind is Kind.SolarEclipse or Kind.LunarEclipse)
                    return new Meeting(day, EclipseCost(kind, day),
                        SolarSystemData.Moon.PositionAuAt(day).Length);

                double separation = kind == Kind.Conjunction
                    ? AngleFromEarth(a, b, day)
                    : AngleFromEarth(null, a, day);
                double au = (a.PositionAuAt(day) - Earth.PositionAuAt(day)).Length;
                return new Meeting(day, separation, au);
            }

            f0 = f1;
            f1 = f2;
        }

        return null;
    }

    /// <summary>
    /// Pins down the dip with the golden section search. The angle has no
    /// simple expression to differentiate, so the search just compares
    /// values – enough iterations to get under a minute in time.
    /// </summary>
    static double Refine(Kind kind, CelestialBody a, CelestialBody? b, double lo, double hi)
    {
        const double phi = 0.6180339887498949;
        double c = hi - (hi - lo) * phi, d = lo + (hi - lo) * phi;
        double fc = Cost(kind, a, b, c), fd = Cost(kind, a, b, d);

        for (int k = 0; k < 40 && hi - lo > 1e-4; k++)
        {
            if (fc < fd)
            {
                hi = d; d = c; fd = fc;
                c = hi - (hi - lo) * phi;
                fc = Cost(kind, a, b, c);
            }
            else
            {
                lo = c; c = d; fc = fd;
                d = lo + (hi - lo) * phi;
                fd = Cost(kind, a, b, d);
            }
        }

        return 0.5 * (lo + hi);
    }

    /// <summary>
    /// What the selector offers. The oppositions cover the bodies that can
    /// have one – the ones outside Earth's orbit. Mercury and Venus can never
    /// stand directly opposite the Sun as seen from here; they always stay
    /// close to it in the sky, which is itself worth knowing.
    ///
    /// The conjunctions are the pairs visible to the naked eye, i.e. the
    /// bright planets. Including Neptune would have been pointless.
    /// </summary>
    public static readonly Choice[] Choices = BuildChoices();

    static Choice[] BuildChoices()
    {
        CelestialBody B(string key) => SolarSystemData.Planets.First(p => p.Key == key);
        var list = new List<Choice>();

        foreach (string key in new[]
                 { "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto" })
            list.Add(new Choice(Kind.Opposition, B(key), null));

        (string A, string B)[] pairs =
        [
            ("Venus", "Jupiter"), ("Venus", "Mars"), ("Venus", "Saturn"),
            ("Mars", "Jupiter"), ("Mars", "Saturn"), ("Jupiter", "Saturn"),
        ];
        foreach (var (a, b) in pairs)
            list.Add(new Choice(Kind.Conjunction, B(a), B(b)));

        list.Add(new Choice(Kind.SolarEclipse, SolarSystemData.Moon, null));
        list.Add(new Choice(Kind.LunarEclipse, SolarSystemData.Moon, null));

        // Halley's perihelion. The distance to the Sun is the same every
        // time – 0.586 AU, which is what a perihelion is – so that number
        // says nothing about this particular visit. What differs between
        // visits is where Earth happens to be, which is why distance and
        // elongation are what get reported.
        list.Add(new Choice(Kind.Perihelion, SolarSystemData.Halley, null));

        return [.. list];
    }
}
