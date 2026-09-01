namespace Solarsystem.Simulation;

/// <summary>
/// Enkel felskrivning till en fil i temp-katalogen.
///
/// Appen har inget fönster för fel och ska inte ha det heller – en lärare mitt
/// i en lektion är inte betjänt av en dialogruta. Men fel som annars skulle
/// passera obemärkta ska gå att hitta i efterhand, och de som upptäcks utanför
/// ritningen ska hamna på samma ställe som ritfelen.
/// </summary>
public static class Diagnostics
{
    /// <summary>Loggfilen. Ligger i temp-katalogen och skapas först när något skrivs.</summary>
    public static string LogPath
        => Path.Combine(Path.GetTempPath(), "solarsystem-draw.log");

    public static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
        }
        catch
        {
            // Går loggen inte att skriva finns inget att göra åt saken, och
            // försöket får framför allt inte fälla det som höll på att
            // rapporteras.
        }
    }
}
