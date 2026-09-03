using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Solarsystem.Rendering;
using Solarsystem.Simulation;

namespace Solarsystem;

public partial class MainPage : ContentPage
{
    /// <summary>
    /// Språket operativsystemet står på, avläst innan appen hunnit byta något.
    /// Det är utgångsläget, och det väljaren återgår till med "Följ systemet".
    /// Står datorn på ett språk appen inte har faller texterna tillbaka på
    /// engelska av sig själva – så fungerar .NET:s resurshantering – medan
    /// datum och tal ändå följer datorns egna vanor.
    /// </summary>
    static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

    static readonly CultureInfo Swedish = new("sv-SE");
    static readonly CultureInfo English = new("en-US");

    /// <summary>Språkväljarens rader. Null betyder "följ systemet".</summary>
    static readonly CultureInfo?[] Languages = [null, Swedish, English];

    /// <summary>
    /// Sant medan väljarnas innehåll byggs om vid ett språkbyte, så att de
    /// byten som sker på vägen inte tolkas som något användaren gjort.
    /// </summary>
    bool _rebuilding;

    readonly SolarSystemDrawable _drawable = new();
    readonly Stopwatch _clock = Stopwatch.StartNew();
    IDispatcherTimer? _timer;

    double _lastSeconds;
    bool _running = true;
    double _simDays;                       // simulerade dygn sedan start
    double _daysPerSecond = 30;
    DateTime _startDate = DateTime.Now;

    double _panLastX, _panLastY;
    int _focusIndex;                       // 0 = solen, sedan kroppar, sist sonder

    /// <summary>Månarna dras in ett steg i fokusväljaren så att de hör ihop med sin planet.</summary>
    const string MoonEntry = "\u00b7 ";

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

        BuildProbeMenu();

        // Följ datorns språk tills någon väljer något annat.
        Strings.Use(SystemCulture);
        StarDensityPicker.SelectedIndex = (int)_drawable.StarDensity;
        LanguagePicker.SelectedIndex = 0;
        ApplyLanguage();

        Loaded += OnPageLoaded;
    }

    void OnPageLoaded(object? sender, EventArgs e)
    {
        if (_timer is not null)
            return;

        HookPlatformInput();

        // Fönstret finns först nu, så titeln kan inte sättas i konstruktorn.
        if (Window is not null)
            Window.Title = Strings.WindowTitle;

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
        UpdateProbePanel(daysSinceJ2000);
        _drawable.Camera.Target = CameraTarget(daysSinceJ2000);
        _drawable.Camera.MinDistance = FocusMinDistance();

        DateLabel.Text = simDate.ToString(Strings.DateFormat, Strings.Culture);
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
        string label = days < 0 ? Strings.Back : Strings.Elapsed;
        double span = Math.Abs(days);
        if (span < 1.0)
            return Strings.Format("msg.elapsedHours", label, span * 24);
        int years = (int)(span / 365.25);
        int rest = (int)(span - years * 365.25);
        return years > 0
            ? Strings.Format("msg.elapsedYears", label, years, rest)
            : Strings.Format("msg.elapsedDays", label, rest);
    }

    // ------------------------------------------------------------------ språk

    /// <summary>
    /// Byter språk på allt som redan står i fönstret.
    ///
    /// Texterna sätts härifrån och inte i XAML, och det är hela skälet till att
    /// varje etikett har ett namn: en text som skrivs in i XAML går inte att
    /// byta sedan, och språkbytet ska slå igenom medan appen kör. Det som ritas
    /// i vyn behöver däremot ingenting här – ritkoden slår upp namnen varje
    /// bildruta, så planeterna byter språk av sig själva.
    /// </summary>
    void ApplyLanguage()
    {
        _rebuilding = true;

        if (Window is not null)
            Window.Title = Strings.WindowTitle;

        HelpLabel.Text = Strings.Help;
        LanguageTitleLabel.Text = Strings.Language;
        PanelToggleButton.Text = PanelBody.IsVisible ? Strings.HidePanel : Strings.ShowPanel;

        SpeedTitleLabel.Text = Strings.Speed;
        OrbitsLabel.Text = Strings.ShowOrbits;
        MoonsLabel.Text = Strings.ShowMoons;
        RealScaleLabel.Text = Strings.RealScale;

        AsteroidsLabel.Text = Strings.AsteroidBelt;
        KuiperLabel.Text = Strings.KuiperBelt;
        HalleyLabel.Text = Strings.Halley;
        ConstellationsLabel.Text = Strings.Constellations;
        StarNamesLabel.Text = Strings.StarNames;
        StarsTitleLabel.Text = Strings.Stars;
        FocusTitleLabel.Text = Strings.Focus;
        ResetButton.Text = Strings.ResetView;

        DateTitleLabel.Text = Strings.Date;
        DateEntry.Placeholder = Strings.DatePlaceholder;
        GoToDateButton.Text = Strings.GoToDate;
        StepYearBackButton.Text = Strings.StepYearBack;
        StepMonthBackButton.Text = Strings.StepMonthBack;
        StepDayBackButton.Text = Strings.StepDayBack;
        TodayButton.Text = Strings.TodayButton;
        StepDayForwardButton.Text = Strings.StepDayForward;
        StepMonthForwardButton.Text = Strings.StepMonthForward;
        StepYearForwardButton.Text = Strings.StepYearForward;

        MissionTitleLabel.Text = Strings.Mission;
        NextWindowButton.Text = Strings.NextWindow;

        MeetingsTitleLabel.Text = Strings.Meetings;
        MeetingButton.Text = Strings.GoToNext;
        MoonOrbitLabel.Text = Strings.MoonOrbit;

        ProbeMenuTitleLabel.Text = Strings.ShowProbes;
        AllProbesButton.Text = Strings.All;
        NoProbesButton.Text = Strings.None;

        // Väljarnas innehåll byts ut, men valet ska stå kvar. Att sätta
        // ItemsSource nollställer SelectedIndex, så det sparas undan först.
        int language = Math.Max(0, LanguagePicker.SelectedIndex);
        LanguagePicker.ItemsSource = new List<string>
            { Strings.FollowSystem, "Svenska", "English" };
        LanguagePicker.SelectedIndex = language;

        int density = Math.Max(0, StarDensityPicker.SelectedIndex);
        StarDensityPicker.ItemsSource = new List<string>
            { Strings.StarsNone, Strings.StarsFew, Strings.StarsNormal, Strings.StarsMany };
        StarDensityPicker.SelectedIndex = density;

        int meeting = Math.Max(0, MeetingPicker.SelectedIndex);
        MeetingPicker.ItemsSource = SkyEvent.Choices.Select(ChoiceLabel).ToList();
        MeetingPicker.SelectedIndex = meeting;

        // De texter som byggs av tillstånd får skrivas om från sina egna ställen.
        UpdateSpeedFromSlider();
        UpdateProbeMenuButton();
        UpdateMissionUi((CurrentDate - SolarSystemData.EpochJ2000).TotalDays);
        RebuildFocusPicker(CurrentFocus());

        _rebuilding = false;
        _settingsChanged = true;
    }

    void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_rebuilding)
            return;

        int index = Math.Max(0, LanguagePicker.SelectedIndex);
        Strings.Use(Languages[index] ?? SystemCulture);
        ApplyLanguage();
    }

    /// <summary>
    /// Etiketten för ett val i mötesväljaren. Den byggs här och inte i
    /// <see cref="SkyEvent"/>, eftersom den beror på språket: "Mars i opposition"
    /// är sorten och kroppens namn satta samman, och båda delarna byts vid ett
    /// språkbyte.
    /// </summary>
    static string ChoiceLabel(SkyEvent.Choice choice) => choice.Kind switch
    {
        SkyEvent.Kind.Opposition =>
            Strings.Format("msg.choiceOpposition", Strings.Name(choice.A.Key)),
        SkyEvent.Kind.Conjunction =>
            Strings.Format("msg.choiceConjunction",
                Strings.Name(choice.A.Key), Strings.Name(choice.B!.Key)),
        SkyEvent.Kind.SolarEclipse => Strings.ChoiceSolarEclipse,
        SkyEvent.Kind.LunarEclipse => Strings.ChoiceLunarEclipse,
        _ => Strings.ChoicePerihelion,
    };

    // ---------------------------------------------------------------- reglage

    void OnStartStopClicked(object? sender, EventArgs e) => ToggleRunning();

    void ToggleRunning()
    {
        _running = !_running;
        StartStopButton.Text = _running ? Strings.Pause : Strings.Start;
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
            return Strings.SpeedStopped;

        string direction = dps < 0 ? Strings.SpeedBackwards : string.Empty;
        double rate = Math.Abs(dps);
        return rate switch
        {
            < 1.0 => Strings.Format("msg.speedHours", rate * 24, direction),
            < 365.25 => Strings.Format("msg.speedDays", rate, direction),
            _ => Strings.Format("msg.speedYears", rate / 365.25, direction),
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
        // ÅÅÅÅ-MM-DD först, oavsett språk: det formatet står i fältet och betyder
        // samma sak överallt. Går det inte, pröva det valda språkets eget sätt att
        // skriva datum.
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            || DateTime.TryParse(text, Strings.Culture, DateTimeStyles.None, out date))
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
        SolarSystemData.Planets.First(p => p.Key == "Earth");
    static readonly CelestialBody MarsBody =
        SolarSystemData.Planets.First(p => p.Key == "Mars");
    static readonly CelestialBody MoonBody = SolarSystemData.Moon;

    /// <summary>
    /// Ställer fokusväljaren på en kropp. Kroppen söks upp i listan i stället för
    /// att platsen räknas fram: listan innehåller både månar och sonder, och
    /// vilka av dem som visas växlar under körningen. Att söka på kroppen och
    /// inte på namnet är dessutom det enda som håller när språket byts.
    /// </summary>
    void FocusOn(CelestialBody body)
    {
        int index = _focusBodies.IndexOf(body);
        if (index >= 0)
            FocusPicker.SelectedIndex = index + 1;      // plats 0 är solen
    }

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

            LaunchButton.Text = toMoon ? Strings.LaunchMars : Strings.AbortMission;
            LaunchButton.IsEnabled = !toMoon;
            LaunchButton.BackgroundColor = toMoon ? LaunchClosed : LaunchReady;
            MoonButton.Text = toMoon ? Strings.AbortMission : Strings.LaunchMoon;
            MoonButton.IsEnabled = toMoon;
            MoonButton.BackgroundColor = toMoon ? LaunchReady : LaunchClosed;
            NextWindowButton.IsEnabled = false;
            NextWindowButton.BackgroundColor = StepClosed;

            // Restid, avstånd och fart står i färdpanelen i stället.
            MissionLabel.Text = string.Empty;
            return;
        }

        LaunchButton.Text = Strings.LaunchMars;
        LaunchButton.IsEnabled = _inLaunchWindow;
        LaunchButton.BackgroundColor = _inLaunchWindow ? LaunchReady : LaunchClosed;

        // Månen är tillbaka på samma ställe var 27:e dygn, så dit går det att åka
        // i stort sett vilken dag som helst – där behövs inga startfönster.
        MoonButton.Text = Strings.LaunchMoon;
        MoonButton.IsEnabled = true;
        MoonButton.BackgroundColor = LaunchReady;

        NextWindowButton.IsEnabled = _nextWindowDay is not null;
        NextWindowButton.BackgroundColor = _nextWindowDay is not null ? StepReady : StepClosed;

        if (_inLaunchWindow)
        {
            // Farten i förhållande till jorden är måttet på hur stor raket som
            // behövs, och det är den som avgör om fönstret räknas som öppet.
            MissionLabel.Text = Strings.Format("msg.windowOpen", _departureSpeedKmS);
        }
        else if (_nextWindowDay is double next)
        {
            var date = SolarSystemData.EpochJ2000.AddDays(next);
            MissionLabel.Text = Strings.Format("msg.windowClosedNext", next - day, date);
        }
        else
        {
            MissionLabel.Text = Strings.WindowClosed;
        }
    }

    // Kameran ska följa med farkosten ner till målet när den kommer fram. Det
    // sker en gång, i själva ankomstögonblicket – därefter får användaren styra
    // fritt igen, och ett nytt val i fokusväljaren stänger av följandet.
    bool _arrivalSeen;
    bool _followCraft;

    /// <summary>
    /// Hur nära kameran får komma det den tittar på. Räknas om varje bildruta
    /// ur den valda kroppens ritade radie, eftersom den ändras både när man
    /// byter kropp och när man slår om mellan förstorat och verkligt läge.
    ///
    /// En farkost eller en sond är en punkt utan utsträckning och får därför
    /// komma hur nära som helst.
    /// </summary>
    float FocusMinDistance()
    {
        if (_followCraft || FocusedProbe is not null)
            return OrbitCamera.AbsoluteMinDistance;

        int body = _focusIndex - 1;
        bool sun = body < 0 || body >= _focusBodies.Count;
        double radiusKm = sun ? SolarSystemData.SunRadiusKm : _focusBodies[body].RadiusKm;

        // En bit utanför ytan, så att klotet fyller bilden utan att kameran
        // hamnar inuti det.
        return _drawable.VisualRadius(radiusKm, sun) * 1.15f;
    }

    /// <summary>Vad kameran tittar på: farkosten efter ankomsten, annars vald kropp.</summary>
    Vector3 CameraTarget(double day)
    {
        if (_followCraft && _drawable.CraftPosition() is { } craft)
            return craft;

        // Före uppskjutningen finns sonden inte, och kameran står kvar vid solen.
        if (FocusedProbe is { } probe)
            return probe.PositionAt(day, SolarSystemDrawable.UnitsPerAu) ?? Vector3.Zero;

        int body = _focusIndex - 1;
        if (body < 0 || body >= _focusBodies.Count)
            return Vector3.Zero;

        // En måne ritas inte där den verkligen är, så kameran måste fråga
        // ritkoden var den hamnade.
        return _focusParents[body] is { } planet
            ? _drawable.MoonPosition(planet, _focusBodies[body], day)
            : _focusBodies[body].PositionAt(day, SolarSystemDrawable.UnitsPerAu);
    }

    /// <summary>
    /// Sonden som är vald i fokusväljaren, eller null. Väljaren räknar solen
    /// först, sedan kropparna – planeter med sina månar under sig – och sist de
    /// sonder som visas.
    /// </summary>
    Probe? FocusedProbe
    {
        get
        {
            int index = _focusIndex - _focusBodies.Count - 1;
            return index >= 0 && index < _focusProbes.Count ? _focusProbes[index] : null;
        }
    }

    /// <summary>
    /// Sondpanelen: var sonden är, hur fort den går och vad den senaste
    /// planetpassagen gav. Farthoppet är hela poängen – det är slungan, och utan
    /// den hade ingen av sonderna kommit längre än till Jupiter.
    /// </summary>
    void UpdateProbePanel(double day)
    {
        if (FocusedProbe is not { } probe)
        {
            ProbePanel.IsVisible = false;
            return;
        }

        ProbePanel.IsVisible = true;
        ProbeTitleLabel.Text = probe.Name;

        if (!probe.Exists(day))
        {
            var launch = SolarSystemData.EpochJ2000.AddDays(probe.LaunchDay);
            ProbeDistanceLabel.Text = Strings.Format("msg.probeLaunches", launch);
            ProbeSpeedLabel.Text = Strings.ProbeWindTime;
            ProbeLastLabel.Text = string.Empty;
            ProbeNextLabel.Text = string.Empty;
            return;
        }

        double au = probe.DistanceAu(day);
        ProbeDistanceLabel.Text = Strings.Format("msg.probeDistance",
            au, au * SolarSystemData.AuKm / 1e9);
        ProbeSpeedLabel.Text = Strings.Format("msg.probeSpeed", probe.SpeedKmPerSecond(day));

        ProbeLastLabel.Text = probe.LastMilestone(day) is { } last
            ? MilestoneText(probe, last)
            : string.Empty;

        if (probe.NextMilestone(day) is { } next)
        {
            var date = SolarSystemData.EpochJ2000.AddDays(next.Day);
            ProbeNextLabel.Text = Strings.Format("msg.probeNext",
                SolarSystemDrawable.MilestoneName(probe, next), date, next.Day - day);
        }
        else
        {
            ProbeNextLabel.Text = Strings.ProbeNoMore;
        }
    }

    /// <summary>Beskriver en passerad milstolpe, med farten planeten gav eller tog.</summary>
    static string MilestoneText(Probe probe, Milestone milestone)
    {
        var date = SolarSystemData.EpochJ2000.AddDays(milestone.Day);
        if (milestone.IsLaunch)
            return Strings.Format("msg.probeLaunched", date);

        string name = SolarSystemDrawable.MilestoneName(probe, milestone);

        // En gräns gav ingen fart och ska inte beskrivas som en förbiflygning.
        if (milestone.IsBoundary)
            return Strings.Format("msg.probePassed", name, date);

        string verb = milestone.SpeedGainKmS >= 0 ? Strings.Gained : Strings.Cost;
        return Strings.Format("msg.probePast", name, date, verb,
            Math.Abs(milestone.SpeedGainKmS));
    }

    /// <summary>
    /// Zoomar ut så att både sonden och solen ryms i bild när en sond väljs.
    /// Sonderna är över hundra gånger längre bort än jorden, så hela
    /// planetsystemet krymper då till en prick kring solen – vilket i sig är
    /// det man ska se.
    ///
    /// Faktorn styr hur långt bort kameran ställer sig, räknat i sondens eget
    /// avstånd från solen. Står kameran f gånger så långt bort hamnar solen som
    /// mest arcsin(1/f) från bildens mitt, och det måste rymmas inom halva
    /// bildhöjden på 25 grader. Det ger f minst 2,37. Här stod tidigare 2,2,
    /// vilket ger 27 grader: solen gled utanför över- eller underkanten så snart
    /// kameran lutades, i ungefär vart tionde läge.
    /// </summary>
    void ZoomToProbe(Probe probe)
    {
        double day = (CurrentDate - SolarSystemData.EpochJ2000).TotalDays;
        float distance = (float)probe.DistanceAu(day) * SolarSystemDrawable.UnitsPerAu;
        _drawable.Camera.Distance =
            Math.Clamp(distance * 2.4f, 200f, OrbitCamera.MaxDistance);
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
            string target = Strings.Name(mission.Target.Key);
            CraftTitleLabel.Text = Strings.Format("msg.craftArrived", target);
            CraftElapsedLabel.Text = Strings.Format("msg.craftTravelTime",
                FormatTravelTime(mission.TravelDays));
            CraftRemainingLabel.Text = Strings.Format("msg.craftArrivedOn", arrival);
            CraftDistanceLabel.Text = Strings.Format("msg.craftTravellingWith", target);
            CraftSpeedLabel.Text = Strings.Format("msg.craftArrivalSpeed", speed);
            return;
        }

        double elapsed = Math.Max(0.0, day - mission.LaunchDay);
        string goal = Strings.Name(mission.Target.Key);
        CraftTitleLabel.Text = Strings.Format("msg.craftHeadingFor", goal);
        CraftElapsedLabel.Text = Strings.Format("msg.craftElapsed", FormatTravelTime(elapsed));
        CraftRemainingLabel.Text = Strings.Format("msg.craftRemaining",
            FormatTravelTime(mission.TravelDays - elapsed), arrival);
        CraftDistanceLabel.Text = Strings.Format("msg.craftDistanceTo",
            goal, FormatDistance(mission.DistanceToTargetKm(day)));
        CraftSpeedLabel.Text = Strings.Format("msg.craftSpeed", speed);
    }

    /// <summary>
    /// Restider skrivs i timmar när de är korta – en månfärd tar bara tre dygn,
    /// och då säger "0,4 dygn" mindre än "9,6 timmar".
    /// </summary>
    static string FormatTravelTime(double days)
    {
        double span = Math.Max(0.0, days);
        return span < 2.0
            ? Strings.Format("msg.hours", span * 24)
            : Strings.Format("msg.days", span);
    }

    /// <summary>Avstånd i kilometer, i miljoner när talen blir för långa att läsa.</summary>
    static string FormatDistance(double km)
        => km >= 1e6
            ? Strings.Format("msg.millionKm", km / 1e6)
            : Strings.Format("msg.km", km);

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
        var mission = Mission.Plan("craft", EarthBody, MarsBody, launchDay);
        if (mission is null)
        {
            MissionLabel.Text = Strings.CraftNoPath;
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
        var mission = Mission.PlanToMoon("craft", EarthBody, MoonBody, launchDay);
        if (mission is null)
        {
            MissionLabel.Text = Strings.CraftNoPath;
            return;
        }

        _drawable.Mission = mission;
        FocusOn(EarthBody);                            // zoomar in via OnFocusChanged
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
        // Släcks månarna ska de också ut ur fokusväljaren, och följde kameran en
        // av dem faller fokus tillbaka till solen.
        RebuildFocusPicker(CurrentFocus());
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

    /// <summary>
    /// Tänder och släcker Halleys komet. Kometen går också att följa, så
    /// fokusväljaren byggs om – på samma sätt som när månarna släcks.
    /// </summary>
    void OnHalleyChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowHalley = e.Value;
        RebuildFocusPicker(CurrentFocus());
        _settingsChanged = true;
    }

    // ----------------------------------------------------------- sondväljaren

    /// <summary>
    /// Sonderna som just nu går att välja i fokusväljaren, i väljarens ordning.
    /// Listan förs parallellt eftersom väljarens innehåll ändras när sonder
    /// tänds och släcks – index går alltså inte längre att räkna ur ProbeData.
    /// </summary>
    readonly List<Probe> _focusProbes = new();

    /// <summary>Kropparna i fokusväljaren, i samma ordning som namnen efter solen.</summary>
    readonly List<CelestialBody> _focusBodies = new();

    /// <summary>Planeten en måne hör till, eller null för planeterna själva.</summary>
    readonly List<CelestialBody?> _focusParents = new();

    /// <summary>Sant medan fokusväljaren byggs om, så att bytet inte tolkas som ett val.</summary>
    bool _rebuildingFocus;

    /// <summary>
    /// Bygger sondväljarens rader ur sonddata: först de fem som lämnat
    /// solsystemet, sedan de två som kretsar kring en planet.
    /// </summary>
    void BuildProbeMenu()
    {
        foreach (var probe in ProbeData.All)
            ProbeMenuItems.Children.Add(ProbeMenuRow(probe.Name, probe.Color));

        ProbeMenuItems.Children.Add(new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb("#2A3340"),
            Margin = new Thickness(0, 4),
        });

        foreach (var orbiter in ProbeData.Orbiters)
            ProbeMenuItems.Children.Add(ProbeMenuRow(orbiter.Name, orbiter.Color));

        UpdateProbeMenuButton();
    }

    /// <summary>En rad i väljaren: kryssruta och namn i sondens egen färg.</summary>
    View ProbeMenuRow(string name, Color color)
    {
        var check = new CheckBox
        {
            IsChecked = _drawable.VisibleProbes.Contains(name),
            Color = Color.FromArgb("#6FA8DC"),
            VerticalOptions = LayoutOptions.Center,
        };
        check.CheckedChanged += (_, e) => OnProbeToggled(name, e.Value);

        return new HorizontalStackLayout
        {
            Spacing = 2,
            Children =
            {
                check,
                new Label
                {
                    Text = name,
                    TextColor = color,
                    FontSize = 14,
                    VerticalOptions = LayoutOptions.Center,
                },
            },
        };
    }

    void OnProbeMenuClicked(object? sender, EventArgs e)
        => ProbeMenu.IsVisible = !ProbeMenu.IsVisible;

    void OnAllProbesClicked(object? sender, EventArgs e) => SetAllProbes(true);

    void OnNoProbesClicked(object? sender, EventArgs e) => SetAllProbes(false);

    /// <summary>Tänder eller släcker allihop via kryssrutorna, som i sin tur gör jobbet.</summary>
    void SetAllProbes(bool visible)
    {
        foreach (var row in ProbeMenuItems.Children.OfType<HorizontalStackLayout>())
            if (row.Children.OfType<CheckBox>().FirstOrDefault() is { } check)
                check.IsChecked = visible;
    }

    /// <summary>
    /// En sond tänds eller släcks. Släcks den sond kameran följer faller fokus
    /// tillbaka till solen och vyn zoomar ut till översikten – kameran ska aldrig
    /// bli stående och följa något som inte ritas.
    /// </summary>
    void OnProbeToggled(string name, bool visible)
    {
        if (visible)
            _drawable.VisibleProbes.Add(name);
        else
            _drawable.VisibleProbes.Remove(name);

        object? keep = CurrentFocus();
        if (keep is Probe followed && !_drawable.VisibleProbes.Contains(followed.Name))
            keep = null;

        RebuildFocusPicker(keep);
        UpdateProbeMenuButton();
        _settingsChanged = true;
    }

    /// <summary>Knappens text visar hur många sonder som är ivalda.</summary>
    void UpdateProbeMenuButton()
    {
        int total = ProbeData.All.Length + ProbeData.Orbiters.Length;
        ProbeMenuButton.Text = Strings.Format("ui.probesButton",
            _drawable.VisibleProbes.Count, total);
    }

    /// <summary>
    /// Bygger om fokusväljaren så att den bara listar de sonder som visas – man
    /// ska inte kunna välja att följa något som inte ritas. Går det tidigare
    /// valet inte att behålla faller det tillbaka till solen, och då zoomar vyn
    /// ut till översikten; annars hade kameran blivit stående hundra AU ut i
    /// tomma rymden.
    /// </summary>
    /// <summary>
    /// Det som är valt i fokusväljaren just nu: null för solen, annars kroppen
    /// eller sonden. Valet följs som en sak och inte som en text – namnen byter
    /// språk, och en text går då inte att känna igen efteråt.
    /// </summary>
    object? CurrentFocus()
    {
        if (FocusedProbe is { } probe)
            return probe;

        int body = _focusIndex - 1;
        return body >= 0 && body < _focusBodies.Count ? _focusBodies[body] : null;
    }

    void RebuildFocusPicker(object? keep)
    {
        _focusProbes.Clear();
        _focusProbes.AddRange(ProbeData.All.Where(p => _drawable.VisibleProbes.Contains(p.Name)));

        // Varje planet följs av sina månar, med en punkt framför så att
        // grupperingen syns i en lista som inte kan dra in rader. Månarna listas
        // bara när de ritas – man ska inte kunna följa något som inte syns,
        // samma regel som för sonderna.
        _focusBodies.Clear();
        _focusParents.Clear();
        var names = new List<string> { Strings.Name("Sun") };
        foreach (var planet in SolarSystemData.Planets)
        {
            _focusBodies.Add(planet);
            _focusParents.Add(null);
            names.Add(Strings.Name(planet.Key));

            if (!_drawable.ShowMoons)
                continue;
            foreach (var moon in planet.Moons)
            {
                _focusBodies.Add(moon);
                _focusParents.Add(planet);
                names.Add(MoonEntry + Strings.Name(moon.Key));
            }
        }
        // Kometen sist bland kropparna, och bara när den ritas. Att kunna följa
        // den är mer värt än för planeterna: banan är sextio gånger längre än den
        // är bred, så utan kamera på plats försvinner kometen ur bild i årtionden.
        if (_drawable.ShowHalley)
        {
            _focusBodies.Add(SolarSystemData.Halley);
            _focusParents.Add(null);
            names.Add(Strings.Name(SolarSystemData.Halley.Key));
        }

        names.AddRange(_focusProbes.Select(p => p.Name));

        // Tappas valet – för att sonden släckts, eller för att månarna gömts –
        // faller det tillbaka till solen.
        int found = 0;                                  // null betyder solen
        if (keep is CelestialBody body)
        {
            int i = _focusBodies.IndexOf(body);
            found = i >= 0 ? i + 1 : -1;
        }
        else if (keep is Probe probe)
        {
            int i = _focusProbes.IndexOf(probe);
            found = i >= 0 ? _focusBodies.Count + 1 + i : -1;
        }

        bool lostFocus = found < 0;
        int index = lostFocus ? 0 : found;

        _rebuildingFocus = true;
        FocusPicker.ItemsSource = names;
        FocusPicker.SelectedIndex = index;
        _rebuildingFocus = false;

        _focusIndex = index;
        _drawable.FocusedProbe = FocusedProbe;

        if (lostFocus)
            _drawable.Camera.Distance = OrbitCamera.DefaultDistance;
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
        if (_rebuilding)
            return;     // väljaren fylls om vid språkbyte, inget användaren gjort

        _drawable.StarDensity = (StarDensity)Math.Max(0, StarDensityPicker.SelectedIndex);
        _settingsChanged = true;
    }

    void OnFocusChanged(object? sender, EventArgs e)
    {
        if (_rebuildingFocus)
            return;     // väljaren byggs om, inget användaren klickat på

        _focusIndex = Math.Max(0, FocusPicker.SelectedIndex);
        _followCraft = false;
        _drawable.FocusedProbe = FocusedProbe;
        // Gränsen först, annars kläms det nya avståndet av den förra kroppens.
        _drawable.Camera.MinDistance = FocusMinDistance();

        if (FocusedProbe is { } probe)
        {
            ZoomToProbe(probe);
        }
        else if (_focusIndex - 1 is int body && body >= 0 && body < _focusBodies.Count)
        {
            // En måne fyller bilden; en planet ramas in så att hela dess
            // månsystem ryms.
            _drawable.Camera.Distance = _focusParents[body] is null
                ? _drawable.SuggestedFocusDistance(_focusBodies[body])
                : _drawable.SuggestedMoonDistance(_focusBodies[body]);
        }

        _settingsChanged = true;
    }

    /// <summary>
    /// Hoppar till nästa möte av den valda sorten. Sökningen börjar vid det
    /// datum man står på, så trycker man igen kommer man till nästa i tur och
    /// ordning – och söker man bakåt får man byta datum först.
    ///
    /// Ett par saker är värda att veta om det som visas. Vid opposition står
    /// planeten närmast jorden, och avståndet skiljer sig rejält mellan
    /// tillfällena: Mars kan vara 0,37 AU bort en gynnsam gång och 0,68 en
    /// ogynnsam, vilket är hela skälet till att somliga oppositioner blir
    /// nyheter. Vid konjunktion står de två bara i samma riktning sett
    /// härifrån – i rymden kan de vara miljardtals kilometer isär.
    /// </summary>
    void OnNextMeetingClicked(object? sender, EventArgs e)
    {
        int index = MeetingPicker.SelectedIndex;
        if (index < 0 || index >= SkyEvent.Choices.Length)
            return;

        var choice = SkyEvent.Choices[index];
        double day = (CurrentDate - SolarSystemData.EpochJ2000).TotalDays;

        if (SkyEvent.Next(choice.Kind, choice.A, choice.B, day) is not { } meeting)
        {
            MeetingLabel.Text = Strings.NoMeeting;
            return;
        }

        var date = SolarSystemData.EpochJ2000.AddDays(meeting.Day);
        GoToDate(date);

        // Hela meningen, inte bara siffran: vad som hittades, när, och hur nära.
        string detail = choice.Kind switch
        {
            SkyEvent.Kind.Opposition =>
                Strings.Format("msg.meetingOpposition", meeting.DistanceAu),
            SkyEvent.Kind.SolarEclipse =>
                Strings.Format("msg.meetingSolarEclipse", meeting.SeparationDeg),
            SkyEvent.Kind.LunarEclipse =>
                Strings.Format("msg.meetingLunarEclipse", meeting.SeparationDeg),
            // Vid periheliet är avståndet till solen alltid detsamma, så det som
            // är värt att veta är hur besöket blir sett härifrån: hur nära jorden
            // kometen kommer, och hur långt från solen den står på himlen. Under
            // ett tiotal grader drunknar den i dagsljuset.
            SkyEvent.Kind.Perihelion =>
                Strings.Format("msg.meetingPerihelion",
                    meeting.DistanceAu, meeting.SeparationDeg),
            _ => Strings.Format("msg.meetingConjunction", meeting.SeparationDeg),
        };
        MeetingLabel.Text = Strings.Format("msg.meetingLine", ChoiceLabel(choice), date, detail);

        // Vid en förmörkelse är förklaringen mer värd än datumet. Ställ in vyn
        // så att den syns: månbanan fram och kameran vid jorden, där man ser att
        // solen står vid nodlinjen just den dagen och därför kan komma i vägen.
        if (choice.Kind is SkyEvent.Kind.SolarEclipse or SkyEvent.Kind.LunarEclipse)
        {
            MoonOrbitCheck.IsChecked = true;
            MoonsCheck.IsChecked = true;
            FocusOn(EarthBody);
        }

        // Att hoppa till Halleys perihelium utan att tända kometen vore att resa
        // till ett tomt datum. Kameran lämnas däremot där den står: det är i
        // översikten man ser vad som faktiskt händer, att kometen dyker in genom
        // hela planetsystemet. Vill man gå nära finns den i fokusväljaren.
        if (choice.Kind is SkyEvent.Kind.Perihelion)
            HalleyCheck.IsChecked = true;
    }

    void OnMoonOrbitChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowMoonOrbit = e.Value;
        _settingsChanged = true;
    }

    /// <summary>
    /// Fäller ihop kontrollpanelen till bara sin list. Panelen har vuxit till
    /// fem rader och tar då en femtedel av fönstret, vilket är i mesta laget när
    /// man bara vill titta på solsystemet.
    /// </summary>
    void OnPanelToggleClicked(object? sender, EventArgs e) => TogglePanel();

    void TogglePanel()
    {
        PanelBody.IsVisible = !PanelBody.IsVisible;
        PanelToggleButton.Text = PanelBody.IsVisible ? Strings.HidePanel : Strings.ShowPanel;
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
            case Windows.System.VirtualKey.M:
                TogglePanel();
                break;
        }
    }
#endif
}
