using Microsoft.Extensions.DependencyInjection;

namespace Solarsystem;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
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