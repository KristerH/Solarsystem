using Microsoft.Extensions.DependencyInjection;

namespace Solarsystem;

public partial class App : Application
{
	public App()
	{
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
			LogCrash(e.ExceptionObject as Exception);
		TaskScheduler.UnobservedTaskException += (_, e) => LogCrash(e.Exception);
		InitializeComponent();
	}

	static void LogCrash(Exception? ex)
	{
		try
		{
			File.AppendAllText(
				Path.Combine(FileSystem.AppDataDirectory, "crash.log"),
				$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n");
		}
		catch
		{
			// Crash logging must never itself cause a failure.
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell())
		{
			// The title is reset once the page has loaded, and again on every language switch.
			Title = Strings.WindowTitle,
			Width = 1400,
			Height = 900,
		};
	}
}