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
			// Kraschloggning får aldrig själv orsaka fel.
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell())
		{
			Title = "Solsystemet i 3D",
			Width = 1400,
			Height = 900,
		};
	}
}