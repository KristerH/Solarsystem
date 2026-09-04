using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Solarsystem.Rendering;
using Solarsystem.Simulation;

namespace Solarsystem;

public partial class MainPage : ContentPage
{
    /// <summary>
    /// The language the operating system is set to, read before the app has
    /// had a chance to change anything. This is the starting point, and
    /// what the selector returns to with "Follow the system". If the
    /// computer is set to a language the app doesn't have, texts fall back
    /// to English on their own – that's how .NET's resource handling works
    /// – while dates and numbers still follow the computer's own
    /// conventions.
    /// </summary>
    static readonly CultureInfo SystemCulture = CultureInfo.CurrentUICulture;

    static readonly CultureInfo Swedish = new("sv-SE");
    static readonly CultureInfo English = new("en-US");

    /// <summary>The language selector's rows. Null means "follow the system".</summary>
    static readonly CultureInfo?[] Languages = [null, Swedish, English];

    /// <summary>
    /// True while the selectors' contents are being rebuilt for a language
    /// switch, so the changes that happen along the way aren't interpreted
    /// as something the user did.
    /// </summary>
    bool _rebuilding;

    readonly SolarSystemDrawable _drawable = new();
    readonly Stopwatch _clock = Stopwatch.StartNew();
    IDispatcherTimer? _timer;

    double _lastSeconds;
    bool _running = true;
    double _simDays;                       // simulated days since start
    double _daysPerSecond = 30;
    DateTime _startDate = DateTime.Now;

    double _panLastX, _panLastY;
    int _focusIndex;                       // 0 = the Sun, then bodies, then probes

    // The last meeting searched for. Kept so the line can be written again in
    // a new language: it's the one text built from a past click rather than
    // from the current state, and without this it would be left standing in
    // the language it was found in.
    SkyEvent.Choice? _meetingChoice;
    SkyEvent.Meeting? _meeting;

    // The launch-window tooltip as it currently stands, so it can be left
    // alone when it hasn't changed. See SetLaunchWindowTip.
    string? _launchWindowTip;

    /// <summary>Moons are indented one step in the focus selector so they read as belonging to their planet.</summary>
    const string MoonEntry = "\u00b7 ";

    // Last rendered state – when nothing has changed, redrawing is skipped entirely.
    float _drawnYaw = float.NaN, _drawnPitch, _drawnDist;
    Vector3 _drawnTarget;
    double _drawnSimDays = double.NaN;
    double _resizeQuietUntil;              // clock time when rendering is allowed to wake up again
    bool _settingsChanged = true;

    // Launch window: every check requires a full orbit to be computed, so
    // the result is cached and checked at most a few times per second.
    double _windowCheckedDay = double.NaN;
    double _windowCheckedAt;
    bool _inLaunchWindow;
    double? _nextWindowDay;
    double _departureSpeedKmS;             // what the launch costs right now

    public MainPage()
    {
        InitializeComponent();

        SpaceView.Drawable = _drawable;

        // Pause rendering while the window is being resized. Every resize
        // step would otherwise force rebuilt caches and a full redraw,
        // keeping pace with the window manager as it waits – that was what
        // froze the whole system. 300 ms after the last step, rendering
        // wakes up and recomputes everything once.
        SpaceView.SizeChanged += (_, _) =>
        {
            _drawable.Suspended = true;
            _resizeQuietUntil = _clock.Elapsed.TotalSeconds + 0.3;

            // The probe selector is anchored to the bottom of the view and
            // grows upward. Seven probes are taller than a short window, and
            // without a ceiling the rows past the edge are simply cut off –
            // Cassini and Juno were unreachable. Capped to the view, the
            // scroll view inside it takes over.
            ProbeMenu.MaximumHeightRequest = Math.Max(120, SpaceView.Height - 24);
        };

        BuildProbeMenu();

        // Follow the computer's language until someone picks something else.
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

        // The window exists only now, so the title can't be set in the constructor.
        if (Window is not null)
            Window.Title = Strings.WindowTitle;

        // 30 frames/second is plenty and halves the CPU load compared to 60.
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(33);
        _timer.Tick += OnTick;
        _lastSeconds = _clock.Elapsed.TotalSeconds;
        _timer.Start();
    }


    // ------------------------------------------------------------- simulation

    void OnTick(object? sender, EventArgs e)
    {
        double now = _clock.Elapsed.TotalSeconds;
        double dt = now - _lastSeconds;
        _lastSeconds = now;

        if (_drawable.Suspended)
        {
            if (now < _resizeQuietUntil)
                return; // resizing in progress – neither simulate nor draw
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

        // Only redraw when something has actually changed (time has advanced
        // or the camera has moved). A paused, still view then costs almost
        // nothing.
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
        // Time can now run either direction, so negative values count as "back".
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

    // ------------------------------------------------------------------ language

    /// <summary>
    /// Switches language on everything already in the window.
    ///
    /// The texts are set from here and not in XAML, and that's the whole
    /// reason every label has a name: text written into XAML can't be
    /// changed afterward, and the language switch has to take effect while
    /// the app is running. What's drawn in the view, on the other hand,
    /// needs nothing here – the rendering code looks up the names every
    /// frame, so the planets switch language on their own.
    /// </summary>
    void ApplyLanguage()
    {
        _rebuilding = true;

        if (Window is not null)
            Window.Title = Strings.WindowTitle;

        HelpLabel.Text = Strings.Help;
        LanguageTitleLabel.Text = Strings.Language;
        PanelToggleButton.Text = PanelBody.IsVisible ? Strings.HidePanel : Strings.ShowPanel;

        StartStopButton.Text = _running ? Strings.Pause : Strings.Start;
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

        // The selectors' contents are swapped out, but the selection should
        // stay put. Setting ItemsSource resets SelectedIndex, so it's saved
        // aside first.
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

        // The texts built from state get rewritten from their own places.
        UpdateSpeedFromSlider();
        UpdateMeetingLabel();
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
    /// The label for a choice in the meeting selector. Built here and not in
    /// <see cref="SkyEvent"/>, since it depends on the language: "Mars at
    /// opposition" is the kind and the body's name put together, and both
    /// parts change on a language switch.
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

    // ---------------------------------------------------------------- controls

    void OnStartStopClicked(object? sender, EventArgs e) => ToggleRunning();

    void ToggleRunning()
    {
        _running = !_running;
        StartStopButton.Text = _running ? Strings.Pause : Strings.Start;
    }

    void OnSpeedChanged(object? sender, ValueChangedEventArgs e) => UpdateSpeedFromSlider();

    void UpdateSpeedFromSlider()
    {
        // Logarithmic scale: 0.1 days/s up to 1000 days/s. The slider runs
        // from -1 to 1, where negative values play time backward and the
        // middle stands still.
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

    // ------------------------------------------------------------------ date

    /// <summary>The date the simulation currently stands on.</summary>
    DateTime CurrentDate => _startDate.AddDays(_simDays);

    /// <summary>Moves the simulation to a given date, backward or forward.</summary>
    void GoToDate(DateTime date)
    {
        _simDays = (date - _startDate).TotalDays;
        _windowCheckedDay = double.NaN;
        _settingsChanged = true;
    }

    void OnGoToDateClicked(object? sender, EventArgs e) => ApplyTypedDate();

    void OnDateEntryCompleted(object? sender, EventArgs e) => ApplyTypedDate();

    /// <summary>
    /// Reads the date field and jumps there. Accepts both "2026-09-06" and
    /// other formats the selected culture understands; on a typo, nothing
    /// happens beyond the field being flagged.
    /// </summary>
    void ApplyTypedDate()
    {
        string text = DateEntry.Text?.Trim() ?? string.Empty;
        // YYYY-MM-DD first, regardless of language: that's the format shown
        // in the field and it means the same thing everywhere. If that
        // fails, try the selected language's own way of writing dates.
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

    /// <summary>Steps the date one day, month or year at a time.</summary>
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

    /// <summary>Resets the clock to now.</summary>
    void OnTodayClicked(object? sender, EventArgs e)
    {
        _startDate = DateTime.Now;
        _simDays = 0;
        _settingsChanged = true;
    }

    // --------------------------------------------------------------- space missions

    // MAUI doesn't itself grey out a button that's been given its own
    // background colour, so the disabled states are repainted by hand.
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
    /// Sets the focus selector to a body. The body is looked up in the list
    /// rather than the position computed: the list contains both moons and
    /// probes, and which of them are shown changes at runtime. Searching by
    /// the body rather than the name is also the only thing that still
    /// works once the language changes.
    /// </summary>
    void FocusOn(CelestialBody body)
    {
        int index = _focusBodies.IndexOf(body);
        if (index >= 0)
            FocusPicker.SelectedIndex = index + 1;      // slot 0 is the Sun
    }

    /// <summary>
    /// Launches a craft toward Mars from the date the view stands on, or
    /// cancels an ongoing trip.
    /// </summary>
    /// <summary>
    /// Keeps track of whether a launch window is open right now and when
    /// the next one falls. The check is expensive – every trial computes a
    /// full transfer orbit – so it's only done when the date has moved
    /// noticeably, and at most four times a second no matter how fast time
    /// is wound forward.
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

    /// <summary>Updates buttons and status text based on the trip's and window's state.</summary>
    void UpdateMissionUi(double day)
    {
        if (_drawable.Mission is { } mission)
        {
            // One trip at a time: the button for the ongoing trip cancels it,
            // and the other is disabled meanwhile.
            bool toMoon = ReferenceEquals(mission.Target, MoonBody);

            LaunchButton.Text = toMoon ? Strings.LaunchMars : Strings.AbortMission;
            LaunchButton.IsEnabled = !toMoon;
            LaunchButton.BackgroundColor = toMoon ? LaunchClosed : LaunchReady;
            MoonButton.Text = toMoon ? Strings.AbortMission : Strings.LaunchMoon;
            MoonButton.IsEnabled = toMoon;
            MoonButton.BackgroundColor = toMoon ? LaunchReady : LaunchClosed;
            NextWindowButton.IsEnabled = false;
            NextWindowButton.BackgroundColor = StepClosed;

            // Travel time, distance and speed are shown in the mission panel instead.
            ShowMissionError(null);
            SetLaunchWindowTip(null);
            return;
        }

        LaunchButton.Text = Strings.LaunchMars;
        LaunchButton.IsEnabled = _inLaunchWindow;
        LaunchButton.BackgroundColor = _inLaunchWindow ? LaunchReady : LaunchClosed;

        // The Moon is back in the same spot every 27 days, so you can travel
        // there on almost any day – no launch windows are needed.
        MoonButton.Text = Strings.LaunchMoon;
        MoonButton.IsEnabled = true;
        MoonButton.BackgroundColor = LaunchReady;

        NextWindowButton.IsEnabled = _nextWindowDay is not null;
        NextWindowButton.BackgroundColor = _nextWindowDay is not null ? StepReady : StepClosed;

        ShowMissionError(null);

        if (_inLaunchWindow)
        {
            // The speed relative to Earth is the measure of how big a rocket
            // is needed, and it's what decides whether the window counts as
            // open.
            SetLaunchWindowTip(Strings.Format("msg.windowOpen", _departureSpeedKmS));
        }
        else if (_nextWindowDay is double next)
        {
            var date = SolarSystemData.EpochJ2000.AddDays(next);
            SetLaunchWindowTip(Strings.Format("msg.windowClosedNext", next - day, date));
        }
        else
        {
            SetLaunchWindowTip(Strings.WindowClosed);
        }
    }

    /// <summary>
    /// The launch-window status, put on the controls it's actually about.
    /// It used to stand under the date in the corner of the view, where it
    /// read as a statement about everything on screen when it only ever
    /// concerned the trip to Mars.
    /// </summary>
    /// <remarks>
    /// All three carry the same text on purpose. The two buttons take turns
    /// being the enabled one – "Launch to Mars" while the window is open,
    /// "Next launch window" while it's shut – and a disabled control gets no
    /// pointer events on Windows, so neither can carry the status on its own.
    /// The row's label is never disabled and is the fallback.
    /// </remarks>
    void SetLaunchWindowTip(string? text)
    {
        // Writing the property again dismisses a tooltip that is on its way
        // up, and the status is rewritten about twice a second while time
        // runs – so without this check it would only ever be readable with
        // the app paused, which is exactly how the bug showed itself. The
        // text only really changes once per simulated day.
        if (text == _launchWindowTip)
            return;

        _launchWindowTip = text;

        SetTip(MissionTitleLabel, text);
        SetTip(LaunchButton, text);
        SetTip(NextWindowButton, text);

        // Null means no tooltip at all, which is a cleared property rather
        // than an empty string – an empty one would still open a blank box.
        static void SetTip(BindableObject control, string? text)
        {
            if (text is null)
                control.ClearValue(ToolTipProperties.TextProperty);
            else
                ToolTipProperties.SetText(control, text);
        }
    }

    /// <summary>
    /// Shows the one mission message that has to be seen rather than hovered
    /// for: that no trip could be planned at all. Hidden when empty, so the
    /// line doesn't take up room under the date the rest of the time.
    /// </summary>
    void ShowMissionError(string? text)
    {
        MissionLabel.Text = text ?? string.Empty;
        MissionLabel.IsVisible = !string.IsNullOrEmpty(text);
    }

    // The camera should follow the craft down to the target when it
    // arrives. That happens once, at the moment of arrival itself –
    // afterward the user steers freely again, and a new choice in the focus
    // selector turns off the following.
    bool _arrivalSeen;
    bool _followCraft;

    /// <summary>
    /// How close the camera is allowed to get to what it's looking at.
    /// Recomputed every frame from the selected body's drawn radius, since
    /// that changes both when the body is switched and when toggling
    /// between magnified and real scale.
    ///
    /// A craft or a probe is a point with no extent and so may get as close
    /// as it likes.
    /// </summary>
    float FocusMinDistance()
    {
        if (_followCraft || FocusedProbe is not null)
            return OrbitCamera.AbsoluteMinDistance;

        int body = _focusIndex - 1;
        bool sun = body < 0 || body >= _focusBodies.Count;
        double radiusKm = sun ? SolarSystemData.SunRadiusKm : _focusBodies[body].RadiusKm;

        // A bit outside the surface, so the globe fills the frame without
        // the camera ending up inside it.
        return _drawable.VisualRadius(radiusKm, sun) * 1.15f;
    }

    /// <summary>What the camera is looking at: the craft after arrival, otherwise the selected body.</summary>
    Vector3 CameraTarget(double day)
    {
        if (_followCraft && _drawable.CraftPosition() is { } craft)
            return craft;

        // Before launch the probe doesn't exist, and the camera stays at the Sun.
        if (FocusedProbe is { } probe)
            return probe.PositionAt(day, SolarSystemDrawable.UnitsPerAu) ?? Vector3.Zero;

        int body = _focusIndex - 1;
        if (body < 0 || body >= _focusBodies.Count)
            return Vector3.Zero;

        // A moon isn't drawn where it really is, so the camera has to ask
        // the rendering code where it ended up.
        return _focusParents[body] is { } planet
            ? _drawable.MoonPosition(planet, _focusBodies[body], day)
            : _focusBodies[body].PositionAt(day, SolarSystemDrawable.UnitsPerAu);
    }

    /// <summary>
    /// The probe selected in the focus selector, or null. The selector
    /// counts the Sun first, then the bodies – planets with their moons
    /// underneath – and finally the probes that are shown.
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
    /// The probe panel: where the probe is, how fast it's going, and what
    /// the last planetary flyby gave it. The speed jump is the whole point
    /// – that's the gravity assist, and without it none of the probes would
    /// have reached farther than Jupiter.
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

    /// <summary>Describes a passed milestone, with the speed the planet gave or took.</summary>
    static string MilestoneText(Probe probe, Milestone milestone)
    {
        var date = SolarSystemData.EpochJ2000.AddDays(milestone.Day);
        if (milestone.IsLaunch)
            return Strings.Format("msg.probeLaunched", date);

        string name = SolarSystemDrawable.MilestoneName(probe, milestone);

        // A boundary gave no speed and shouldn't be described as a flyby.
        if (milestone.IsBoundary)
            return Strings.Format("msg.probePassed", name, date);

        string verb = milestone.SpeedGainKmS >= 0 ? Strings.Gained : Strings.Cost;
        return Strings.Format("msg.probePast", name, date, verb,
            Math.Abs(milestone.SpeedGainKmS));
    }

    /// <summary>
    /// Zooms out so both the probe and the Sun fit in frame when a probe is
    /// selected. The probes are over a hundred times farther out than
    /// Earth, so the whole planetary system then shrinks to a dot around
    /// the Sun – which is itself the point.
    ///
    /// The factor controls how far away the camera positions itself,
    /// measured in the probe's own distance from the Sun. With the camera f
    /// times that distance away, the Sun sits at most arcsin(1/f) from the
    /// centre of frame, and that has to fit within half the frame height of
    /// 25 degrees. That gives f at least 2.37. This used to be 2.2, which
    /// gives 27 degrees: the Sun slid past the top or bottom edge as soon as
    /// the camera was tilted, in roughly one case in ten.
    /// </summary>
    void ZoomToProbe(Probe probe)
    {
        double day = (CurrentDate - SolarSystemData.EpochJ2000).TotalDays;
        float distance = (float)probe.DistanceAu(day) * SolarSystemDrawable.UnitsPerAu;
        _drawable.Camera.Distance =
            Math.Clamp(distance * 2.4f, 200f, OrbitCamera.MaxDistance);
    }

    /// <summary>
    /// The panel that follows the trip: how long the craft has been
    /// travelling, how long remains, how far it has left to the target, and
    /// how fast it's going. Speed is the interesting line. It falls with
    /// distance, exactly as Kepler's second law says: toward the Moon from
    /// 10.8 km/s at launch to under 1 km/s on arrival, toward Mars from 33
    /// to 21 km/s.
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

        // Arrival: follow down to the target, once.
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
    /// Travel times are written in hours when they're short – a lunar trip
    /// only takes three days, and there "0.4 days" says less than "9.6
    /// hours".
    /// </summary>
    static string FormatTravelTime(double days)
    {
        double span = Math.Max(0.0, days);
        return span < 2.0
            ? Strings.Format("msg.hours", span * 24)
            : Strings.Format("msg.days", span);
    }

    /// <summary>Distance in kilometres, in millions once the numbers get too long to read.</summary>
    static string FormatDistance(double km)
        => km >= 1e6
            ? Strings.Format("msg.millionKm", km / 1e6)
            : Strings.Format("msg.km", km);

    /// <summary>Jumps forward to the next launch opportunity.</summary>
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
            ShowMissionError(Strings.CraftNoPath);
            return;
        }

        _drawable.Mission = mission;
        StartMission(launchDay);
    }

    /// <summary>
    /// Launches a craft toward the Moon from the date the view stands on, or
    /// cancels an ongoing lunar trip. The view is moved to Earth at the same
    /// time: the whole lunar trip fits within 0.003 AU, under a fifth of a
    /// pixel in the overview, so without zooming in there'd be nothing to
    /// see.
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
            ShowMissionError(Strings.CraftNoPath);
            return;
        }

        _drawable.Mission = mission;
        FocusOn(EarthBody);                            // zooms in via OnFocusChanged
        StartMission(launchDay);
    }

    /// <summary>Shared by both launches: the trip starts over from the beginning.</summary>
    void StartMission(double launchDay)
    {
        _arrivalSeen = false;
        _followCraft = false;
        UpdateMissionUi(launchDay);
        _settingsChanged = true;
    }

    /// <summary>Cancels the trip and forces a fresh check of the launch window.</summary>
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
        // If the moons are turned off, they should also leave the focus
        // selector, and if the camera was following one, focus falls back
        // to the Sun.
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
    /// Turns Halley's Comet on and off. The comet can also be followed, so
    /// the focus selector is rebuilt – the same way as when the moons are
    /// turned off.
    /// </summary>
    void OnHalleyChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowHalley = e.Value;
        RebuildFocusPicker(CurrentFocus());
        _settingsChanged = true;
    }

    // ----------------------------------------------------------- the probe selector

    /// <summary>
    /// The probes currently selectable in the focus selector, in the
    /// selector's order. The list is kept in parallel since the selector's
    /// contents change as probes are turned on and off – so the index can
    /// no longer be computed straight from ProbeData.
    /// </summary>
    readonly List<Probe> _focusProbes = new();

    /// <summary>The bodies in the focus selector, in the same order as the names after the Sun.</summary>
    readonly List<CelestialBody> _focusBodies = new();

    /// <summary>The planet a moon belongs to, or null for the planets themselves.</summary>
    readonly List<CelestialBody?> _focusParents = new();

    /// <summary>True while the focus selector is being rebuilt, so the change isn't interpreted as a choice.</summary>
    bool _rebuildingFocus;

    /// <summary>
    /// Builds the probe selector's rows from the probe data: first the five
    /// that left the Solar System, then the two orbiting a planet.
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

    /// <summary>A row in the selector: checkbox and name in the probe's own colour.</summary>
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

    /// <summary>Turns everything on or off via the checkboxes, which do the actual work.</summary>
    void SetAllProbes(bool visible)
    {
        foreach (var row in ProbeMenuItems.Children.OfType<HorizontalStackLayout>())
            if (row.Children.OfType<CheckBox>().FirstOrDefault() is { } check)
                check.IsChecked = visible;
    }

    /// <summary>
    /// A probe is turned on or off. If the probe the camera is following is
    /// turned off, focus falls back to the Sun and the view zooms out to
    /// the overview – the camera should never be left following something
    /// that isn't drawn.
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

    /// <summary>The button's text shows how many probes are selected.</summary>
    void UpdateProbeMenuButton()
    {
        int total = ProbeData.All.Length + ProbeData.Orbiters.Length;
        ProbeMenuButton.Text = Strings.Format("ui.probesButton",
            _drawable.VisibleProbes.Count, total);
    }

    /// <summary>
    /// Rebuilds the focus selector so it only lists the probes that are
    /// shown – you shouldn't be able to choose to follow something that
    /// isn't drawn. If the previous choice can't be kept, it falls back to
    /// the Sun, and the view zooms out to the overview; otherwise the
    /// camera would be left standing a hundred AU out in empty space.
    /// </summary>
    /// <summary>
    /// What's currently selected in the focus selector: null for the Sun,
    /// otherwise the body or the probe. The selection is tracked as an
    /// object rather than a text string – names switch language, and a text
    /// string wouldn't be recognisable afterward.
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

        // Each planet is followed by its moons, with a dot in front so the
        // grouping shows in a list that can't indent rows. Moons are only
        // listed while drawn – you shouldn't be able to follow something
        // that isn't visible, the same rule as for the probes.
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
        // The comet last among the bodies, and only while drawn. Being able
        // to follow it matters more than for the planets: the orbit is
        // sixty times longer than it is wide, so without the camera in
        // place the comet disappears from view for decades.
        if (_drawable.ShowHalley)
        {
            _focusBodies.Add(SolarSystemData.Halley);
            _focusParents.Add(null);
            names.Add(Strings.Name(SolarSystemData.Halley.Key));
        }

        names.AddRange(_focusProbes.Select(p => p.Name));

        // If the selection is lost – because the probe was turned off, or
        // the moons were hidden – it falls back to the Sun.
        int found = 0;                                  // null means the Sun
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
            return;     // the selector is refilled on a language switch, not something the user did

        _drawable.StarDensity = (StarDensity)Math.Max(0, StarDensityPicker.SelectedIndex);
        _settingsChanged = true;
    }

    void OnFocusChanged(object? sender, EventArgs e)
    {
        if (_rebuildingFocus)
            return;     // the selector is being rebuilt, nothing the user clicked

        _focusIndex = Math.Max(0, FocusPicker.SelectedIndex);
        _followCraft = false;
        _drawable.FocusedProbe = FocusedProbe;
        // The limit first, otherwise the new distance gets clamped by the previous body's.
        _drawable.Camera.MinDistance = FocusMinDistance();

        if (FocusedProbe is { } probe)
        {
            ZoomToProbe(probe);
        }
        else if (_focusIndex - 1 is int body && body >= 0 && body < _focusBodies.Count)
        {
            // A moon fills the frame; a planet is framed so its whole moon
            // system fits.
            _drawable.Camera.Distance = _focusParents[body] is null
                ? _drawable.SuggestedFocusDistance(_focusBodies[body])
                : _drawable.SuggestedMoonDistance(_focusBodies[body]);
        }

        _settingsChanged = true;
    }

    /// <summary>
    /// Jumps to the next meeting of the selected kind. The search starts at
    /// the date currently displayed, so pressing again gives the next one
    /// in turn – and searching backward means changing the date first.
    ///
    /// A couple of things are worth knowing about what's shown. At
    /// opposition the planet stands closest to Earth, and the distance
    /// differs considerably between occasions: Mars can be 0.37 AU away on
    /// a favourable one and 0.68 on an unfavourable one, which is the whole
    /// reason some oppositions make the news. At conjunction the two merely
    /// stand in the same direction as seen from here – in space they can be
    /// billions of kilometres apart.
    /// </summary>
    void OnNextMeetingClicked(object? sender, EventArgs e)
    {
        int index = MeetingPicker.SelectedIndex;
        if (index < 0 || index >= SkyEvent.Choices.Length)
            return;

        var choice = SkyEvent.Choices[index];
        double day = (CurrentDate - SolarSystemData.EpochJ2000).TotalDays;

        _meetingChoice = choice;
        _meeting = SkyEvent.Next(choice.Kind, choice.A, choice.B, day);
        UpdateMeetingLabel();

        if (_meeting is not { } meeting)
            return;

        GoToDate(SolarSystemData.EpochJ2000.AddDays(meeting.Day));

        // For an eclipse, the explanation is worth more than the date. Set
        // up the view so it's visible: the Moon's orbit shown and the camera
        // at Earth, where you can see the Sun standing on the node line that
        // very day and so being able to get in the way.
        if (choice.Kind is SkyEvent.Kind.SolarEclipse or SkyEvent.Kind.LunarEclipse)
        {
            MoonOrbitCheck.IsChecked = true;
            MoonsCheck.IsChecked = true;
            FocusOn(EarthBody);
        }

        // Jumping to Halley's perihelion without lighting up the comet would
        // be travelling to an empty date. The camera, though, is left where
        // it stands: it's in the overview that you see what actually
        // happens, the comet diving in through the whole planetary system.
        // To get close, it's in the focus selector.
        if (choice.Kind is SkyEvent.Kind.Perihelion)
            HalleyCheck.IsChecked = true;
    }

    /// <summary>
    /// Writes the line about the last meeting searched for. Separate from the
    /// button so the language selector can call it too – the date, the numbers
    /// and the wording all follow the language, and the search doesn't need
    /// running again to say the same thing in another one.
    /// </summary>
    void UpdateMeetingLabel()
    {
        // Nothing searched for yet, so there's nothing to say.
        if (_meetingChoice is not { } choice)
            return;

        if (_meeting is not { } meeting)
        {
            MeetingLabel.Text = Strings.NoMeeting;
            return;
        }

        var date = SolarSystemData.EpochJ2000.AddDays(meeting.Day);

        // The whole sentence, not just the number: what was found, when, and how close.
        string detail = choice.Kind switch
        {
            SkyEvent.Kind.Opposition =>
                Strings.Format("msg.meetingOpposition", meeting.DistanceAu),
            SkyEvent.Kind.SolarEclipse =>
                Strings.Format("msg.meetingSolarEclipse", meeting.SeparationDeg),
            SkyEvent.Kind.LunarEclipse =>
                Strings.Format("msg.meetingLunarEclipse", meeting.SeparationDeg),
            // At perihelion the distance to the Sun is always the same, so
            // what's worth knowing is what the visit looks like from here:
            // how close to Earth the comet comes, and how far from the Sun
            // it stands in the sky. Under about ten degrees it drowns in
            // daylight.
            SkyEvent.Kind.Perihelion =>
                Strings.Format("msg.meetingPerihelion",
                    meeting.DistanceAu, meeting.SeparationDeg),
            _ => Strings.Format("msg.meetingConjunction", meeting.SeparationDeg),
        };
        MeetingLabel.Text = Strings.Format("msg.meetingLine", ChoiceLabel(choice), date, detail);
    }

    void OnMoonOrbitChanged(object? sender, CheckedChangedEventArgs e)
    {
        _drawable.ShowMoonOrbit = e.Value;
        _settingsChanged = true;
    }

    /// <summary>
    /// Collapses the control panel down to just its bar. The panel has
    /// grown to five rows and then takes up a fifth of the window, which is
    /// a lot when all you want to do is look at the Solar System.
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

    // ------------------------------------------------------------ mouse & gestures

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

    // --------------------------------------------- keyboard & scroll wheel

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
            case (Windows.System.VirtualKey)187: // "+" on the main keyboard
                _drawable.Camera.ZoomBy(0.86f);
                break;
            case Windows.System.VirtualKey.S:
            case Windows.System.VirtualKey.Subtract:
            case (Windows.System.VirtualKey)189: // "-" on the main keyboard
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
