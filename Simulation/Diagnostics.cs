namespace Solarsystem.Simulation;

/// <summary>
/// Simple error logging to a file in the temp directory.
///
/// The app has no error window, and shouldn't: a teacher in the middle of a
/// lesson gets no benefit from a dialog box. But errors that would otherwise
/// pass unnoticed should be findable afterward, and ones caught outside the
/// drawing code should end up in the same place as drawing errors.
/// </summary>
public static class Diagnostics
{
    /// <summary>The log file. Lives in the temp directory and is created only when something is written.</summary>
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
            // If the log itself can't be written there's nothing to do about
            // it, and above all the attempt must not take down whatever was
            // being reported.
        }
    }
}
