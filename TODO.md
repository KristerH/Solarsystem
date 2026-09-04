# To do: moons, rings and belts

The plan is to add one or a few items at a time, so that each step can be
verified before the next one begins. Tick off with `[x]` when a stage is
finished and approved.

**Done so far:** Stages 1–8 are complete and verified – moons, rings and both
small-body belts are in place.

**The order of what remains** is deliberate: the space flight (9) starts with
being able to change the date, which the Voyager stage (10) needs in order to
rewind to 1977. Surfaces and rotation (11) stands on its own and can be moved
forward if you would rather have something visual in between. Language support
(12) comes last, so that all the texts are in place and only need to be moved
out once.

---

## Stage 1 – Infrastructure: a general moon system

- [x] Rebuild the moon code so that every planet can have a list of moons in
      `SolarSystemData` (today Earth's Moon is a special case in the renderer).
      Same rules as for the Moon: geocentric/planetocentric orbital elements,
      Kepler computation around the parent planet, only visible when zoomed in,
      no name label (or a small label at high zoom – the decision is made here).
- [x] In magnified mode the moons are shown at a compressed distance (like the
      Moon's 3 × Earth radii) but with correct relative distance proportions
      between the moons within the same system.

**Verify:** Earth's Moon looks and behaves exactly as it did before the rebuild.

---

## Stage 2 – The moons of Mars

- [x] Phobos (radius 11 km, orbital period 7.7 hours – faster than Mars rotates!)
- [x] Deimos (radius 6 km, orbital period 30.3 hours)

**Verify:** Focus Mars, lower the speed to a few hours/sec. Phobos should manage
several laps per Martian day. Both are irregular little rocks – in the app they
become dots.

---

## Stage 3 – Jupiter's Galilean moons (the greatest teaching value!)

- [x] Io (orbital period 1.77 days)
- [x] Europa (3.55 days)
- [x] Ganymede (7.15 days – larger than Mercury)
- [x] Callisto (16.69 days)

The orbital periods form an almost exact 1:2:4 resonance (Io:Europa:Ganymede) –
worth being able to show. These were the four Galileo saw in 1610.

**Verify:** Focus Jupiter at ~1 day/sec: Io clearly spins around fastest,
Callisto slowest. Check the resonance: two Io laps per Europa lap.

---

## Stage 4 – Pluto and Charon (the double planet)

- [x] Charon (orbital period 6.39 days, radius 606 km – half of Pluto's!)
- [x] Possibly: a shared centre of mass outside Pluto, so that Pluto "wobbles"
      too – that is what makes the pair almost a double planet.
- [x] The small moons Styx, Nix, Kerberos, Hydra (dots, can be skipped)

**Verify:** Focus Pluto. Charon should be strikingly large relative to Pluto
compared with other moons.

---

## Stage 5 – The largest moons of Saturn and the ice giants

- [x] Saturn: Titan (15.9 days; possibly Rhea and Enceladus as well)
- [x] Uranus: Titania, Oberon, possibly Miranda
- [x] Neptune: Triton (5.88 days, **retrograde** – it orbits backwards!)

**Verify:** Triton should travel in the opposite direction to everything else in
the app – the clearest evidence that it was captured from the Kuiper belt.

---

## Stage 6 – Rings around the other giant planets

The same technique as Saturn's existing rings, but thinner and fainter:

- [x] Jupiter: a very thin, dark dust ring
- [x] Uranus: narrow dark rings – and Uranus's extreme axial tilt (98°!) should
      be added at the same time so that the rings stand "on edge"
- [x] Neptune: a faint ring (the arcs/clumps can be simplified away)

**Verify:** The rings are only visible when zoomed in and do not obscure the
planets in the overview. Uranus's rings should stand almost perpendicular to the
orbital plane.

---

## Stage 7 – The asteroid belt (around the Sun)

- [x] A discreet band of small dots between Mars (1.52 AU) and Jupiter
      (5.20 AU), densest around 2.2–3.3 AU. Randomised orbits with a spread in
      inclination (± a couple of degrees) and eccentricity, rotating at
      Keplerian speeds (the inner lap faster than the outer).
- [x] A "Show the asteroid belt" checkbox (off by default, so that the view does
      not get cluttered).
- [x] Possibly: the dwarf planet Ceres as a named dot in the belt.

**Verify:** The belt should look sparse even in the app – one teaching point is
that the asteroid belt is mostly empty space in reality (space probes fly
straight through without trouble). Performance: no noticeable degradation when
rotating/zooming (the points are cached like the star sky).

---

## Stage 8 – The Kuiper belt (around the Sun, beyond Neptune)

- [x] A sparse band of icy dots at about 30–50 AU, with a wider spread in
      inclination than the asteroid belt. Pluto sits in the middle of it – good
      to be able to show.
- [x] The same checkbox as the asteroid belt, or one of its own.

**Verify:** Zoom out and tilt the camera: the Kuiper belt should be thicker and
"fuzzier" vertically than the asteroid belt, and Pluto's inclined orbit should
lie within its swarm.

---

## Stage 9 – A space flight to Mars or the Moon

A little space flight of your own: the pupils pick a destination, launch a
spacecraft and follow it all the way there. This is where everything the app can
already do – orbits, time and scale – comes together in something the pupils
steer themselves. The stage is split into five parts, each of which can be built
and verified on its own.

### 9.1 – Change which date you are on

Needed first, and useful even without space flights: you should be able to jump
to any date at all, both backwards and forwards, instead of always starting on
today's date and only being able to go forwards, as at present.

- [x] A date field where you type a year, a month and a day, and the view jumps
      there.
- [x] Buttons for stepping ± a day, ± a month and ± a year, so that you can leaf
      your way forward without typing.
- [x] A "Today" button that resets to the present.
- [x] Let the speed slider go backwards, so that time can be played in reverse.

The Kepler maths already handles negative time, so the work lies in the user
interface and in the clock (`_startDate` plus `_simDays` in `MainPage`). This is
also the foundation for the launch windows in 9.3, and is needed in stage 10 in
order to rewind to Voyager's launch in 1977.

**Verify:** Jump to a known date and check that the planets are plausibly
placed. Step backwards across a New Year and a leap day. Run time in reverse and
watch the planets go anticlockwise. A fun side effect: look up your own birthday
and see where the planets stood then.

---

### 9.2 – The spacecraft and the transfer orbit to Mars

- [x] **The spacecraft as a celestial body**: a small dot with a name and a
      trail behind it (the last couple of hundred positions), following a Kepler
      orbit exactly as the planets do.
- [x] **Hohmann orbit**: the most energy-efficient route is half an ellipse with
      perihelion at Earth's orbit (1.00 AU) and aphelion at Mars (1.52 AU). The
      semi-major axis then becomes 1.26 AU, which gives a travel time of roughly
      259 days – half that orbit's period.
- [x] The orbital plane must be tilted so that the spacecraft meets Mars
      vertically too; Mars lies up to 1.85° out of the ecliptic.

**Verify:** The travel time should come out at roughly 259 days, and the
spacecraft's position on arrival should coincide with Mars – not merely be at
the right distance from the Sun.

---

### 9.3 – Launch windows

- [x] The spacecraft has to be launched when Mars is 44.3° ahead of Earth.
      During those 259 days Mars only manages 135.7° of its lap, while the
      spacecraft covers 180° – and 44.3 + 135.7 = 180.
- [x] The alignment repeats every 780 days (25.6 months). The "Launch" button
      should be disabled in between, with "Jump to the next launch window" next
      to it.
- [x] Show how long is left until the next window.

This is why real Mars missions are always launched in clusters: in the summer of
2020 the USA, China and the United Arab Emirates each sent a probe within two
weeks, and then nothing happened for two years.

**Verify:** If Mars is in the wrong place at launch, the spacecraft should
arrive at empty space – being 20° off corresponds to 80 million km. That is the
whole point of launch windows, and something the pupils can try for themselves.

**Corrected afterwards (discovered in 10.1):** both the orbit and the window
criterion rested on the assumption that the journey sweeps the shortest way
between Earth and Mars. In all eight windows tested the shortest way was
backwards around the Sun, so the spacecraft flew in the wrong direction and
would have required 63 km/s relative to Earth instead of the real world's 3–4.
The arrival at Mars was correct all the same, which is why it did not show up in
verification. The orbit is now solved with the Lambert solver from 10.1, and the
travel time is chosen so that the launch is as cheap as possible. At the same
time the window criterion has been changed from the sweep angle – which pointed
straight into the point where the orbital plane becomes undefined – to the cost:
the window is open when the launch costs at most 0.1 km/s more than the window's
best day.

---

### 9.4 – A journey to the Moon

- [x] The same thing but around Earth: an ellipse from low Earth orbit out to
      the Moon's distance, travel time about 3 days. Requires that the
      spacecraft can orbit a planet instead of the Sun, much as the moons do
      today.

The "Launch towards the Moon" button can be pressed on any day at all, and the
view jumps to Earth at the same time – the whole lunar journey fits within
0.003 AU and would otherwise be less than a pixel. The spacecraft orbits Earth
instead of the Sun: the orbit is computed with Earth's gravitational parameter
and placed in the Moon's own orbital plane. The launch takes place from a low
orbit at 400 km altitude, which becomes the orbit's perigee.

A pure Hohmann orbit out to the Moon would take 4.95 days. To manage it in
three, the spacecraft has to be launched with more speed, so that the far end of
the orbit ends up 440,000–630,000 km away, that is, a good way beyond the Moon,
and the Moon is caught up on the way out – before the turning point. That is
exactly how Apollo flew.

**Verify:** The lunar journey should take about 3 days. A good contrast to Mars:
the Moon is back in the same place every 27 days, so you can travel there more
or less whenever you like.

Checked: the travel time comes out at 3.00 days and the spacecraft meets the
Moon to the metre for 40 launch dates spread over a year. The launch speed comes
out at 10.84 km/s, which is the real speed at a launch towards the Moon (Apollo's
rocket stage gave 10.8 km/s), and the speed falls to 0.6–0.9 km/s on arrival.
The orbital plane coincides with the Moon's to within 0.02°.

---

### 9.5 – A panel during the journey, and arrival

- [x] **Panel**: elapsed travel time, remaining time, distance left to the
      destination and the spacecraft's speed.
- [x] **Arrival**: the spacecraft meets its target and the journey is marked as
      finished. Already done in 9.2: the spacecraft follows the planet after
      arrival instead of being left standing where the planet happened to be,
      and the label changes to "Spacecraft arrived".
- [x] **The camera** follows it down to the planet on arrival.

The panel appears at the top left as soon as a spacecraft is on its way, and
disappears when the journey is cancelled. On arrival it switches to travel time,
arrival date and the speed on arrival. At the same moment the camera latches on
to the spacecraft and zooms in to the destination – once, at the very instant of
arrival, so that the user is then free to steer again. A new choice in the focus
picker, or "Reset view", releases the grip.

**Verify:** The speed should vary along the orbit – fastest at launch near the
Sun and slowest on arrival, exactly as Kepler's second law says.

Checked: the speed falls monotonically the whole way, towards Mars from 33.14 to
20.51 km/s and towards the Moon from 10.83 to 0.62 km/s. The distance to the
Moon shrinks the whole way, but the distance to Mars first grows from 246 to
409 million km before it falls – the spacecraft goes around the Sun after all,
not straight towards the planet, and Mars is on the far side of the Sun at
launch.

---

## Stage 10 – Voyager and the other space probes

The craft humanity has actually sent out. Stage 9 is about an imagined journey
that the pupil steers; this one is about the real journeys, with real dates.
Voyager 1 is the most distant object humans have built.

The stage is the largest so far and is therefore split into five parts, each of
which can be built and verified on its own – the same arrangement as stage 9.

The guiding idea for the whole stage: the probes' orbits are not built from
entered orbital elements but from the real dates. Each leg of the journey is the
orbit that goes from one planet to the next in exactly the time the flybys
actually took, and it is computed from the app's own planetary positions. That
way the probes end up at the right planet on the right day by themselves, and
the gravity assist shows up as a jump in speed between two legs – which is
precisely what it is.

### 10.1 – Hyperbolic orbits in the Kepler code

- [x] **The hyperbolic Kepler equation**. The probes have been given so much
      speed that they will never come back: their orbits have an eccentricity
      greater than 1 and are therefore hyperbolas, not ellipses. Today
      `SolveKepler` only solves `E - e*sin E = M`, which holds for ellipses. For
      hyperbolas `e*sinh H - H = M` is needed, along with a position formula of
      its own. This is the stage's only real piece of mathematics and should be
      done first.
- [x] **A conic from a state**. A probe changes orbit at every planetary flyby,
      so the orbit has to be constructible from a position and a velocity
      instead of from fixed orbital elements like the planets'. The same class
      must handle both ellipses and hyperbolas; the semi-major axis becomes
      negative for the latter.
- [x] **The Lambert solver**: the orbit that goes from one position to another
      in a given time. It is what makes it possible to build the probes from
      real dates.

This lives in `Simulation/Kepler.cs`, `Simulation/Conic.cs` and
`Simulation/Lambert.cs`. None of it is visible in the app yet – it is the core
that 10.2 onwards builds on. Lambert uses universal variables, where a single
variable z describes every conic: positive for ellipses, negative for hyperbolas
and zero for the parabola in between. The travel time grows monotonically with
z, so the right orbit is squeezed in by bisection.

Two traps showed up along the way. The initial guess for the hyperbolic Kepler
equation has to be arsinh(M/e) and not the more obvious M/(e-1): for orbits just
above the parabola the latter becomes enormous, and sinh of a large number blows
the floating-point range immediately. And below a certain limit for z there is
no orbit at all – the conic simply does not reach – which the search has to
count as "faster than everything else" so as not to get stuck there.

**Verify:** Can be done entirely without a user interface. An orbit built from a
state must give back the same position and speed it was built from. Lambert must
reproduce the Mars orbit from 9.2, and a hyperbolic orbit must be computable
both forwards and backwards in time.

Checked: the hyperbolic Kepler equation is solved with a relative error below
1e-12 in 66 cases, with eccentricities from 1.0001 to 12 and mean anomalies up
to 4000, in both time directions. An orbit built from a state gives back the
same position to within a couple of kilometres and the same speed to within a
hundredth of a metre per second. All seven real probe legs that 10.2 and 10.3
need can be solved, and the probe ends up at the right planet on the flyby day:
the worst is Voyager 2 at Jupiter with 21,000 km, that is a third of a Jupiter
radius, and the others are below 1,000 km. Five of the seven legs are hyperbolas.

**Note:** Lambert simultaneously exposed a bug in stage 9.2. The Mars spacecraft
there assumes that the journey sweeps the shortest way between origin and
destination, but in all eight launch windows tested the shortest way is
backwards around the Sun. The orbit therefore goes in the wrong direction, and
would require 63 km/s relative to Earth instead of the real world's 3–4. The
arrival at Mars is correct all the same, which is why it was not discovered
earlier. The right solution is the same sweep angle but the other way round –
190° instead of 170° – and you get it for free by letting `Mission` use the
Lambert solver. The lunar journey in 9.4 is not affected; it builds its
direction from the Moon's own orbit.

The fix is done: see the note under 9.3. Afterwards all five nearest windows go
in the right direction, cost 2.90–3.20 km/s and fall in the same rhythm as the
real Mars windows.

---

### 10.2 – Voyager 1 and 2

- [x] **Voyager 1 and Voyager 2** (launched in 1977) with real directions and
      speeds. Voyager 2 is the only craft to have visited all four giant planets
      – made possible by a planetary alignment that only comes round every 176
      years.
- [x] **Out of the ecliptic**: after Saturn, Voyager 1 bent off steeply upwards
      (about 35°) and Voyager 2 downwards (about 48°). A good opportunity to
      show that the solar system is a disc which the probes have now left.
- [x] The probes are drawn in the view with their trails behind them.

This lives in `Simulation/Probe.cs` and `Simulation/ProbeData.cs`, with the
"Space probes" checkbox in the panel. No orbital elements are entered: each leg
is the orbit that goes from one planet to the next in exactly the time the
flybys took, and the last leg goes out to the probe's known position today
(distance and direction on the sky). The inclination out of the ecliptic is
therefore a result and not an input.

**Verify:** Wind time to March 1979 – Voyager 1 should then be at Jupiter, not
somewhere else. The same for Voyager 2 at Neptune in August 1989. Check today's
distances against NASA's figures (Voyager 1 is around 167 AU and Voyager 2
around 140 AU in 2026). Tilt the camera and see that the two Voyager probes have
left the ecliptic in opposite directions.

Checked: all six planetary flybys hit the right planet on the right day, the
worst being 602 km off at Neptune, that is two hundredths of a planetary radius.
Today (August 2026) Voyager 1 is at 169 AU and Voyager 2 at 142 AU, with speeds
of 16.7 and 15.0 km/s – NASA's figures are 167 and 140 AU together with 17.0 and
15.4 km/s. The probes are inclined +35.6° and −47.9° respectively to the
ecliptic, against the accepted 35° and 48°; since the inclination is not an
input, that is a check that the whole chain holds together. Two of the eight legs
are ellipses – precisely the two from Earth to Jupiter, because it was Jupiter
that gave the probes enough speed never to come back. The rest are hyperbolas.

The speed jumps are already visible in the orbits, even though the panel that
displays them belongs to 10.4: Jupiter gave Voyager 1 +10.8 km/s and Voyager 2
+10.0. At Neptune, Voyager 2 was *braked* by 2.3 km/s – the price of turning
down towards the moon Triton, and the reason it left the ecliptic more steeply
than its twin.

---

### 10.3 – Pioneer 10 and 11, and New Horizons

- [x] **Pioneer 10 and 11, and New Horizons** (Pluto 2015). Five craft are on
      their way out of the solar system.

Built the same way as the Voyagers in 10.2, with their real flyby dates:
Pioneer 10 past Jupiter on 4 December 1973, Pioneer 11 past Jupiter on
3 December 1974 and Saturn on 1 September 1979, New Horizons past Jupiter on
28 February 2007 and Pluto on 14 July 2015.

**Verify:** New Horizons should pass Pluto in July 2015 – and Pluto is then far
outside the plane of the ecliptic, so it is also a test that the orbits really
are computed in three dimensions.

Checked: all eleven planetary flybys hit the right planet on the right day. The
worst is Voyager 2 at Neptune with 602 km; New Horizons meets Pluto at 319 km,
which is a quarter of a Pluto radius. Today's positions agree with the known
ones: Pioneer 10 at 142 AU with 11.8 km/s (the answer being ~140 AU and 11.9),
Pioneer 11 at 120 AU with 11.3 km/s (~118 and 11.2), New Horizons at 65 AU with
13.3 km/s (~63 and 13.9). All thirteen legs are prograde.

**A correction to the verification above:** in July 2015 Pluto was not below but
just *above* the ecliptic, and not "far" measured as an angle – only 1.91°.
Measured as a distance it is nevertheless 1.10 AU outside the plane, that is,
more than Earth's entire orbital radius, so the three-dimensional test holds.
Pluto was on its way down and crossed the plane a few years later. That the
probe meets Pluto there, and not in the plane, is exactly what was to be shown.

Pioneer 11 turned out to be the stage's interesting case. Jupiter did not fling
it outwards but inwards and clean across the solar system: the orbit falls from
4.97 AU in to 3.79 AU, goes half a lap around the Sun – from longitude 351° to
167° – and then climbs out to Saturn at 9.38 AU, at most 11.6° above the
ecliptic. The leg therefore sweeps 176°, dangerously close to the point where
the orbital plane becomes undefined, but the solution hits Saturn to within
144 km. That leg is an ellipse into the bargain: after Jupiter, Pioneer 11 did
not have enough speed to leave the solar system, and would have come back had
Saturn not flung it out. The same holds for Pioneer 10 and both Voyager probes
at Jupiter – their first legs are ellipses, the rest hyperbolas. New Horizons is
the exception: it was on a hyperbola right from launch, the fastest ever made.

---

### 10.4 – Milestones, panel and scale

- [x] **The planetary flybys as milestones** with dates, e.g. Voyager 1 at
      Jupiter in March 1979 and at Saturn in November 1980, Voyager 2 at Neptune
      in August 1989. Can be shown as marked points along the orbit.
- [x] **Gravity assist**: the probes gained speed by swinging past the planets.
      Show the speed in a panel so that the jumps at each flyby are visible –
      that is the very explanation of how they could reach so far.
- [x] **The scale**: the probes are today more than 100 AU away, three times
      farther than Neptune. The camera has to be able to zoom out that far, and
      then the whole planetary system shrinks to a dot – which is itself the
      point. The probes should also be selectable in the focus picker, otherwise
      they are hard to find out there.

The milestones fall out of the legs instead of being entered: each leg begins at
one, and the speed jump is the difference between the ending and the beginning
leg's speed at the same point. They are drawn as rings along the trail, with
years – and, for the probe selected in the focus picker, with the planet's name,
the date and the speed jump. The years do not go through the label stacking that
the celestial bodies use; eleven flybys stacked downwards would have become a
pillar of text right across the view, so a year is instead skipped when it would
land on top of one already written.

If you select a probe in the focus picker, the camera follows it and zooms out
to a little over twice the probe's distance, so that the Sun just fits in the
frame and the whole planetary system shrinks to a dot. The camera's ceiling was
raised from 25,000 to 40,000 units (666 AU) to make that possible.

**Verify:** The speed should jump upwards at every flyby and then slowly fall
while the probe climbs out of the Sun's gravity.

Checked: the speed falls monotonically on the last leg for all five probes.
Voyager 1 goes from 27.4 km/s shortly after launch to 20.4 at Saturn in 1982,
17.7 in 1990 and 16.67 today – the curve flattens out, exactly as it should when
the Sun's grip weakens with distance. The jumps at the flybys: Pioneer 10 got
+12.1 km/s from Jupiter, Voyager 1 +10.8, Voyager 2 +10.0 and Pioneer 11 +7.1
followed by +5.6 at Saturn. Two flybys gave no speed at all: Neptune took
2.3 km/s from Voyager 2, and Pluto took 0.3 from New Horizons – the dwarf planet
is simply too light to fling anything. The camera handles all five probes within
the ceiling, with the Sun still in frame.

---

### 10.5 – Orbiting probes

- [x] **Orbiting probes** such as Cassini at Saturn (1997–2017) and Juno at
      Jupiter – simpler cases, ordinary ellipses around a planet.

Note: Cassini's journey out to Saturn (1997–2004) went via Venus, Venus, Earth
and Jupiter, and those legs wind more than a full lap around the Sun. The
Lambert solver in 10.1 cannot handle such orbits, as it only deals with less
than one lap. Cassini is therefore shown from its arrival in 2004, and Juno from
its arrival in 2016.

An important difference from the rest of the stage: these two orbits are **not**
reconstructed from real dates. Cassini flew nearly three hundred different laps
over thirteen years, with orbital periods from a week to four months and
inclinations from the ring plane up to 75 degrees, so there is no single orbit
to show. What is drawn is a representative lap – size, shape, orbital period and
orbital plane are real, but where the probe is in the orbit on a given date is
not. The orbits are therefore given in planetary radii and inclination to the
planet's equator, which are the measures such orbits are usually described with,
instead of in orbital elements.

The inclination is measured against the planet's equator and not against the
ecliptic: a polar orbit is 90 degrees to the equator regardless of how the
planet itself is tilted. That is arranged by adding the inclination to the
planet's own with the same ascending node – turning the equatorial plane a
quarter turn about the node line gives precisely a plane through both poles.

The orbits are compressed by the same factor as the planet's moons; otherwise
they would disappear inside the magnified globe. It comes out right
proportionally too: Cassini's lap is almost exactly the same size as Titan's
orbit, and Juno's reaches a good four times farther out than Callisto.

**Verify:** Cassini's lap around Saturn should take a couple of tens of days and
Juno's around Jupiter about 53 – and Juno's orbit should go over the poles, not
along the equator like the moons'.

Checked: Cassini 16.00 days and Juno 53.42. The orbital planes measured against
the planet's equator give Cassini 20.0° and Juno 90.0°, that is exactly polar,
while the control case Io gives 0.0° – the moon lies in the equatorial plane,
exactly as it should. Juno's speed at perijove comes out at 57.7 km/s, which
agrees with the roughly 58 km/s that make Juno the fastest object humans have
sent relative to a planet; at apojove it is down to 0.54 km/s. A full lap brings
the probe back to the same point to within less than a metre.

**Things to bear in mind:** neither of the two is visible at the app's start
date. Cassini was ended on 15 September 2017 by being steered down into Saturn's
atmosphere, so that a probe carrying terrestrial bacteria would not risk one day
crashing on Enceladus with its ocean beneath the ice. For Juno the end date is
set to the extended mission's planned end on 30 September 2025; if it continued
after that, the end date is one line to change in `ProbeData`. To see them you
therefore have to set the date back – Cassini between 2004 and 2017, Juno
between 2016 and 2025 – and select Saturn or Jupiter respectively in the focus
picker. Juno's orbit is so wide that you need to zoom out a couple of steps to
see the whole ellipse.

---

### 10.6 – Choose which probes are shown

- [x] A picker where each probe can be ticked individually, instead of today's
      "Space probes" checkbox that turns them all on and off. It should be
      possible to show only Voyager 1, or Voyager 1 and 2, or any other
      combination at all.
- [x] The choice should apply to everything about the probe: the dot, the trail
      and the milestones.
- [x] If the probe selected in the focus picker is turned off, focus falls back
      to the Sun. The camera must never be left following something that is not
      drawn.

The need comes out of 10.2–10.4. Five probes with trails and eleven flybys make
the overview cluttered, and most of what you want to look at concerns one or two
at a time: the two Voyager probes' opposite inclinations out of the ecliptic, or
Pioneer 11's detour across the solar system compared with the Voyagers' straight
route. Today the only choice is all five or none.

MAUI's `Picker` can only select one option, so a multiple-choice list does not
come ready-made. Three routes, in order from simplest to nicest:

1. Five checkboxes straight in the control panel. Simplest, but the panel is
   already cramped – that was precisely why the space-flight buttons needed a
   row of their own in 10.4.
2. A button that unfolds a box with the checkboxes in it, that is, a home-made
   drop-down: a `Border` with a `VerticalStackLayout` that is shown and hidden.
   Keeps the width down and is closest to what is asked for.
3. `CollectionView` with `SelectionMode="Multiple"` in the same unfolded box, if
   the selection style feels better than checkboxes.

In the code, `ShowProbes` in `SolarSystemDrawable` is no longer enough, being a
single on/off flag. The drawing today goes through `ProbeData.All`, so it needs
to ask instead which probes are selected – a set of the names, say, or a
`HashSet<Probe>`.

The focus picker should therefore only list the probes that are shown, so that
you cannot select a probe that is not on screen. That brings one thing to watch
out for: the picker's contents become variable, whereas `MainPage` today works
out which probe is selected from a fixed index – the Sun, the planets and then
`ProbeData.All` in order. That coupling has to be rebuilt so that it starts from
the visible probes and is redone every time the selection changes; otherwise the
index points at the wrong probe as soon as one is turned off.

Built as route 2 in the list above: the button "Space probes 7/7" unfolds a box
with one checkbox per probe, the names in the probe's own colour, plus "All" and
"None" to save seven clicks. The box sits as a box of its own on top of the view
and not in the control panel, partly because the panel is cramped, partly
because the text stack where the panels sit lets clicks through and the
checkboxes would then have been impossible to hit. The rows are built from probe
data, so a new probe turns up in the picker by itself.

**Corrected afterwards: there was no room for all seven.** The claim above –
that a new probe turns up by itself – held for the rows but not for the box
around them. The selector is anchored to the bottom of the view and grows
upward, so in a short window the rows past the top edge were simply cut off
rather than making the box scroll, and Cassini and Juno sat below the separator
where they could not be reached at all. The count on the button said 7 while the
list showed five and a half, which is what gave it away. The box is now capped
to the height of the view and the rows scroll inside it, so the number of probes
no longer has to fit the window. The list had to become a grid with a star row
for that: a stack hands every child its full desired height and would let the
list run off the edge again instead of scrolling.

The scroll view brought one thing of its own: the box stretched to the full
width of the window instead of hugging its rows, because a scroll view measures
itself to the width it is offered. Anchoring it and its grid to the start fixes
that. It applies to all three drop-downs, the selector's two younger siblings
from the control panel included.

The two orbiting probes from 10.5 came along into the same picker – they are
probes too, and the old checkbox controlled them as well. They are not in the
focus picker, however, since you look at them by selecting their planet.

`ShowProbes` in `SolarSystemDrawable` has been replaced by `VisibleProbes`, a set
of the names of the probes to be drawn. An empty set turns them all off, exactly
as the old checkbox did.

**Verify:** Tick only Voyager 1 and check that the trail, dot and milestones for
the other four disappear, while Voyager 1 looks exactly as before. Tick both
Voyager probes and tilt the camera: now the two opposite routes out of the
ecliptic should be comparable without the Pioneer probes and New Horizons
getting in the way. Untick them all and compare with how the view looks when
today's "Space probes" checkbox is turned off – it should be the same picture.

Checked: the focus picker's index logic tested in 22 cases against real probe
data. The trap foreseen in the plan is the most important one: if you are
following Voyager 2 and turn off Voyager 1, Voyager 2 moves from position 11 to
position 10 in the picker, and since the selection is preserved by name instead
of by index the right probe is still followed afterwards. If you turn off the
probe you are following, focus falls to the Sun and the view zooms out to the
overview; if you are following a planet, nothing is affected; "None" followed by
"All" gives back the starting state.

---

## Stage 11 – Surfaces and rotation on the other bodies

Earth already has continents, polar caps and real rotation. The same technique
can give the other bodies their distinguishing marks – they do not have to be as
detailed, but enough for you to recognise them and see that they spin.

No image texture is needed: the Earth globe draws longitude/latitude polygons
directly onto the sphere's surface, and the gas giants' bands are in fact
simpler than continents since they are only latitude belts. The stage is divided
up so that one or two bodies are taken at a time.

All eight planets rotate – but several do it so oddly that it is worth a lesson
of its own: Venus spins backwards and so slowly that its day (243 Earth days) is
longer than its year (225), Mercury manages exactly three turns in two years,
and the Moon rotates once per orbit and therefore always turns the same side
towards us.

### 11.1 – Infrastructure: general globe rendering

- [x] Extract `DrawEarthGlobe` into something all bodies can use, and `EarthMap`
      into one surface map per body. That amounts to 73 and 98 lines
      respectively, so the extraction itself is manageable. What costs is that
      Earth is today a special case all the way through: the axial tilt is a
      constant in the drawing code (`ObliquityRad`) and the rotation is computed
      from sidereal time. Both have to go into the general mechanism without
      Earth changing appearance.
- [x] Put rotation data on `CelestialBody`: rotation period (negative for
      retrograde) and the position of the prime meridian at the epoch.
- [x] **Axis data for the bodies that lack it.** The plan assumed that the pole
      directions were already there from the moon stages, and that holds for
      Mars, Jupiter, Saturn, Uranus and Neptune – as well as Pluto, whose plane
      lies in Charon's orbital elements. But Mercury, Venus and the Moon have
      neither moons nor rings and therefore no equatorial data at all. It has to
      be entered before 11.5 and 11.6 can be done:
      - Mercury is tilted 0.03°, that is, practically upright.
      - Venus is tilted 177.4°, almost upside down. That is the whole
        explanation for why it spins backwards, so that figure is the stage's
        most important one.
      - The Moon is tilted 1.5° to the ecliptic, 6.7° to its own orbit. (The
        plan had the two numbers swapped; corrected when the axis was computed.)
      Note that the tilt alone does not determine the axis – the node's position
      is needed too, exactly as for the equatorial planes that already exist.
- [x] Take the opportunity to move the existing equatorial constants from
      `SolarSystemData` to the bodies themselves. Today they sit as loose
      `const`s that the moons, the rings and the orbiting probes each fetch
      separately – Cassini and Juno reach all the way into
      `SolarSystemData.SaturnEquator...` to get hold of them.
- [x] The same threshold as today: the surface is only drawn once the globe is
      large enough.

This is how it turned out: `BodyAxis` describes a body's axis with the same four
numbers for all of them – the equator's tilt to the ecliptic, the node's
longitude, the rotation period and the prime meridian's position at the epoch –
and computes the three basis vectors that both the surface drawing and the rings
need. `SurfaceMap` (formerly `EarthMap`) is one surface map among several, with
Earth as the first instance. `DrawGlobe` takes the map and the axis instead of
knowing anything about Earth, and `DrawBody` chooses between globe and disc, now
for the ringed planets too – they were previously shut out of the globe branch
and could therefore never have been given a surface.

**One convention was changed relative to the plan.** It said here that the
rotation period should be negative for retrograde bodies. That did not fit with
the rest: a negative period presupposes that the pole always points north of the
ecliptic, but the moons' orbital planes are already written around the rotation
axis (Uranus's moons are inclined 97.7° for precisely that reason). Two
conventions in the same data would have required an inversion every time a moon
reads its planet's axis. Now the pole always points the way the right-hand rule
gives, the period is always positive, and retrograde rotation shows up as a tilt
over 90°: Venus 178.8°, Uranus 97.7°, Pluto 112.8°. Miranda can read `UranusAxis`
directly.

**Corrected afterwards: all the equatorial nodes were 180° wrong.** The numbers
came in during the moon stages, computed as the pole's longitude *minus* 90°
where it should be *plus*. The consequence was that every planet's equatorial
plane was tilted in exactly the opposite direction: the right tilt, the right
times for equinoxes and ring plane crossings, but the wrong hemisphere turned
towards the Sun. Moon by moon, ring by ring, and Charon and Triton as well. The
error was invisible as long as nothing was drawn on the surfaces – it is only
now, when the axis determines what you see, that it would have been noticed. All
the nodes have been moved 180° (Mars's tilt was corrected at the same time from
26.74° to 25.40°).

**Verify:** Earth must look and rotate exactly as before the rebuild. That is
the stage's most important check, since everything else builds on that code. The
moons' and rings' planes must also remain unchanged when the constants are
moved – Uranus's moons on edge and Triton's retrograde orbit are sensitive tests.

Checked outside the app, against the old code and against reality:

- **Earth is unchanged to the letter.** The new axis code against the old
  sidereal-time code, 10 points on Earth's surface × 9 dates between 1950 and
  2054: largest deviation exactly 0. Not "below a pixel" but the same
  floating-point numbers.
- **The axes agree with the IAU's pole directions** for all nine planets,
  deviation 0.000° – an independent route to the same numbers.
- **The moons lie in their planet's equatorial plane**: Phobos, Deimos, the four
  Galileans, Enceladus, Rhea, Titan, Uranus's three and Charon all 0.00°.
  Triton comes out at 156.91° to Neptune's equator, against a measured 156.9 –
  and that number only falls out if both Neptune's node and Triton's are
  corrected.
- **The seasons come out right**, which is the test that separates the right
  node from the wrong one: Uranus stands 7.9° from the Sun in January 1986
  (Voyager 2 arrived in the middle of the southern summer) and 90.0° in December
  2007 (the equinox, to the month). Saturn 63.3° in May 2017 (northern summer
  solstice during Cassini's last laps, and 63.3 is precisely 90 − 26.7) and 90.0°
  in May 2025, when the Sun really did cross the ring plane. Earth 66.6° at
  midsummer 2026. With the old nodes: Uranus 172.2° and Saturn 119.2°, that is,
  the wrong hemisphere in sunlight both times.
- **The Moon's tidally locked rotation** gives a sub-Earth longitude of −6.5° to
  +6.1° over 110 years, against the real optical libration of ±6.3°. No drift.
- **Cassini and Juno are unchanged** relative to their planet: 16.00 days and
  20.0° to Saturn's equator, 53.42 days and 90.0° to Jupiter's, the same
  eccentricities as in 10.5. Their orbital planes came along when the nodes were
  corrected.

Left to see with the eye: that the Earth globe is drawn as before in the app.
The numbers say it must be, but it is in the test list under R1 all the same.

### 11.2 – Mars

- [x] A red-brown surface with the dark regions (Syrtis Major is the clearest),
      white polar caps and preferably Valles Marineris as a line.
- [x] Rotation 24 h 37 min, almost like Earth's.

The map has twelve features: Syrtis Major as the dark triangle around 70° east,
Mare Acidalium, Sinus Sabaeus and Sinus Meridiani along the equator, Mare
Erythraeum, Tyrrhenum, Cimmerium and Sirenum, Solis Lacus ("the eye of Mars"),
Boreosyrtis at Utopia, the bright highlands Hellas and Tharsis, and Valles
Marineris as a line. The dark fields are not seas but bedrock that the wind has
swept clean of bright dust – which is why they slowly change shape between storm
seasons.

The polar caps are drawn with the same extent all year round. In reality they
breathe with the seasons, but the app has no model for frost, and that is stated
in the code comment.

**Verify:** The polar caps should stay still while the surface spins beneath
them.

Checked outside the app:

- **The polar caps stand still.** Over two Martian days the northern cap keeps
  to exactly 76.00° north and the southern to 74.00° south, while Syrtis Major
  sweeps 720.0° in the same time. The surface therefore spins beneath caps that
  do not move.
- **Six Mars landings come out at the right time of day**, which tests both the
  prime meridian and which way longitude is counted: Viking 1 15:43 (known
  16:13), Viking 2 09:52 (09:49), Pathfinder 02:54 (03:07), Opportunity 13:18
  (13:15), Curiosity 15:32 (14:53) and Perseverance 15:21 (15:53). Largest error
  40 minutes. The known times are mean solar time while the model computes true
  solar time, and Mars's equation of time reaches ±50 minutes – the orbit is so
  eccentric (e = 0.093) that the Sun is well ahead of or behind its mean
  position. If the sign of the longitude were wrong they would land 6–19 hours
  away; that Pathfinder falls before dawn and Viking 2 in the morning while the
  other four fall in the afternoon is what pins the sign down.
- **The Martian day is right in both forms**: the sidereal day 24:37:22 against
  a known 24:37:22, and the solar day, measured from the model as the time
  between two noons, 24:39:47 against a known 24:39:35. Twelve seconds of error,
  and the difference of a good two minutes between the two days falls out by
  itself – Mars covers a stretch of its orbit while it spins.
- **The features lie correctly on the globe**: the polygons' centroids come out
  at most 3° from the positions they should have, that is, within 200 km on the
  Martian surface.

**Smoothed outlines afterwards.** Once drawn, the fields were visibly angular –
pentagons and hexagons rather than patches. Mars's albedo boundaries are diffuse
dust boundaries, so they are now rounded with two rounds of Chaikin's corner
cutting. It is a choice per map and not a general change: Earth's coastlines
should remain angular, and Jupiter's bands must have straight edges so that the
quadrilaterals do not gape.

The longitudes are unwrapped into a continuous sequence before the corners are
cut. Without that, the mean of 358° and 8° would become 183°, that is, straight
across on the other side of the globe, and Sinus Meridiani lies exactly over the
prime meridian.

Checked afterwards: Earth's map unchanged across 11 surfaces and 507 points,
Mars grows from 391 to 529 points, the polar caps remain at exactly 76.00° north
and 74.00° south without gaps in longitude, and the features' centroids move at
most one degree (largest deviation from the measured position 4° against 3°
before). Valles Marineris is still a line and not a blob, which was the real
risk in rounding a narrow figure.

### 11.3 – Jupiter

- [x] Cloud bands in latitude, bright zones and dark belts.
- [x] The Great Red Spot as an oval in the southern hemisphere.
- [x] A rotation of only 9 h 55 min – the fastest in the solar system despite
      being the largest. Perfectly visible in the app at low speed.

Seven bands at their accepted latitudes, two polar regions, the Great Red Spot
and three of the white ovals at 41° south. A band cannot be drawn as a single
polygon – it is a ring with a hole in it – so each band is built from eight
quadrilaterals that overlap slightly at the edges, the same trick the rings
already use. The polar regions, on the other hand, are ordinary caps, exactly
like Earth's ice.

**The Red Spot's longitude is picked freehand**, and that is a deliberate
simplification stated in the code comment. The spot drifts westwards relative to
the planet's interior rotation, a full lap in 3.7 years, and the app does not
follow the drift. That the spot exists, how large it is and how it travels round
the limb is right; where it stands on a given date is not. Anything else would
be hard to get right – the drift has been irregular for over a hundred years.

**Verify:** The Red Spot should disappear round the limb and come back on the
other side after roughly five hours of simulated time.

Checked outside the app:

- **The spot is gone for 4 hours 59 minutes** and visible for 4 hours 56, seen
  from Earth's real position over three days in March 2026. Together 9 h 55 min
  34 s, that is exactly one lap. That the two halves are not quite equally long
  is correct: Jupiter is tilted 2.2° and Earth is not in the equatorial plane.
- **The rotation period** comes out at 09:55:29 against System III's 09:55:30,
  and Jupiter is the fastest of all the bodies in the data – 2.42 laps per Earth
  day. The equator races along at 12.3 km/s, twenty-seven times Earth's 0.46.
- **The band boundaries agree to the tenth** with the accepted ones: the
  equatorial zone ±7°, the north equatorial belt 7–17°, the southern 20–7°
  south, and the temperate ones in pairs out towards the poles.
- **The Red Spot measures 16,517 × 11,958 km**, against a measured roughly
  16,000 × 12,000. It is therefore wider than Earth, whose diameter is
  12,742 km – which is the whole point of showing it.
- **The cost**: 62 surfaces and 1,584 points, against Earth's 11 and 507. Three
  times as much, but only while you are zoomed in on Jupiter.

**Corrected after seeing the map drawn, twice.** First the palette was too hard.
Dark brown belts against cream white gave a striped ball rather than a planet –
in photographs the difference between belt and zone is surprisingly small, and
it is the pattern that carries it, not the contrast. The tones now lie close
together, the Red Spot is orange instead of brick red, and the polar region is
drawn in two steps (55° and 70°) since a single cap became a hard grey dome on
top.

The second time the problem was a different one: all the belts were equally
strong, which gave a beach ball. In a photograph the two equatorial belts
dominate while the temperate ones are barely visible. Now there are five tones
instead of two, and the belts fade away towards the poles. That is the version
seen in the picture.

The checks were made by drawing the maps outside the app with the same
mathematics – `BodyAxis.Direction`, the same cap clipping, the same orthographic
projection – with the Earth globe as a reference: if Africa is recognisable, the
port is faithful. That confirmed two things at once that numbers could not
settle: the quadrilaterals' seams are invisible, and the caps fill in the right
direction.

### 11.4 – Saturn, Uranus and Neptune

- [x] Saturn: fainter bands than Jupiter's, in a yellowish beige. Rotation
      10 h 39 min.
- [x] Uranus: almost a single blue-green colour – the point is that it is so
      smooth. Rotation 17 h 14 min, retrograde and on its side.
- [x] Neptune: blue with the Great Dark Spot. Rotation 16 h 06 min.

The band builders from 11.3 have been lifted out of the Jupiter code and are now
shared by all four giants: `Band`, `Cap` and `Oval`. To them was added
`PolarPolygon`, which was needed for one single thing.

**Saturn's hexagon.** Around the north pole lies a jet stream that holds six
straight sides, almost 30,000 km across – discovered by Voyager in 1980,
photographed anew by Cassini, and the only known shape of its kind in the solar
system. It is not in the plan, but it is too remarkable to leave out when the
globe is being drawn anyway. The edges have to be computed in the plane seen
straight down from the pole: two corners at the same latitude joined by a
latitude line give a circular arc bulging the wrong way, and the figure becomes
a circle.

**Uranus was given two faint bands and a lighter polar cap** even though the
point is that it is smooth. The reason is practical: without the slightest
feature on the surface it is impossible to see that the planet rolls, and that
is the whole reward of Uranus.

**Neptune's dark spot is a state, not a feature.** It is drawn as Voyager 2 saw
it in 1989, with its white companion cloud. When Hubble looked in 1994 it was
gone. Unlike Jupiter's Red Spot, which has persisted for centuries, Neptune's
spots come and go – that is stated in the code comment.

**Verify:** Uranus should spin about an axis that lies almost in the orbital
plane, so that the surface rolls instead of spinning.

Checked outside the app:

- **Uranus rolls.** The axis lies 97.8° from the orbital plane's normal, that is,
  only 8° from the orbital plane itself. Over one lap around the Sun, the Sun
  travels between 82.2° south and 82.2° north on the planet – it stands almost
  straight over the poles at the solstices. Compare Earth and Saturn, where the
  Sun never gets farther than 23.4° and 26.7° respectively from the equator. That
  is the difference between rolling and spinning, in numbers.
- **The rotation periods**: Saturn 10:39:22 against a known 10:39:22, Uranus
  17:14:23 against 17:14:24, Neptune 16:06:36 against 16:06:36.
- **The hexagon is measurably hexagonal.** The corners come out at 75.7° north
  and the edge midpoints at 77.6° – a difference of 1.9°, which is precisely what
  a regular hexagon gives. A circle would have given 0.0°. The width corner to
  corner comes out at 29,067 km against a measured roughly 29,000.
- **Neptune's dark spot** measures 12,989 × 6,618 km against Voyager's roughly
  13,000 × 6,600, that is, Earth-sized (Earth's diameter being 12,742 km).
- **The cost**: Saturn 61 surfaces and 1,780 points, Neptune 28 and 824, Uranus
  17 and 488 – the smooth planet is also the cheapest to draw.

Seen in the picture with the same check as before: Saturn is distinctly softer
than Jupiter, the hexagon is visible from above, Uranus is almost a single colour
but rolls visibly, and Neptune's spot with its companion cloud sits where it
should.

### 11.5 – Mercury and Venus

- [x] Mercury: grey and cratered, very like the Moon. Rotation 58.6 days –
      exactly three turns in two of its years, a 3:2 resonance with the Sun.
- [x] Venus: none of the surface is visible, only an even yellowish-white cloud
      deck. Rotation 243 days **backwards**, that is, longer than its year of
      225 days.

Mercury has the four named basins at their measured positions – Caloris,
Beethoven, Rembrandt and Tolstoj – plus Kuiper and Debussy with their bright ray
systems. The other forty-six craters are randomised from a fixed seed, so the
picture comes out the same every time without anyone having to draw them in by
hand. The latitude is drawn from arcsine so that they spread evenly over the
globe instead of clumping together at the poles.

**Venus was given streaks even though the point is that it is smooth**, and it
is worth being clear about why. The cloud cover is completely opaque; in
ordinary light Venus is an even disc without features, and it took radar from
orbit to map the ground. But without something to follow with the eye it is
impossible to see that the planet rotates, and that it rotates backwards is the
whole reward. The streaks are the Y pattern visible in ultraviolet light,
rendered so palely that it is barely noticeable.

With that comes a simplification: **in reality the clouds race around the planet
in four days while the ground beneath takes 243.** The app lets the streaks
follow the ground, so what is shown is the planet's rotation and not the clouds'.
It is the planet's rotation the stage is about, but anyone looking closely will
see clouds moving sixty times too slowly. Stated in the code comment.

The smoothing from 11.2 is switched off for both. The craters are already round
from `Oval`, and Venus's Y pattern contains a band of quadrilaterals that must
keep straight edges – rounded corners would have made the seams gape. This was
discovered when Mercury's map first cost 3,424 points; without smoothing it came
to 920.

**Verify:** Venus should spin the opposite way to all the other planets, and so
slowly that you need to turn up the speed to see it.

Checked outside the app:

- **Venus's solar day comes out at 116.75 days**, exactly the known value. That
  is the test that pins down the rotation direction: for a planet that rotates
  the right way round, the solar day is longer than the sidereal day, but for
  Venus it is *shorter* – 116.75 against 243.02 – because the surface moves to
  meet the Sun. Computed by hand, 1/243.02 + 1/224.70 gives the same 116.75, and
  it is a sum precisely because the two motions are in opposite directions.
- **The day is longer than the year**: 243.02 against 224.70 days. A point on
  Venus's equator turns 1.5 degrees per hour against Earth's 15, that is, ten
  times more slowly – hence having to turn up the speed.
- **Mercury's 3:2 resonance is exact**: the year divided by the rotation comes
  out at 1.5000. The solar day from the model comes out at 175.94 days = 2.000
  Mercurian years. On Mercury, then, two years pass in one day.
- **Mariner 10 saw the same half three times.** The probe's orbit was 176 days,
  that is, precisely one Mercurian day, so the same side was in sunlight at every
  flyby – which is why barely half the planet is unknown from those images. The
  model puts the Sun over 263.5°, 263.3° and 263.1° east at the three flybys in
  March 1974, September 1974 and March 1975. Over a year, then, it travels
  0.4 degrees.
- **The whole table of solar day against sidereal day** picks out the three
  retrograde ones: Venus −126.3 days, Pluto −43 s, Uranus negative. Earth +237 s
  (expected 236), Mars +126 s, Mercury +117.3 days. For Uranus and Neptune it is
  a matter of a couple of seconds, and the measurement window covers only one per
  cent of their lap around the Sun – there the sign is meaningful but the
  magnitude is noise.

### 11.6 – The Moon and Pluto

- [x] The Moon: grey with the dark seas (Mare Imbrium, Mare Tranquillitatis where
      Apollo 11 landed) and bright crater rays around Tycho.
- [x] **Tidal locking**: the Moon rotates exactly once per orbit and therefore
      always turns the same side towards Earth. Being able to show that is one of
      the best points in the whole app.
- [x] Pluto: Tombaugh Regio, the bright heart-shaped region that New Horizons
      photographed in 2015. Rotation 6.4 days, locked to Charon.

Eleven seas on the Moon at their measured positions and sizes, Tycho's and
Copernicus's ray systems, and Pluto with Tombaugh Regio, Cthulhu Macula and its
bright north polar cap. The tidal locking already came with the axis data in 11.1.

To this a new helper, `Streak`, which lays out a narrow line along a **great
circle** in a given compass direction. It matters: a ray going due north from
Tycho at 43° south and 1,400 km away ends up in completely different places
depending on whether you compute in the coordinate grid or on the sphere. The
same helper will be needed for Europa's cracks in 11.7.

**Corrected data: Charon's phase.** The model showed Charon over longitude 171°
on Pluto, that is, almost straight away from the prime meridian – despite the
pair being tidally locked and the IAU defining Pluto's prime meridian as the one
pointing towards Charon. The error was not in the axis but in Charon's mean
longitude, which had stood at 0.0 as a placeholder ever since the moon data was
written (the comment there already said that the phases were approximate). It is
now set so that Charon ends up over the prime meridian. The consequence is that
Sputnik Planitia, around 175° east, turns **away** from Charon – and that really
is how it looks. It is probably no coincidence: the plain is heavy enough to have
turned the whole of Pluto into place.

**Verify:** Zoom in on Earth and follow the Moon for a full lap – the same side
should be turned towards Earth the whole time.

Checked outside the app:

- **The Apollo 11 site never disappears round the limb.** Mare Tranquillitatis at
  8.5° north and 31.4° east stands at most 37.9° from the direction to Earth,
  measured every day for 110 years. Below 90° means visible, and 38° means a good
  margin.
- **The point opposite Earth keeps between −6.5° and +6.1° longitude** over the
  same 110 years, without drift. That is the optical libration, really ±6.3°,
  which is the reason we see 59 per cent of the Moon instead of half.
- **The far side has no seas, and that falls out of the data**: eleven seas on
  the map, zero on the far side. They are there because that is where they are.
- **Charon stands over longitude −1.3° to +1.3° on Pluto** over 110 years. That
  small wandering is not libration but drift: Pluto's rotation according to the
  IAU and Charon's orbital period in our data differ by 0.60 seconds per lap,
  which gives 2.3° per century. Invisible, but worth knowing it is there.
- **The sizes are right**: Oceanus Procellarum 2,571 km against a known roughly
  2,500, Mare Imbrium 1,146 against 1,150, Sputnik Planitia 895 against roughly
  1,000.

Two things had to be redone after the maps were seen drawn. **Tycho's rays** were
eight fat evenly spaced spikes, 210 km wide, which gave a cartoon star instead of
a crater; now twelve narrow ones with irregular directions and lengths. **Pluto's
base tone** was too bright for the heart to stand out.

A third thing was my own fault and did not concern the app: the check page aimed
the camera with a fixed elevation angle and only computed the compass direction,
which does not work for a body whose axis is tilted 113°. The heart ended up on
the far side and looked to be missing. The page now aims straight at the point
that should be in the middle of the frame.

---

### 11.7 – The large moons

The four Galilean moons and Titan are already drawn as discs when you zoom in on
their planet, so they are large enough to carry a surface. They also have some of
the most distinctive appearances in the solar system.

- [x] Io: sulphur yellow and orange-blotched, the most volcanically active body
      in the solar system. No craters at all – the surface is remade all the time.
- [x] Europa: almost white ice, criss-crossed by red-brown cracks. The smoothest
      thing we know of.
- [x] Ganymede: grey-brown, with bright younger regions against dark older ones.
      The solar system's largest moon, larger than Mercury.
- [x] Callisto: dark and densely cratered – the oldest surface of the four, never
      renewed.
- [x] Titan: an even orange haze. Here no surface should be visible at all,
      exactly as with Venus, since the atmosphere is opaque.
- [x] **Tidal locking** for all five: they turn the same side towards their
      planet, exactly as the Moon does towards Earth. The mechanism comes from
      11.6 and only needs rotation data per moon.

All five lie in their planet's equatorial plane, so the axis data is already
there after 11.1.

**Titan was deliberately given no surface map.** That is the answer to the task,
not a gap: the haze is opaque and the moon is drawn as an evenly orange disc with
light and shadow, exactly like Venus. The axis is included all the same, since
the locking is true whether or not it is visible.

Longitude is counted from the point that faces the planet. Zero lies towards the
planet, 180° straight away, **270° in the middle of the leading half** and 90° on
the trailing one. That is not bookkeeping but physics: Jupiter's magnetic field
rotates faster than the moons get round, so it sweeps past them from behind, and
bakes sulphur from Io's volcanoes into precisely the trailing side. That is why
Europa's trailing half is darker and redder, and Callisto's leading half
brighter.

A new helper, `Annulus`, draws a ring around a point on the surface, for the same
reason that the cloud bands are built from quadrilaterals: a ring has a hole in
it and cannot be filled. It was needed for Valhalla, the mark left by Callisto's
largest impact.

**Verify:** Follow Io for one lap around Jupiter – the same side should be turned
towards the planet the whole time, like the Moon towards Earth. Titan should lack
visible features however much you zoom, unlike the other four.

Checked outside the app:

- **Io keeps its prime meridian towards Jupiter through the whole lap.** Measured
  every 45° in the orbit it deviates at most 0.5°, and that deviation is not an
  error but the libration: the orbit is elliptical, so the speed varies while the
  rotation is even.
- **The libration comes out at exactly 2e for all five**, which is what the
  theory says: Io ±0.5°, Europa ±1.1°, Ganymede ±0.1°, Callisto ±0.8°, Titan
  ±3.3°. Measured every day for 110 years, without any drift.
- **The direction of travel points towards longitude 270°** for all five, so the
  leading half lies where it should. Europa's darker fields have their centre at
  exactly 90° and Callisto's brighter one at 270° – the right half each.
- **Valhalla's outer ring** reaches 45° from the centre, which gives 3,786 km
  across against a measured roughly 3,800.
- **Ganymede's radius 2,634 km against Mercury's 2,440**, that is, larger than
  the planet, which is the very point of it.

Two things had to be redone after the maps were seen drawn: Valhalla's rings were
a sharp target instead of the faint wave crests one can just make out, and
Callisto was too sparse to be called saturated with craters (60 craters became
110). Ganymede's groove bundles went from ten to eighteen, since about half the
moon is that kind of terrain.

### 11.8 – The moons in the focus picker

Came about after 11.7 was finished, since the maps could not otherwise be
inspected: the camera could only be centred on planets, so a moon never had time
to become large enough for a globe before it left the frame.

- [x] The focus picker lists each planet's moons under the planet, with a bullet
      in front so that the grouping is visible in a list that cannot indent rows.
- [x] The moons are only listed when they are drawn. If you clear the "Moons"
      checkbox they disappear from the picker, and if the camera was following one
      of them, focus falls to the Sun – the same rule that already applied to the
      probes.
- [x] The camera aims at the **drawn** position, not the real one. The moons are
      pulled in towards their planet so as not to end up outside the frame, and if
      you aim at the real position the camera points far to one side. The drawing
      code therefore got a `MoonPosition` that the focus picker shares with it.
- [x] A selected moon gets the same field of view as a planet, that is, twelve
      times its own radius. Without that you end up either inside the moon or so
      far away that it is just a dot.
- [x] `EarthFocusIndex`, which counted places in the list, has been replaced by a
      name lookup. With moons and probes coming and going in the list it is no
      longer possible to count your way to a row.

**Verify:** Select a moon in the focus picker and see that the camera ends up at
it, not beside it. Turn off the moons while one of them is being followed and see
that the view falls back.

Checked outside the app, 38 checks without a single failure:

- **The list is built correctly**: 24 bodies and 30 rows with everything turned
  on, the Moon directly under Earth, Io under Jupiter, Charon under Pluto.
- **The probes' indices hold despite fifteen new rows in between.** That was the
  trap present in 10.6, now with a larger shift: all five probes point correctly.
- **If you turn off the moons**, focus falls from Ganymede to the Sun, while
  Jupiter and Voyager 1 are kept – the selection is preserved by name, not by
  position.
- **All moons with a surface map get an 80-pixel radius** at the proposed
  distance, against a threshold of 14. The maps will therefore be visible, with a
  good margin.
- **The Moon lies 2.9 planetary radii out and Ganymede 6.3** at the distance the
  camera picks, that is, outside the planet and inside the frame.

Phobos is the exception: it is so small that twelve times the radius falls below
the camera's minimum distance, so it becomes a dot three pixels across. That is
the same thing remaining item R4 describes, and it has no surface map to show
anyway.

---

## Stage 12 – What else the solar system has to show

Five additions and one decision, ordered by what they give divided by what they
cost. The first three rest on things the model already handles – that has been
checked, and the numbers are under each item. The fourth is not an item to build
but an architectural choice to take a position on, and it is worth reading before
anyone starts on eclipses.

### 12.1 – Planetary alignments: go to the next meeting

A picker along the lines of the launch windows for Mars: "the next opposition for
Mars", "the next time Jupiter and Saturn meet", "the next time all the planets
are on the same side of the Sun". You choose, the app jumps to the date, and you
see the arrangement from above.

- [x] A search function for conjunctions and oppositions between two chosen
      bodies. The same shape as `Mission.NextLaunchWindow`: step day by day, find
      the minimum, refine. That code exists and is already fast enough for a
      button press.
- [x] A button or list in the control panel that jumps to the next such date.
- [x] Show the distance at the moment – Mars is twice as close at a favourable
      opposition as at an unfavourable one, and that is the whole explanation for
      why some oppositions make big news.

**Checked in advance:** all four Mars oppositions in 2025–2031 fall on the right
day out of the model – 16 Jan 2025 (0.644 AU), 19 Feb 2027 (0.678), 25 Mar 2029
(0.649) and 4 May 2031 (0.559) – against reality's 16 Jan, 19 Feb, 25 Mar and
4 May. The eccentricity is visible directly in the distances.

**Corrected in the plan above: the warning about outer conjunctions was not
needed.** It said here that the great Jupiter–Saturn conjunction comes out seven
weeks wrong and that dates for outer conjunctions should therefore be shown with
a caveat. That was my own measurement error: I compared the planets'
*heliocentric* longitudes. A conjunction is something you see from Earth, and
computed from there it comes out on the right day – 21 December 2020, to within
0.11 degrees. It is Earth's own position that determines when two planets appear
to meet, and Earth's position is what the model knows best.

Built as `Simulation/SkyEvent.cs`. Two kinds of meeting, a single search: for a
conjunction the angle between the two bodies as seen from Earth is minimised, for
an opposition how far the planet is from standing directly opposite the Sun is
minimised. A coarse day-by-day search, then golden-section down to a minute. The
picker has six oppositions (bodies outside Earth's orbit – Mercury and Venus
cannot have one) and six conjunctions between the bright planets. A conjunction
only counts if the bodies come within five degrees, that is, roughly a field of
view in binoculars.

The search uses `PositionAuAt` in double precision, which came with R3.

**Verify:** The four Mars oppositions above should be found on the right day by
the app's own search function, not just by the test program.

Checked outside the app, 17 checks without a single failure, all through
`SkyEvent.Next`:

- **The four Mars oppositions on the right day**: 2025-01-16 (0.644 AU),
  2027-02-19 (0.678), 2029-03-25 (0.649) and 2031-05-04 (0.559).
- **Three well-documented conjunctions on the right day**: the great conjunction
  on 21 December 2020 at 0.11 degrees (known 0.10), Venus meets Jupiter on
  2 March 2023 at 0.49 (known 0.52) and Mars meets Jupiter on 14 August 2024 at
  0.30 (known 0.31).
- **The synodic periods are right**, which is the check that does not rest on any
  memory: the time between two oppositions should be the planet's synodic period.
  Over twenty intervals the model gives Mars 781.2 days (known 779.9), Jupiter
  398.8 (398.9), Saturn 378.3 (378.1), Uranus 369.7 (369.7), Neptune 367.5
  (367.5) and Pluto 366.8 (366.7).
- **The spread in those intervals is itself worth seeing**: Mars varies between
  764 and 811 days while Neptune keeps between 367 and 368. The rounder and more
  distant the orbit, the steadier the beat. That was also why a first test over
  only five intervals looked to miss Mars by five days – too few samples.
- **Pressing again takes you further**: five presses give 2027-02, 2029-03,
  2031-05, 2033-06 and 2035-09, never the same date twice.
- **All twelve choices give an answer**, and quickly: the longest is Jupiter meets
  Saturn at 10 ms, all twelve together 14 ms. It then searches fourteen years
  ahead.
- **Mars's oppositions vary between 0.382 and 0.678 AU** over sixteen years, that
  is, almost twice as far away at an unfavourable one. That is the whole point of
  showing the distance.

### 12.2 – The heliopause and the edge of the solar system

The probes already travel out of the solar system, but where the edge lies is not
visible.

- [x] A transparent sphere at 120 AU, drawn only once you have zoomed out enough
      for it to fit. That is where the solar wind meets the interstellar medium
      and the Sun's dominion ends.
- [x] The Voyagers' crossings as milestones: 25 August 2012 and 5 November 2018.
      They are the only two occasions on which any craft from Earth has passed
      that boundary.
- [x] Preferably a note that the edge is not a ball but bulges – the solar system
      travels through the interstellar medium and gets a bow shock in front of it.

**Checked in advance:** the model puts Voyager 1 at 120.1 AU at its crossing date
against a measured 121.6, and Voyager 2 at 117.7 against a measured 119.0. Within
one and a half AU without anything having to be added – the figures already lie
in the probe data.

The sphere is drawn as a circle, but only when the camera is **outside** it. A
ball seen from outside projects to a circle with the angular radius arcsin(R/d),
that is, R·f/√(d²−R²) on the screen – which is not the same as projecting the
limb straight off. From inside it would fill the whole frame and become nothing
but a blue veil, so it is only drawn when it fits in the frame. In practice that
means beyond about 290 AU, which you reach by selecting one of the four outermost
probes in the focus picker.

The milestone is a new kind. It has no speed jump, because the probe merely
passed a boundary, and it is therefore described as "Crossed the heliopause"
instead of "Past ... gave 10.8 km/s". The position is taken from the orbit the
probe was following on that very day, so the boundary ends up where the probe
really was and not where someone has written in that it was. The method
`Probe.Crossing` is added after `Build`, since `Build` takes its points as
`params` and has no room left for more kinds of data.

**Verify:** Set the date to 25 August 2012 with Voyager 1 selected. The probe
should stand on the sphere, not inside or outside it.

Checked outside the app, 13 checks without a single failure:

- **The probes stand on the sphere at their crossing dates**: Voyager 1 at
  120.1 AU on 25 August 2012, Voyager 2 at 117.7 AU on 5 November 2018, against
  the sphere's 120. Nothing has been added to make it fit – the figures already
  lay in the probe data from stage 10.
- **The measured crossings were at 121.6 and 119.0 AU**, that is, not at the same
  distance. That is itself the point of the note about the edge bulging: it lies
  at different distances in different directions, and 120 is a round number in
  between.
- **The milestone sits correctly**: the right date, in chronological order among
  the others (Earth → Jupiter → Saturn → the heliopause for Voyager 1, with
  Uranus and Neptune in between for Voyager 2), zero speed jump, and the position
  agrees to the kilometre with the probe's own position on the same day.
- **The three other probes have no boundary milestone**, which they should not
  have – Pioneer 11 and New Horizons are still inside.
- **Where the probes stand today**: Voyager 1 at 169.3 AU, Voyager 2 at 142.1 and
  Pioneer 10 at 141.7 are outside; Pioneer 11 at 119.6 and New Horizons at 64.9
  are inside. That Pioneer 11 lies so close to the edge is worth a look.

Left to see with the eye: how the transparent sphere comes across. The geometry
is computed but the colour choices are guesses, and the limit for when it is
drawn – that the circle should fit within 55 per cent of the frame's shortest
side – is set by feel.

### 12.3 – The plane of the Moon's orbit and the wandering of the nodes

The honest version of the eclipse idea – see 12.4 for why the direct route does
not work. The question to be answered is not *when* there will be an eclipse but
*why there is not one every month*.

- [x] Add `AscNodeRateDegPerDay` to `CelestialBody`, so that an orbital plane can
      turn with time. The Moon's node travels backwards, a full lap in 18.6 years,
      that is, 19.4 degrees per year.
- [x] The same field benefits Triton. The code comment there already says that its
      orbital plane precesses with a period of roughly 640 years and that the
      orientation in the data is the position at the epoch rather than a permanent
      property.
- [x] A mode that draws the plane of the Moon's orbit against the ecliptic and
      marks the two nodes, so that you can see that the Moon mostly passes above
      or below the Sun instead of in front of it.

**It became two fields, not one.** `PerihelionRateDegPerDay` had to come at the
same time, and the reason is that the mean anomaly is computed as the mean
longitude minus the longitude of perihelion. If you let the perihelion stand
still while the node moves, the orbit gets the right plane but the wrong position
in that plane. With both, it also comes out right by itself: the Moon's mean
longitude still runs its sidereal lap of 27.32 days while the mean anomaly runs
the anomalistic one of 27.55, and the difference between the two is precisely the
motion of perigee.

The Moon: node −0.0529539 degrees per day, perigee +0.1114041. Triton: +0.00154,
that is, forwards. The direction there follows from the orbit being retrograde –
Neptune's oblateness turns the node at a rate that goes as the cosine of the
orbital inclination, and the inclination is over 90 degrees. Where most moons have
their node dragged backwards, Triton's travels forwards.

The mode is called "Lunar orbit" and sits in the meetings row. It draws the
Moon's orbit, the plane of the ecliptic as a yellow ring to compare against, the
node line where the two intersect, and the two nodes marked with names.

**Verify:** Follow the node over twenty years – it should go a full lap backwards
in 18.6 years. The Moon's orbital inclination to the ecliptic should stay at
5.1 degrees the whole time; it is only the node's longitude that changes.

Checked outside the app, 10 checks without a single failure:

- **A full lap backwards in 18.61 years**: the orbital pole sweeps −359.5 degrees
  in 6798 days, measured from the positions and not read off from the data. The
  node stands today at 329.3 degrees against 125.0 at the epoch.
- **Only the node moves**: the orbital inclination keeps between 5.142 and
  5.167 degrees over the whole lap.
- **Perigee a full lap in 8.85 years**, and the anomalistic month comes out at
  27.5532 days against a known 27.5546 – without anyone having entered it.
- **Nothing else was affected**: the Mars window is still on 21 October 2026 at
  3.12 km/s and hits Mars at zero kilometres, Voyager 1 travels at 16.66 km/s and
  crosses the heliopause at 120.1 AU. The tidal locking and the visibility of the
  Apollo 11 site also still hold.

**Remaining as a simplification: the Moon's spin axis does not follow the node.**
Cassini's law says the axis should hold a constant 6.7 degrees to the orbital
plane; measured, it varies between 3.6 and 6.7 over the node cycle, that is, at
most 3.1 degrees of error in the axis's direction. Correcting that requires
`BodyAxis` to be given a node rate too, and the date to be threaded through the
whole of the surface drawing. The tidal locking is not affected – it is checked
and holds.

**Why it is worth it:** with the node in motion the eclipse seasons slide nineteen
days backwards every year, and that is the whole explanation of the 18.6-year
cycle and of why saros exists. That motion comes out right even if individual
dates do not.

**Verify:** Follow the node over twenty years – it should go a full lap backwards
in 18.6 years. The Moon's orbital inclination to the ecliptic should stay at
5.1 degrees the whole time; it is only the node's longitude that changes.

### 12.4 – Eclipses (and why they are hard)

**Rewritten after 12.3. The investigation that stood here was too pessimistic.**

It said here that two things were needed for eclipses: the wandering of the node,
and periodic terms in the Moon's position. The node motion was done in 12.3, and
then it was measured what that alone was enough for. The answer was more than
expected.

**Of eighteen real solar eclipses between 1999 and 2030 the model finds them all,
and seventeen of them on the right calendar day.** The only deviation is
20/21 May 2012, which crossed the date line and is dated differently depending on
where you stood. This with nothing but mean orbital elements plus the motion of
the node and of perigee.

Before 12.3 the same model was 9 to 18 degrees off at these dates and found a
single "eclipse" during 2026, on the wrong day. The conclusion that periodic
terms were needed to get the *dates* right was therefore mistaken. What they are
needed for is something else, see below.

- [x] Decision: should the Moon be given periodic terms, or should the app settle
      for explaining the mechanism (12.3) without predicting dates?

**The decision was: no periodic terms.** They are not needed for what the app can
use them for. The dates are already right, and the next step in accuracy –
whether the eclipse will be total, annular or partial, and where on Earth it is
visible – cannot be reached with them anyway. That is determined by the parallax
from the place the observer is standing, that is, by where on the globe you are,
and that is an entirely different sort of computation from a better lunar orbit.
The app shows the solar system from outside and has no observer on the ground.

What remains, then, is not mathematics but user interface:

- [x] A list of eclipse dates to jump to, along the lines of the meetings picker
      in 12.1. The dates can be searched out of the model the same way the tests
      did: a new or full moon with the Moon within a couple of degrees of the
      ecliptic.
- [x] On the jump: switch on "Lunar orbit" automatically and zoom to Earth, so
      that you see the Sun standing at the node line at that moment. That is the
      explanation, and it is worth more than the date itself.
- [x] Be clear about what is not shown: whether the eclipse is total or partial,
      and where on Earth. That requires an observer on the ground, which the app
      does not have.

No list was needed: the eclipses became two more entries in the meetings picker
from 12.1, and the dates are computed instead of written in. It is the same
search as for conjunctions and oppositions, with two differences. The threshold
is stricter – 1.55 degrees for the Sun, which is the width of the Sun and Moon
plus the degree that parallax can shift the Moon between different places on
Earth, and 1.0 degrees for the Moon, which is Earth's umbra minus the Moon's own
radius. And the Moon's position must not be treated like a planet's: its orbital
elements are geocentric, so its position **is** already the direction from Earth.
Subtracting Earth's position once more would have laid Earth's orbit on top of
the Moon's.

If you click your way to an eclipse, "Lunar orbit" and "Show moons" are switched
on and the camera positions itself at Earth, so that you see the Sun standing at
the node line on that very day. The label says outright what is not shown: "type
and location are not shown".

**Verify:** The model's series of eclipses should agree with reality, not just
individual dates.

Checked outside the app through `SkyEvent.Next`, 5 checks without a single
failure:

- **All ten solar eclipses in 2024–2028 in exact order**, none extra and none
  missed: 8 Apr 2024, 2 Oct 2024, 29 Mar 2025, 21 Sep 2025, 17 Feb 2026,
  12 Aug 2026, 6 Feb 2027, 2 Aug 2027, 26 Jan 2028 and 22 Jul 2028. That is the
  whole series, not a selection – including the partial ones that are barely
  noticeable.
- **The lunar eclipses: seven found, all real, no false alarms.** One is missed,
  12 January 2028, and it is a shallow partial one lying right at the threshold.
  It is the cut-off that decides it, not the model.
- **The intervals are whole synodic months**: 6, 6, 6, 5, 6, 6, 6, 6, 6 laps. That
  one of them is five and not six is correct – eclipse seasons come 173 days
  apart, which does not divide evenly into lunar laps.
- **The saros period falls out by itself.** The total solar eclipse across the USA
  on 21 August 2017 recurs in the model on 1 September 2035, that is, 6585 days
  later against the saros period's known 6585.3. The Babylonians knew that period
  and could predict eclipses with it; here it comes out of the wandering of the
  node and of perigee without anyone having entered it.

### 12.5 – Halley's Comet

A single object with much to show: eccentricity 0.967, a retrograde orbit and a
lap of 76 years that takes it from inside Venus to outside Neptune.

- [x] Halley as a body with its orbital elements. `Conic` already handles any
      eccentricity at all, so most of it is there.
- [x] Preferably a tail that points away from the Sun and grows near perihelion –
      the tail does not lie behind the comet in the direction of travel but away
      from the Sun, which is one of the things it is easiest to be wrong about.
- [x] The next perihelion is in 2061. With the date picker you can travel there.

**Verify:** Perihelion on 9 February 1986 at 0.586 AU and aphelion in 1948 at
35.1 AU, that is, outside Neptune's orbit.

**Outcome:** 15 of 15 checks.

| check | model | reference |
|---|---|---|
| perihelion | 1986-02-09, 0.586 AU | 1986-02-09, 0.586 |
| aphelion | 1948-05-18, 35.13 AU | 1948, 35.1 |
| next perihelion | 2061-07-28 | 2061-07-28 |
| speed at perihelion | 54.6 km/s | ~54 |
| speed at aphelion | 0.91 km/s | ~0.9 |
| closest to Earth in 1986 | 10 April, 0.416 AU | 11 April, 0.42 |

The last row is the one that means something. The first five follow from the
elements that were entered, but the meeting with Earth does not – it depends on
where Earth happens to be, and that the model hits the day and the distance is a
check against something it was not told.

Two things fell out of the model rather than into it:

- **The tail points away from the Sun, not backwards.** Measured as the angle
  between the direction away from the Sun and the direction of travel: 138 degrees
  sixty days before perihelion, 42 degrees sixty days after. On the way out the
  comet therefore travels tail first, which is the thing it is easiest to be wrong
  about.
- **Halley is invisible for 74 years out of 75.** The ice only vaporises inside
  3 AU, and the comet is there for 368 days out of its 27,563 – 1.3 per cent of
  the lap. Inside 1 AU it is there for 78 days. The number is not written in
  anywhere but measured on the orbit.

**Addition: "Halley at perihelion" in the meetings picker.** The item above said
that you travel there with the date picker, and you can – but the meetings picker
already existed and does the same thing without your having to know the date by
heart. Six more checks:

| check | outcome |
|---|---|
| finds the 1986 perihelion | the same minute as a direct minimisation |
| the next one from today's date | 2061-07-28 |
| four clicks in a row | 1910-08-24, 1986-02-09, 2061-07-28, 2137-01-13 |
| the intervals | 27,563, 27,563, 27,562 days |

The search minimises the distance to the Sun instead of an angle on the sky. The
quantity has a different unit from the others, but that makes no difference – the
machinery looks for a minimum, and a distance has just as clear a minimum as an
angle. It is even easier: an orbit has exactly one perihelion per lap, so it can
neither be missed nor counted twice.

What is reported, on the other hand, is deliberately not the distance to the Sun.
That is 0.586 AU every time – that is what a perihelion is, after all – and
therefore says nothing about that particular occasion. What distinguishes the
visits is where Earth happens to be, and there the model gives an answer it was
not told:

| | distance to Earth | elongation from the Sun |
|---|---|---|
| 9 February 1986 | 1.55 AU | 8° |
| 28 July 2061 | 0.48 AU | 20° |

**That is the history, computed.** The 1986 visit was the worst in two thousand
years: at perihelion Halley stood on the other side of the Sun, eight degrees from
it on the sky, that is, in the middle of daylight. The comet did not become
anything to look at until two months later, on 10 April, and then at 0.42 AU.
2061 will be the opposite: the comet stands on the same side as Earth and comes as
close as 0.482 AU on the perihelion day itself, against NASA's prediction of
0.48 AU on 29 July. None of that is entered.

**Choices and caveats:**

- **The elements are anchored in two perihelia**, 1986 and 2061, instead of taken
  from an ephemeris. The reason is that a fixed Kepler orbit cannot hit them all:
  Halley's real orbital period varies between 74 and 79 years, because Jupiter and
  Saturn tug at the comet on every lap and the gas jets from the heated nucleus
  nudge it like a weak rocket. The price is visible immediately – the model puts
  the 1910 perihelion at the end of August instead of on 20 April. That is stated
  in the code.
- **Two tails, not one.** The ion tail is gas that the solar wind tears away
  straight from the Sun; the dust tail is grains that are too heavy to be carried
  along, keep the comet's speed and trail behind along the orbit. The difference
  between them is precisely what the item above is about, and it is only visible
  if both are drawn.
- **The coma is drawn after the activity and not after the body.** The nucleus is
  5 km across and would never be visible; what you see in reality is the gas cloud
  around it, which near the Sun becomes wider than the Sun itself.
- **The tail has a minimum length on screen.** 0.3 AU is large against Earth's
  orbit but small against Halley's own, so zoomed out the tail would be a few
  pixels. The direction is the computed one, only the length is generous – the
  same sort of concession as the planets' magnification.
- **Off by default.** The comet is away for 74 years out of 75, and its orbit is
  so drawn out that it obscures the planets' if it is left in the picture.
- The comet can be selected in the focus picker, which it needs to be: without a
  camera in place it disappears from the frame for decades. The distance frames
  the tail and not the body.
- **The jump to perihelion lights the comet up but does not move the camera.**
  Travelling to a date where the comet is dark would be travelling to nothing. The
  camera is left in the overview, however, because that is where you see what
  happens: that the comet dives in through the whole planetary system and out
  again. If you want to go close, it is there in the focus picker.

### 12.6 – The Sun's rotation

The app is thinnest right at the Sun – it is a shaded disc. The rotation is what
gives the most back.

- [x] The Sun gets a `BodyAxis` and a simple surface map with a few sunspots.
- [x] A rotation period of 25 days at the equator.

**A caveat that has to be stated in the code:** the Sun rotates at different
speeds at different latitudes – 25 days at the equator, 34 at the poles. That is
itself one of the best things to show, but a single rotation period cannot express
it, so sunspots at high latitudes will drift wrongly. Either it has to stand as a
stated simplification, or the surface needs a mechanism of its own where the
rotation depends on the latitude.

**Verify:** A spot at the equator should come back to the same position after
25 days.

**Outcome:** 10 of 10 checks. Of the two routes in the caveat, the second was
chosen: the surface got a mechanism of its own.

| check | model | reference |
|---|---|---|
| the equator's tilt | 7.252° | 7.25 |
| a lap at the equator | 25.03 days | ~25 |
| a lap at 30° latitude | 26.39 days | 26.4 |
| a synodic lap, seen from Earth | 26.87 days | ~26.9 |
| north pole most towards us | 8 September, B₀ = +7.25° | 7–8 September |
| south pole most towards us | 6 March, B₀ = −7.25° | 5–6 March |

The last two rows are the ones that mean something. The tilt of 7.25 degrees is
tested against itself, but **the node of 75.77 is tested by nothing else** – and
it is what determines which month the Sun's north pole leans towards us. That the
model puts it at the beginning of September and the south pole at the beginning of
March is therefore a check on a number that could not be checked any other way.

**Differential rotation, for real.** The Sun does not rotate as one piece, and it
is the only body in the app that does not. The rate follows Newton and Nunn's
measurement on sunspots from 1951:

    ω(φ) = 14.38 − 2.96 · sin²φ  degrees per day

What that gives, measured in the model:

| latitude | lap | consequence |
|---|---|---|
| 0° | 25.03 days | |
| 30° | 26.39 days | 0.74°/day slower than the equator |
| 35° | 26.85 days | |

Two groups lying at 8 and 22 degrees of latitude drift apart by 0.36 degrees a
day. Over a year the lower one has pulled ahead of the upper by 131 degrees, that
is, a third of a lap. It can be seen in the app by zooming in on the Sun and
stepping forward month by month, and it is the whole point of the stage: **a solid
body cannot do that.** That the Sun rotates at different speeds at different
latitudes is the proof that it is gas all the way through.

**Choices and caveats:**

- **The rotation period is the sunspots', not the plasma's.** The two differ:
  measured with Doppler shift the equator goes in 24.5 days, measured on sunspots
  25.0. Since it is spots that are drawn, it is the spots' rate that applies. It
  is also the one that gives the 25 days the item above asks for; Carrington's
  classic 25.38 is a third number and applies at 26 degrees of latitude.
- **The law only holds where there are spots**, that is, within ±35 degrees of
  latitude. Stretched to the pole it gives 31.5 days against a measured 34. That
  makes no difference here since only spots are drawn, but it would be wrong to
  conceal.
- **The turning is taken per surface and not per corner.** A sunspot group comes
  along as a lump, which is what real groups do. If every corner were allowed to
  go at its own rate, a group spanning a few degrees of latitude would be drawn
  out into a smear over a couple of years – and real groups never get that far,
  they die within weeks.
- **The spots stand still and last forever.** In reality a group lives a few
  weeks, the number follows the eleven-year cycle, and their latitudes travel
  towards the equator over the course of the cycle – drawn in a diagram the
  pattern becomes the famous butterfly pair. The map therefore shows how the Sun
  looks in a year near maximum.
- **They are drawn somewhat larger than the real ones.** Altogether the visible
  spots cover barely one per cent of the disc, against roughly half a per cent at
  a strong maximum. Any smaller and they are invisible: at the threshold of
  30 pixels the largest group is barely two pixels wide.
- **The faculae are left out.** The bright veils around the spots are visible
  almost only near the limb, where you look obliquely through the gas. A flat
  colour surface cannot express that.
- **The prime meridian is a pure convention.** The Sun has no permanent features
  to count from – no crater, no coastline, only gas that is replaced. Carrington's
  prime meridian is put in for form's sake.
- **The limb darkening was already there.** The solar disc has always been drawn
  with a gradient from white in the middle to orange at the edge, and that turns
  out to be physics and not decoration: at the limb you look obliquely into the
  gas and only reach the upper, cooler layers.

---

## Stage 13 – Public release

Two parts that belong together without depending on each other: that the app can
be shown in more languages than Swedish, and that the code can be published
openly. They can be taken in any order.

### 13.1 – Language support

The app should be viewable in Swedish and English, with the languages in files of
their own so that further languages (German, say) are only one more file – no
code change.

- [x] Put all the texts in .resx resource files (the standard mechanism in .NET):
      `Resources/Strings/AppStrings.resx` (English as the base language) and
      `AppStrings.sv.resx` (Swedish). A new language = a new `AppStrings.xx.resx`.
- [x] Move out and translate:
      - [x] Menus and controls (Pause/Start, Speed, Show orbits, Real size,
            Constellations, Star names, Stars, Focus, Reset view,
            Few/Normal/Many)
      - [x] Clock and info texts (Elapsed, days/sec, the help line, the window
            title)
      - [x] Names of celestial bodies (Solen/The Sun, Jorden/Earth, Månen/The
            Moon ...) – the data in `SolarSystemData` gets language-neutral keys,
            the names are looked up in the resources
      - [x] Constellation names (Karlavagnen - Stora björn / Big Dipper - Ursa
            Major, Lilla björn / Little Bear ...). The stars' proper names
            (Betelgeuse, Sirius, Polaris) are international and only need
            translating in exceptional cases.
- [x] Language selection: follow the operating system's language by default, plus
      a picker in the control panel so that the teacher can switch language right
      there in the lesson.
- [x] Date and number formats follow the chosen language (today hard-coded to
      sv-SE).
- [x] A README in English. A couple of parallel files were tried first,
      `README.md` and `README.en.md` with cross links, but that did not survive a
      simple question: what does a Swedish README win when the app itself already
      has a language picker? Nothing the reader does not already get in the app,
      against keeping two documents in sync forever. `README.md` is now English
      outright, which is also the GitHub convention for what is shown as the front
      page.

**Verify:** Switch language in the picker: all the texts, planet names and
constellation names change language immediately, the date is formatted correctly
("fredag 5 september" / "Friday, September 5") and no texts are cut off in the
control panel. Swedish should look exactly as it does today.

**Outcome:** 16 of 16 checks, in a standalone test program that links in the real
resource files (the same sort of independent check that the test program in
`scratchpad/moontest` has used since stage 9 – see Notes).

| check | Swedish | English |
|---|---|---|
| date format | "fredag 5 september 2025, 14:30" | "Friday, September 5, 2025 14:30" |
| pause button | "⏸ Pausa" | "⏸ Pause" |
| Earth's name | "Jorden" | "Earth" |
| constellation | "Karlavagnen - Stora björn" | "Big Dipper - Ursa Major" |
| a decimal number in a message | "Fart: 16,67 km/s" | "Speed: 16.67 km/s" |

The two date rows are precisely the examples the item itself asked for, and they
show why a single format string would not have been enough: the day-before-month
order is the very difference between the languages, not just the translation of
the words. English was first built with the same pattern as Swedish ("Friday
5 September"), which the test came down on immediately – the reference answer was
written in the TODO but the format string had been copied straight across. The
correction was to give `msg.dateFormat` a value of its own per language instead of
a shared one: `dddd d MMMM yyyy, HH:mm` for Swedish (word for word what was in the
code before the stage began – verified against commit 5493e48),
`dddd, MMMM d, yyyy HH:mm` for English.

**How it is built:**

- 165 keys, divided into `name.*` (celestial bodies, constellations, the two stars
  whose names differ between the languages), `ui.*` (static interface text) and
  `msg.*` (messages with inserted values, formatted with `Strings.Format`).
- `CelestialBody.Name`, `Probe`/`Milestone.Name`, `Mission.Name` and
  `Constellation.Name` became `...Key` – a language-neutral identity (`"Earth"`,
  `"UrsaMajor"`) that `Strings.Name(key)` looks up when drawing. That is what
  makes the 3D view change language by itself without the drawing code knowing
  about it: it asks `Strings` every frame instead of remembering a text.
- `MainPage.xaml` got an `x:Name` on every text-bearing control instead of
  `Text="..."`; `ApplyLanguage()` in the code sets them all and is called both at
  start-up and on every change in the language picker.
- The probes' and spacecraft's own names (Voyager 1, Cassini, "Farkost"/"craft")
  are not translated – they are proper names, exactly like the stars'.

**Corrected afterwards: the star selector opened on the wrong entry.** It said
"None" at start-up while the sky was drawn with stars. The setting was never
wrong, only the label: refilling a selector resets its selection, so
`ApplyLanguage` saves the index aside and puts it back afterwards – but at
start-up there is nothing to save. The selector has no items that early, an
index set before then never takes, and reading it back gives -1, which clamped
to the first entry. The star density is the only one of the selectors whose
default is not the first entry (`StarSky.Density` starts at `Medium`), which is
why it was the only one that showed. It now reads its selection from the
drawable instead of from its own index, so the setting has one home rather than
two. The others are correct, but for the weaker reason that their default
happens to be zero.

**Tested but not inspected in the app.** The test above links the compiled
`Solarsystem.dll` and the satellite resource `sv/Solarsystem.resources.dll` –
exactly the files the app actually uses – but runs as a standalone console
program. That the panel's buttons are not cut off in English (shorter Swedish
text, longer English) has been checked by giving seven date buttons a wider
`WidthRequest` in XAML; the other text-bearing buttons have no fixed width and sit
in a horizontally scrolling panel, so a longer word only pushes the panel further
rather than being cut off. No human has seen the picker switch in the window.

**A decision made afterwards about the README:** a first version had `README.md`
(Swedish) and `README.en.md` (English) side by side with cross links. That was
changed to a single English `README.md` – the app's own language picker makes a
translated README duplicated work without an audience, and GitHub only shows one
file as the front page anyway.

---

### 13.2 – Open source on GitHub

The review below was made against this repository and not written from a general
checklist. Seven things were found, and the second should be settled before
anything at all is published – it cannot be taken back.

- [x] **There is no licence file.** Without one, full copyright applies by
      default: the code can be read on GitHub but nobody may legally use, modify
      or distribute it, which is hardly the intention of putting it up. For a
      teaching app, MIT or Apache 2.0 are the obvious candidates; MIT is shorter,
      Apache 2.0 gives explicit patent protection.

      **Outcome:** MIT chosen. `LICENSE` added, copyright holder "Krister
      Hellsing" (the person, not the employer – see the next item).

- [x] **The work email is in every commit.** The whole history is signed
      `krister.hellsing@swedavia.se`, that is, an employer address, and it comes
      along when the repository is published. That is a choice to make
      deliberately and not to discover afterwards: changing it after the fact
      requires a rewrite of the entire history, which changes every commit id. The
      simplest thing is to decide first. Worth considering at the same time is
      whether anything in the employer's rules concerns code written in one's own
      time.

      **Outcome:** The history was rewritten. All 56 commits had their
      author/committer changed from `krister.hellsing
      <krister.hellsing@swedavia.se>` to `Christer.hellsing
      <krister.hellsing@gmail.com>` – trees, messages and timestamps unchanged and
      verified identical, only the identity changed. Done with `git
      fast-export`/`fast-import` instead of `filter-branch`, which turned out to be
      unreasonably slow in this environment (aborted after two minutes without
      finishing 56 commits).

      `filter-branch` with `--env-filter` checks out every commit in its
      entirety in order to replay it, whereas `fast-export` merely streams the
      text format and lets a simple text substitution replace the identity lines
      – milliseconds instead of minutes. One pitfall along the way: running the
      substitution through `sed` in Git Bash on Windows corrupted the stream
      (line-ending translation touched byte-exact binary blobs, such as the
      icon's SVG files), which crashed the import. Solved with a Python
      replacement that keeps everything in binary mode. A safety branch
      (`backup-before-email-rewrite`) still points at the original history
      locally. The force-push to `origin` (the repository had already been pushed
      there privately, with the old history) was saved until the end of the
      stage, so that only one force-push was needed; it has since been done and
      verified – `origin/main` now carries the rewritten history.

- [x] **Leftovers from the project template.** `ApplicationId` is still
      `com.companyname.solarsystem`. `Resources/Images/dotnet_bot.png` is still
      there but is not used anywhere in the code – it is Microsoft's mascot and
      does not need to come along. The same goes for `Resources/Raw/AboutAssets.txt`,
      which is only the template's own instruction text. The app icon and splash
      screen are still the .NET template's purple.

      **Outcome:** `ApplicationId` changed to `com.kristerhellsing.solarsystem`.
      `dotnet_bot.png` and `AboutAssets.txt` removed, as was the line in
      `.csproj` that pointed at the former. Icon and splash screen replaced: a
      dark space-blue background (`#0B1220`) with a sun, a tipped orbit and a
      blue planet – the same colours already used in the app (`#FFA226`/`#FFE08A`
      for the Sun, `#4C8CE8` for Earth). Approved.

- [x] **Attribution for what has been borrowed.** Open Sans is genuinely used,
      through `Styles.xaml`, and its Apache 2.0 licence requires the licence text
      to come along. The data should also have its sources written out: the
      orbital elements come from NASA/JPL, the pole directions from the IAU's
      working group on cartographic coordinates, and the star catalogue's origin
      should be established and stated.

      **Outcome:** `THIRD-PARTY-NOTICES.md` with the Apache 2.0 licence text for
      Open Sans. The star catalogue's origin was already stated in a comment in
      `StarCatalog.cs` – the Yale Bright Star Catalogue. All four sources
      (NASA/JPL, IAU, Yale, Open Sans) are now in the README under "Credits".

- [x] **The README needs to become a repository README.** The present one is an
      excellent feature description but lacks what a visitor wants first: a
      sentence about what it is, a screenshot, how to build it (.NET 10 SDK, the
      MAUI workload, Windows) and which licence applies.

      **Outcome:** The build section got the requirements list (.NET 10 SDK, the
      MAUI workload, Windows), and the README got sections for Platform, Credits
      and License. The screenshot could not be taken during the session (screen
      access denied), but the user added one themselves at
      `Resources/Images/screenshot.png` and the README now points there.

- [x] **The platform folders promise more than the app delivers.**
      `Platforms/Android`, `iOS` and `MacCatalyst` are left over from the
      template while `TargetFrameworks` only contains Windows. Either remove
      them, or add a line in the README saying that only Windows is buildable
      today.

      **Outcome:** Removed. They were never built anyway – MSBuild only includes
      the platform folder that matches `TargetFrameworks`, so the three folders
      were already dead content. `Platforms/Windows` remains, being the only one
      ever compiled. The README got a Platform section explaining the situation
      and what is required to add a platform back.

- [x] **Translate all the code comments into English.** They are Swedish all the
      way through today, which shuts out everyone who does not read Swedish – and
      that is most of the people who find a repository on GitHub.

      It is the single largest item in the whole stage. Counted in the code: 1,562
      lines of XML documentation and 494 ordinary comment lines, together 2,056 of
      7,329 lines, that is, 28 per cent. The heaviest are `SurfaceMap.cs` with 339
      lines, `SolarSystemDrawable.cs` with 279, `SolarSystemData.cs` with 263 and
      `Mission.cs` with 202.

      Nor is it a mechanical translation. The comments carry the project's real
      documentation – why the nodes were 180 degrees wrong, why conjunctions are
      computed from Earth and not from the Sun, why Venus has streaks despite
      being smooth. That kind of reasoning has to hold up just as well in English,
      otherwise it is better left alone.

      Write simple English, not elegant English. Whoever forks it probably has
      neither Swedish nor English as a first language but English as a second, and
      then short sentences and common words carry further than idioms and puns.
      Several of the Swedish comments play with the language – that may happily be
      lost.

      README.md has been English since 13.1. The decision about TODO.md is in the
      item below. That the app is otherwise made for Swedish pupils is settled and
      requires nothing – see the decision under Notes.

      **Outcome:** All `.cs` and `.xaml` files searched character by character for
      å/ä/ö/Å/Ä/Ö afterwards – two hits left, both correct: a quotation of the
      Swedish resource string "Karlavagnen - Stora björn" in an English comment
      explaining how it differs from "Big Dipper - Ursa Major", and the
      constellation name "Boötes" (Latin, not Swedish). The files were done one at
      a time, largest first: `SolarSystemDrawable.cs` (1554 lines),
      `MainPage.xaml.cs` (1278), `SurfaceMap.cs` (1081, 116 separate text
      substitutions since the file is mostly coordinate data that had to be left
      untouched), `SolarSystemData.cs` (619), `StarCatalog.cs` (492, plus all the
      constellations' English section headings), `Mission.cs` (506), `Probe.cs`,
      `StarSky.cs`, `SkyEvent.cs`, `Conic.cs`, `SmallBodyBelt.cs`, `ProbeData.cs`,
      `Lambert.cs`, `BodyAxis.cs`, `OrbitCamera.cs`, `Vec3.cs`, `Kepler.cs`,
      `Diagnostics.cs`, as well as the remaining XAML comments in `MainPage.xaml`.
      Built and verified error-free after every file.

- [x] **Decision: should TODO.md come along?** It contains internal notes and
      test lists, but also the only collected explanation of why things look the
      way they do – which errors were found, which simplifications were made and
      why. For someone who wants to understand the code it is probably worth more
      than the README.

      **Decision: yes.** A consistently English repository weighs more heavily
      than the saving of leaving it in Swedish, even though it nearly doubles the
      translation work (TODO.md is itself over 2,000 lines, comparable to the
      comments' 2,056).

**Verify:** Clone the repository into an empty directory on another machine and
build it using only the instructions in the README. If that does not work, the
README is not finished.

---

### 13.3 – Translate TODO.md into English

The decision was taken in 13.2: a consistently English repository weighs more
heavily than saving the translation work. Broken out into an item of its own
because it is as large a task by itself as the rest of 13.2 put together –
TODO.md is over 2,200 lines, comparable to the code comments' 2,056.

- [x] Translate the whole of TODO.md into English: every stage's description,
      test lists, "Outcome" sections, Notes and Remaining items.

      No mechanical translation – the document carries the whole project's
      history and reasoning (why the nodes were wrong, why Halley's orbit was
      anchored in two perihelia instead of taken from an ephemeris, why the
      force-push was chosen over filter-branch). That reasoning has to hold up
      just as well in English.

      Two decisions were left open until it was time. Both are now settled:

      **The file keeps the name `TODO.md`.** `ROADMAP.md` would describe the
      contents better, but the name is understood by everyone, nothing links to it
      that would have to be updated, and the git history follows the file without
      a detour.

      **No Swedish copy is kept in the repository.** The Swedish text remains in
      every commit up to the translation, which is a better archive than a frozen
      file in the working directory that would never be updated and would diverge
      from the first edit onwards.

**Verify:** No Swedish text left in the file outside quotations of UI strings
(the same sort of check as was made after 13.2's comment translation).

**Outcome:** The file was translated section by section, in twelve passes, and
then searched character by character for å/ä/ö/Å/Ä/Ö. What remains are the
quoted UI strings that must stay Swedish to make their point: the date format
"fredag 5 september 2025, 14:30", the pause button "⏸ Pausa", the body name
"Jorden", the constellation "Karlavagnen - Stora björn" and the message "Fart:
16,67 km/s" in 13.1's comparison table, along with the same constellation name
where 13.2 explains the two remaining hits in the code. The Swedish stage titles
became English ones ("Etapp" → "Stage"), and the headings' section numbers are
unchanged, so every cross-reference in the document still points where it did.

---

### 13.4 – An installer, and signing it

Publishing the source was 13.2. This is the other half of a public release:
that somebody who does not build things themselves can install and run it.
Today there is no answer to "where do I get the app" other than "install the
.NET SDK and the MAUI workload, then build it".

The state of the repository, checked rather than assumed:

- `<WindowsPackageType>None</WindowsPackageType>` in `Solarsystem.csproj`, so
  the build produces a loose folder of files and no installer at all.
- `Platforms/Windows/Package.appxmanifest` is still there and still holds the
  template's placeholders – `Identity Name="maui-package-name-placeholder"`,
  `Publisher="CN=User Name"`, `<DisplayName>$placeholder$</DisplayName>`,
  `<PublisherDisplayName>User Name</PublisherDisplayName>`. It is dead while
  the package type is `None`, but it is the same kind of template leftover
  that 13.2 went through the project for, and it comes alive the moment MSIX
  is chosen.
- `ApplicationDisplayVersion` is `1.0` and `ApplicationVersion` is `1`, both
  untouched since the project was created. A release needs a version that
  means something and a rule for when it moves.

- [ ] **Decide the form of distribution.** Three routes, and the choice
      decides everything below it:
      1. A zip of a self-contained publish. Simplest by far, no manifest and
         no installer to maintain, but the user unpacks it themselves and
         there is no uninstall entry.
      2. MSIX. The manifest above becomes real, the app gets a proper install
         and uninstall, and it is what the MAUI project is set up for. It also
         requires a certificate the machine trusts before it will install at
         all, which is the catch for anyone downloading from GitHub.
      3. A conventional installer (Inno Setup, WiX) around an unpackaged
         publish. Familiar to users, no store machinery, but a third-party
         tool in the build.

- [ ] **Decide about signing, honestly.** This is the part that costs money
      rather than time, so it is worth being clear about what each level
      actually buys:
      - Unsigned: SmartScreen warns on first run, with a "More info" step
        before the app will start. It does work, and for a school laptop it
        may be enough.
      - Self-signed: does nothing for SmartScreen. It is only useful for MSIX,
        where the user first has to install the certificate by hand – which
        is a worse experience than the warning it replaces.
      - A real code-signing certificate (OV): removes the publisher warning
        and builds SmartScreen reputation over time. It costs a few hundred
        euro a year and now requires the key to live on hardware or in a
        signing service, which is the part that makes it awkward for a
        hobby project.

      Worth weighing against the audience: this is a teaching app, and the
      people who most need an installer are the ones least likely to click
      past a security warning.

- [ ] **Fill in or delete the manifest placeholders**, depending on the route
      chosen above. Deleting is a real option – `Package.appxmanifest` is
      unused with `WindowsPackageType` set to `None`, and leaving a file full
      of `$placeholder$` in a public repository is the same untidiness 13.2
      cleaned up elsewhere.

- [ ] **Give the version a meaning.** Decide what 1.0 is, and whether the
      number moves per release or per commit.

- [ ] **Write down how a release is built**, in the README next to the build
      instructions. A release nobody remembers how to make is made once.

**Verify:** Take the built artifact to a machine that has never had the .NET
SDK on it – that is the whole point, and a developer machine cannot tell you
whether it works. Install, run, check that the app icon and name from 13.2
show up in the Start menu and in Add/Remove Programs, then uninstall and check
that it leaves nothing behind.

---

## Stage 14 – Separating logic from user interface

The goal is for the same solar system to be drivable by more than one user
interface, so that the app can in time run in a browser alongside the desktop app.

**The starting position is better than it looks.** A measurement of the code's
dependencies:

| folder | lines | dependencies on the user interface |
|---|---|---|
| `Simulation/` | 3,100 | only `Color`, 209 uses |
| `Rendering/` | 1,500 | `Microsoft.Maui.Graphics`: ICanvas, PathF, RectF, PointF |
| `MainPage.xaml(.cs)` | 1,350 | the whole of MAUI Controls, 41 controls, 30 event methods |

The simulation does not import MAUI anywhere – the only `using` in the whole
folder is `System.Numerics`. That is not a guess: the test program that has
verified every stage since stage 9 compiles the whole of `Simulation/` with a
twelve-line replacement for `Color` and nothing else. The logic is therefore
already free, it merely lives in the same project as the user interface.

### 14.1 – Extract the core into a project of its own

- [ ] Move `Simulation/` to `Solarsystem.Core`, a library without a dependency on
      MAUI.
- [ ] Decide what should happen to `Color`. Two routes: a small type of our own,
      or a dependency on the `Microsoft.Maui.Graphics` package. The latter sounds
      less innocent than it is – the package is independent of MAUI Controls and
      contains only drawing and colour primitives. But a type of our own makes the
      core entirely dependency-free, and 209 uses is not a large rewrite.
- [ ] Let the test program reference the project instead of copying the files and
      shimming `Color`. That has worked for a long time but is a copy that can
      drift out of step with the original.

**Verify:** `Solarsystem.Core` should build without a single reference to MAUI,
and all the test program's checks from stage 9 to 12 should pass against the built
library instead of against copied files.

### 14.2 – The drawing layer as a project of its own

- [ ] Move `Rendering/` to `Solarsystem.Rendering`, which may depend on
      `Microsoft.Maui.Graphics` but not on MAUI Controls.
- [ ] Investigate the question that determines the whole value of stage 14 – **can
      `ICanvas` be implemented on the web?** `SolarSystemDrawable` is 1,282 lines
      and draws everything through that interface. If there is a canvas
      implementation for the web, the whole drawing layer can be reused unchanged.
      If there is not, it has to be rewritten, and then it is the largest item in
      the entire project.

**Verify:** The desktop app should look the same after the move. The drawing code
is not touched, only where it lives.

### 14.3 – The state out of MainPage

This is the real difficulty. `MainPage.xaml.cs` is 1,080 lines and contains two
kinds of code mixed together: what talks to MAUI's controls, and what knows what
the app does. A second user interface has to be able to use the latter.

- [ ] Extract the state into a class with no user-interface dependency: the clock
      and the speed, the selected body in the focus picker, an ongoing space
      flight, which probes are shown, the camera.
- [ ] Let `MainPage` become thin – receive clicks, pass them on, display what
      comes back.

**Verify:** Count the lines in `MainPage.xaml.cs` afterwards. If it does not
become substantially shorter, the state has not moved, only gained company.

### 14.4 – Choose a web technology

An investigation, not a build. The question is not settled and should not be
settled until 14.1 to 14.3 have shown how much can actually be reused.

- [ ] Compare the alternatives. Blazor WebAssembly is the obvious candidate since
      everything is already C# and the core can then come along unchanged. But
      there are other routes: Blazor Server, the core compiled to WebAssembly with
      a user interface in something else, or a user interface written directly
      against a web canvas.
- [ ] Settle the drawing question from 14.2 first. It weighs more heavily than the
      choice of framework: if the drawing layer can be reused, the rest is mostly
      work on buttons, whereas a rewritten drawing layer is half a new project.
- [ ] Take into account that the app is computationally heavy on the client –
      orbits, surfaces and the star sky are recomputed every frame. That argues for
      the computation happening in the browser and not on a server.

**Verify:** A decision with the reasons written down, not a framework chosen by
feel.

---

## Notes and decisions

- **Scale:** moons follow the Moon's principle – in magnified mode a compressed
  distance to the parent planet (otherwise they end up unreasonably far away), in
  "Real size" mode true geometry.
- **Visibility:** moons and rings are only drawn when zoomed in (the same
  threshold as Earth's Moon) so that the overview stays clean.
- **Performance:** the belts are made with the same position cache as the star
  sky; no gradients in large quantities (the lesson from the Milky Way).
- **Number of moons:** we only draw the large/pedagogically useful moons. That
  Jupiter has ~95 and Saturn ~274 known moons can instead be mentioned in an info
  text.
- **New texts up until the language support:** written in Swedish as today for the
  time being, but preferably collected in a few places in the code so that stage
  13.1 (language support) is simple to carry out.
- **The control panel is grouped into drop-downs, not spread across rows.**
  It had grown to five rows and nine checkboxes, and the second row alone held
  nine controls of four different kinds. The checkboxes now live in two
  drop-downs built the same way as the probe selector from 10.6 – a button that
  unfolds a bordered box over the view – grouped by subject rather than by
  type: "Solar System" (orbits, moons, the two belts, Halley, the Moon's orbit,
  real size) and "Sky" (constellations, star names, star density). The panel is
  four rows instead of five, and its top row is now the whole of what the view
  shows.

  Two rules follow for anything added later. Each button carries a count of
  what is ticked, so a shut menu still says what is on – that is what made the
  clipped probe list noticeable in the first place. And only one menu is open
  at a time: all three are anchored to the same corner of the view, so two open
  at once would cover each other. Every drop-down is also capped to the height
  of the view and scrolls inside it, which is what 10.6's correction is about.

  **The top row has a width budget, and it is nearly spent.** Merging two rows
  into one pushed "Reset view" off the right edge of the window. Four controls
  there held 640 fixed pixels between them; the speed slider and its readout
  were the generous ones and gave back 90. The panel also shows its horizontal
  scrollbar now instead of hiding it – a row that outgrows the window should
  say so rather than quietly cut off whatever is last. Anything added to that
  row from here needs something else taken out of it, and the next thing to
  move would be the focus selector and its reset button, the two that are about
  the camera rather than about what is drawn.

- **The app may remain made for Swedish pupils.** That question came up ahead of
  stage 13.2: should the Swedish slant be toned down when the code is published
  openly? No. Open source solves that in its own way – whoever wants a variant for
  their own country forks it and adjusts it. It only concerns a couple of things
  anyway, since the solar system looks the same everywhere: the interface
  language, which 13.1 makes selectable, and the constellations' Swedish names
  such as Karlavagnen. The code comments are a separate question and are
  translated into English in 13.2, so that a fork is possible to make at all.

---

## Remaining items

Things that are known, deliberately left and not urgent, but that should be dealt
with at some point. Every project has a little technical debt – the point of the
list is that it is written down instead of forgotten.

### R1 – Try the user interface for real ✔

Stages 9 and 10 are verified with numbers: orbits, speeds, travel times,
milestones, hits on the planets and the index logic in the probe picker have all
been checked in a separate program outside the app. The drawing and the user
interface, on the other hand, have only been run far enough not to crash – nobody
has looked at how it actually appears. What is written here is therefore not a
known fault, only untested.

The space flights (9.4 and 9.5):

- [x] The lunar journey's orbit is visible once you have launched and zoomed to
      Earth, and the spacecraft meets the Moon on arrival.
- [x] The journey panel at the top left appears at launch, disappears when the
      journey is cancelled and changes its text on arrival.
- [x] The camera latches on to the spacecraft on arrival and zooms in to the
      destination, once.

The probes (10.2 to 10.4):

- [x] The probes' dots and trails look plausible, and the colours can be told
      apart.
- [x] The milestone rings end up correctly along the trails.
- [x] The years by the rings are legible and not a single porridge when several
      flybys are crowded together – the skipping on collision is the mechanism
      meant to prevent that, and it is completely untested in practice.
- [x] The selected probe's milestones get their full text (planet, month, speed
      jump) without writing over anything else.
- [x] The probe panel shows the right thing when the probe has not yet been
      launched, during the journey and after the last flyby.
- [x] **Selecting a probe in the focus picker.** First tick a probe in the "Space
      probes" box – otherwise it is not in the focus picker. Then select it there.
      The camera should jump out and put the probe in the middle of the frame, and
      the Sun should be visible as a dot in the frame with the whole planetary
      system shrunk around it. The point is the distance: Voyager 1 is 169 AU out,
      so the camera positions itself 406 AU from the Sun. Turn the camera round
      and tilt it – the Sun should stay in frame the whole way round.

The orbiting probes (10.5):

- [x] Cassini's ellipse is visible when you set the date between 2004 and 2017 and
      select Saturn, and is of the same order of size as Titan's orbit.
- [x] Juno's ellipse is visible at Jupiter between 2016 and 2025, and it is
      possible to zoom out far enough to see the whole wide lap.
- [x] Neither of them is drawn outside its mission time.

The probe picker (10.6):

- [x] The box unfolds and can be clicked in. It sits on top of the view and not in
      the control panel, so this is worth trying first of all in the list.
- [x] "All" and "None" do what they should, and the counter in the button text
      follows along.
- [x] **Turning off the probe you are following.** Follow a probe as in the item
      above, and then turn it off in the "Space probes" box while the camera is
      following it. Focus should fall back to the Sun, and the camera should jump
      **in** to the overview's 15 AU. Without that last part it would be left
      standing 300–400 AU out looking towards a dot, since it is only the target
      point that changes and not the distance. (The test list previously said that
      the view "zooms out" here. That was written wrongly: it is a zoom in, from
      hundreds of AU to fifteen.)

Stage 11:

- [x] The Earth globe looks and spins exactly as before the rebuild in 11.1.
- [x] Mars's surface map looks like Mars: the right rust tone, the features are
      recognisable and the polar cap sits as a white cap. But see the rest of the
      note below.
- [x] Jupiter's bands look like bands and not like striped joins. The 2 % overlap
      was enough – no seams are visible in the picture.
- [x] Jupiter's polar caps fill in the right direction, despite lying much farther
      from the pole than Earth's ice.
- [x] Jupiter's muted palette, seen in the picture and redone twice (see 11.3).
- [x] **The large moons were in practice impossible to see.** The camera could
      only be centred on planets, and a moon is only drawn as a globe at a
      14-pixel radius: Ganymede is 3.8 per cent of Jupiter's radius, so Jupiter
      would have to be 372 pixels – and then Io is 1,100 pixels from the centre of
      the frame, outside the window. Fixed by putting the moons in the focus
      picker, see the note after 11.7.
- [x] The Moon and Pluto in the app. The Moon is only drawn as a globe once it
      becomes large enough in the frame, and that threshold has not been tested for
      a moon – only for planets.
- [x] The control panel can be folded away. The "Hide controls" button at the top
      of the panel, or the M key, should leave only a 24-pixel-high strip and let
      the view grow. The button text should switch to "Show controls".
- [x] The lunar orbit and the nodes in the picture. Tick "Lunar orbit", select
      Earth and zoom in: the lunar orbit should be inclined to the yellow ecliptic
      ring and the node line should pass through Earth with both nodes named. The
      colours and sizes are guesses.
- [x] The eclipse view. Select "Solar eclipse" and press "Go to next": the lunar
      orbit and the moons should switch on by themselves, the camera position
      itself at Earth, and the Sun stand along the node line on that very day.
- [x] The heliopause in the picture. Select Voyager 1 in the focus picker so that
      the camera ends up at 406 AU; the sphere should then be visible as a faint
      circle with the solar system inside it, and Voyager 1 should lie outside it
      while New Horizons lies inside.
- [x] Halley's Comet in the picture. Tick "Halley's Comet", type 1986-02-09 in the
      date box and select the comet in the focus picker. The tails should point
      away from the Sun – the blue one straight and the yellow one shorter and
      curved – and as time moves forward they should swing round so that the comet
      travels tail first on the way out. Also try stepping a few years forward: the
      tail should disappear completely once the comet has come out past the
      asteroid belt. The orbit's shape and colour and the tails' width are guesses
      and have only been seen in code.
- [x] The sunspots in the picture. Zoom in on the Sun until the disc is a few
      centimetres wide – the spots appear at a 30-pixel radius. They should lie in
      two belts on either side of the equator, have a dark core in a lighter
      penumbra, and travel across the disc as time passes. Then step forward a year
      at a time: the lower groups should pull ahead of the upper ones. The colours
      of the core and penumbra are guesses, as is the size of the spots, and they
      have only been seen in a check drawing outside the app.
- [x] "Halley at perihelion" in the meetings picker. With the comet turned off:
      select it and press "Go to next". The date should become 2061-07-28, the
      checkbox should light up by itself, the camera should stay where it stood,
      and the text should say 0.48 AU from Earth and 20 degrees from the Sun.
- [x] Mercury's craters in the app. In the external check they look like craters,
      but they lie close together and can become speckled at high zoom.
- [x] Saturn, Uranus and Neptune in the app. They have been seen in the external
      check but not with the rings in place – Saturn in particular should be seen
      together with its rings, and Uranus's south polar cap lies near the edge
      where the rings cross.
- [x] **Mars's dark fields had visibly straight edges.** They are drawn with five
      to nine corners, and `Densify` subdivides long edges without rounding them –
      it only lays out more points along the same straight line. Fixed with
      Chaikin's corner cutting, two rounds, as a choice per map: Mars is rounded,
      Earth is not. Coastlines ARE angular, and Jupiter's bands must also keep
      straight edges so as not to gape. See the note in 11.2.
- [x] Saturn and Uranus are tilted in the right direction after the node
      correction – easiest to see on the rings at a couple of years far apart.

The control panel:

- [x] The "Space flight:" row with its three buttons fits even in a narrow window.
      It was precisely that row that overflowed the edge before the buttons were
      moved to a row of their own in 10.4.
- [x] **The pause button carries its text from the start**, "⏸ Pause", without
      having to be clicked once first. It was blank until then: when the texts
      moved out of XAML in 13.1 this was the one control whose text is derived
      from state (`_running`) rather than fixed, and it never got its line in
      `ApplyLanguage`. Fixed, but the fix has only been compiled, not seen.
      Switch language while time is running and check that the button follows
      along instead of keeping the old word.
- [x] **The launch-window tooltip on the mission row.** The status used to stand
      under the date, where it read as a statement about the whole view although
      it only ever concerned the trip to Mars; it now hangs on "Mission:",
      "Launch to Mars" and "Next launch window". The open question is whether
      Windows shows a tooltip on a *disabled* control at all – if it does not,
      the enabled button covers each state in turn (the launch button while the
      window is open, the next-window button while it is shut) and the
      "Mission:" label is the fallback that is never disabled. Check both
      states: inside a launch window, and outside one. Check also that the line
      under the date is gone, and that it comes back if a launch cannot be
      planned at all.

      **Corrected afterwards: it only appeared with the app paused.** Writing
      the tooltip property again dismisses one that is on its way up, and the
      status was rewritten roughly twice a second while time ran – it never
      survived its own hover delay. Paused, the date stops moving,
      `UpdateLaunchWindow` returns early and nothing rewrites it, which is why
      it worked there and only there. The text is now left alone unless it has
      actually changed, which is about once per simulated day.

      **Decided: paused is enough.** Wound forward fast the countdown still
      changes quicker than the hover delay, so the tooltip stays out of reach
      there. Chasing that would mean holding the text still while the number
      underneath moves, and the moment anybody wants to read a launch window
      is the moment they have stopped to look. Left as it is.

      Still unanswered, and now of little consequence: whether Windows shows
      the tooltip on a *disabled* control. Measured against the screenshot it
      came from "Next launch window", which is the enabled one. Until someone
      hovers a greyed-out "Launch to Mars" the status stays on all three
      controls, the row's label included, so it is reachable either way.

**Outcome: all of it seen in the app.** Every item above has now been run in the
running window, which is what the section was written for – the numbers were
never in doubt, only what they looked like. That matters most for the four items
that were colour judgements rather than function tests: the lunar orbit against
the ecliptic ring, the heliopause circle, Halley's two tails and the sunspots.
Their geometry had been verified outside the app, but the colours, sizes and
thresholds were guesses that only the eye could settle.

The two items added last – the pause button's text at start-up and the
launch-window tooltip on the mission row – were both fixes made without being
able to see them, and they hold.

### R2 – Confirm Juno's end date ✔

Juno was drawn until 30 September 2025, which was the extended mission's planned
end. If it was extended further, the app was not showing a probe that is actually
orbiting Jupiter. The choice was deliberate – better to miss a probe that is
flying than show one that does not exist – but it rested on a figure that could
not be checked when the code was written.

- [x] Find out whether Juno continued after September 2025 and correct the end
      date in `ProbeData`. It is one line.

**It flew on.** Juno passed the planned end and sent data throughout the spring of
2026; on 1 May 2026 it took close-up images of the little moon Thebe with its star
camera. After that there is nothing confirmed. That the information was hard to
come by in the autumn of 2025 had a dreary explanation: the American government
was shut down just when the mission was to have ended, so nobody could say
anything about the probe's fate.

The end date is now 1 May 2026, and the text says "Last confirmed contact" instead
of asserting a mission end. The risk ahead is budgetary rather than technical: the
probe works, but it was among the missions proposed for cancellation in the 2026
budget proposal. A proposal is not a law, but the situation is undecided.

Two further things came to light and are now in the code comment, since they
correct a reasonable but mistaken guess:

- **Juno will not be steered down into Jupiter**, unlike Cassini at Saturn. That
  was the plan from the start, for the same reason – a probe crashing on Europa
  could carry terrestrial bacteria into the ocean beneath the ice – but over the
  years in orbit the moons' gravity bent the orbit so much that Juno finally did
  not pass anywhere near Europa at all. Then there was nothing left to protect
  against, and the controlled crash was dropped.
- **The model's lap count does not agree with reality's.** The app draws a
  representative lap of 53 days, while the real Juno shortened its orbital period
  several times after the flybys of Ganymede and Io. The 213 extra days the probe
  is now visible correspond to four laps at the model's rate, but more in reality.

**Verify:** Juno should be drawn on 1 October 2025, which it was not before, and
no longer after 1 May 2026.

Checked outside the app, 12 checks without a single failure: Juno is drawn from
its arrival on 5 July 2016 up to and including 1 May 2026 and not the day after,
the orbit is unchanged (53.42 days, e = 0.9815, 57.7 km/s at perijove) and Cassini
is untouched.

### R3 – The precision of the drawn orbit ✔

The spacecraft's orbit was built Lambert → `Vector3` in single precision →
`Conic.FromState`, where the energy becomes a small difference between two large
numbers and the precision is thinned out. The lunar journey instead goes via
`Conic.FromPeriapsis`, which computes analytically in double precision and ends a
few centimetres from the Moon.

- [x] Hand over the state from Lambert in double precision, or let the solver
      return the conic directly.

**Corrected in the item above: it was not the Mars orbit.** It said here that it
ends about 850 km from Mars. Measured, it ends *exactly* at Mars – none of forty
orbits tested deviates more than the floating-point numbers can distinguish.
Where the figure came from I do not know; it was probably measured before the
Lambert-based rewrite of `Mission.Plan` in stage 10.

The error did exist, however, and in another place: **the space probes' legs**.
The join where two legs meet is the same point in space seen from two directions
and ought to be zero. It was up to 40,000 km. The reason is that Mars orbits are
well-behaved ellipses while the probe legs are almost parabolic, and then
2/r − v²/µ becomes a difference between two nearly equal numbers. Every digit
missing from the input is magnified a hundredfold in the answer. On top of that,
`Vector3.LengthSquared()` returned its value in single precision, so the speed
lost digits even before the subtraction.

Fixed with a `Vec3` in double precision, used only by the orbital mathematics:
`PositionAuAt` on the bodies, Lambert in and out, `Conic.FromState`, `Waypoint`
and `StarCatalog.EquatorialToWorldAu`. The drawing is unchanged – single precision
is quite enough there, one pixel being many thousands of kilometres.

**Verify:** The probe legs' joins should be down at the resolution of the
floating-point numbers, and everything verified in stage 10 should still hold.

Checked outside the app, 8 checks without a single failure, with the code from
before the change checked out of git and run through the same measurement:

| flyby | before | after |
|---|---|---|
| Voyager 1 at Jupiter | 610 km | 36 km |
| Voyager 2 at Jupiter | **20,589 km** | 51 km |
| Voyager 2 at Saturn | 802 km | 18 km |
| Pioneer 10 at Jupiter | 1,084 km | 71 km |
| Pioneer 11 at Saturn | **39,984 km** | 37 km |
| New Horizons at Jupiter | 1,042 km | 71 km |

The worst deviation goes from 39,984 km to 404 km, and the two gross outliers are
gone. What remains is no longer the model's error but the measurement's: the joins
lie at half to barely four floating-point steps, and one floating-point step is
36 km at Jupiter and 285 km out at Pluto. Below that limit there is no difference
to measure, and it is also the resolution the app draws with.

Everything else is unchanged: Voyager 1's speed today 16.66 km/s, the assist at
Jupiter +10.8 km/s, sixteen legs, the Mars windows on the same dates at the same
cost (3.12 / 3.09 / 3.00 / 2.93 / 3.30 km/s) and the lunar journey still exact.

**Two notes from stage 10 are wrong.** It says there that the Mars windows cost
"2.90–3.20 km/s"; the 2035 window costs 3.30, both before and after this change.
It also says there that all eleven flybys hit within "74–602 km". That cannot be
reproduced: with the code as it looked before the change I measure 610 to
39,984 km at the flyby date, and 610 to 34,214 km if you instead take the nearest
point along the orbit. Which method gave the old numbers I do not know, so they
stand as unreproduced rather than corrected to something I have guessed.

### R4 – "Real size" cannot be zoomed into ✔

The camera came no closer than 1.5 units, and `SuggestedFocusDistance` also had a
floor of 8. At real scale Earth's radius is 0.0026 units, so when you selected a
planet in the focus picker or followed the spacecraft down on arrival you ended up
far too far away to see anything. The positions were correct – it was only the
camera that fell short.

- [x] Let both the floor and the minimum distance depend on the selected body's
      visual radius instead of being fixed numbers.

`OrbitCamera.MinDistance` is no longer a constant but a property set every frame
from the body the camera is looking at: its drawn radius times 1.15, so that you
get close without ending up inside. The distance is clamped immediately when the
limit changes, so a switch between the modes never leaves the camera inside a
planet. The floor in `SuggestedFocusDistance` follows the body too. A spacecraft or
probe is a point without extent and may come as close as it likes.

All that remains is an absolute floor of 0.001 units, and that is set not by any
body but by the floating-point numbers: the world coordinates are single precision
and reach 2,400 units out at Neptune, where the step between two adjacent numbers
is a couple of ten-thousandths of a unit.

**Verify:** In "Real size", select a planet and zoom in – the surface should be
visible. Switch between the modes while zoomed in and see that the camera does not
end up inside.

Checked outside the app, 33 checks without a single failure:

- **Real scale now works.** If you select Mercury it goes from 0.1 pixels to 80.
  Earth gives 5.9 pixels on selection – the view frames the lunar orbit, as it
  should – and can be zoomed to 839. Before, 2 pixels was the closest you could
  get, however much you scrolled.
- **All bodies with a surface map can be zoomed to a globe in both modes**,
  against the threshold of 14 pixels. At the minimum distance the sphere fills
  839 pixels regardless of both mode and body, since the limit is proportional to
  the radius.
- **Magnified mode is unchanged** for eight of nine planets. Pluto comes closer,
  57 pixels becoming 80: it was the only planet the floor of eight units actually
  held back.
- **One more bug fell out into the bargain.** The old floor of 1.5 units lay
  *inside* several bodies in magnified mode – Earth is drawn with a radius of 2.6,
  Jupiter 28.0 and the Sun 8.4 – so you could zoom into them and see them from the
  inside. Now the limit always lies outside the surface.

The smallest bodies at real scale – Pluto, the Moon, Io – meet the absolute floor
before they meet their own and stop at 460 to 705 pixels instead of 839. Far more
than is needed.

### R5 – An unsolvable probe leg is skipped in silence ✔

`Probe.Build` skipped a leg that Lambert cannot handle and built the probe from
the rest. All the legs can be solved today, and that is checked outside the app,
but if someone entered an impossible pair of dates the probe would silently get a
gap in its orbit instead of the error being visible.

- [x] Let an unsolvable leg be noticed – return null from `Build`, or at least
      write to the same log that drawing errors go to.

The choice fell on the latter, for two reasons. `Build` is called from static
fields in `ProbeData`, so null would have forced null checks through the whole
chain – and an exception there would have brought down the whole app at start-up
over an error in the data. Besides, a probe with a gap is more useful than no
probe at all. But the error no longer passes unnoticed: skipped legs are written
to the log and collected in `Probe.SkippedLegs`, so that the test programs outside
the app can require the list to be empty. The message says which probe, which two
flybys, how many days and why.

The log path was baked into the drawing code. It now lives in
`Simulation/Diagnostics`, which both use – that was the precondition for being
able to write "to the same log" at all.

**Corrected in the text above:** it said here "all thirteen legs". That is wrong,
and it showed when the test counted them: the probes have **sixteen** legs.
Voyager 1 has three, Voyager 2 five, Pioneer 10 two, Pioneer 11 three and New
Horizons three. Thirteen was probably a count that missed the closing legs out to
today's positions.

**Verify:** Real probe data should give an empty list and no log at all.

Checked outside the app, 12 checks without a single failure:

- **Real data comes through clean.** Five probes, sixteen legs, an empty list, not
  even a log file created. Voyager 1's speed today is an unchanged 16.66 km/s and
  the eleven planetary flybys are still there.
- **Both error branches have actually been triggered**, not merely read. Flybys in
  the wrong order give "skipped (−365 days) – the flybys do not come in
  chronological order", and the probe gets zero legs and therefore does not exist.
  Two points directly opposite each other as seen from the Sun – where the orbital
  plane is undefined and every plane will do equally well – make Lambert genuinely
  fail, and it is noticed the same way.
- **The lines end up in the log**, in the same file as drawing errors.

**What the guard does not catch.** A leg that is implausible but mathematically
solvable gets through: Jupiter to Saturn in one day gives an orbit at 9,147 km/s,
and Lambert solves it without complaint. The guard looks for unsolvable legs, not
for implausible ones. Sifting on plausibility would be a different test – an upper
limit on the speed would do – but that is not what this item was about, and a
limit set at random risks sifting away real data.
