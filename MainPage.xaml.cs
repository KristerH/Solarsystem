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

    // Senast ritade tillstånd – när inget ändrats hoppas omritningen över helt.
    float _drawnYaw = float.NaN, _drawnPitch, _drawnDist;
    Vector3 _drawnTarget;
    double _drawnSimDays = double.NaN;
    double _resizeQuietUntil;              // klocktid då renderingen får vakna igen
    bool _settingsChanged = true;

    public MainPage()
    {
        InitializeComponent();

        SpaceView.Drawable = _drawable;

        // Pausa renderingen medan fönstret ändrar storlek. Varje storlekssteg
        // hade annars tvingat fram ombyggda cachar och en full omritning, i takt
        // med att fönsterhanteraren väntar – det var det som frös hela systemet.
        // 300 ms efter sista steget vaknar renderingen och räknar om allt en gång.
        SpaceView.SizeChanged += (_, _) =>
        {
            _drawable.Suspended = true;
            _resizeQuietUntil = _clock.Elapsed.TotalSeconds + 0.3;
        };

        var names = new List<string> { "Solen" };
        names.AddRange(SolarSystemData.Planets.Select(p => p.Name));
        FocusPicker.ItemsSource = names;
        FocusPicker.SelectedIndex = 0;

        StarDensityPicker.ItemsSource = new List<string> { "Få", "Normalt", "Många" };
        StarDensityPicker.SelectedIndex = (int)_drawable.StarDensity;

        UpdateSpeedFromSlider();
        Loaded += OnPageLoaded;
    }

    void OnPageLoaded(object? sender, EventArgs e)
    {
        if (_timer is not null)
            return;

        HookPlatformInput();

        // 30 bildrutor/sekund räcker gott och halverar CPU-lasten mot 60.
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(33);
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

        if (_drawable.Suspended)
        {
            if (now < _resizeQuietUntil)
                return; // storleksändring pågår – varken simulera eller rita
            _drawable.Suspended = false;
            _settingsChanged = true;
        }

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

        // Rita bara om när något faktiskt har ändrats (tiden gått framåt eller
        // kameran flyttats). Pausad och stillastående vy kostar då nästan inget.
        var cam = _drawable.Camera;
        if (_simDays != _drawnSimDays || cam.Yaw != _drawnYaw ||
            cam.Pitch != _drawnPitch || cam.Distance != _drawnDist ||
            cam.Target != _drawnTarget || _settingsChanged)
        {
            _drawnSimDays = _simDays;
            _drawnYaw = cam.Yaw;
            _drawnPitch = cam.Pitch;
            _drawnDist = cam.Distance;
            _drawnTarget = cam.Target;
            _settingsChanged = false;
            SpaceView.Invalidate();
        }
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

    void OnOrbitsChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowOrbits = e.Value;
        _settingsChanged = true;
    }

    void OnMoonsChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowMoons = e.Value;
        _settingsChanged = true;
    }

    void OnRealScaleChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.RealScale = e.Value;
        _settingsChanged = true;
    }

    void OnAsteroidsChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowAsteroidBelt = e.Value;
        _settingsChanged = true;
    }

    void OnConstellationsChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowConstellations = e.Value;
        _settingsChanged = true;
    }

    void OnStarNamesChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowStarNames = e.Value;
        _settingsChanged = true;
    }

    void OnStarDensityChanged(object? sender, EventArgs e)
    {
        _drawable.StarDensity = (StarDensity)Math.Max(0, StarDensityPicker.SelectedIndex);
        _settingsChanged = true;
    }

    void OnFocusChanged(object? sender, EventArgs e)
    {
        _focusIndex = Math.Max(0, FocusPicker.SelectedIndex);
        if (_focusIndex > 0)
        {
            // Zooma så att planeten – och hela dess månsystem – syns.
            var planet = SolarSystemData.Planets[_focusIndex - 1];
            _drawable.Camera.Distance = _drawable.SuggestedFocusDistance(planet);
        }
        _settingsChanged = true;
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
