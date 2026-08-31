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
- **Månar**: 15 månar kretsar kring sina planeter med riktiga banelement och
  visas när man zoomar in – jordens måne, Mars Phobos och Deimos, Jupiters
  fyra galileiska månar, Saturnus Enceladus, Rhea och Titan, Uranus
  Miranda, Titania och Oberon, Neptunus Triton samt Plutos Charon.
  Titan är större än Merkurius, och Enceladus är solsystemets ljusaste
  kropp. Fem av månarna kretsar **retrograd**, alltså åt motsatt håll mot
  allt annat: Uranus tre månar (planeten ligger på sidan), Charon (Pluto
  roterar baklänges) och framför allt Triton, som med all sannolikhet är
  en infångad dvärgplanet från Kuiperbältet.
  Phobos gör ett varv på 7,7 timmar – snabbare än Mars snurrar runt sin egen
  axel. Jupitermånarnas faslägen är valda så att Laplace-resonansen gäller:
  Io, Europa och Ganymedes har omloppstider i förhållandet 1:2:4 och kan
  därför aldrig stå på linje samtidigt – när Io och Europa möts står
  Ganymedes alltid 90° bort. Månsystemens geometri följer planeternas förstoring, så
  att avstånden blir rätt i förhållande till planeten; systemet komprimeras
  först när det behövs – när innersta månen skulle hamna längre ut än
  3 planetradier (vår egen måne ligger på 60) eller yttersta månen längre ut
  än 10 (Callisto ligger på 27 jupiterradier). En måne trycks aldrig
  närmare än 2,5 planetradier, så att Enceladus håller sig utanför
  Saturnus ringar. I läget "Verklig storlek"
  används äkta geometri rakt igenom. Väljer man en planet i fokusväljaren
  zoomar kameran så att hela dess månsystem ryms i bild.
- **Dubbelplaneten Pluto–Charon**: Charon har halva Plutos diameter och en
  åttondel av dess massa, så parets gemensamma tyngdpunkt hamnar 2 126 km
  från Plutos centrum – alltså utanför Pluto självt, som har radien
  1 188 km. Appen räknar därför Kepler-banan som tyngdpunktens bana och
  låter Pluto vagga kring den, i stället för att stå stilla. Jorden–månen
  har samma effekt men mycket svagare: den tyngdpunkten ligger inuti
  jorden. Charon kretsar dessutom retrograd, eftersom Pluto själv roterar
  åt "fel" håll.
- **Ringar kring alla fyra jätteplaneter** – inte bara Saturnus. Radierna
  är verkliga: Jupiters tunna dammring (122 500–129 000 km), Saturnus
  breda isringar (74 700–136 800 km), Uranus smala kolmörka ringar
  (38 000–51 150 km) och Neptunus svaga ringar ut till Adams-ringen
  (41 900–62 933 km). Ringarna ligger i planetens ekvatorsplan, samma
  plan som månarna, så Uranus ringar står på högkant – 82° mot ekliptikan.
  Saturnus ringar syns även i översiktsvyn, medan de tre andra är så
  svaga att de upptäcktes först med rymdsonder och därför bara ritas när
  man zoomat in ordentligt.
- **Rymdfärd till Mars**: knappen "Skjut upp mot Mars" skickar i väg en
  farkost från jordens läge det datum vyn står på. Farkosten följer sedan
  sin överföringsbana utan att styra, precis som en verklig sond mellan
  raketmotorns två korta brinntider. Den tillryggalagda sträckan ritas
  ljusare än den som återstår.

  Banan är i grunden en Hohmann-överföring – den energisnålaste vägen – men
  en sådan kan bara nå punkter exakt 180° bort, och Mars ligger 1,85° ur
  ekliptikan och är därför nästan aldrig exakt antiparallell med jorden.
  Därför löses banan ur sina randvillkor i stället, vilket ger träffar inom
  några hundra kilometer – gott och väl innanför Mars radie på 3 390 km.
  Vid det bästa fönstret i slutet av 2026 blir restiden 264 dygn, något
  längre än lärobokens 259 eftersom Mars just då står 1,57 AU ut i stället
  för sitt medelavstånd 1,52.

  Vid framkomsten följer farkosten med planeten i stället för att bli
  stående kvar där Mars råkade vara – en verklig sond går ju in i omloppsbana
  eller landar. Etiketten byts till "Farkost framme".

  **Startfönster**: knappen är bara aktiv när en energisnål färd faktiskt går
  att göra, och "Nästa startfönster" hoppar fram till nästa tillfälle. Kravet
  är att banan sveper minst 170° kring solen, alltså ligger nära en halv
  ellips; då blir fönstren 30–50 dygn långa och återkommer var 780:e dygn,
  precis som i verkligheten. Det är därför Mars-uppdrag alltid skjuts upp i
  klungor – sommaren 2020 skickade USA, Kina och Förenade arabemiraten var
  sin sond inom två veckor, och sedan hände ingenting på två år.

  Både restid och fasvinkel varierar mellan fönstren – 235 till 264 dygn,
  och Mars ligger mellan 29° och 63° framför jorden vid uppskjutningen.
  Lärobokens 259 dygn och 44° gäller cirkulära banor; Mars excentricitet
  gör att de faktiska värdena pendlar kring dem.
- **Rymdfärd till månen**: knappen "Skjut upp mot Månen" skickar i väg en
  farkost från en låg omloppsbana på 400 km höjd, och vyn hoppar samtidigt in
  till jorden – hela färden ryms inom 0,003 AU och vore annars mindre än en
  pixel. Här kretsar farkosten kring jorden i stället för kring solen: samma
  ellips och samma Kepler-ekvation, men med jordens gravitationsparameter,
  som är 330 000 gånger mindre än solens.

  Restiden är tre dygn, som Apollo. En ren Hohmann-bana ut till månen skulle
  ta 4,95 dygn, så farkosten måste skjutas upp med mer fart än så: banans
  bortre ände hamnar 440 000–630 000 km bort, alltså långt bortom månen, och
  månen hinns ikapp på vägen ut – före vändpunkten. Startfarten blir
  10,84 km/s, precis som i verkligheten, och farten har fallit till under
  1 km/s vid framkomsten.

  Den stora skillnaden mot Mars är att uppskjutningen kan ske vilken dag som
  helst. Från en omloppsbana kan farkosten lämna åt vilket håll som helst, så
  startpunkten väljs så att banan möter månen – och månen är dessutom tillbaka
  på samma ställe var 27:e dygn. Inga startfönster behövs.
- **Skalenliga avstånd**: Avstånden mellan banorna är alltid skalenliga (1 AU =
  60 enheter). Planeternas storlekar är inbördes skalenliga men förstorade så
  att de syns – bocka i **Verklig storlek** för att se hur små planeterna
  faktiskt är jämfört med avstånden.
- **Asteroidbältet** kan slås på med en kryssruta (av från start). 1 400
  småkroppar mellan Mars och Jupiter, med samma statistiska fördelning som
  det verkliga bältet: halva storaxlar 2,06–3,27 AU, medelexcentricitet
  0,14 och medelbanlutning 9,5°. Varje asteroid följer sin egen
  Kepler-bana, så inre delen av bältet roterar snabbare än den yttre.
  **Kirkwood-gapen** är utsparade – de tomma spalter där Jupiters
  upprepade knuffar har rensat bort asteroiderna genom resonanser.
  Dvärgplaneten **Ceres**, som ensam rymmer en fjärdedel av bältets massa,
  ritas med namn på sin verkliga bana.
- **Kuiperbältet** bortom Neptunus, också med egen kryssruta. 1 100 isiga
  kroppar i två befolkningar: *plutinos* låsta i 3:2-resonans med Neptunus
  vid 39,4 AU – de hinner två varv medan Neptunus hinner tre, och Pluto
  själv är en av dem – och det *klassiska bältet* mellan 42 och 47,8 AU,
  där det tar abrupt slut vid "Kuiperklippan". Banlutningarna är ungefär
  som asteroidbältets, men eftersom bältet ligger sexton gånger längre bort
  blir det i absoluta mått femton gånger tjockare: kropparna når 18 AU upp
  och ner mot asteroidbältets 1,2. Luta kameran och zooma ut så syns
  skillnaden tydligt.
- **Visa/dölj månar**: en kryssruta slår av och på alla planeters månar
  på en gång, för en renare översiktsvy.
- **Start/paus** av rotationen (knapp eller mellanslag) och justerbar
  hastighet (0,1–1000 dygn per sekund).
- **Klocka och datumstyrning**: Simulerat datum (år-månad-dag, timme:minut)
  samt förfluten tid, så man t.ex. kan se att jorden går ett varv på
  365 dagar. Man kan hoppa till vilket datum som helst genom att skriva in
  det, stega ± dag, månad eller år med knappar, och återvända till nuet med
  "Idag". Hastighetsreglaget går åt båda hållen: mitten står still, höger
  halva spelar tiden framåt och vänster baklänges. Banberäkningen fungerar
  lika bra bakåt – kontrollerad mot solens läge ända tillbaka till 1977.
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
  styr man tätheten: "Inga" släcker hela stjärnhimlen för en helt svart
  bakgrund – bra när eleverna ska titta enbart på solsystemet – medan "Få"
  visar bara katalogens riktiga stjärnor och "Normalt" och "Många" lägger
  till bakgrundsstjärnor och Vintergatan.

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
- `Simulation/SmallBodyBelt.cs` – asteroid- och Kuiperbältets slumpade banor,
  med förberäknad rotation så att en position kostar en Kepler-lösning.
- `Simulation/Mission.cs` – planerar och räknar överföringsbanor: till Mars
  löst ur randvillkoren, till månen ur en given restid. Samma klass klarar
  banor kring solen och kring en planet.
- `Simulation/StarCatalog.cs` – stjärnkatalogen och stjärnbildernas figurer,
  samt omräkningen från ekvatorial- till världskoordinater.
- `Rendering/StarSky.cs` – ritar stjärnor, stjärnbilder och Vintergatan.
- `Rendering/OrbitCamera.cs` – kamera som kretsar kring en målpunkt och
  projicerar 3D-punkter till skärmen.
- `Rendering/SolarSystemDrawable.cs` – ritar stjärnor, banor, solen,
  planeter (djupsorterade och skuggade mot solen), Saturnus ringar och
  etiketter via MAUI:s `GraphicsView`.
- `MainPage.xaml(.cs)` – UI, simuleringsklocka och mus-/tangentbordsstyrning.
