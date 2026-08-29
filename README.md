# Solsystemet i 3D

En .NET MAUI-skrivbordsapp (Windows) som visar solsystemet i 3D – byggd för
undervisning, så att elever kan se hur planeterna rör sig runt solen.

## Funktioner

- **Verkliga banor**: De åtta planeterna samt dvärgplaneten Pluto följer
  keplerska ellipsbanor med verklig excentricitet, banlutning och omloppstid.
  Startpositionerna beräknas från banelement vid epok J2000, så lägena stämmer
  ungefär med verkligheten för det simulerade datumet. Plutos bana är en bra
  kontrast till planeternas: 17° lutning, 248 års omloppstid och så excentrisk
  att den tidvis går innanför Neptunus bana.
- **Månar**: Jordens måne (27,3 dygn per varv), Mars Phobos och Deimos samt
  Jupiters fyra galileiska månar Io, Europa, Ganymedes och Callisto kretsar
  kring sina planeter med riktiga banelement, och visas när man zoomar in.
  Phobos gör ett varv på 7,7 timmar – snabbare än Mars snurrar runt sin egen
  axel. Jupitermånarnas faslägen är valda så att Laplace-resonansen gäller:
  Io, Europa och Ganymedes har omloppstider i förhållandet 1:2:4 och kan
  därför aldrig stå på linje samtidigt – när Io och Europa möts står
  Ganymedes alltid 90° bort. Månsystemens geometri följer planeternas förstoring, så
  att avstånden blir rätt i förhållande till planeten; systemet komprimeras
  först när det behövs – när innersta månen skulle hamna längre ut än
  3 planetradier (vår egen måne ligger på 60) eller yttersta månen längre ut
  än 10 (Callisto ligger på 27 jupiterradier). I läget "Verklig storlek"
  används äkta geometri rakt igenom. Väljer man en planet i fokusväljaren
  zoomar kameran så att hela dess månsystem ryms i bild.
- **Skalenliga avstånd**: Avstånden mellan banorna är alltid skalenliga (1 AU =
  60 enheter). Planeternas storlekar är inbördes skalenliga men förstorade så
  att de syns – bocka i **Verklig storlek** för att se hur små planeterna
  faktiskt är jämfört med avstånden.
- **Visa/dölj månar**: en kryssruta slår av och på alla planeters månar
  på en gång, för en renare översiktsvy.
- **Start/paus** av rotationen (knapp eller mellanslag) och justerbar
  hastighet (0,1–1000 dygn per sekund).
- **Klocka**: Simulerat datum (år-månad-dag, timme:minut) samt förfluten tid,
  så man t.ex. kan se att jorden går ett varv på 365 dagar.
- **Namnetiketter** under varje himlakropp.
- **Fri kamera**: dra med musen för att rotera, skrollhjul/W/S för att zooma,
  piltangenter för att rotera, R återställer vyn. Med **Fokus**-väljaren kan
  kameran följa en enskild planet.
- **Riktig stjärnhimmel**: 225 av himlens ljusstarkaste stjärnor med verkliga
  koordinater (epok J2000), verklig skenbar magnitud och verklig färg (räknad
  ur färgindex B-V, så Betelgeuse blir röd och Rigel blåvit). 27 stjärnbilder
  kan visas med figurlinjer och svenska namn – Orion, Karlavagnen - Stora
  björn, Södra
  korset, Skorpionen och så vidare. Vintergatan ligger längs det verkliga
  galaktiska planet och är som ljusast mot galaktiska centrum i Skytten.
  Eftersom stjärnorna räknas om från ekvatorial- till ekliptikakoordinater
  hamnar de rätt i förhållande till planetbanorna: planeterna rör sig genom
  zodiakens stjärnbilder, precis som på riktigt. Med **Stjärnor**-väljaren
  styr man tätheten: "Få" visar bara katalogens riktiga stjärnor, "Normalt"
  och "Många" lägger till bakgrundsstjärnor och Vintergatan.

Prestanda: stjärnhimlen ligger på oändligt avstånd, så alla skärmpositioner
cachas och räknas bara om när kameran roteras – zoom och planetrörelser rör
den inte. Banornas skärmfigurer cachas på samma sätt, alla färger är
förberäknade och allt utanför skärmen hoppas över. Appen ritar i 30 bilder/s
och hoppar över omritningen helt när den är pausad och kameran står stilla.

## Bygga och köra

Öppna `Solarsystem.sln` i Visual Studio (med arbetsbelastningen
".NET Multi-platform App UI development" installerad) och tryck F5,
eller från kommandoraden:

```
dotnet build Solarsystem.csproj
dotnet run --project Solarsystem.csproj -f net10.0-windows10.0.19041.0
```

## Planerade utbyggnader

Se [TODO.md](TODO.md) – en etappindelad lista för fler månar, ringar och
asteroid-/Kuiperbälten, tänkt att byggas och verifieras en etapp i taget.

## Kodöversikt

- `Simulation/SolarSystemData.cs` – planetdata (banelement J2000) och
  Kepler-beräkning av positioner.
- `Simulation/StarCatalog.cs` – stjärnkatalogen och stjärnbildernas figurer,
  samt omräkningen från ekvatorial- till världskoordinater.
- `Rendering/StarSky.cs` – ritar stjärnor, stjärnbilder och Vintergatan.
- `Rendering/OrbitCamera.cs` – kamera som kretsar kring en målpunkt och
  projicerar 3D-punkter till skärmen.
- `Rendering/SolarSystemDrawable.cs` – ritar stjärnor, banor, solen,
  planeter (djupsorterade och skuggade mot solen), Saturnus ringar och
  etiketter via MAUI:s `GraphicsView`.
- `MainPage.xaml(.cs)` – UI, simuleringsklocka och mus-/tangentbordsstyrning.
