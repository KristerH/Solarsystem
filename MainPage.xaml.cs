using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Solarsystem.Rendering;
using Solarsystem.Simulation;

namespace Solarsystem;

public partial class MainPage : ContentPage
{
    static readonly CultureInfo Swedish = new("sv-SE");

    readonly SolarSystemDrawable _drawable = new();
    readonly Stopwatch _clock = Stopwatch.StartNew();
    IDispatcherTimer? _timer;

    double _lastSeconds;
    bool _running = true;
    double _simDays;                       // simulerade dygn sedan start
    double _daysPerSecond = 30;
    readonly DateTime _startDate = DateTime.Now;

    double _panLastX, _panLastY;
    int _focusIndex;                       // 0 = solen, 1.. = planeter

    public MainPage()
    {
        InitializeComponent();

        SpaceView.Drawable = _drawable;

        var names = new List<string> { "Solen" };
        names.AddRange(SolarSystemData.Planets.Select(p => p.Name));
        FocusPicker.ItemsSource = names;
        FocusPicker.SelectedIndex = 0;

        UpdateSpeedFromSlider();
        Loaded += OnPageLoaded;
    }

    void OnPageLoaded(object? sender, EventArgs e)
    {
        if (_timer is not null)
            return;

        HookPlatformInput();

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnTick;
        _lastSeconds = _clock.Elapsed.TotalSeconds;
        _timer.Start();
    }

    // ------------------------------------------------------------- simulering

    void OnTick(object? sender, EventArgs e)
    {
        double now = _clock.Elapsed.TotalSeconds;
        double dt = now - _lastSeconds;
        _lastSeconds = now;

        if (_running)
            _simDays += dt * _daysPerSecond;

        var simDate = _startDate.AddDays(_simDays);
        double daysSinceJ2000 = (simDate - SolarSystemData.EpochJ2000).TotalDays;
        _drawable.DaysSinceJ2000 = daysSinceJ2000;

        // Följ vald himlakropp med kameran.
        _drawable.Camera.Target = _focusIndex <= 0
            ? Vector3.Zero
            : SolarSystemData.Planets[_focusIndex - 1]
                .PositionAt(daysSinceJ2000, SolarSystemDrawable.UnitsPerAu);

        DateLabel.Text = simDate.ToString("dddd d MMMM yyyy, HH:mm", Swedish);
        ElapsedLabel.Text = FormatElapsed(_simDays);

        SpaceView.Invalidate();
    }

    static string FormatElapsed(double days)
    {
        if (days < 1.0)
            return $"Förflutet: {days * 24:0.0} timmar";
        int years = (int)(days / 365.25);
        int rest = (int)(days - years * 365.25);
        return years > 0
            ? $"Förflutet: {years} år, {rest} dagar"
            : $"Förflutet: {rest} dagar";
    }

    // ---------------------------------------------------------------- reglage

    void OnStartStopClicked(object? sender, EventArgs e) => ToggleRunning();

    void ToggleRunning()
    {
        _running = !_running;
        StartStopButton.Text = _running ? "⏸ Pausa" : "▶ Starta";
    }

    void OnSpeedChanged(object? sender, ValueChangedEventArgs e) => UpdateSpeedFromSlider();

    void UpdateSpeedFromSlider()
    {
        // Logaritmisk skala: 0,1 dygn/s upp till 1000 dygn/s.
        _daysPerSecond = Math.Pow(10, -1 + SpeedSlider.Value * 4);
        SpeedLabel.Text = FormatSpeed(_daysPerSecond);
    }

    static string FormatSpeed(double dps) => dps switch
    {
        < 1.0 => string.Create(Swedish, $"{dps * 24:0.#} timmar/sek"),
        < 365.25 => string.Create(Swedish, $"{dps:0.#} dygn/sek"),
        _ => string.Create(Swedish, $"{dps / 365.25:0.##} år/sek"),
    };

    void OnOrbitsChanged(object? sender, CheckedChangedEventArgs e) =>
        _drawable.ShowOrbits = e.Value;

    void OnRealScaleChanged(object? sender, CheckedChangedEventArgs e) =>
        _drawable.RealScale = e.Value;

    void OnConstellationsChanged(object? sender, CheckedChangedEventArgs e) =>
        _drawable.ShowConstellations = e.Value;

    void OnStarNamesChanged(object? sender, CheckedChangedEventArgs e) =>
        _drawable.ShowStarNames = e.Value;

    void OnFocusChanged(object? sender, EventArgs e)
    {
        _focusIndex = Math.Max(0, FocusPicker.SelectedIndex);
        if (_focusIndex > 0)
        {
            // Zooma in lagom nära planeten när den väljs.
            var planet = SolarSystemData.Planets[_focusIndex - 1];
            float visualR = (float)(planet.RadiusKm / SolarSystemData.AuKm)
                            * SolarSystemDrawable.UnitsPerAu * 1000f;
            _drawable.Camera.Distance = Math.Min(_drawable.Camera.Distance,
                Math.Max(visualR * 12f, 8f));
        }
    }

    void OnResetClicked(object? sender, EventArgs e) => ResetView();

    void ResetView()
    {
        _drawable.Camera.ResetView();
        _focusIndex = 0;
        FocusPicker.SelectedIndex = 0;
    }

    // ------------------------------------------------------------ mus & gester

    void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panLastX = _panLastY = 0;
                break;
            case GestureStatus.Running:
                double dx = e.TotalX - _panLastX;
                double dy = e.TotalY - _panLastY;
                _panLastX = e.TotalX;
                _panLastY = e.TotalY;
                _drawable.Camera.Rotate(-(float)(dx * 0.006), (float)(dy * 0.006));
                break;
        }
    }

    void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Running && e.Scale > 0)
            _drawable.Camera.ZoomBy(1f / (float)e.Scale);
    }

    // --------------------------------------------- tangentbord & skrollhjul

    void HookPlatformInput()
    {
#if WINDOWS
        if (SpaceView.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement view)
        {
            view.PointerWheelChanged += (s, e) =>
            {
                int delta = e.GetCurrentPoint(view).Properties.MouseWheelDelta;
                _drawable.Camera.ZoomBy(delta > 0 ? 0.86f : 1.16f);
                e.Handled = true;
            };
        }

        var root = (Window?.Handler?.PlatformView as Microsoft.UI.Xaml.Window)?.Content;
        root?.AddHandler(
            Microsoft.UI.Xaml.UIElement.KeyDownEvent,
            new Microsoft.UI.Xaml.Input.KeyEventHandler(OnWindowsKeyDown),
            handledEventsToo: true);
#endif
    }

#if WINDOWS
    void OnWindowsKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        const float step = 0.07f;
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Left:
                _drawable.Camera.Rotate(step, 0);
                break;
            case Windows.System.VirtualKey.Right:
                _drawable.Camera.Rotate(-step, 0);
                break;
            case Windows.System.VirtualKey.Up:
                _drawable.Camera.Rotate(0, step);
                break;
            case Windows.System.VirtualKey.Down:
                _drawable.Camera.Rotate(0, -step);
                break;
            case Windows.System.VirtualKey.W:
            case Windows.System.VirtualKey.Add:
            case (Windows.System.VirtualKey)187: // "+" på huvudtangentbordet
                _drawable.Camera.ZoomBy(0.86f);
                break;
            case Windows.System.VirtualKey.S:
            case Windows.System.VirtualKey.Subtract:
            case (Windows.System.VirtualKey)189: // "-" på huvudtangentbordet
                _drawable.Camera.ZoomBy(1.16f);
                break;
            case Windows.System.VirtualKey.Space:
                ToggleRunning();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.R:
                ResetView();
                break;
        }
    }
#endif
}
