namespace Solarsystem.Simulation;

/// <summary>
/// Keplers ekvation, i sina två former.
///
/// Planeter, månar och överföringsbanor går i ellipser, och där gäller
/// E - e·sin E = M. Rymdsonderna fick vid planetpassagerna så hög fart att de
/// aldrig kommer tillbaka: deras banor är hyperbler, och då gäller i stället
/// e·sinh H - H = M. Det är samma ekvation med cirkelfunktionerna utbytta mot
/// hyperbolfunktioner, och den excentriska anomalin E har bytts mot sin
/// hyperboliska motsvarighet H.
///
/// Ingen av dem går att lösa ut för hand – det var just det som gjorde Keplers
/// ekvation berömd – så båda löses numeriskt.
/// </summary>
public static class Kepler
{
    /// <summary>
    /// Elliptiska fallet, löst med intervallhalvering. Överföringsbanor kan ha
    /// excentricitet uppåt 0,97, och där kan Newtons metod skena i väg eftersom
    /// nämnaren 1 - e·cos E går mot noll nära perihelium. Intervallhalvering tar
    /// fler varv men kan inte missa: lösningen ligger alltid mellan M och M + e.
    /// </summary>
    public static double Elliptic(double meanAnomaly, double e)
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

    /// <summary>
    /// Hyperboliska fallet: e·sinh H - H = M. Här fungerar Newtons metod bra,
    /// för derivatan e·cosh H - 1 är alltid minst e - 1 och kan aldrig gå mot
    /// noll när e är större än 1.
    ///
    /// Startgissningen är arsinh(M/e), och den är vald med omsorg. Eftersom
    /// e·sinh H = M + H är sinh H alltid större än M/e, så gissningen ligger
    /// garanterat under svaret – och funktionen är växande och konvex, vilket
    /// gör att Newton då närmar sig lösningen underifrån utan att någonsin
    /// skjuta över. Den uppenbara gissningen M/(e - 1) fungerar inte: för banor
    /// nära parabeln, där e är strax över 1, blir den enorm, och sinh av ett
    /// stort tal spränger flyttalen direkt.
    /// </summary>
    public static double Hyperbolic(double meanAnomaly, double e)
    {
        double m = Math.Abs(meanAnomaly);
        double h = Math.Asinh(m / e);

        for (int k = 0; k < 60; k++)
        {
            double step = (e * Math.Sinh(h) - h - m) / (e * Math.Cosh(h) - 1.0);
            if (double.IsNaN(step) || double.IsInfinity(step))
                break;
            h -= step;
            if (Math.Abs(step) < 1e-13)
                break;
        }

        // Ekvationen är udda: H(-M) = -H(M). Före perihelium är alltså allt
        // spegelvänt, vilket är precis vad som behövs för att kunna räkna
        // bakåt i tiden.
        return meanAnomaly < 0 ? -h : h;
    }
}
