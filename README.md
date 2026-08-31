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
  en sådan kan bara nå punkter exakt 180° bort, och just där blir banplanet
  obestämt: Mars ligger 1,85° ur ekliptikan och är nästan aldrig exakt
  antiparallell med jorden. Därför löses banan i stället ur sina randvillkor
  med en Lambert-lösare, som ger den bana som går från jordens läge på
  uppskjutningsdagen till Mars läge på ankomstdagen. Träffen hamnar inom någon
  tusendels AU, alltså gott och väl innanför Mars radie på 3 390 km.

  Restiden väljs så att uppskjutningen blir så billig som möjligt, mätt som den
  fart farkosten måste ha i förhållande till jorden när den lämnar. Det ger
  omkring 3 km/s, vilket också är vad verkliga Mars-uppdrag kostar. Den
  billigaste banan sveper kring 200° i stället för exakt 180 – lite mer än ett
  halvt varv, eftersom Mars ska hinna fram till mötet – och restiden blir
  294 dygn i fönstret hösten 2026, med ankomst i augusti 2027.

  Vid framkomsten följer farkosten med planeten i stället för att bli
  stående kvar där Mars råkade vara – en verklig sond går ju in i omloppsbana
  eller landar. Etiketten byts till "Farkost framme".

  **Startfönster**: knappen är bara aktiv när en energisnål färd faktiskt går
  att göra, och "Nästa startfönster" hoppar fram till nästa tillfälle. Kravet är
  att uppskjutningen kostar högst 0,1 km/s mer än fönstrets allra bästa dag. Det
  ger fönster på 11–34 dygn som återkommer var 780:e dygn, precis som i
  verkligheten: de fem närmaste infaller i oktober 2026, november 2028, januari
  2031, mars 2033 och juni 2035, vilket är samma rytm som de verkliga
  Mars-fönstren. Det är därför Mars-uppdrag alltid skjuts upp i klungor –
  sommaren 2020 skickade USA, Kina och Förenade arabemiraten var sin sond inom
  två veckor, och sedan hände ingenting på två år.

  Måttet är medvetet relativt i stället för en fast gräns i km/s, eftersom
  fönstren är olika bra: det billigaste tillfället pendlar mellan 2,90 och
  3,20 km/s beroende på var Mars står i sin excentriska bana. Restiden varierar
  ännu mer, mellan 178 och 318 dygn, för ibland är den korta vägen strax under
  ett halvt varv billigast och ibland den långa strax över. Lärobokens 259 dygn
  och 44° fasvinkel gäller cirkulära banor; verklighetens värden pendlar kring
  dem.
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
- **Färdpanel och ankomst**: medan en farkost är i väg visas förfluten restid,
  återstående tid, avståndet kvar till målet och farkostens fart. Farten är den
  intressanta raden: den faller hela vägen, precis som Keplers andra lag säger –
  mot Mars från 33,1 till 20,5 km/s, mot månen från 10,8 till 0,6 km/s.
  Avståndet till månen krymper hela vägen, men avståndet till Mars *växer*
  först, från 246 till 409 miljoner km, innan det faller. Farkosten går ju runt
  solen och inte rakt mot planeten, och Mars står på andra sidan solen när
  färden börjar.

  Vid framkomsten hakar kameran på farkosten och zoomar in till målet, så att
  man ser den komma fram. Det sker en gång, i själva ankomstögonblicket –
  därefter styr man fritt igen, och ett nytt val i fokusväljaren eller
  "Återställ vy" släpper greppet.
- **De fem rymdsonderna** som är på väg ut ur solsystemet – Voyager 1 och 2,
  Pioneer 10 och 11 samt New Horizons – med spår efter sig. De är inte inmatade som banelement utan byggda ur sina verkliga
  datum: varje ben av färden är banan som går från en planet till nästa på exakt
  den tid passagerna tog, räknad ur appens egna planetpositioner. Sonderna
  hamnar därför vid rätt planet rätt dag av sig själva – sämsta träffen av de
  elva passagerna är 602 km vid Neptunus, alltså två hundradels planetradie, och
  New Horizons möter Pluto på 319 km.

  Sista benet går ut till sondens kända läge i dag, och därmed blir också
  lutningen ut ur ekliptikan ett resultat i stället för en inmatning: +35,6° för
  Voyager 1 och −47,9° för Voyager 2, mot de vedertagna 35° och 48°. Luta
  kameran så syns det direkt att de lämnat solsystemets skiva åt var sitt håll.
  I dag ligger de på 169 och 142 AU med farterna 16,7 och 15,0 km/s (NASA:s
  siffror: 167 och 140 AU, 17,0 och 15,4 km/s). Pioneer 10 och 11 följer nästan
  ekliptikan, på 142 respektive 120 AU, och New Horizons har hunnit 65 AU.
  Pioneer 10 tystnade 2003 och Pioneer 11 redan 1995, så deras lägen är
  framräknade snarare än mätta.

  Gravitationsslungan följer på köpet, eftersom benen möts i samma läge men med
  olika hastighet: Jupiter gav Voyager 1 hela 10,8 km/s och Pioneer 10 så mycket
  som 12,1. Vid Neptunus *bromsades* Voyager 2 i stället med 2,3 km/s, priset
  för att svänga ner mot månen Triton, och vid Pluto händer i praktiken
  ingenting med New Horizons – Pluto är för liten för att slunga något.

  Banornas form berättar samma sak: sondernas första ben, från jorden till
  Jupiter, är ellipser medan allt efter Jupiter är hyperbler. Det var alltså
  Jupiter som gav dem fart nog att aldrig komma tillbaka. Två undantag finns.
  Pioneer 11 slungades av Jupiter inte utåt utan inåt och tvärs över
  solsystemet – banan faller in till 3,8 AU, går ett halvt varv runt solen och
  klättrar ut till Saturnus, elva grader över ekliptikan – och först Saturnus
  gav den fart nog att lämna. New Horizons är det motsatta undantaget: den var
  på en hyperbel redan från uppskjutningen, den snabbaste som gjorts, och
  passerade månens bana efter nio timmar mot Apollos tre dygn.

  Pluto-passagen 2015 är också provet på att banorna räknas i tre dimensioner.
  Pluto låg då 1,10 AU utanför ekliptikans plan – mer än hela jordens banradie –
  och New Horizons möter den där, inte i planet.

  **Milstolpar och panel**: uppskjutningen och varje planetpassage markeras med
  en ring längs spåret, med årtal. Väljer man en sond i fokusväljaren får den
  fullständiga etiketter – planet, månad och farthopp – och en panel visar
  avstånd, fart, vad den senaste passagen gav och när nästa infaller. Farten är
  raden att titta på: den hoppar vid varje passage och sjunker sedan långsamt
  medan sonden klättrar ur solens gravitation. Voyager 1 går från 27,4 km/s
  efter uppskjutningen till 20,4 vid Saturnus 1982, 17,7 år 1990 och 16,67 i
  dag, och kurvan planar ut.

  **Välja sonder**: knappen "Rymdsonder" fäller ut en ruta där varje sond bockas
  i för sig, så att man kan visa bara Voyager 1, eller båda Voyagersonderna för
  att jämföra deras motsatta vägar ut ur ekliptikan, utan att de andra ligger i
  vägen. Valet gäller prick, spår och milstolpar, och de två kretsande sonderna
  finns i samma lista. Släcker man den sond kameran följer faller fokus tillbaka
  till solen och vyn zoomar ut till översikten.

  **Skalan**: väljer man en sond zoomar kameran ut till drygt två gånger dess
  avstånd, så att solen precis ryms i bild. Då krymper hela planetsystemet till
  en prick i mitten, vilket i sig är poängen: Voyager 1 är 167 gånger längre
  bort än jorden och fyra gånger längre än Neptunus.
- **Kretsande sonder**: Cassini vid Saturnus (2004–2017) och Juno vid Jupiter
  (från 2016) ritas med hela sin banellips kring planeten. De är enklare fall än
  de fem som lämnat solsystemet – vanliga ellipser – men banorna är också av ett
  annat slag: de är representativa snarare än återskapade. Cassini flög nästan
  trehundra olika varv, så storlek, form, omloppstid och banplan är verkliga
  medan sondens plats i banan ett givet datum inte är det.

  Kontrasten mellan de två är poängen. Cassinis varv tar 16 dygn och lutar
  20° mot ringplanet; Junos tar 53 dygn och går rakt över polerna, till skillnad
  från månarna som ligger i ekvatorsplanet. Junos bana är dessutom extrem: ned
  till 1,08 Jupiterradier, alltså några tusen kilometer över molntopparna, och ut
  igen till 116 radier. Farten växlar därefter, mellan 57,7 km/s vid närmaste
  punkt – vilket gör Juno till det snabbaste föremål människan skickat i
  förhållande till en planet – och 0,54 km/s längst ut.

  Banorna trycks ihop med samma faktor som månbanorna, så proportionerna hålls:
  Cassinis varv är nästan exakt lika stort som Titans bana, och Junos sträcker
  sig drygt fyra gånger längre ut än Callisto. Välj Saturnus eller Jupiter i
  fokusväljaren vid ett datum inom uppdragstiden för att se dem.
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
- `Simulation/Mission.cs` – planerar och räknar överföringsbanor: till Mars med
  Lambert-lösaren och den billigaste restiden, till månen ur en given restid.
  Samma klass klarar banor kring solen och kring en planet.
- `Simulation/Kepler.cs` – Keplers ekvation i sina två former: `E - e·sin E = M`
  för ellipser och `e·sinh H - H = M` för hyperbler. Den senare behövs för
  rymdsonder som fått så hög fart att de aldrig kommer tillbaka.
- `Simulation/Conic.cs` – ett kägelsnitt byggt ur ett tillstånd, alltså ett läge
  och en hastighet vid en tidpunkt. Klarar både ellipser och hyperbler, och är
  det som beskriver en sonds bana mellan två planetpassager.
- `Simulation/Lambert.cs` – banan som går från ett läge till ett annat på exakt
  en given tid, löst med universella variabler. Det är den som gör att sonderna
  kan byggas ur verkliga uppskjutnings- och passagedatum i stället för ur
  inmatade banelement.
- `Simulation/Probe.cs` – en verklig rymdsond som en kedja av ben, där varje ben
  är den bana som går mellan två passager på den tid de faktiskt tog.
- `Simulation/ProbeData.cs` – de sju sondernas data: Voyager 1 och 2, Pioneer 10
  och 11 samt New Horizons med sina passagedatum och sina kända lägen i dag,
  plus Cassini och Juno med sina banor kring Saturnus och Jupiter.
- `Simulation/Orbiter.cs` – en sond som kretsar kring en planet i stället för att
  fara förbi. Banan anges i planetradier och lutning mot planetens ekvator, så
  att en polär bana blir polär oavsett hur planeten själv lutar.
- `Simulation/StarCatalog.cs` – stjärnkatalogen och stjärnbildernas figurer,
  samt omräkningen från ekvatorial- till världskoordinater.
- `Rendering/StarSky.cs` – ritar stjärnor, stjärnbilder och Vintergatan.
- `Rendering/OrbitCamera.cs` – kamera som kretsar kring en målpunkt och
  projicerar 3D-punkter till skärmen.
- `Rendering/SolarSystemDrawable.cs` – ritar stjärnor, banor, solen,
  planeter (djupsorterade och skuggade mot solen), Saturnus ringar och
  etiketter via MAUI:s `GraphicsView`.
- `MainPage.xaml(.cs)` – UI, simuleringsklocka och mus-/tangentbordsstyrning.
