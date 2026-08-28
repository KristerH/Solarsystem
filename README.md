# Solsystemet i 3D

En .NET MAUI-skrivbordsapp (Windows) som visar solsystemet i 3D – byggd för
undervisning, så att elever kan se hur planeterna rör sig runt solen.

## Funktioner

- **Verkliga banor**: Planeterna följer keplerska ellipsbanor med verklig
  excentricitet, banlutning och omloppstid. Startpositionerna beräknas från
  banelement vid epok J2000, så planeternas lägen stämmer ungefär med
  verkligheten för det simulerade datumet.
- **Skalenliga avstånd**: Avstånden mellan banorna är alltid skalenliga (1 AU =
  60 enheter). Planeternas storlekar är inbördes skalenliga men förstorade så
  att de syns – bocka i **Verklig storlek** för att se hur små planeterna
  faktiskt är jämfört med avstånden.
- **Start/paus** av rotationen (knapp eller mellanslag) och justerbar
  hastighet (0,1–1000 dygn per sekund).
- **Klocka**: Simulerat datum (år-månad-dag, timme:minut) samt förfluten tid,
  så man t.ex. kan se att jorden går ett varv på 365 dagar.
- **Namnetiketter** under varje himlakropp.
- **Fri kamera**: dra med musen för att rotera, skrollhjul/W/S för att zooma,
  piltangenter för att rotera, R återställer vyn. Med **Fokus**-väljaren kan
  kameran följa en enskild planet.
- Stjärnhimmel med ett antytt vintergatsband som bakgrund.

## Bygga och köra

Öppna `Solarsystem.sln` i Visual Studio (med arbetsbelastningen
".NET Multi-platform App UI development" installerad) och tryck F5,
eller från kommandoraden:

```
dotnet build Solarsystem.csproj
dotnet run --project Solarsystem.csproj -f net10.0-windows10.0.19041.0
```

## Kodöversikt

- `Simulation/SolarSystemData.cs` – planetdata (banelement J2000) och
  Kepler-beräkning av positioner.
- `Rendering/OrbitCamera.cs` – kamera som kretsar kring en målpunkt och
  projicerar 3D-punkter till skärmen.
- `Rendering/SolarSystemDrawable.cs` – ritar stjärnor, banor, solen,
  planeter (djupsorterade och skuggade mot solen), Saturnus ringar och
  etiketter via MAUI:s `GraphicsView`.
- `MainPage.xaml(.cs)` – UI, simuleringsklocka och mus-/tangentbordsstyrning.
