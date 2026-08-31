using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// Lamberts problem: vilken bana går från ett läge till ett annat på exakt en
/// given tid?
///
/// Det är den frågan som gör att rymdsonderna kan byggas ur verkliga datum.
/// Voyager 1 sköts upp 5 september 1977 och var vid Jupiter 5 mars 1979. Var
/// jorden och Jupiter stod de dagarna vet appen redan, och restiden är känd –
/// alltså är banan bestämd, utan att ett enda banelement behöver matas in.
///
/// Lösningen använder Bate, Mueller och Whites universella variabler. Tanken är
/// att beskriva alla kägelsnitt med samma formler genom variabeln z, som är
/// positiv för ellipser, negativ för hyperbler och noll för parabeln mitt
/// emellan. Restiden växer monotont med z, så rätt bana går att klämma in med
/// intervallhalvering.
///
/// Metoden hanterar färder på mindre än ett varv, vilket räcker för alla
/// sondben i appen.
/// </summary>
public static class Lambert
{
    /// <summary>
    /// Löser banan mellan två lägen (AU) på given tid (dygn). Hastigheterna
    /// returneras i AU/dygn vid start och vid ankomst. Falskt när ingen bana
    /// hittas – till exempel när lägena ligger rakt mitt emot varandra sett från
    /// centrum, där banplanet inte går att bestämma.
    /// </summary>
    public static bool Solve(Vector3 from, Vector3 to, double travelDays, double mu,
        out Vector3 departureVelocity, out Vector3 arrivalVelocity)
    {
        departureVelocity = arrivalVelocity = default;

        double r1 = from.Length(), r2 = to.Length();
        if (r1 < 1e-12 || r2 < 1e-12 || travelDays <= 0 || mu <= 0)
            return false;

        double cosSweep = Math.Clamp(Vector3.Dot(from, to) / (r1 * r2), -1.0, 1.0);
        if (1.0 - cosSweep < 1e-12)
            return false;                       // samma riktning: ingen bana

        double sweep = Math.Acos(cosSweep);

        // Alla sonder går prograd, alltså moturs sett norrifrån. I appens
        // koordinater pekar Y norrut, så ett moturs varv har r1×r2 uppåt. Är den
        // nedåt har färden svept mer än ett halvt varv – den "långa vägen".
        if (Vector3.Cross(from, to).Y < 0)
            sweep = Math.PI * 2 - sweep;

        double a = Math.Sin(sweep) * Math.Sqrt(r1 * r2 / (1.0 - cosSweep));
        if (Math.Abs(a) < 1e-14)
            return false;

        // Klämma in z. Restiden växer monotont med z: långt ned i det negativa
        // ligger de snabba hyperblerna, vid noll parabeln, och uppåt allt
        // långsammare ellipser, ända tills restiden går mot oändligheten vid
        // ett helt varv.
        //
        // Under en viss gräns finns ingen bana alls – kägelsnittet når helt
        // enkelt inte fram, vilket y avslöjar genom att bli negativ. Restiden
        // går mot noll när den gränsen närmas underifrån, så de omöjliga z:na
        // räknas som "snabbare än allt annat". Då förblir sökningen monoton och
        // intervallhalveringen kan inte fastna i det omöjliga området.
        double lo = -4096.0, hi = 4.0 * Math.PI * Math.PI - 1e-9;
        // 60 halveringar krymper intervallet från fyra tusen till långt under
        // vad flyttalen kan skilja på; fler varv skulle bara kosta tid, och den
        // här sökningen körs hundratals gånger när ett startfönster ska hittas.
        for (int k = 0; k < 60; k++)
        {
            double mid = 0.5 * (lo + hi);
            if ((TimeOfFlight(mid, a, r1, r2, mu) ?? 0.0) < travelDays)
                lo = mid;
            else
                hi = mid;
        }

        double z = 0.5 * (lo + hi);
        if (YOf(z, a, r1, r2) is not { } y || y <= 0)
            return false;

        // Lagranges f och g: de knyter ihop de två lägena med hastigheterna.
        double f = 1.0 - y / r1;
        double g = a * Math.Sqrt(y / mu);
        double gDot = 1.0 - y / r2;
        if (Math.Abs(g) < 1e-14)
            return false;

        // Subtraktionerna nedan tar bort nästan lika stora tal, och då spelar
        // varje siffra roll: räknas de i float blir hastigheten så pass fel att
        // en sond missar Jupiter med ett par planetradier efter två års färd.
        departureVelocity = Combine(to, 1.0 / g, from, -f / g);
        arrivalVelocity = Combine(to, gDot / g, from, -1.0 / g);
        return true;
    }

    /// <summary>Räknar ut u·a + v·b komponentvis i dubbel precision.</summary>
    static Vector3 Combine(Vector3 a, double u, Vector3 b, double v) => new(
        (float)(a.X * u + b.X * v),
        (float)(a.Y * u + b.Y * v),
        (float)(a.Z * u + b.Z * v));

    /// <summary>
    /// Hjälpstorheten y: hur långt kägelsnittet sträcker sig för ett givet z.
    /// Negativ y betyder att banan inte når fram, och då finns ingen restid.
    /// </summary>
    static double? YOf(double z, double a, double r1, double r2)
    {
        double c = StumpffC(z);
        if (c <= 0)
            return null;

        double y = r1 + r2 + a * (z * StumpffS(z) - 1.0) / Math.Sqrt(c);
        return y > 0 ? y : null;
    }

    /// <summary>Restiden i dygn för ett givet z, eller null när banan inte når fram.</summary>
    static double? TimeOfFlight(double z, double a, double r1, double r2, double mu)
    {
        if (YOf(z, a, r1, r2) is not { } y)
            return null;

        double x = Math.Sqrt(y / StumpffC(z));
        return (x * x * x * StumpffS(z) + a * Math.Sqrt(y)) / Math.Sqrt(mu);
    }

    // Stumpffs funktioner C och S. De är samma serieutveckling hela vägen, men
    // skrivs med cosinus och sinus för ellipser (z > 0), med hyperbolfunktioner
    // för hyperbler (z < 0) och som serie nära noll, där båda formerna annars
    // blir noll delat med noll.
    static double StumpffC(double z)
    {
        if (z > 1e-6)
        {
            double s = Math.Sqrt(z);
            return (1.0 - Math.Cos(s)) / z;
        }
        if (z < -1e-6)
        {
            double s = Math.Sqrt(-z);
            return (Math.Cosh(s) - 1.0) / -z;
        }
        return 0.5 - z / 24.0 + z * z / 720.0;
    }

    static double StumpffS(double z)
    {
        if (z > 1e-6)
        {
            double s = Math.Sqrt(z);
            return (s - Math.Sin(s)) / (s * s * s);
        }
        if (z < -1e-6)
        {
            double s = Math.Sqrt(-z);
            return (Math.Sinh(s) - s) / (s * s * s);
        }
        return 1.0 / 6.0 - z / 120.0 + z * z / 5040.0;
    }
}
