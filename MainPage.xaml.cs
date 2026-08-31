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
    DateTime _startDate = DateTime.Now;

    double _panLastX, _panLastY;
    int _focusIndex;                       // 0 = solen, 1.. = planeter

    // Senast ritade tillstånd – när inget ändrats hoppas omritningen över helt.
    float _drawnYaw = float.NaN, _drawnPitch, _drawnDist;
    Vector3 _drawnTarget;
    double _drawnSimDays = double.NaN;
    double _resizeQuietUntil;              // klocktid då renderingen får vakna igen
    bool _settingsChanged = true;

    // Startfönster: varje prövning kräver att en hel bana räknas fram, så
    // resultatet cachas och kontrolleras högst några gånger per sekund.
    double _windowCheckedDay = double.NaN;
    double _windowCheckedAt;
    bool _inLaunchWindow;
    double? _nextWindowDay;
    double _departureSpeedKmS;             // vad uppskjutningen kostar just nu

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

        StarDensityPicker.ItemsSource = new List<string> { "Inga", "Få", "Normalt", "Många" };
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

        UpdateMissionPanel(daysSinceJ2000);
        _drawable.Camera.Target = CameraTarget(daysSinceJ2000);

        DateLabel.Text = simDate.ToString("dddd d MMMM yyyy, HH:mm", Swedish);
        ElapsedLabel.Text = FormatElapsed(_simDays);
        UpdateLaunchWindow(daysSinceJ2000);

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
        // Tiden kan numera gå åt båda hållen, så negativa värden räknas som "tillbaka".
        string label = days < 0 ? "Tillbaka" : "Förflutet";
        double span = Math.Abs(days);
        if (span < 1.0)
            return $"{label}: {span * 24:0.0} timmar";
        int years = (int)(span / 365.25);
        int rest = (int)(span - years * 365.25);
        return years > 0
            ? $"{label}: {years} år, {rest} dagar"
            : $"{label}: {rest} dagar";
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
        // Logaritmisk skala: 0,1 dygn/s upp till 1000 dygn/s. Reglaget går från
        // -1 till 1, där negativa värden spelar tiden baklänges och mitten står still.
        double v = SpeedSlider.Value;
        _daysPerSecond = Math.Abs(v) < 0.02
            ? 0.0
            : Math.Sign(v) * Math.Pow(10, -1 + Math.Abs(v) * 4);
        SpeedLabel.Text = FormatSpeed(_daysPerSecond);
    }

    static string FormatSpeed(double dps)
    {
        if (dps == 0)
            return "stillastående";

        string direction = dps < 0 ? " bakåt" : "";
        double rate = Math.Abs(dps);
        return rate switch
        {
            < 1.0 => string.Create(Swedish, $"{rate * 24:0.#} timmar/sek{direction}"),
            < 365.25 => string.Create(Swedish, $"{rate:0.#} dygn/sek{direction}"),
            _ => string.Create(Swedish, $"{rate / 365.25:0.##} år/sek{direction}"),
        };
    }

    // ------------------------------------------------------------------ datum

    /// <summary>Det datum simuleringen just nu står på.</summary>
    DateTime CurrentDate => _startDate.AddDays(_simDays);

    /// <summary>Flyttar simuleringen till ett givet datum, bakåt eller framåt.</summary>
    void GoToDate(DateTime date)
    {
        _simDays = (date - _startDate).TotalDays;
        _windowCheckedDay = double.NaN;
        _settingsChanged = true;
    }

    void OnGoToDateClicked(object? sender, EventArgs e) => ApplyTypedDate();

    void OnDateEntryCompleted(object? sender, EventArgs e) => ApplyTypedDate();

    /// <summary>
    /// Läser datumfältet och hoppar dit. Accepterar både "2026-09-06" och andra
    /// format som svensk kultur förstår; vid felskrivning händer ingenting mer än
    /// att fältet markeras.
    /// </summary>
    void ApplyTypedDate()
    {
        string text = DateEntry.Text?.Trim() ?? string.Empty;
        if (DateTime.TryParse(text, Swedish, DateTimeStyles.None, out var date))
        {
            DateEntry.TextColor = Colors.White;
            GoToDate(date);
        }
        else
        {
            DateEntry.TextColor = Color.FromArgb("#E8927C");
        }
    }

    /// <summary>Stegar datumet en dag, månad eller ett år i taget.</summary>
    void OnStepDateClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: string step })
            return;

        var date = CurrentDate;
        GoToDate(step switch
        {
            "y-" => date.AddYears(-1),
            "y+" => date.AddYears(1),
            "m-" => date.AddMonths(-1),
            "m+" => date.AddMonths(1),
            "d-" => date.AddDays(-1),
            "d+" => date.AddDays(1),
            _ => date,
        });
    }

    /// <summary>Återställer klockan till nuet.</summary>
    void OnTodayClicked(object? sender, EventArgs e)
    {
        _startDate = DateTime.Now;
        _simDays = 0;
        _settingsChanged = true;
    }

    // --------------------------------------------------------------- rymdfärd

    // MAUI gråar inte själv en knapp som fått en egen bakgrundsfärg, så de
    // avstängda lägena målas om för hand.
    static readonly Color LaunchReady = Color.FromArgb("#2E5A4A");
    static readonly Color LaunchClosed = Color.FromArgb("#222A28");
    static readonly Color StepReady = Color.FromArgb("#26303C");
    static readonly Color StepClosed = Color.FromArgb("#1B2028");

    static readonly CelestialBody EarthBody =
        SolarSystemData.Planets.First(p => p.Name == "Jorden");
    static readonly CelestialBody MarsBody =
        SolarSystemData.Planets.First(p => p.Name == "Mars");
    static readonly CelestialBody MoonBody = SolarSystemData.Moon;

    /// <summary>Jordens plats i fokusväljaren, som har solen först.</summary>
    static readonly int EarthFocusIndex =
        Array.FindIndex(SolarSystemData.Planets, p => p.Name == "Jorden") + 1;

    /// <summary>
    /// Skjuter upp en farkost mot Mars från det datum vyn står på, eller avbryter
    /// en pågående färd.
    /// </summary>
    /// <summary>
    /// Håller reda på om ett startfönster är öppet just nu och när nästa infaller.
    /// Kontrollen är dyr – varje prövning räknar fram en hel överföringsbana –
    /// så den görs bara när datumet flyttat sig märkbart, och högst fyra gånger
    /// per sekund oavsett hur snabbt tiden spolas.
    /// </summary>
    void UpdateLaunchWindow(double day)
    {
        double realNow = _clock.Elapsed.TotalSeconds;
        bool dayMoved = double.IsNaN(_windowCheckedDay) || Math.Abs(day - _windowCheckedDay) >= 0.5;
        if (!dayMoved || realNow - _windowCheckedAt < 0.25)
            return;

        _windowCheckedDay = day;
        _windowCheckedAt = realNow;

        _inLaunchWindow = Mission.IsLaunchWindow(EarthBody, MarsBody, day);
        if (_inLaunchWindow)
        {
            _nextWindowDay = null;
            _departureSpeedKmS = Mission.CheapestDeparture(EarthBody, MarsBody, day).SpeedKmS;
        }
        else if (_nextWindowDay is not double cached || day > cached || day < cached - 900)
        {
            _nextWindowDay = Mission.NextLaunchWindow(EarthBody, MarsBody, day);
        }

        UpdateMissionUi(day);
    }

    /// <summary>Uppdaterar knappar och statustext efter färdens och fönstrets läge.</summary>
    void UpdateMissionUi(double day)
    {
        if (_drawable.Mission is { } mission)
        {
            // En färd i taget: knappen för den pågående färden avbryter den, och
            // den andra är avstängd så länge.
            bool toMoon = ReferenceEquals(mission.Target, MoonBody);

            LaunchButton.Text = toMoon ? "Skjut upp mot Mars" : "Avbryt färden";
            LaunchButton.IsEnabled = !toMoon;
            LaunchButton.BackgroundColor = toMoon ? LaunchClosed : LaunchReady;
            MoonButton.Text = toMoon ? "Avbryt färden" : "Skjut upp mot Månen";
            MoonButton.IsEnabled = toMoon;
            MoonButton.BackgroundColor = toMoon ? LaunchReady : LaunchClosed;
            NextWindowButton.IsEnabled = false;
            NextWindowButton.BackgroundColor = StepClosed;

            // Restid, avstånd och fart står i färdpanelen i stället.
            MissionLabel.Text = string.Empty;
            return;
        }

        LaunchButton.Text = "Skjut upp mot Mars";
        LaunchButton.IsEnabled = _inLaunchWindow;
        LaunchButton.BackgroundColor = _inLaunchWindow ? LaunchReady : LaunchClosed;

        // Månen är tillbaka på samma ställe var 27:e dygn, så dit går det att åka
        // i stort sett vilken dag som helst – där behövs inga startfönster.
        MoonButton.Text = "Skjut upp mot Månen";
        MoonButton.IsEnabled = true;
        MoonButton.BackgroundColor = LaunchReady;

        NextWindowButton.IsEnabled = _nextWindowDay is not null;
        NextWindowButton.BackgroundColor = _nextWindowDay is not null ? StepReady : StepClosed;

        if (_inLaunchWindow)
        {
            // Farten i förhållande till jorden är måttet på hur stor raket som
            // behövs, och det är den som avgör om fönstret räknas som öppet.
            MissionLabel.Text = string.Create(Swedish,
                $"Startfönstret är öppet – uppskjutningen kräver {_departureSpeedKmS:0.0} km/s");
        }
        else if (_nextWindowDay is double next)
        {
            var date = SolarSystemData.EpochJ2000.AddDays(next);
            MissionLabel.Text = string.Create(Swedish,
                $"Stängt – nästa fönster om {next - day:0} dygn ({date:yyyy-MM-dd})");
        }
        else
        {
            MissionLabel.Text = "Stängt";
        }
    }

    // Kameran ska följa med farkosten ner till målet när den kommer fram. Det
    // sker en gång, i själva ankomstögonblicket – därefter får användaren styra
    // fritt igen, och ett nytt val i fokusväljaren stänger av följandet.
    bool _arrivalSeen;
    bool _followCraft;

    /// <summary>Vad kameran tittar på: farkosten efter ankomsten, annars vald kropp.</summary>
    Vector3 CameraTarget(double day)
    {
        if (_followCraft && _drawable.CraftPosition() is { } craft)
            return craft;

        return _focusIndex <= 0
            ? Vector3.Zero
            : SolarSystemData.Planets[_focusIndex - 1]
                .PositionAt(day, SolarSystemDrawable.UnitsPerAu);
    }

    /// <summary>
    /// Panelen som följer färden: hur länge farkosten varit i väg, hur länge det
    /// är kvar, hur långt den har till målet och hur fort den går. Farten är den
    /// intressanta raden. Den faller med avståndet, precis som Keplers andra lag
    /// säger: mot månen från 10,8 km/s vid uppskjutningen till under 1 km/s vid
    /// framkomsten, mot Mars från 33 till 21 km/s.
    /// </summary>
    void UpdateMissionPanel(double day)
    {
        if (_drawable.Mission is not { } mission)
        {
            MissionPanel.IsVisible = false;
            _arrivalSeen = false;
            return;
        }

        MissionPanel.IsVisible = true;

        // Ankomsten: följ med ner till målet, en gång.
        bool arrived = mission.HasArrived(day);
        if (arrived && !_arrivalSeen)
        {
            _followCraft = true;
            _drawable.Camera.Distance = _drawable.SuggestedFocusDistance(mission.Target);
        }
        _arrivalSeen = arrived;

        var arrival = SolarSystemData.EpochJ2000.AddDays(mission.ArrivalDay);
        double speed = mission.SpeedKmPerSecond(day);

        if (arrived)
        {
            CraftTitleLabel.Text = $"Farkost framme vid {mission.Target.Name}";
            CraftElapsedLabel.Text = $"Restid: {FormatTravelTime(mission.TravelDays)}";
            CraftRemainingLabel.Text = string.Create(Swedish, $"Framme {arrival:yyyy-MM-dd}");
            CraftDistanceLabel.Text = $"Följer nu med {mission.Target.Name}";
            CraftSpeedLabel.Text = string.Create(Swedish, $"Fart vid ankomsten: {speed:0.00} km/s");
            return;
        }

        double elapsed = Math.Max(0.0, day - mission.LaunchDay);
        CraftTitleLabel.Text = $"Farkost mot {mission.Target.Name}";
        CraftElapsedLabel.Text = $"Förfluten restid: {FormatTravelTime(elapsed)}";
        CraftRemainingLabel.Text = string.Create(Swedish,
            $"Återstår: {FormatTravelTime(mission.TravelDays - elapsed)} (framme {arrival:yyyy-MM-dd})");
        CraftDistanceLabel.Text =
            $"Avstånd till {mission.Target.Name}: {FormatDistance(mission.DistanceToTargetKm(day))}";
        CraftSpeedLabel.Text = string.Create(Swedish, $"Fart: {speed:0.00} km/s");
    }

    /// <summary>
    /// Restider skrivs i timmar när de är korta – en månfärd tar bara tre dygn,
    /// och då säger "0,4 dygn" mindre än "9,6 timmar".
    /// </summary>
    static string FormatTravelTime(double days)
    {
        double span = Math.Max(0.0, days);
        return span < 2.0
            ? string.Create(Swedish, $"{span * 24:0.0} timmar")
            : string.Create(Swedish, $"{span:0.0} dygn");
    }

    /// <summary>Avstånd i kilometer, i miljoner när talen blir för långa att läsa.</summary>
    static string FormatDistance(double km)
        => km >= 1e6
            ? string.Create(Swedish, $"{km / 1e6:N1} miljoner km")
            : string.Create(Swedish, $"{km:N0} km");

    /// <summary>Hoppar fram till nästa uppskjutningstillfälle.</summary>
    void OnNextWindowClicked(object? sender, EventArgs e)
    {
        if (_nextWindowDay is not double next)
            return;
        GoToDate(SolarSystemData.EpochJ2000.AddDays(next));
    }

    void OnLaunchClicked(object? sender, EventArgs e)
    {
        if (_drawable.Mission is not null)
        {
            CancelMission();
            return;
        }

        double launchDay = (CurrentDate - SolarSystemData.EpochJ2000).TotalDays;
        var mission = Mission.Plan("Farkost", EarthBody, MarsBody, launchDay);
        if (mission is null)
        {
            MissionLabel.Text = "Ingen bana gick att räkna fram just nu";
            return;
        }

        _drawable.Mission = mission;
        StartMission(launchDay);
    }

    /// <summary>
    /// Skjuter upp en farkost mot månen från det datum vyn står på, eller
    /// avbryter en pågående månfärd. Vyn flyttas samtidigt till jorden: hela
    /// månfärden ryms inom 0,003 AU, alltså under en femtedels pixel i
    /// översiktsvyn, så utan inzoomning vore det ingenting att se.
    /// </summary>
    void OnMoonLaunchClicked(object? sender, EventArgs e)
    {
        if (_drawable.Mission is not null)
        {
            CancelMission();
            return;
        }

        double launchDay = (CurrentDate - SolarSystemData.EpochJ2000).TotalDays;
        var mission = Mission.PlanToMoon("Farkost", EarthBody, MoonBody, launchDay);
        if (mission is null)
        {
            MissionLabel.Text = "Ingen bana gick att räkna fram just nu";
            return;
        }

        _drawable.Mission = mission;
        FocusPicker.SelectedIndex = EarthFocusIndex;    // zoomar in via OnFocusChanged
        StartMission(launchDay);
    }

    /// <summary>Gemensamt för båda uppskjutningarna: färden börjar om från början.</summary>
    void StartMission(double launchDay)
    {
        _arrivalSeen = false;
        _followCraft = false;
        UpdateMissionUi(launchDay);
        _settingsChanged = true;
    }

    /// <summary>Avbryter färden och tvingar fram en ny koll av startfönstret.</summary>
    void CancelMission()
    {
        _drawable.Mission = null;
        _windowCheckedDay = double.NaN;
        _arrivalSeen = false;
        _followCraft = false;
        _settingsChanged = true;
    }

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

    void OnKuiperChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowKuiperBelt = e.Value;
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
        _followCraft = false;
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
        _followCraft = false;
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
