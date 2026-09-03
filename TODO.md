# Att göra: månar, ringar och bälten

Planen är att lägga till en eller några punkter i taget, så att varje steg
hinner verifieras innan nästa påbörjas. Bocka av med `[x]` när en etapp är
klar och godkänd.

**Klart hittills:** Etapp 1–8 är genomförda och verifierade – månar, ringar
och båda småkroppsbältena finns på plats.

**Ordningen på de kvarvarande** är vald med flit: rymdfärden (9) börjar med
att man ska kunna ställa om datumet, vilket Voyager-etappen (10) behöver för
att kunna spola tillbaka till 1977. Ytor och rotation (11) är fristående och
kan flyttas fram om man hellre vill ha något visuellt däremellan. Språkstödet
(12) ligger sist, så att alla texter finns på plats och bara behöver flyttas
ut en enda gång.

---

## Etapp 1 – Infrastruktur: generellt månsystem

- [x] Bygg om månkoden så att varje planet kan ha en lista av månar i
      `SolarSystemData` (i dag är jordens måne ett specialfall i renderaren).
      Samma regler som för Månen: geocentriska/planetcentriska banelement,
      Kepler-beräkning kring moderplaneten, synlig först vid inzoomning,
      ingen namnetikett (eller liten etikett vid hög inzoomning – beslut tas här).
- [x] I förstorat läge visas månarna på komprimerat avstånd (som Månens
      3 x jordradien) men med inbördes korrekta avståndsproportioner
      mellan månarna i samma system.

**Verifiera:** Jordens måne ser ut och beter sig exakt som före ombyggnaden.

---

## Etapp 2 – Mars månar

- [x] Phobos (radie 11 km, omloppstid 7,7 timmar – snabbare än Mars rotation!)
- [x] Deimos (radie 6 km, omloppstid 30,3 timmar)

**Verifiera:** Fokusera Mars, sänk hastigheten till några timmar/sek.
Phobos ska hinna flera varv per Mars-dygn. Båda är oregelbundna småstenar –
i appen blir de prickar.

---

## Etapp 3 – Jupiters galileiska månar (störst pedagogiskt värde!)

- [x] Io (omloppstid 1,77 dygn)
- [x] Europa (3,55 dygn)
- [x] Ganymedes (7,15 dygn – större än Merkurius)
- [x] Callisto (16,69 dygn)

Omloppstiderna bildar nästan exakt 1:2:4-resonans (Io:Europa:Ganymedes) –
värt att kunna visa. Det var dessa fyra Galilei såg 1610.

**Verifiera:** Fokusera Jupiter vid ~1 dygn/sek: Io snurrar tydligt snabbast,
Callisto långsammast. Kolla resonansen: två Io-varv per Europa-varv.

---

## Etapp 4 – Pluto och Charon (dubbelplaneten)

- [x] Charon (omloppstid 6,39 dygn, radie 606 km – halva Plutos!)
- [x] Eventuellt: gemensam tyngdpunkt utanför Pluto, så att även Pluto
      "vaggar" – det är det som gör paret nästan till en dubbelplanet.
- [x] Småmånarna Styx, Nix, Kerberos, Hydra (prickar, kan hoppas över)

**Verifiera:** Fokusera Pluto. Charon ska vara påfallande stor i förhållande
till Pluto jämfört med andra månar.

---

## Etapp 5 – Saturnus och isjättarnas största månar

- [x] Saturnus: Titan (15,9 dygn; ev. även Rhea, Enceladus)
- [x] Uranus: Titania, Oberon, ev. Miranda
- [x] Neptunus: Triton (5,88 dygn, **retrograd** – kretsar baklänges!)

**Verifiera:** Triton ska gå åt motsatt håll mot allt annat i appen –
tydligast beviset på att den är infångad från Kuiperbältet.

---

## Etapp 6 – Ringar runt de andra jätteplaneterna

Samma teknik som Saturnus befintliga ringar, men tunnare och svagare:

- [x] Jupiter: mycket tunn, mörk dammring
- [x] Uranus: smala mörka ringar – och Uranus extrema axellutning (98°!)
      bör läggas in samtidigt så att ringarna står "på högkant"
- [x] Neptunus: svag ring (bågarna/klumparna kan förenklas bort)

**Verifiera:** Ringarna syns bara vid inzoomning och skymmer inte planeterna
i översiktsvyn. Uranus ringar ska stå nästan vinkelrätt mot banplanet.

---

## Etapp 7 – Asteroidbältet (runt solen)

- [x] Ett diskret band av små prickar mellan Mars (1,52 AU) och Jupiter
      (5,20 AU), tätast kring 2,2–3,3 AU. Slumpade banor med spridning i
      lutning (± ett par grader) och excentricitet, som roterar med
      keplerska hastigheter (inre varvet snabbare än yttre).
- [x] Kryssruta "Visa asteroidbältet" (av som standard, så att vyn inte blir
      plottrig).
- [x] Eventuellt: dvärgplaneten Ceres som namngiven prick i bältet.

**Verifiera:** Bältet ska se glest ut även i appen – en pedagogisk poäng är
att asteroidbältet i verkligheten mest är tomrum (rymdsonder flyger igenom
utan problem). Prestanda: ingen märkbar försämring vid rotation/zoom
(punkterna cachas som stjärnhimlen).

---

## Etapp 8 – Kuiperbältet (runt solen, bortom Neptunus)

- [x] Glest band av isprickar ca 30–50 AU, med större spridning i lutning
      än asteroidbältet. Pluto ligger mitt i det – bra att kunna visa.
- [x] Samma kryssruta som asteroidbältet eller en egen.

**Verifiera:** Zooma ut och luta kameran: Kuiperbältet ska vara tjockare/
"luddigare" i höjdled än asteroidbältet, och Plutos lutande bana ska ligga
inom dess svärm.

---

## Etapp 9 – Rymdfärd till Mars eller månen

En egen liten rymdfärd: eleverna väljer mål, skjuter upp en farkost och följer
den hela vägen fram. Här möts allt appen redan kan – banor, tid och skala –
i något eleverna själva styr. Etappen är uppdelad i fem delar som var och en
går att bygga och verifiera för sig.

### 9.1 – Ställ om vilket datum man befinner sig på

Behövs först, och är användbart även utan rymdfärder: man ska kunna hoppa till
vilket datum som helst, både bakåt och framåt, i stället för att som i dag
alltid starta på dagens datum och bara kunna gå framåt.

- [x] Ett datumfält där man skriver in år, månad och dag, och vyn hoppar dit.
- [x] Knappar för att stega ± dag, ± månad och ± år, så att man kan bläddra
      sig fram utan att skriva.
- [x] Knappen "Idag" som återställer till nuet.
- [x] Låt hastighetsreglaget kunna gå bakåt, så att tiden kan spelas baklänges.

Kepler-matematiken klarar redan negativ tid, så arbetet ligger i gränssnittet
och i klockan (`_startDate` plus `_simDays` i `MainPage`). Detta är också
grunden för startfönstren i 9.3, och behövs i etapp 10 för att spola tillbaka
till Voyagers uppskjutning 1977.

**Verifiera:** Hoppa till ett känt datum och kontrollera att planeterna står
rimligt. Stega bakåt över ett årsskifte och en skottdag. Kör tiden baklänges
och se att planeterna går motsols. Rolig bieffekt: slå upp din egen
födelsedag och se var planeterna stod då.

---

### 9.2 – Farkosten och överföringsbanan till Mars

- [x] **Farkosten som himlakropp**: en liten prick med namn och ett spår efter
      sig (de senaste par hundra positionerna), som följer en Kepler-bana precis
      som planeterna.
- [x] **Hohmann-bana**: den energisnålaste vägen är en halv ellips med
      perihelium vid jordens bana (1,00 AU) och aphelium vid Mars (1,52 AU).
      Halva storaxeln blir då 1,26 AU, vilket ger en restid på ungefär
      259 dygn – hälften av den banans omloppstid.
- [x] Banplanet ska luta så att farkosten möter Mars även i höjdled; Mars
      ligger upp till 1,85° ur ekliptikan.

**Verifiera:** Restiden ska bli ungefär 259 dygn, och farkostens läge vid
ankomst ska sammanfalla med Mars – inte bara ligga på rätt avstånd från solen.

---

### 9.3 – Startfönster

- [x] Farkosten måste skjutas upp när Mars ligger 44,3° framför jorden. Under
      de 259 dygnen hinner Mars nämligen bara 135,7° av sitt varv, medan
      farkosten går 180° – och 44,3 + 135,7 = 180.
- [x] Läget upprepas var 780:e dygn (25,6 månader). Knappen "Skjut upp" bör
      vara inaktiv däremellan, med "Hoppa till nästa startfönster" bredvid.
- [x] Visa hur långt det är kvar till nästa fönster.

Det är därför verkliga Mars-uppdrag alltid skjuts upp i klungor: sommaren 2020
skickade USA, Kina och Förenade arabemiraten var sin sond inom två veckor, och
sedan hände ingenting på två år.

**Verifiera:** Ligger Mars fel vid uppskjutningen ska farkosten anlända till
tom rymd – 20° fel motsvarar 80 miljoner km. Det är hela poängen med
startfönster och något eleverna kan prova själva.

**Rättat i efterhand (upptäckt i 10.1):** både banan och fönsterkriteriet
byggde på antagandet att färden sveper den kortaste vägen mellan jorden och
Mars. I samtliga åtta prövade fönster var den kortaste vägen baklänges runt
solen, så farkosten flög åt fel håll och skulle ha krävt 63 km/s i förhållande
till jorden i stället för verklighetens 3–4. Ankomsten till Mars stämde ändå,
vilket är skälet att det inte syntes i verifieringen. Banan löses nu med
Lambert-lösaren från 10.1, och restiden väljs så att uppskjutningen blir så
billig som möjligt. Fönsterkriteriet är samtidigt bytt från sveptvinkeln – som
pekade rakt in i den punkt där banplanet blir obestämt – till kostnaden:
fönstret är öppet när uppskjutningen kostar högst 0,1 km/s mer än fönstrets
bästa dag.

---

### 9.4 – Färd till månen

- [x] Samma sak fast kring jorden: en ellips från låg omloppsbana ut till
      månens avstånd, restid ca 3 dygn. Kräver att farkosten kan kretsa kring
      en planet i stället för kring solen, ungefär som månarna gör i dag.

Knappen "Skjut upp mot Månen" går att trycka på vilken dag som helst, och vyn
hoppar samtidigt till jorden – hela månfärden ryms inom 0,003 AU och vore
annars mindre än en pixel. Farkosten kretsar kring jorden i stället för kring
solen: banan räknas med jordens gravitationsparameter och läggs i månens eget
banplan. Uppskjutningen sker från en låg omloppsbana på 400 km höjd, som blir
banans perigeum.

En ren Hohmann-bana ut till månen skulle ta 4,95 dygn. För att hinna på tre
måste farkosten skjutas upp med mer fart, så att banans bortre ände hamnar
440 000–630 000 km bort, alltså en bra bit bortom månen, och månen hinns ikapp
på vägen ut – före vändpunkten. Det var precis så Apollo flög.

**Verifiera:** Månfärden ska ta ca 3 dygn. Bra kontrast till Mars: månen är
tillbaka på samma ställe var 27:e dygn, så dit kan man åka i stort sett när
som helst.

Kontrollerat: restiden blir 3,00 dygn och farkosten möter månen på metern när
för 40 startdatum spridda över ett år. Startfarten blir 10,84 km/s, vilket är
den verkliga farten vid en uppskjutning mot månen (Apollos raketsteg gav
10,8 km/s), och farten faller till 0,6–0,9 km/s vid framkomsten. Banplanet
sammanfaller med månens inom 0,02°.

---

### 9.5 – Panel under färden och ankomst

- [x] **Panel**: förfluten restid, återstående tid, avstånd kvar till målet
      och farkostens fart.
- [x] **Ankomst**: farkosten möter målet och färden markeras som avslutad.
      Gjordes redan i 9.2: farkosten följer med planeten efter framkomsten i
      stället för att bli stående kvar där planeten råkade vara, och etiketten
      byts till "Farkost framme".
- [x] **Kameran** följer med ner till planeten vid ankomsten.

Panelen dyker upp uppe till vänster så snart en farkost är i väg och försvinner
när färden avbryts. Vid ankomsten byter den till restid, ankomstdatum och farten
vid framkomsten. Kameran hakar samtidigt på farkosten och zoomar in till målet –
en gång, i själva ankomstögonblicket, så att användaren sedan får styra fritt
igen. Ett nytt val i fokusväljaren eller "Återställ vy" släpper greppet.

**Verifiera:** Farten ska variera längs banan – snabbast vid uppskjutningen
nära solen och långsammast vid ankomsten, precis som Keplers andra lag säger.

Kontrollerat: farten faller monotont hela vägen, mot Mars från 33,14 till
20,51 km/s och mot månen från 10,83 till 0,62 km/s. Avståndet till månen
minskar hela vägen, men avståndet till Mars växer först från 246 till
409 miljoner km innan det faller – farkosten går ju runt solen, inte rakt mot
planeten, och Mars står på andra sidan solen vid uppskjutningen.

---

## Etapp 10 – Voyager och de andra rymdsonderna

De farkoster mänskligheten faktiskt har skickat ut. Etapp 9 handlar om en
påhittad resa som eleven själv styr; den här handlar om de verkliga färderna,
med riktiga datum. Voyager 1 är det avlägsnaste föremål människan har byggt.

Etappen är den största hittills och är därför uppdelad i fem delar, som var och
en går att bygga och verifiera för sig – samma upplägg som etapp 9.

Grundtanken för hela etappen: sondernas banor byggs inte ur inmatade banelement
utan ur de verkliga datumen. Varje ben av färden är banan som går från en planet
till nästa på exakt den tid passagerna faktiskt tog, och den räknas fram ur
appens egna planetpositioner. Då hamnar sonderna vid rätt planet rätt dag av sig
själva, och gravitationsslungan syns som ett språng i farten mellan två ben –
vilket är precis vad den är.

### 10.1 – Hyperboliska banor i Kepler-koden

- [x] **Hyperbolisk Kepler-ekvation**. Sonderna har fått så hög fart att de
      aldrig kommer tillbaka: deras banor har excentricitet större än 1 och är
      alltså hyperbler, inte ellipser. `SolveKepler` löser i dag bara
      `E - e*sin E = M`, som gäller för ellipser. För hyperbler behövs
      `e*sinh H - H = M` och en egen positionsformel. Detta är etappens enda
      riktiga matematikarbete och bör göras först.
- [x] **Kägelsnitt ur ett tillstånd**. En sond byter bana vid varje
      planetpassage, så banan måste gå att bygga ur läge och hastighet i stället
      för ur fasta banelement som planeternas. Samma klass ska klara både
      ellipser och hyperbler; halva storaxeln blir negativ för de senare.
- [x] **Lambert-lösaren**: banan som går från ett läge till ett annat på en
      given tid. Det är den som gör att sonderna kan byggas ur verkliga datum.

Ligger i `Simulation/Kepler.cs`, `Simulation/Conic.cs` och
`Simulation/Lambert.cs`. Ingenting av det syns ännu i appen – det är kärnan som
10.2 och framåt bygger på. Lambert använder universella variabler, där en enda
variabel z beskriver alla kägelsnitt: positiv för ellipser, negativ för
hyperbler och noll för parabeln mitt emellan. Restiden växer monotont med z, så
rätt bana kläms in med intervallhalvering.

Två fällor visade sig på vägen. Startgissningen till den hyperboliska Kepler-
ekvationen måste vara arsinh(M/e) och inte det närmare till hands liggande
M/(e-1): för banor strax över parabeln blir den senare enorm, och sinh av ett
stort tal spränger flyttalen direkt. Och under en viss gräns för z finns ingen
bana alls – kägelsnittet når helt enkelt inte fram – vilket sökningen måste
räkna som "snabbare än allt annat" för att inte fastna där.

**Verifiera:** Går att göra helt utan gränssnitt. En bana som byggs ur ett
tillstånd ska ge tillbaka samma läge och fart som den byggdes ur. Lambert ska
reproducera Mars-banan från 9.2, och en hyperbolisk bana ska gå att räkna både
framåt och bakåt i tiden.

Kontrollerat: den hyperboliska Kepler-ekvationen löses med ett relativt fel
under 1e-12 i 66 fall, med excentricitet från 1,0001 till 12 och medelanomali
upp till 4000, åt båda tidshållen. En bana byggd ur ett tillstånd ger tillbaka
samma läge på ett par kilometer när och samma fart på en hundradels meter per
sekund. Alla sju verkliga sondben som 10.2 och 10.3 behöver går att lösa, och
sonden hamnar vid rätt planet på passagedagen: sämst är Voyager 2 vid Jupiter
med 21 000 km, alltså en tredjedels Jupiterradie, och de övriga ligger under
1 000 km. Fem av de sju benen är hyperbler.

**Not:** Lambert avslöjade samtidigt ett fel i etapp 9.2. Mars-farkosten där
antar att färden sveper den kortaste vägen mellan start och mål, men i alla
åtta startfönster som testats är den kortaste vägen baklänges runt solen. Banan
går alltså åt fel håll, och skulle kräva 63 km/s i förhållande till jorden i
stället för verklighetens 3–4. Ankomsten till Mars stämmer ändå, vilket är
skälet att det inte upptäcktes tidigare. Rätt lösning är samma sveptvinkel fast
åt andra hållet – 190° i stället för 170° – och den får man gratis genom att
låta `Mission` använda Lambert-lösaren. Månfärden i 9.4 är inte drabbad; den
bygger sin riktning ur månens egen bana.

Fixen är gjord: se noten under 9.3. Efteråt går alla fem närmaste fönster åt
rätt håll, kostar 2,90–3,20 km/s och infaller i samma rytm som de verkliga
Mars-fönstren.

---

### 10.2 – Voyager 1 och 2

- [x] **Voyager 1 och Voyager 2** (uppskjutna 1977) med verkliga riktningar och
      farter. Voyager 2 är den enda farkost som besökt alla fyra
      jätteplaneterna – möjligt tack vare en planetuppställning som bara
      återkommer vart 176:e år.
- [x] **Ut ur ekliptikan**: efter Saturnus böjde Voyager 1 av brant uppåt
      (ca 35°) och Voyager 2 nedåt (ca 48°). Bra tillfälle att visa att
      solsystemet är en skiva som sonderna nu lämnat.
- [x] Sonderna ritas i vyn med sitt spår efter sig.

Ligger i `Simulation/Probe.cs` och `Simulation/ProbeData.cs`, med kryssrutan
"Rymdsonder" i panelen. Inga banelement matas in: varje ben är banan som går
från en planet till nästa på exakt den tid passagerna tog, och sista benet går
ut till sondens kända läge i dag (avstånd och riktning på himlen). Lutningen ut
ur ekliptikan blir därför ett resultat och inte en inmatning.

**Verifiera:** Spola tiden till mars 1979 – Voyager 1 ska då vara vid Jupiter,
inte någon annanstans. Samma sak för Voyager 2 vid Neptunus i augusti 1989.
Kontrollera dagens avstånd mot NASA:s siffror (Voyager 1 ligger kring 167 AU
och Voyager 2 kring 140 AU år 2026). Luta kameran och se att de två Voyager-
sonderna lämnat ekliptikan åt var sitt håll.

Kontrollerat: alla sex planetpassager träffar rätt planet rätt dag, sämst
602 km fel vid Neptunus, alltså två hundradels planetradie. I dag (augusti
2026) ligger Voyager 1 på 169 AU och Voyager 2 på 142 AU, med farterna 16,7
och 15,0 km/s – NASA:s siffror är 167 och 140 AU samt 17,0 och 15,4 km/s.
Sonderna lutar +35,6° respektive −47,9° mot ekliptikan, mot de vedertagna 35°
och 48°; eftersom lutningen inte matas in är det en kontroll av att hela
kedjan stämmer. Två av de åtta benen är ellipser – just de två från jorden
till Jupiter, för det var Jupiter som gav sonderna fart nog att aldrig komma
tillbaka. Resten är hyperbler.

Farthoppen syns redan nu i banorna, även om panelen som visar dem hör till
10.4: Jupiter gav Voyager 1 +10,8 km/s och Voyager 2 +10,0. Vid Neptunus
*bromsades* Voyager 2 med 2,3 km/s – priset för att svänga ner mot månen
Triton, och skälet till att den lämnade ekliptikan brantare än sin tvilling.

---

### 10.3 – Pioneer 10 och 11 samt New Horizons

- [x] **Pioneer 10 och 11 samt New Horizons** (Pluto 2015). Fem farkoster är
      på väg ut ur solsystemet.

Byggda på samma sätt som Voyagerna i 10.2, med sina verkliga passagedatum:
Pioneer 10 förbi Jupiter 4 december 1973, Pioneer 11 förbi Jupiter 3 december
1974 och Saturnus 1 september 1979, New Horizons förbi Jupiter 28 februari 2007
och Pluto 14 juli 2015.

**Verifiera:** New Horizons ska passera Pluto i juli 2015 – och Pluto ligger då
långt utanför ekliptikans plan, så det är också ett prov på att banorna
verkligen räknas i tre dimensioner.

Kontrollerat: alla elva planetpassager träffar rätt planet rätt dag. Sämst är
Voyager 2 vid Neptunus med 602 km; New Horizons möter Pluto på 319 km, vilket
är en fjärdedels Plutoradie. Lägena i dag stämmer med de kända: Pioneer 10 på
142 AU med 11,8 km/s (facit ~140 AU och 11,9), Pioneer 11 på 120 AU med
11,3 km/s (~118 och 11,2), New Horizons på 65 AU med 13,3 km/s (~63 och 13,9).
Samtliga tretton ben går prograd.

**Rättelse till verifieringen ovan:** Pluto låg i juli 2015 inte under utan
strax *över* ekliptikan, och inte "långt" räknat i vinkel – bara 1,91°. Räknat
i sträcka är det ändå 1,10 AU utanför planet, alltså mer än hela jordens
banradie, så provet på tre dimensioner håller. Pluto var på väg ned och korsade
planet några år senare. Att sonden möter Pluto där, och inte i planet, är just
vad som skulle visas.

Pioneer 11 blev etappens intressanta fall. Jupiter slungade den inte utåt utan
inåt och tvärs över solsystemet: banan faller från 4,97 AU in till 3,79 AU,
går ett halvt varv runt solen – från longitud 351° till 167° – och klättrar
sedan ut till Saturnus på 9,38 AU, som mest 11,6° över ekliptikan. Benet sveper
alltså 176°, farligt nära den punkt där banplanet blir obestämt, men lösningen
träffar Saturnus på 144 km. Det benet är dessutom en ellips: efter Jupiter hade
Pioneer 11 inte fart nog att lämna solsystemet, utan hade kommit tillbaka om
inte Saturnus slungat ut den. Samma sak gäller Pioneer 10 och de båda
Voyagersonderna vid Jupiter – deras första ben är ellipser, resten hyperbler.
New Horizons är undantaget: den var på en hyperbel redan från uppskjutningen,
den snabbaste som gjorts.

---

### 10.4 – Milstolpar, panel och skala

- [x] **Planetpassagerna som milstolpar** med datum, t.ex. Voyager 1 vid
      Jupiter i mars 1979 och vid Saturnus i november 1980, Voyager 2 vid
      Neptunus i augusti 1989. Kan visas som markerade punkter längs banan.
- [x] **Gravitationsslunga**: sonderna fick fart genom att svänga förbi
      planeterna. Visa farten i en panel så att hoppen vid varje passage syns –
      det är själva förklaringen till hur de kunde nå så långt.
- [x] **Skalan**: sonderna är i dag över 100 AU bort, tre gånger längre än
      Neptunus. Kameran måste kunna zooma ut så långt, och då krymper hela
      planetsystemet till en prick – vilket i sig är poängen. Sonderna bör också
      gå att välja i fokusväljaren, annars är de svåra att hitta där ute.

Milstolparna faller ut ur benen i stället för att matas in: varje ben börjar i
en, och farthoppet är skillnaden mellan det avslutande och det påbörjade benets
fart i samma punkt. De ritas som ringar längs spåret, med årtal – och för den
sond som är vald i fokusväljaren med planetnamn, datum och farthopp. Årtalen
går inte via etikettstaplingen som himlakropparna använder; elva passager som
staplas nedåt hade blivit en textpelare tvärs över vyn, så ett årtal hoppas
i stället över när det skulle hamna ovanpå ett som redan skrivits.

Väljer man en sond i fokusväljaren följer kameran den och zoomar ut till drygt
två gånger sondens avstånd, så att solen precis ryms i bild och hela
planetsystemet krymper till en prick. Kamerans tak höjdes från 25 000 till
40 000 enheter (666 AU) för att det ska gå.

**Verifiera:** Farten ska hoppa uppåt vid varje passage och sedan sjunka långsamt
medan sonden klättrar ur solens gravitation.

Kontrollerat: farten faller monotont på sista benet för alla fem sonderna.
Voyager 1 går från 27,4 km/s strax efter uppskjutningen till 20,4 vid Saturnus
1982, 17,7 år 1990 och 16,67 i dag – kurvan planar ut, precis som den ska när
solens grepp avtar med avståndet. Hoppen vid passagerna: Pioneer 10 fick
+12,1 km/s av Jupiter, Voyager 1 +10,8, Voyager 2 +10,0 och Pioneer 11 +7,1
följt av +5,6 vid Saturnus. Två passager gav ingen fart alls: Neptunus tog
2,3 km/s från Voyager 2, och Pluto tog 0,3 från New Horizons – dvärgplaneten är
helt enkelt för lätt för att slunga något. Kameran klarar alla fem sonderna
inom takhöjden, med solen kvar i bild.

---

### 10.5 – Kretsande sonder

- [x] **Kretsande sonder** som Cassini vid Saturnus (1997–2017) och Juno vid
      Jupiter – enklare fall, vanliga ellipser kring en planet.

Not: Cassinis resa ut till Saturnus (1997–2004) gick via Venus, Venus, jorden
och Jupiter, och de benen svarvar mer än ett helt varv kring solen. Sådana banor
klarar inte Lambert-lösaren i 10.1, som bara hanterar mindre än ett varv. Därför
visas Cassini från ankomsten 2004, och Juno från ankomsten 2016.

En viktig skillnad mot resten av etappen: de här två banorna är **inte**
återskapade ur verkliga datum. Cassini flög nästan trehundra olika varv under
tretton år, med omloppstider från en vecka till fyra månader och lutningar från
ringplanet upp till 75 grader, så det finns ingen enda bana att visa. Det som
ritas är ett representativt varv – storlek, form, omloppstid och banplan är
verkliga, men var sonden befinner sig i banan ett givet datum är det inte.
Banorna anges därför i planetradier och lutning mot planetens ekvator, vilket
är måtten sådana här banor brukar beskrivas med, i stället för i banelement.

Lutningen räknas mot planetens ekvator och inte mot ekliptikan: en polär bana
är 90 grader mot ekvatorn oavsett hur planeten själv lutar. Det ordnas genom att
lägga lutningen till planetens egen med samma uppstigande nod – att vrida
ekvatorsplanet ett kvarts varv kring nodlinjen ger just ett plan genom båda
polerna.

Banorna trycks ihop med samma faktor som planetens månar, annars skulle de
försvinna inne i det förstorade klotet. Det blir riktigt även proportionellt:
Cassinis varv är nästan exakt lika stort som Titans bana, och Junos sträcker sig
drygt fyra gånger längre ut än Callisto.

**Verifiera:** Cassinis varv kring Saturnus ska ta ett par tiotal dygn och Junos
kring Jupiter ungefär 53 – och Junos bana ska gå över polerna, inte längs
ekvatorn som månarnas.

Kontrollerat: Cassini 16,00 dygn och Juno 53,42. Banplanen mätta mot planetens
ekvator ger Cassini 20,0° och Juno 90,0°, alltså exakt polär, medan kontrollen
Io ger 0,0° – månen ligger i ekvatorsplanet, precis som den ska. Junos fart vid
perijovium blir 57,7 km/s, vilket stämmer med de omkring 58 km/s som gör Juno
till det snabbaste föremål människan skickat i förhållande till en planet; vid
apojovium är den nere i 0,54 km/s. Ett helt varv för tillbaka sonden till samma
punkt på mindre än en meter.

**Att tänka på:** ingen av de två syns vid appens startdatum. Cassini avslutades
15 september 2017 genom att styras ned i Saturnus atmosfär, för att en sond med
jordbakterier inte skulle riskera att en dag krascha på Enceladus med sitt hav
under isen. För Juno är slutdatumet satt till det förlängda uppdragets planerade
slut 30 september 2025; fortsatte det efter det är slutdatumet en rad att ändra
i `ProbeData`. Vill man se dem får man alltså ställa datumet tillbaka – Cassini
mellan 2004 och 2017, Juno mellan 2016 och 2025 – och välja Saturnus respektive
Jupiter i fokusväljaren. Junos bana är så vid att man behöver zooma ut ett par
steg för att se hela ellipsen.

---

### 10.6 – Välj vilka sonder som visas

- [x] En väljare där varje sond kan bockas i för sig, i stället för dagens
      kryssruta "Rymdsonder" som tänder och släcker allihop. Man ska kunna visa
      bara Voyager 1, eller Voyager 1 och 2, eller vilken annan kombination som
      helst.
- [x] Valet ska gälla sondens allt: pricken, spåret och milstolparna.
- [x] Släcks den sond som är vald i fokusväljaren faller fokus tillbaka till
      solen. Kameran ska aldrig bli stående och följa något som inte ritas.

Behovet kommer av 10.2–10.4. Fem sonder med spår och elva passageringar gör
översikten rörig, och det mesta man vill titta på handlar om en eller två i
taget: de båda Voyagersondernas motsatta lutningar ut ur ekliptikan, eller
Pioneer 11:s omväg tvärs över solsystemet jämförd med Voyagernas raka väg. I dag
är enda valet alla fem eller inga.

MAUI:s `Picker` kan bara välja ett alternativ, så en flervalslista finns inte
färdig. Tre vägar, i tur och ordning från enklast till trevligast:

1. Fem kryssrutor rakt i kontrollpanelen. Enklast, men panelen är redan trång –
   det var just därför rymdfärdsknapparna behövde en egen rad i 10.4.
2. En knapp som fäller ut en ruta med kryssrutorna i, alltså en egenbyggd
   rullgardin: en `Border` med en `VerticalStackLayout` som visas och döljs.
   Håller nere bredden och är närmast det som efterfrågas.
3. `CollectionView` med `SelectionMode="Multiple"` i samma utfällda ruta, om
   markeringsläget känns bättre än kryssrutor.

I koden räcker det inte längre med `ShowProbes` i `SolarSystemDrawable`, som är
en enda av/på-flagga. Ritningen går i dag igenom `ProbeData.All`, så den behöver
i stället fråga vilka sonder som är valda – förslagsvis en mängd med namnen,
eller en `HashSet<Probe>`.

Fokusväljaren bör därför bara lista de sonder som visas, så att man inte kan
välja en sond som inte finns i bild. Det för med sig en sak att se upp med:
väljarens innehåll blir föränderligt, medan `MainPage` i dag räknar ut vilken
sond som är vald ur ett fast index – solen, planeterna och sedan `ProbeData.All`
i tur och ordning. Den kopplingen måste byggas om så att den utgår från de
synliga sonderna och görs om varje gång valet ändras, annars pekar indexet på
fel sond så fort någon släcks.

Byggt som väg 2 i listan ovan: knappen "Rymdsonder 7/7" fäller ut en ruta med en
kryssruta per sond, namnen i sondens egen färg, plus "Alla" och "Inga" för att
slippa sju klick. Rutan ligger som en egen ruta ovanpå vyn och inte i
kontrollpanelen, dels för att panelen är trång, dels för att textstapeln där
panelerna ligger är genomsläpplig för klick och kryssrutorna då inte hade gått
att träffa. Raderna byggs ur sonddata, så en ny sond dyker upp i väljaren av sig
själv.

De två kretsande sonderna från 10.5 kom med i samma väljare – de är också
sonder, och den gamla kryssrutan styrde dem också. De står däremot inte i
fokusväljaren, eftersom man tittar på dem genom att välja deras planet.

`ShowProbes` i `SolarSystemDrawable` är ersatt av `VisibleProbes`, en mängd med
namnen på de sonder som ska ritas. Tom mängd släcker allihop, precis som den
gamla kryssrutan gjorde.

**Verifiera:** Bocka i bara Voyager 1 och kontrollera att spår, prick och
milstolpar för de andra fyra försvinner, medan Voyager 1 ser ut precis som förut.
Bocka i båda Voyagersonderna och luta kameran: nu ska de två motsatta vägarna ut
ur ekliptikan gå att jämföra utan att Pioneersonderna och New Horizons ligger i
vägen. Bocka ur allihop och jämför med hur vyn ser ut när dagens kryssruta
"Rymdsonder" släcks – det ska vara samma bild.

Kontrollerat: fokusväljarens indexlogik provad i 22 fall mot riktig sonddata.
Fällan som förutsågs i planen är den viktigaste: följer man Voyager 2 och
släcker Voyager 1 flyttar Voyager 2 från plats 11 till plats 10 i väljaren, och
eftersom valet bevaras på namn i stället för index följs rätt sond även efteråt.
Släcker man den sond man följer faller fokus till solen och vyn zoomar ut till
översikten; följer man en planet påverkas ingenting; "Inga" följt av "Alla" ger
tillbaka utgångsläget.

---

## Etapp 11 – Ytor och rotation på övriga kroppar

Jorden har redan världsdelar, polarisar och verklig rotation. Samma teknik kan
ge övriga kroppar sina kännetecken – de behöver inte vara lika detaljerade,
men tillräckligt för att man ska känna igen dem och se att de snurrar.

Ingen bildtextur behövs: jordgloben ritar långa/breda-polygoner direkt på
klotytan, och gasjättarnas band blir faktiskt enklare än kontinenter eftersom
de bara är latitudbälten. Etappen är uppdelad så att en eller två kroppar tas
i taget.

Alla åtta planeter roterar – men flera gör det så egendomligt att det är värt
en egen lektion: Venus snurrar baklänges och så långsamt att dess dygn (243
jorddygn) är längre än dess år (225), Merkurius hinner exakt tre varv på två
år, och månen roterar ett varv per omloppsbana och vänder därför alltid samma
sida mot oss.

### 11.1 – Infrastruktur: generell globrendering

- [x] Bryt ut `DrawEarthGlobe` till något alla kroppar kan använda, och
      `EarthMap` till en ytkarta per kropp. Det rör sig om 73 respektive 98
      rader, så själva utbrytningen är överkomlig. Det som kostar är att jorden
      i dag är specialfall rakt igenom: axellutningen är en konstant i ritkoden
      (`ObliquityRad`) och rotationen räknas ur stjärntid. Båda måste in i den
      generella mekanismen utan att jorden ändrar utseende.
- [x] Lägg rotationsdata på `CelestialBody`: rotationsperiod (negativ för
      retrograd) och nollmeridianens läge vid epoken.
- [x] **Axeldata för de kroppar som saknar den.** Planen utgick från att
      polriktningarna redan fanns sedan månetapperna, och det stämmer för Mars,
      Jupiter, Saturnus, Uranus och Neptunus – samt Pluto, vars plan ligger i
      Charons banelement. Men Merkurius, Venus och månen har varken månar eller
      ringar och därför ingen ekvatorsdata alls. Den måste läggas in innan 11.5
      och 11.6 går att göra:
      - Merkurius lutar 0,03°, alltså praktiskt taget upprätt.
      - Venus lutar 177,4°, nästan upp och ner. Det är hela förklaringen till
        att den snurrar baklänges, så den siffran är etappens viktigaste.
      - Månen lutar 1,5° mot ekliptikan, 6,7° mot sin egen bana. (Planen hade
        de två talen omkastade; rättat när axeln räknades fram.)
      Observera att lutningen ensam inte bestämmer axeln – nodens läge behövs
      också, precis som för de ekvatorsplan som redan finns.
- [x] Passa på att flytta de befintliga ekvatorskonstanterna från
      `SolarSystemData` till kropparna själva. I dag ligger de som lösa `const`
      som månar, ringar och de kretsande sonderna hämtar var för sig – Cassini
      och Juno sträcker sig ända in i `SolarSystemData.SaturnEquator...` för att
      få tag i dem.
- [x] Samma tröskel som i dag: ytan ritas först när klotet är stort nog.

Så här blev det: `BodyAxis` beskriver en kropps axel med samma fyra tal för alla
– ekvatorns lutning mot ekliptikan, nodens longitud, rotationstiden och
nollmeridianens läge vid epoken – och räknar fram de tre basvektorer som både
ytritningen och ringarna behöver. `SurfaceMap` (före detta `EarthMap`) är en
ytkarta bland flera, med jorden som första instans. `DrawGlobe` tar kartan och
axeln i stället för att veta något om jorden, och `DrawBody` väljer mellan glob
och skiva, numera även för ringplaneterna – de var tidigare utestängda från
globgrenen och kunde alltså aldrig ha fått en yta.

**En konvention ändrades mot planen.** Här stod att rotationstiden skulle vara
negativ för retrograda kroppar. Det gick inte ihop med resten: en negativ tid
förutsätter att polen alltid pekar norrut om ekliptikan, men månarnas banplan är
redan skrivna kring rotationsaxeln (Uranus månar lutar 97,7° just därför). Två
konventioner i samma data hade krävt en omvändning varje gång en måne läser sin
planets axel. Nu pekar polen alltid åt det håll högerhandsregeln ger, tiden är
alltid positiv, och retrograd rotation syns som lutning över 90°: Venus 178,8°,
Uranus 97,7°, Pluto 112,8°. Miranda kan läsa `UranusAxis` rakt av.

**Rättat i efterhand: alla ekvatorsnoder låg 180° fel.** Talen kom in under
månetapperna, räknade som polens longitud *minus* 90° där det ska vara *plus*.
Följden blev att varje planets ekvatorsplan lutade åt rakt motsatt håll: rätt
lutning, rätt tidpunkter för dagjämningar och ringpassager, men fel halvklot
vänt mot solen. Måne för måne, ring för ring, och även Charon och Triton.
Felet var osynligt så länge ingenting ritades på ytorna – det är först nu, när
axeln avgör vad man ser, som det hade märkts. Alla noder har flyttats 180°
(Mars lutning rättades samtidigt från 26,74° till 25,40°).

**Verifiera:** Jorden ska se ut och rotera exakt som före ombyggnaden. Det är
etappens viktigaste kontroll, eftersom allt annat bygger vidare på den koden.
Månarnas och ringarnas plan ska också ligga kvar oförändrade när konstanterna
flyttas – Uranus månar på högkant och Tritons retrograda bana är känsliga prov.

Kontrollerat utanför appen, mot den gamla koden och mot verkligheten:

- **Jorden är oförändrad in på biten.** Nya axelkoden mot den gamla
  stjärntidskoden, 10 punkter på jordytan × 9 datum mellan 1950 och 2054:
  största avvikelse exakt 0. Inte "under en pixel" utan samma flyttal.
- **Axlarna stämmer mot IAU:s polriktningar** för alla nio planeterna, avvikelse
  0,000° – en oberoende väg fram till samma tal.
- **Månarna ligger i sin planets ekvatorsplan**: Phobos, Deimos, de fyra
  galileiska, Enceladus, Rhea, Titan, Uranus tre och Charon alla 0,00°.
  Triton hamnar på 156,91° mot Neptunus ekvator, mot uppmätta 156,9 – och det
  talet faller bara ut om både Neptunus nod och Tritons rättas.
- **Årstiderna hamnar rätt**, vilket är provet som skiljer rätt nod från fel:
  Uranus står 7,9° från solen i januari 1986 (Voyager 2 kom fram mitt i
  sydsommaren) och 90,0° i december 2007 (dagjämningen, på månaden). Saturnus
  63,3° i maj 2017 (nordsommarsolstånd under Cassinis sista varv, och 63,3 är
  just 90 − 26,7) och 90,0° i maj 2025, då solen verkligen passerade ringplanet.
  Jorden 66,6° vid midsommar 2026. Med de gamla noderna: Uranus 172,2° och
  Saturnus 119,2°, alltså fel halvklot i sol båda gångerna.
- **Månens bundna rotation** ger sub-jord-longitud −6,5° till +6,1° över 110 år,
  mot den verkliga optiska librationen på ±6,3°. Ingen drift.
- **Cassini och Juno är oförändrade** i förhållande till sin planet: 16,00 dygn
  och 20,0° mot Saturnus ekvator, 53,42 dygn och 90,0° mot Jupiters, samma
  excentriciteter som i 10.5. Deras banplan följde med när noderna rättades.

Kvar att se med ögat: att jordgloben ritas som förut i appen. Siffrorna säger
att den måste, men den står ändå i provlistan under R1.

### 11.2 – Mars

- [x] Rödbrun yta med de mörka områdena (Syrtis Major är det tydligaste),
      vita polarisar och gärna Valles Marineris som ett streck.
- [x] Rotation 24 h 37 min, nästan som jordens.

Kartan har tolv drag: Syrtis Major som den mörka triangeln kring 70° öst, Mare
Acidalium, Sinus Sabaeus och Sinus Meridiani längs ekvatorn, Mare Erythraeum,
Tyrrhenum, Cimmerium och Sirenum, Solis Lacus ("Mars öga"), Boreosyrtis vid
Utopia, de ljusa högslätterna Hellas och Tharsis, samt Valles Marineris som ett
streck. De mörka fälten är inte hav utan berggrund som vinden sopat ren från
ljust damm – därför byter de långsamt form mellan stormsäsongerna.

Polarisarna ritas med samma utsträckning året om. I verkligheten andas de med
årstiderna, men appen har ingen modell för frost, och det står i kodkommentaren.

**Verifiera:** Polarisarna ska ligga still medan ytan snurrar under dem.

Kontrollerat utanför appen:

- **Polarisarna står stilla.** Över två marsdygn håller sig norra kalotten på
  exakt 76,00° nord och den södra på 74,00° syd, medan Syrtis Major under samma
  tid sveper 720,0°. Ytan snurrar alltså under kalotter som inte rör sig.
- **Sex marslandningar hamnar på rätt tid av dygnet**, vilket provar både
  nollmeridianen och åt vilket håll longituden räknas: Viking 1 15:43 (känt
  16:13), Viking 2 09:52 (09:49), Pathfinder 02:54 (03:07), Opportunity 13:18
  (13:15), Curiosity 15:32 (14:53) och Perseverance 15:21 (15:53). Störst fel
  40 minuter. De kända klockslagen är medelsoltid medan modellen räknar sann
  soltid, och Mars tidsekvation når ±50 minuter – banan är så excentrisk
  (e = 0,093) att solen ligger rejält före eller efter sin medelposition. Vore
  longitudens tecken fel skulle de hamna 6–19 timmar bort; att Pathfinder faller
  före gryningen och Viking 2 på morgonen medan de andra fyra faller på
  eftermiddagen är det som binder tecknet.
- **Marsdygnet stämmer i båda formerna**: stjärndygnet 24:37:22 mot kända
  24:37:22, och soldygnet, mätt ur modellen som tiden mellan två middagar,
  24:39:47 mot kända 24:39:35. Tolv sekunders fel, och skillnaden på drygt två
  minuter mellan de två dygnen faller ut av sig själv – Mars hinner en bit i sin
  bana medan den snurrar.
- **Dragen ligger rätt på klotet**: polygonernas tyngdpunkter hamnar högst 3°
  från de lägen de ska ha, alltså inom 200 km på Mars yta.

**Utjämnade konturer i efterhand.** Ritade blev fälten synligt kantiga –
femhörningar och sexhörningar snarare än fläckar. Mars albedogränser är diffusa
dammgränser, så de rundas nu med Chaikins hörnkapning i två varv. Det är ett val
per karta och inte en generell ändring: jordens kustlinjer ska förbli kantiga,
och Jupiters band måste ha raka kanter för att fyrhörningarna inte ska glipa.

Longituderna vecklas ut till en sammanhängande följd innan hörnen kapas.
Utan det skulle medelvärdet av 358° och 8° bli 183°, alltså rakt över på andra
sidan klotet, och Sinus Meridiani ligger just över nollmeridianen.

Kontrollerat efteråt: jordens karta oförändrad på 11 ytor och 507 punkter, Mars
växer från 391 till 529 punkter, polarisarna ligger kvar på exakt 76,00° nord och
74,00° syd utan hål i longitud, och dragens tyngdpunkter flyttar sig högst en
grad (största avvikelse från uppmätt läge 4° mot 3° förut). Valles Marineris är
fortfarande ett streck och inte en klump, vilket var den verkliga risken med att
runda en smal figur.

### 11.3 – Jupiter

- [x] Molnband i latitud, ljusa zoner och mörka bälten.
- [x] Stora röda fläcken som en oval på södra halvklotet.
- [x] Rotation på bara 9 h 55 min – snabbast i solsystemet trots att den är
      störst. Går utmärkt att se i appen på låg hastighet.

Sju band på sina vedertagna breddgrader, två polarområden, Stora röda fläcken
och tre av de vita ovalerna på 41° syd. Ett band går inte att rita som en enda
polygon – det är en ring med hål i – så varje band byggs av åtta fyrhörningar
som överlappar en aning i kanterna, samma knep som ringarna redan använder.
Polarområdena är däremot vanliga kalotter, precis som jordens isar.

**Röda fläckens longitud är vald på fri hand**, och det är en medveten
förenkling som står i kodkommentaren. Fläcken driver västerut i förhållande till
planetens inre rotation, ett helt varv på 3,7 år, och appen följer inte driften.
Att fläcken finns, hur stor den är och hur den vandrar runt kanten stämmer; var
den står ett givet datum gör det inte. Något annat vore svårt att göra rätt –
driften har varit oregelbunden i över hundra år.

**Verifiera:** Röda fläcken ska försvinna runt kanten och komma tillbaka på
andra sidan efter ungefär fem timmar simulerad tid.

Kontrollerat utanför appen:

- **Fläcken är borta 4 timmar 59 minuter** och synlig 4 timmar 56, sett från
  jordens verkliga läge under tre dygn i mars 2026. Tillsammans 9 h 55 min 34 s,
  alltså exakt ett varv. Att de två halvorna inte är riktigt lika långa är rätt:
  Jupiter lutar 2,2° och jorden står inte i ekvatorsplanet.
- **Rotationstiden** blir 09:55:29 mot System III:s 09:55:30, och Jupiter är
  snabbast av alla kroppar i datan – 2,42 varv per jorddygn. Ekvatorn far fram i
  12,3 km/s, tjugosju gånger jordens 0,46.
- **Bandgränserna stämmer på tiondelen** mot de vedertagna: ekvatorialzonen
  ±7°, norra ekvatorialbältet 7–17°, södra 20–7° syd, och de tempererade i par
  ut mot polerna.
- **Röda fläcken mäter 16 517 × 11 958 km**, mot uppmätta ungefär
  16 000 × 12 000. Den är alltså bredare än jorden, vars diameter är 12 742 km –
  vilket är hela poängen med att visa den.
- **Kostnaden**: 62 ytor och 1 584 punkter, mot jordens 11 och 507. Tre gånger
  så mycket, men bara medan man är inzoomad på Jupiter.

**Rättat efter att kartan setts ritad, två gånger.** Först var paletten för hård. Mörkbruna bälten mot
gräddvitt gav en randig boll snarare än en planet – på fotografier är skillnaden
mellan bälte och zon förvånansvärt liten, och det är mönstret som bär, inte
kontrasten. Tonerna ligger nu nära varandra, röda fläcken är orange i stället för
tegelröd, och polarområdet ritas i två steg (55° och 70°) eftersom en enda kalott
blev en hård grå kupol på toppen.

Andra gången var problemet ett annat: alla bälten var lika starka, vilket gav en
strandboll. På ett foto dominerar de två ekvatorsbältena medan de tempererade
knappt syns. Nu finns fem toner i stället för två, och bältena tonar bort mot
polerna. Det är den versionen som är sedd i bild.

Kontrollerna gjordes genom att rita kartorna utanför appen med samma matematik –
`BodyAxis.Direction`, samma kalottklippning, samma ortografiska projektion – med
jordgloben som referens: känns Afrika igen är portningen trogen. Det bekräftade
samtidigt två saker som siffror inte kunde avgöra: fyrhörningarnas fogar syns
inte, och kalotterna fyller åt rätt håll.

### 11.4 – Saturnus, Uranus och Neptunus

- [x] Saturnus: svagare band än Jupiters, i gulbeige. Rotation 10 h 39 min.
- [x] Uranus: nästan enfärgat blågrönt – poängen är att den är så slät.
      Rotation 17 h 14 min, retrograd och på sidan.
- [x] Neptunus: blå med Stora mörka fläcken. Rotation 16 h 06 min.

Bandbyggarna från 11.3 är utlyfta ur Jupiter-koden och delas nu av alla fyra
jättarna: `Band`, `Cap` och `Oval`. Till dem kom `PolarPolygon`, som behövdes för
en enda sak.

**Saturnus sexhörning.** Kring nordpolen ligger en jetström som håller sex raka
sidor, nästan 30 000 km tvärs över – upptäckt av Voyager 1980, fotograferad på
nytt av Cassini, och den enda kända formen av sitt slag i solsystemet. Den står
inte i planen, men den är för märklig för att utelämna när klotet ändå ritas.
Kanterna måste räknas i planet sett rakt ovanifrån polen: två hörn på samma
breddgrad förbundna med en latitudlinje ger en cirkelbåge som buktar åt fel håll,
och figuren blir en cirkel.

**Uranus fick två svaga band och en ljusare polkalott** trots att poängen är att
den är slät. Skälet är praktiskt: utan minsta drag på ytan går det inte att se
att planeten rullar, och det är hela behållningen med Uranus.

**Neptunus mörka fläck är ett tillstånd, inte ett drag.** Den är ritad som
Voyager 2 såg den 1989, med sitt vita följeslagarmoln. När Hubble tittade efter
1994 var den borta. Till skillnad från Jupiters röda fläck, som hållit i sig i
århundraden, kommer och går Neptunus fläckar – det står i kodkommentaren.

**Verifiera:** Uranus ska snurra kring en axel som ligger nästan i banplanet,
så att ytan rullar i stället för att snurra.

Kontrollerat utanför appen:

- **Uranus rullar.** Axeln ligger 97,8° från banplanets lodlinje, alltså bara 8°
  från själva banplanet. Under ett varv kring solen vandrar solen mellan 82,2°
  syd och 82,2° nord på planeten – den står nästan rakt över polerna vid
  solstånden. Jämför jorden och Saturnus, där solen aldrig kommer längre än
  23,4° respektive 26,7° från ekvatorn. Det är skillnaden mellan att rulla och
  att snurra, i siffror.
- **Rotationstiderna**: Saturnus 10:39:22 mot kända 10:39:22, Uranus 17:14:23 mot
  17:14:24, Neptunus 16:06:36 mot 16:06:36.
- **Sexhörningen är mätbart sexkantig.** Hörnen hamnar på 75,7° nord och
  kantmitterna på 77,6° – en skillnad på 1,9°, vilket är precis vad en
  regelbunden sexhörning ger. En cirkel hade gett 0,0°. Bredden hörn till hörn
  blir 29 067 km mot uppmätta ungefär 29 000.
- **Neptunus mörka fläck** mäter 12 989 × 6 618 km mot Voyagers ungefär
  13 000 × 6 600, alltså jordstor (jordens diameter 12 742 km).
- **Kostnaden**: Saturnus 61 ytor och 1 780 punkter, Neptunus 28 och 824, Uranus
  17 och 488 – den släta planeten är också den billigaste att rita.

Sett i bild med samma kontroll som förut: Saturnus är tydligt mjukare än Jupiter,
sexhörningen syns uppifrån, Uranus är nästan enfärgad men rullar synligt, och
Neptunus fläck med följemoln sitter där den ska.

### 11.5 – Merkurius och Venus

- [x] Merkurius: grå och kraterrik, mycket lik månen. Rotation 58,6 dygn –
      exakt tre varv på två av sina år, en 3:2-resonans med solen.
- [x] Venus: inget av ytan syns, bara ett jämnt gulvitt moltack. Rotation
      243 dygn **baklänges**, alltså längre än dess år på 225 dygn.

Merkurius har de fyra namngivna bassängerna på sina uppmätta lägen – Caloris,
Beethoven, Rembrandt och Tolstoj – plus Kuiper och Debussy med sina ljusa
strålsystem. De övriga fyrtiosex kratrarna är slumpade ur ett fast frö, så bilden
blir densamma varje gång utan att någon behöver rita in dem för hand. Latituden
dras ur arcsin så att de fördelar sig jämnt över klotet i stället för att klumpa
ihop sig vid polerna.

**Venus fick strimmor trots att poängen är att den är slät**, och det är värt att
vara tydlig med varför. Molntäcket är helt ogenomskinligt; i vanligt ljus är
Venus en jämn skiva utan drag, och det tog radar från omloppsbana att kartlägga
marken. Men utan något att följa med blicken går det inte att se att planeten
roterar, och att den roterar baklänges är hela behållningen. Strimmorna är det
Y-mönster som syns i ultraviolett ljus, återgivet så blekt att det knappt märks.

Med det följer en förenkling: **molnen far i verkligheten runt planeten på fyra
dygn medan marken under tar 243.** Appen låter strimmorna följa marken, så det
som visas är planetens rotation och inte molnens. Det är planetens rotation
etappen handlar om, men den som tittar noga ser alltså moln som rör sig sextio
gånger för långsamt. Står i kodkommentaren.

Utjämningen från 11.2 är avstängd för båda. Kratrarna är redan runda från `Oval`,
och Venus Y-mönster innehåller ett band av fyrhörningar som måste behålla raka
kanter – rundade hörn hade fått fogarna att glipa. Det upptäcktes när Merkurius
karta först kostade 3 424 punkter; utan utjämning blev det 920.

**Verifiera:** Venus ska snurra åt motsatt håll mot alla andra planeter, och
så långsamt att man behöver skruva upp hastigheten för att se det.

Kontrollerat utanför appen:

- **Venus soldygn faller ut till 116,75 dygn**, exakt det kända värdet. Det är
  provet som binder rotationsriktningen: för en planet som roterar rättvänt blir
  soldygnet längre än stjärndygnet, men för Venus blir det *kortare* – 116,75 mot
  243,02 – eftersom ytan går solen till mötes. Räknat för hand ger
  1/243,02 + 1/224,70 samma 116,75, och det är en summa just för att de två
  rörelserna är motriktade.
- **Dygnet är längre än året**: 243,02 mot 224,70 dygn. En punkt på Venus ekvator
  vrider sig 1,5 grader per timme mot jordens 15, alltså tio gånger långsammare –
  därav att man måste skruva upp hastigheten.
- **Merkurius 3:2-resonans stämmer exakt**: år delat med rotation blir 1,5000.
  Soldygnet ur modellen blir 175,94 dygn = 2,000 merkuriusår. På Merkurius går
  det alltså två år på ett dygn.
- **Mariner 10 såg samma halva tre gånger.** Sondens bana var 176 dygn, alltså
  precis ett merkuriusdygn, så samma sida låg i solljus vid varje förbiflygning –
  därför är knappt halva planeten okänd från de bilderna. Modellen ger solen över
  263,5°, 263,3° och 263,1° öst vid de tre passagerna i mars 1974, september 1974
  och mars 1975. Under ett år vandrar den alltså 0,4 grader.
- **Hela tabellen soldygn mot stjärndygn** pekar ut de tre retrograda: Venus
  −126,3 dygn, Pluto −43 s, Uranus negativt. Jorden +237 s (väntat 236), Mars
  +126 s, Merkurius +117,3 dygn. För Uranus och Neptunus rör det sig om ett par
  sekunder, och mätfönstret täcker bara en procent av deras varv kring solen –
  där är tecknet meningsfullt men storleken brus.

### 11.6 – Månen och Pluto

- [x] Månen: grå med de mörka haven (Mare Imbrium, Mare Tranquillitatis där
      Apollo 11 landade) och ljusa kraterstrålar kring Tycho.
- [x] **Bunden rotation**: månen roterar exakt ett varv per omloppsbana och
      vänder därför alltid samma sida mot jorden. Det är en av de bästa
      poängerna i hela appen att kunna visa.
- [x] Pluto: Tombaugh Regio, det ljusa hjärtformade området som New Horizons
      fotograferade 2015. Rotation 6,4 dygn, bunden till Charon.

Elva hav på månen med sina uppmätta lägen och storlekar, Tychos och Copernicus
strålsystem, och Pluto med Tombaugh Regio, Cthulhu Macula och sin ljusa
nordpolskalott. Den bundna rotationen kom redan med axeldatan i 11.1.

Till detta en ny hjälpare, `Streak`, som lägger ut ett smalt streck längs en
**storcirkel** i en given kompassriktning. Det spelar roll: en stråle som går
rakt norrut från Tycho på 43° syd och 1 400 km bort hamnar på helt olika ställen
beroende på om man räknar i gradnätet eller på klotet. Samma hjälpare kommer att
behövas för Europas sprickor i 11.7.

**Rättad data: Charons fasläge.** Modellen visade Charon över longitud 171° på
Pluto, alltså nästan rakt bort från nollmeridianen – trots att paret är
tidvattenlåst och IAU definierar Plutos nollmeridian som den som pekar mot
Charon. Felet låg inte i axeln utan i Charons medellongitud, som stod på 0,0 som
platshållare sedan mångdatan skrevs (kommentaren där sade redan att faslägena var
approximativa). Den är nu satt så att Charon hamnar över nollmeridianen. Följden
är att Sputnik Planitia, kring 175° öst, vänder sig **bort** från Charon – och så
ser det verkligen ut. Det är förmodligen ingen slump: slätten är tung nog att ha
vridit hela Pluto på plats.

**Verifiera:** Zooma in på jorden och följ månen ett helt varv – samma sida
ska vara vänd mot jorden hela tiden.

Kontrollerat utanför appen:

- **Apollo 11-platsen försvinner aldrig runt kanten.** Mare Tranquillitatis på
  8,5° nord och 31,4° öst står som mest 37,9° från jordriktningen, mätt varje
  dygn i 110 år. Under 90° betyder synlig, och 38° betyder god marginal.
- **Punkten mitt emot jorden håller sig mellan −6,5° och +6,1° longitud** över
  samma 110 år, utan drift. Det är den optiska librationen, verkligt ±6,3°, som
  är skälet till att vi ser 59 procent av månen i stället för halva.
- **Baksidan saknar hav, och det faller ut av datan**: elva hav på kartan, noll
  på baksidan. De ligger där för att det är där de finns.
- **Charon står över longitud −1,3° till +1,3° på Pluto** under 110 år. Den lilla
  vandringen är inte libration utan drift: Plutos rotation enligt IAU och Charons
  omloppstid i vår data skiljer 0,60 sekunder per varv, vilket ger 2,3° per sekel.
  Osynligt, men värt att veta att det är där.
- **Storlekarna stämmer**: Oceanus Procellarum 2 571 km mot kända ungefär 2 500,
  Mare Imbrium 1 146 mot 1 150, Sputnik Planitia 895 mot ungefär 1 000.

Två saker fick göras om efter att kartorna setts ritade. **Tychos strålar** var
åtta feta spikar med jämna mellanrum, 210 km breda, vilket gav en tecknad stjärna
i stället för en krater; nu tolv smala med oregelbundna riktningar och längder.
**Plutos grundton** var för ljus för att hjärtat skulle avteckna sig.

En tredje sak var mitt eget fel och rörde inte appen: kontrollsidan siktade
kameran med en fast höjdvinkel och räknade bara ut kompassriktningen, vilket inte
fungerar för en kropp vars axel lutar 113°. Hjärtat hamnade på baksidan och såg
ut att saknas. Sidan siktar nu rakt på den punkt som ska stå mitt i bild.

---

### 11.7 – De stora månarna

De fyra galileiska månarna och Titan ritas redan som skivor när man zoomar in på
sin planet, så de är stora nog att bära en yta. De har dessutom några av
solsystemets mest särpräglade utseenden.

- [x] Io: svavelgul och orangefläckig, den vulkaniskt mest aktiva kroppen i
      solsystemet. Inga kratrar alls – ytan görs om hela tiden.
- [x] Europa: nästan vit is, korsad av rödbruna sprickor. Slätast av allt vi
      känner till.
- [x] Ganymedes: gråbrun, med ljusa yngre områden mot mörka äldre. Solsystemets
      största måne, större än Merkurius.
- [x] Callisto: mörk och tätt kraterrik – den äldsta ytan av de fyra, som aldrig
      förnyats.
- [x] Titan: jämnt orange dis. Här ska ingen yta synas alls, precis som hos
      Venus, eftersom atmosfären är ogenomskinlig.
- [x] **Bunden rotation** för alla fem: de vänder samma sida mot sin planet,
      precis som månen mot jorden. Mekanismen kommer från 11.6 och behöver bara
      rotationsdata per måne.

Alla fem ligger i sin planets ekvatorsplan, så axeldatan finns redan efter 11.1.

**Titan fick medvetet ingen ytkarta.** Det är svaret på uppgiften, inte en lucka:
dimman är ogenomskinlig och månen ritas som en jämnt orange skiva med ljus och
skugga, precis som Venus. Axeln finns ändå med, eftersom bundenheten är sann
oavsett om den syns.

Longituden räknas från den punkt som pekar mot planeten. Nollan ligger mot
planeten, 180° rakt bort, **270° mitt på den ledande halvan** och 90° på den
eftersläpande. Det är inte bokföring utan fysik: Jupiters magnetfält roterar
fortare än månarna hinner runt, sveper alltså förbi dem bakifrån, och bakar in
svavel från Ios vulkaner i just den eftersläpande sidan. Därför är Europas
eftersläpande halva mörkare och rödare, och Callistos ledande halva ljusare.

Ny hjälpare `Annulus` ritar en ring kring en punkt på ytan, av samma skäl som
molnbanden byggs av fyrhörningar: en ring har hål i sig och går inte att fylla.
Den behövdes för Valhalla, märket efter Callistos största nedslag.

**Verifiera:** Följ Io ett varv kring Jupiter – samma sida ska vara vänd mot
planeten hela tiden, som månen mot jorden. Titan ska sakna synliga drag hur
mycket man än zoomar, till skillnad från de andra fyra.

Kontrollerat utanför appen:

- **Io håller nollmeridianen mot Jupiter genom hela varvet.** Mätt var 45° i
  banan avviker den som mest 0,5°, och den avvikelsen är inte ett fel utan
  librationen: banan är elliptisk, så farten varierar medan rotationen är jämn.
- **Librationen blir exakt 2e för alla fem**, vilket är vad teorin säger: Io
  ±0,5°, Europa ±1,1°, Ganymedes ±0,1°, Callisto ±0,8°, Titan ±3,3°. Mätt varje
  dygn i 110 år, utan någon drift.
- **Färdriktningen pekar mot longitud 270°** för alla fem, alltså ligger den
  ledande halvan där den ska. Europas mörkare fält har sin mitt på exakt 90° och
  Callistos ljusare på 270° – rätt halva var.
- **Valhallas ytterring** når 45° från mitten, vilket ger 3 786 km tvärs över mot
  uppmätta ungefär 3 800.
- **Ganymedes radie 2 634 km mot Merkurius 2 440**, alltså större än planeten,
  vilket är själva poängen med den.

Två saker fick göras om efter att kartorna setts ritade: Valhallas ringar var en
skarp måltavla i stället för de svaga vågkammar man anar, och Callisto var för
gles för att kallas mättat kraterrik (60 kratrar blev 110). Ganymedes fårknippen
gick från tio till arton, eftersom ungefär halva månen är sådan terräng.

### 11.8 – Månarna i fokusväljaren

Kom till efter att 11.7 var klar, eftersom kartorna annars inte gick att granska:
kameran kunde bara centreras på planeter, så en måne hann aldrig bli stor nog för
en glob innan den lämnade bilden.

- [x] Fokusväljaren listar varje planets månar under planeten, med en punkt
      framför så att grupperingen syns i en lista som inte kan dra in rader.
- [x] Månarna listas bara när de ritas. Släcker man kryssrutan "Månar" försvinner
      de ur väljaren, och följde kameran en av dem faller fokus till solen –
      samma regel som redan gällde för sonderna.
- [x] Kameran siktar på det **ritade** läget, inte det verkliga. Månarna dras in
      mot sin planet för att inte hamna utanför bild, och siktar man på den
      verkliga positionen pekar kameran långt bredvid. Ritkoden fick därför en
      `MoonPosition` som fokusväljaren delar med den.
- [x] En vald måne får samma bildvinkel som en planet, alltså tolv gånger sin
      egen radie. Utan det hamnar man antingen inuti månen eller så långt bort
      att den bara blir en prick.
- [x] `EarthFocusIndex`, som räknade platser i listan, är ersatt av en
      namnuppslagning. Med månar och sonder som kommer och går i listan går det
      inte längre att räkna sig fram till en rad.

**Verifiera:** Välj en måne i fokusväljaren och se att kameran hamnar vid den,
inte bredvid. Släck månarna medan en av dem följs och se att vyn faller tillbaka.

Kontrollerat utanför appen, 38 kontroller utan fel:

- **Listan byggs rätt**: 24 kroppar och 30 rader med allt påslaget, månen direkt
  under jorden, Io under Jupiter, Charon under Pluto.
- **Sondernas index håller trots femton nya rader emellan.** Det var den fällan
  som fanns i 10.6, nu med en större förskjutning: alla fem sonder pekar rätt.
- **Släcker man månarna** faller fokus från Ganymedes till solen, medan Jupiter
  och Voyager 1 behålls – valet bevaras på namn, inte på plats.
- **Alla månar med ytkarta blir 80 pixlars radie** vid det föreslagna avståndet,
  mot tröskeln 14. Kartorna kommer alltså att synas, med god marginal.
- **Månen ligger 2,9 planetradier ut och Ganymedes 6,3** vid det avstånd kameran
  väljer, alltså utanför planeten och inne i bild.

Phobos är undantaget: den är så liten att tolv gånger radien hamnar under
kamerans minsta avstånd, så den blir en prick på tre pixlar. Det är samma sak som
restpunkt R4 beskriver, och den har ingen ytkarta att visa ändå.

---

## Etapp 12 – Vad solsystemet mer har att visa

Fem tillskott och ett beslut, ordnade efter vad de ger delat med vad de kostar.
De tre första vilar på sådant modellen redan klarar – det är kontrollerat, och
siffrorna står under varje punkt. Den fjärde är inte en punkt att bygga utan ett
arkitekturval att ta ställning till, och det är värt att läsa innan någon börjar
med förmörkelser.

### 12.1 – Planetkonstellationer: gå till nästa möte

En väljare i stil med startfönstren för Mars: "nästa opposition för Mars",
"nästa gång Jupiter och Saturnus möts", "nästa gång alla planeter ligger på
samma sida om solen". Man väljer, appen hoppar till datumet, och man ser
ställningen uppifrån.

- [x] Sökfunktion för konjunktion och opposition mellan två valda kroppar.
      Samma form som `Mission.NextLaunchWindow`: stega dagvis, hitta minimum,
      förfina. Den koden finns och är redan snabb nog för knapptryck.
- [x] Knapp eller lista i kontrollpanelen som hoppar till nästa sådant datum.
- [x] Visa avståndet vid tillfället – Mars är dubbelt så nära vid en gynnsam
      opposition som vid en ogynnsam, och det är hela förklaringen till varför
      somliga oppositioner blir stora nyheter.

**Kontrollerat i förväg:** alla fyra Mars-oppositioner 2025–2031 faller på rätt
dygn ur modellen – 16 jan 2025 (0,644 AU), 19 feb 2027 (0,678), 25 mars 2029
(0,649) och 4 maj 2031 (0,559) – mot verklighetens 16 jan, 19 feb, 25 mars och
4 maj. Excentriciteten syns direkt i avstånden.

**Rättat i planen ovan: varningen om yttre konjunktioner behövdes inte.** Här
stod att den stora konjunktionen Jupiter–Saturnus hamnar sju veckor fel och att
datum för yttre konjunktioner därför borde visas med brasklapp. Det var mitt
mätfel: jag jämförde planeternas *heliocentriska* longituder. En konjunktion är
något man ser från jorden, och räknat därifrån hamnar den på rätt dag – 21
december 2020, på 0,11 grader. Det är jordens eget läge som avgör när två
planeter ser ut att mötas, och jordens läge är det modellen kan bäst.

Byggt som `Simulation/SkyEvent.cs`. Två sorters möte, en enda sökning: vid
konjunktion minimeras vinkeln mellan de två kropparna sett från jorden, vid
opposition minimeras hur långt planeten är från att stå rakt mitt emot solen.
Grovsökning dagvis, sedan gyllene snittet ned till en minut. Väljaren har sex
oppositioner (kropparna utanför jordens bana – Merkurius och Venus kan inte ha
någon) och sex konjunktioner mellan de ljusa planeterna. En konjunktion räknas
bara om kropparna kommer inom fem grader, alltså ungefär ett kikarfält.

Sökningen använder `PositionAuAt` i dubbel precision, som kom med R3.

**Verifiera:** De fyra Mars-oppositionerna ovan ska hittas på rätt dygn av
appens egen sökfunktion, inte bara av provprogrammet.

Kontrollerat utanför appen, 17 kontroller utan fel, allt genom `SkyEvent.Next`:

- **De fyra Mars-oppositionerna på rätt dygn**: 2025-01-16 (0,644 AU),
  2027-02-19 (0,678), 2029-03-25 (0,649) och 2031-05-04 (0,559).
- **Tre väldokumenterade konjunktioner på rätt dygn**: stora konjunktionen
  21 december 2020 på 0,11 grader (känt 0,10), Venus möter Jupiter 2 mars 2023
  på 0,49 (känt 0,52) och Mars möter Jupiter 14 augusti 2024 på 0,30 (känt 0,31).
- **Synodperioderna stämmer**, vilket är den kontroll som inte vilar på något
  minne: tiden mellan två oppositioner ska vara planetens synodperiod. Över
  tjugo mellanrum ger modellen Mars 781,2 dygn (känt 779,9), Jupiter 398,8
  (398,9), Saturnus 378,3 (378,1), Uranus 369,7 (369,7), Neptunus 367,5 (367,5)
  och Pluto 366,8 (366,7).
- **Spridningen i de mellanrummen är i sig värd att se**: Mars varierar mellan
  764 och 811 dygn medan Neptunus håller sig mellan 367 och 368. Ju rundare och
  avlägsnare bana, desto jämnare takt. Det var också därför ett första prov över
  bara fem mellanrum såg ut att missa Mars med fem dygn – för få stickprov.
- **Trycker man igen kommer man vidare**: fem tryckningar ger 2027-02, 2029-03,
  2031-05, 2033-06 och 2035-09, aldrig samma datum två gånger.
- **Alla tolv valen ger svar**, och snabbt: längst tar Jupiter möter Saturnus på
  10 ms, alla tolv tillsammans 14 ms. Den söker då fjorton år framåt.
- **Mars oppositioner varierar mellan 0,382 och 0,678 AU** över sexton år, alltså
  nästan dubbelt så långt bort vid en ogynnsam gång. Det är hela poängen med att
  visa avståndet.

### 12.2 – Heliopausen och solsystemets kant

Sonderna far redan ut ur solsystemet, men det syns inte var kanten går.

- [x] En genomskinlig sfär vid 120 AU, ritad först när man zoomat ut nog för
      att den ska rymmas. Det är där solvinden möter det interstellära mediet
      och solens välde tar slut.
- [x] Voyagernas genomgångar som milstolpar: 25 augusti 2012 och 5 november
      2018. De är de enda två tillfällen någon farkost från jorden passerat den
      gränsen.
- [x] Gärna en notering om att kanten inte är en kula utan buktar – solsystemet
      far genom det interstellära mediet och får en stötvåg framför sig.

**Kontrollerat i förväg:** modellen sätter Voyager 1 på 120,1 AU vid sitt
genomgångsdatum mot uppmätta 121,6, och Voyager 2 på 117,7 mot uppmätta 119,0.
Inom en och en halv AU utan att någonting behöver läggas till – uppgifterna
ligger redan i sonddatan.

Sfären ritas som en cirkel, men bara när kameran står **utanför** den. En kula
sedd utifrån projiceras till en cirkel med vinkelradien arcsin(R/d), alltså
R·f/√(d²−R²) på skärmen – det är inte samma sak som att projicera kanten rakt av.
Innanför skulle den fylla hela bilden och bara bli en blå slöja, så den ritas
först när den ryms i bild. Praktiskt betyder det bortom ungefär 290 AU, vilket
man kommer till genom att välja en av de fyra yttersta sonderna i fokusväljaren.

Milstolpen är en ny sort. Den har inget farthopp, för sonden passerade bara en
gräns, och den beskrivs därför som "Passerade Heliopausen" i stället för
"Förbi ... gav 10,8 km/s". Läget hämtas ur den bana sonden följde just den dagen,
så gränsen hamnar där sonden verkligen var och inte där någon skrivit in att den
var. Metoden `Probe.Crossing` läggs till efter `Build`, eftersom `Build` tar sina
punkter som `params` och inte har plats kvar för fler sorters uppgifter.

**Verifiera:** Ställ datumet till 25 augusti 2012 med Voyager 1 vald. Sonden ska
stå på sfären, inte innanför eller utanför den.

Kontrollerat utanför appen, 13 kontroller utan fel:

- **Sonderna står på sfären vid sina genomgångsdatum**: Voyager 1 på 120,1 AU
  den 25 augusti 2012, Voyager 2 på 117,7 AU den 5 november 2018, mot sfärens
  120. Ingenting har lagts till för att få det att stämma – uppgifterna låg
  redan i sonddatan sedan etapp 10.
- **De uppmätta genomgångarna var 121,6 och 119,0 AU**, alltså inte på samma
  avstånd. Det är i sig poängen med noten om att kanten buktar: den ligger olika
  långt ut åt olika håll, och 120 är ett runt tal mitt emellan.
- **Milstolpen sitter rätt**: rätt datum, i tidsordning bland de andra
  (Jorden → Jupiter → Saturnus → Heliopausen för Voyager 1, med Uranus och
  Neptunus emellan för Voyager 2), noll farthopp, och läget stämmer på
  kilometern med sondens egen position samma dag.
- **De tre andra sonderna har ingen gränsmilstolpe**, vilket de inte ska ha –
  Pioneer 11 och New Horizons är fortfarande innanför.
- **Var sonderna står i dag**: Voyager 1 på 169,3 AU, Voyager 2 på 142,1 och
  Pioneer 10 på 141,7 är utanför; Pioneer 11 på 119,6 och New Horizons på 64,9
  är innanför. Att Pioneer 11 ligger så nära kanten är värt att titta på.

Kvar att se med ögat: hur den genomskinliga sfären tar sig ut. Geometrin är
räknad men färgvalen är gissade, och gränsen för när den ritas – att cirkeln ska
rymmas inom 55 procent av bildens kortaste sida – är satt på känsla.

### 12.3 – Månbanans plan och nodernas vandring

Den ärliga versionen av förmörkelseidén – se 12.4 för varför den raka vägen inte
går. Frågan som ska besvaras är inte *när* det blir förmörkelse utan *varför det
inte blir det varje månad*.

- [x] Lägg `AscNodeRateDegPerDay` på `CelestialBody`, så att ett banplan kan
      vrida sig med tiden. Månens nod vandrar baklänges ett varv på 18,6 år,
      alltså 19,4 grader per år.
- [x] Samma fält kommer Triton till del. Kodkommentaren där säger redan att
      dess banplan precesserar med ungefär 640 års period och att orienteringen
      i datan är läget vid epoken snarare än en bestående egenskap.
- [x] Ett läge som ritar ut månbanans plan mot ekliptikan och markerar de två
      noderna, så att man ser att månen för det mesta passerar ovanför eller
      under solen i stället för framför den.

**Det blev två fält, inte ett.** `PerihelionRateDegPerDay` måste till samtidigt,
och skälet är att medelanomalin räknas som medellongitud minus perihelielongitud.
Låter man periheliet stå stilla medan noden rör sig får banan rätt plan men fel
läge i planet. Med båda blir det dessutom rätt av sig själv: månens medellongitud
går kvar sitt sideriska varv på 27,32 dygn medan medelanomalin går det
anomalistiska på 27,55, och skillnaden mellan de två är precis perigeums rörelse.

Månen: noden −0,0529539 grader per dygn, perigeum +0,1114041. Triton: +0,00154,
alltså framåt. Riktningen där följer av att banan är retrograd – Neptunus
tillplattning vrider noden med en hastighet som går som cosinus för banlutningen,
och lutningen är över 90 grader. Där de flesta månar får noden dragen baklänges
vandrar Tritons framåt.

Läget heter "Månbanan" och sitter i mötesraden. Det ritar månbanan, ekliptikans
plan som en gul ring att jämföra mot, nodlinjen där de två skär varandra, och de
två noderna utmärkta med namn.

**Verifiera:** Följ noden över tjugo år – den ska gå ett helt varv baklänges på
18,6 år. Månens banlutning mot ekliptikan ska ligga kvar på 5,1 grader hela
tiden; det är bara nodens longitud som ändras.

Kontrollerat utanför appen, 10 kontroller utan fel:

- **Ett varv baklänges på 18,61 år**: banpolen sveper −359,5 grader på 6798
  dygn, mätt ur lägena och inte avläst ur datan. Noden står i dag på 329,3
  grader mot 125,0 vid epoken.
- **Bara noden rör sig**: banlutningen håller sig mellan 5,142 och 5,167 grader
  över hela varvet.
- **Perigeum ett varv på 8,85 år**, och den anomalistiska månaden faller ut till
  27,5532 dygn mot kända 27,5546 – utan att någon skrivit in den.
- **Ingenting annat påverkades**: Mars-fönstret ligger kvar på 21 oktober 2026
  till 3,12 km/s och träffar Mars på noll kilometer, Voyager 1 går i 16,66 km/s
  och passerar heliopausen på 120,1 AU. Den bundna rotationen och Apollo
  11-platsens synlighet står också kvar.

**Kvar som förenkling: månens spinnaxel följer inte med noden.** Cassinis lag
säger att axeln ska hålla konstant 6,7 grader mot banplanet; mätt varierar den
mellan 3,6 och 6,7 över nodcykeln, alltså som mest 3,1 graders fel i axelns
riktning. Att rätta det kräver att `BodyAxis` också får en nodhastighet, och att
dagen trädas igenom hela ytritningen. Den bundna rotationen påverkas inte – den
är kontrollerad och håller.

**Varför det är värt det:** med noden i rörelse glider förmörkelsesäsongerna
nitton dygn bakåt varje år, och det är hela förklaringen till 18,6-årscykeln och
till att saros finns. Den rörelsen blir riktig även om enskilda datum inte blir
det.

**Verifiera:** Följ noden över tjugo år – den ska gå ett helt varv baklänges på
18,6 år. Månens banlutning mot ekliptikan ska ligga kvar på 5,1 grader hela
tiden; det är bara nodens longitud som ändras.

### 12.4 – Förmörkelser (och varför de är svåra)

**Skrivet om efter 12.3. Den utredning som stod här var för pessimistisk.**

Här stod att två saker behövdes för förmörkelser: nodens vandring, och periodiska
termer i månens läge. Nodrörelsen är gjord i 12.3, och sedan mättes vad den
ensam räckte till. Svaret var mer än väntat.

**Av arton verkliga solförmörkelser mellan 1999 och 2030 hittar modellen alla,
och sjutton av dem på rätt kalenderdag.** Den enda avvikelsen är 20/21 maj 2012,
som korsade datumlinjen och dateras olika beroende på var man stod. Detta med
enbart medelbanelement plus nodens och perigeums rörelse.

Före 12.3 låg samma modell 9 till 18 grader fel vid dessa datum och hittade en
enda "förmörkelse" under 2026, på fel dag. Slutsatsen att periodiska termer
behövdes för att få *datum* rätt var alltså felaktig. Vad de behövs till är något
annat, se nedan.

- [x] Beslut: ska månen få periodiska termer, eller ska appen nöja sig med
      att förklara mekanismen (12.3) utan att förutsäga datum?

**Beslutet blev: inga periodiska termer.** De behövs inte för det appen kan
använda dem till. Datumen är redan rätt, och nästa steg i noggrannhet – om
förmörkelsen blir total, ringformig eller partiell, och var på jorden den syns –
går ändå inte att nå med dem. Det avgörs av parallaxen från den plats betraktaren
står på, alltså av var på jordklotet man befinner sig, och det är en helt annan
sorts räkning än en bättre månbana. Appen visar solsystemet utifrån och har ingen
betraktare på markytan.

Det som återstår är alltså inte matematik utan gränssnitt:

- [x] En lista med förmörkelsedatum att hoppa till, i stil med mötesväljaren i
      12.1. Datumen kan sökas fram ur modellen på samma sätt som proven gjorde:
      nymåne eller fullmåne med månen inom ett par grader från ekliptikan.
- [x] Vid hoppet: slå på "Månbanan" automatiskt och zooma till jorden, så att man
      ser att solen står vid nodlinjen just då. Det är förklaringen, och den är
      värd mer än datumet i sig.
- [x] Var tydlig med vad som inte visas: om förmörkelsen är total eller partiell,
      och var på jorden. Det kräver en betraktare på markytan, vilket appen inte
      har.

Ingen lista behövdes: förmörkelserna blev två poster till i mötesväljaren från
12.1, och datumen räknas fram i stället för att skrivas in. Det är samma sökning
som för konjunktioner och oppositioner, med två skillnader. Tröskeln är hårdare –
1,55 grader för sol, vilket är solens och månens bredd plus den grad parallaxen
kan flytta månen mellan olika platser på jorden, och 1,0 grader för måne, vilket
är jordens kärnskugga minus månens egen radie. Och månens läge får inte behandlas
som en planets: dess banelement är geocentriska, så dess läge **är** redan
riktningen från jorden. Att dra bort jordens läge en gång till hade lagt jordbanan
ovanpå månbanan.

Klickar man fram en förmörkelse slås "Månbanan" och "Visa månar" på och kameran
ställer sig vid jorden, så att man ser solen stå vid nodlinjen just den dagen.
Etiketten säger rakt ut vad som inte visas: "typ och plats visas inte".

**Verifiera:** Modellens serie av förmörkelser ska stämma med verkligheten, inte
bara enstaka datum.

Kontrollerat utanför appen genom `SkyEvent.Next`, 5 kontroller utan fel:

- **Alla tio solförmörkelser 2024–2028 i exakt ordning**, ingen extra och ingen
  missad: 8 apr 2024, 2 okt 2024, 29 mar 2025, 21 sep 2025, 17 feb 2026,
  12 aug 2026, 6 feb 2027, 2 aug 2027, 26 jan 2028 och 22 jul 2028. Det är hela
  serien, inte ett urval – även de partiella som knappt märks.
- **Månförmörkelserna: sju funna, alla verkliga, inga falska larm.** En missas,
  den 12 januari 2028, och det är en grund partiell som ligger precis vid
  tröskeln. Det är gränsdragningen som avgör den, inte modellen.
- **Mellanrummen är hela synodmånader**: 6, 6, 6, 5, 6, 6, 6, 6, 6 varv. Att ett
  av dem är fem och inte sex är riktigt – förmörkelsesäsongerna kommer med 173
  dygns mellanrum, vilket inte går jämnt upp i månvarv.
- **Sarosperioden faller ut av sig själv.** Den totala solförmörkelsen över USA
  den 21 augusti 2017 återkommer i modellen den 1 september 2035, alltså 6585
  dygn senare mot sarosperiodens kända 6585,3. Babylonierna kände till den
  perioden och kunde förutsäga förmörkelser med den; här kommer den ut ur
  nodens och perigeums vandring utan att någon skrivit in den.

### 12.5 – Halleys komet

Ett enda objekt med mycket att visa: excentricitet 0,967, retrograd bana och ett
varv på 76 år som tar den från innanför Venus till utanför Neptunus.

- [x] Halley som en kropp med sina banelement. `Conic` klarar redan vilken
      excentricitet som helst, så det mesta finns.
- [x] Gärna en svans som pekar bort från solen och växer nära periheliet –
      svansen ligger inte bakom komet i färdriktningen utan bort från solen,
      vilket är en av de saker som är lättast att ha fel för sig om.
- [x] Nästa perihelium är 2061. Med datumväljaren går det att resa dit.

**Verifiera:** Perihelium 9 februari 1986 på 0,586 AU och aphelium 1948 på
35,1 AU, alltså utanför Neptunus bana.

**Utfall:** 15 av 15 kontroller.

| kontroll | modell | facit |
|---|---|---|
| perihelium | 1986-02-09, 0,586 AU | 1986-02-09, 0,586 |
| aphelium | 1948-05-18, 35,13 AU | 1948, 35,1 |
| nästa perihelium | 2061-07-28 | 2061-07-28 |
| fart i perihelium | 54,6 km/s | ~54 |
| fart i aphelium | 0,91 km/s | ~0,9 |
| närmast jorden 1986 | 10 april, 0,416 AU | 11 april, 0,42 |

Den sista raden är den som betyder något. De fem första följer av de element som
matats in, men mötet med jorden gör inte det – det beror på var jorden råkar
vara, och att modellen träffar dagen och avståndet är en kontroll mot något den
inte fått veta.

Två saker föll ut ur modellen snarare än in i den:

- **Svansen pekar bort från solen, inte bakåt.** Mätt som vinkeln mellan
  riktningen bort från solen och färdriktningen: 138 grader sextio dygn före
  periheliet, 42 grader sextio dygn efter. På vägen ut går kometen alltså med
  svansen före, vilket är det som är lättast att ha fel för sig om.
- **Halley är osynlig 74 år av 75.** Isen ångar först innanför 3 AU, och där
  ligger kometen 368 dygn av sina 27 563 – 1,3 procent av varvet. Innanför 1 AU
  är den 78 dygn. Talet är inte inskrivet någonstans utan mätt på banan.

**Tillägg: "Halley i perihelium" i mötesväljaren.** Punkten ovan sade att man
reser dit med datumväljaren, och det går – men mötesväljaren fanns redan och
gör samma sak utan att man behöver kunna datumet utantill. Sex kontroller till:

| kontroll | utfall |
|---|---|
| hittar periheliet 1986 | samma minut som en direkt minimering |
| nästa från dagens datum | 2061-07-28 |
| fyra klick i rad | 1910-08-24, 1986-02-09, 2061-07-28, 2137-01-13 |
| mellanrummen | 27 563, 27 563, 27 562 dygn |

Sökningen minimerar avståndet till solen i stället för en vinkel på himlen.
Storheten har en annan enhet än de andra, men det spelar ingen roll – maskineriet
letar dalläge, och ett avstånd har ett lika tydligt dalläge som en vinkel. Det är
till och med lättare: en bana har exakt ett perihelium per varv, så det kan
varken missas eller dubbelräknas.

Det som rapporteras är däremot medvetet inte solavståndet. Det är 0,586 AU varje
gång – det är ju vad ett perihelium är – och säger därför ingenting om just den
gången. Vad som skiljer besöken åt är var jorden råkar vara, och där ger modellen
ett svar den inte fått veta:

| | avstånd till jorden | elongation från solen |
|---|---|---|
| 9 februari 1986 | 1,55 AU | 8° |
| 28 juli 2061 | 0,48 AU | 20° |

**Det där är historien, uträknad.** 1986 års besök var det sämsta på tvåtusen år:
vid periheliet stod Halley på andra sidan solen, åtta grader från den på himlen,
alltså mitt i dagsljuset. Kometen blev inte något att se förrän två månader
senare, den 10 april, och då på 0,42 AU. 2061 blir tvärtom: kometen står på
samma sida som jorden och kommer som närmast 0,482 AU redan på perihelidagen,
mot NASA:s förutsägelse 0,48 AU den 29 juli. Ingenting av det är inmatat.

**Val och förbehåll:**

- **Elementen är förankrade i två perihelier**, 1986 och 2061, i stället för
  hämtade ur en efemerid. Skälet är att en fast keplerbana inte kan träffa alla:
  Halleys verkliga omloppstid varierar mellan 74 och 79 år, eftersom Jupiter och
  Saturnus drar i kometen vid varje varv och gasstrålarna från den upphettade
  kärnan knuffar den som en svag raket. Priset syns direkt – modellen lägger 1910
  års perihelium i slutet av augusti i stället för den 20 april. Det står i
  koden.
- **Två svansar, inte en.** Jonsvansen är gas som solvinden river med sig rakt
  bort från solen; dammsvansen är korn som är för tunga att ryckas med, behåller
  kometens fart och släpar efter i banan. Skillnaden mellan dem är just den sak
  punkten ovan handlar om, och den syns bara om båda ritas.
- **Kaman ritas efter aktiviteten och inte efter kroppen.** Kärnan är 5 km bred
  och skulle aldrig gå att se; det man ser i verkligheten är gasmolnet omkring,
  som nära solen blir bredare än solen själv.
- **Svansen har en minsta längd på skärmen.** 0,3 AU är stort mot jordens bana
  men litet mot Halleys egen, så utzoomat vore svansen några pixlar. Riktningen
  är den uträknade, bara längden är tilltagen – samma sorts eftergift som
  planeternas förstoring.
- **Av som standard.** Kometen är borta 74 år av 75, och dess bana är så
  utdragen att den skymmer planeternas om den ligger kvar i bilden.
- Kometen går att välja i fokusväljaren, vilket den behöver: utan kamera på
  plats försvinner den ur bild i årtionden. Avståndet ramar in svansen och inte
  kroppen.
- **Hoppet till periheliet tänder kometen men rör inte kameran.** Att resa till
  ett datum där kometen är släckt vore att resa till ingenting. Kameran lämnas
  däremot i översikten, för det är där man ser vad som händer: att kometen dyker
  in genom hela planetsystemet och ut igen. Vill man gå nära finns den i
  fokusväljaren.

### 12.6 – Solens rotation

Appen är tunnast just vid solen – den är en skuggad skiva. Rotationen är det
som ger mest tillbaka.

- [x] Solen får en `BodyAxis` och en enkel ytkarta med några solfläckar.
- [x] Rotationstid 25 dygn vid ekvatorn.

**Förbehåll som måste stå i koden:** solen roterar olika fort på olika
breddgrader – 25 dygn vid ekvatorn, 34 vid polerna. Det är i sig en av de bästa
sakerna att visa, men en enda rotationstid kan inte uttrycka det, så solfläckar
på höga breddgrader kommer att driva fel. Antingen får det stå som en uttalad
förenkling, eller så behöver ytan en egen mekanism där rotationen beror på
latituden.

**Verifiera:** En fläck vid ekvatorn ska komma tillbaka till samma läge efter
25 dygn.

**Utfall:** 10 av 10 kontroller. Av de två vägarna i förbehållet valdes den
andra: ytan fick en egen mekanism.

| kontroll | modell | facit |
|---|---|---|
| ekvatorns lutning | 7,252° | 7,25 |
| varv vid ekvatorn | 25,03 dygn | ~25 |
| varv vid 30° bredd | 26,39 dygn | 26,4 |
| synodiskt varv, sett från jorden | 26,87 dygn | ~26,9 |
| nordpolen mest mot oss | 8 september, B₀ = +7,25° | 7–8 september |
| sydpolen mest mot oss | 6 mars, B₀ = −7,25° | 5–6 mars |

De två sista raderna är de som betyder något. Lutningen 7,25 grader prövas mot
sig själv, men **noden 75,77 prövas av ingenting annat** – och det är den som
avgör vilken månad solens nordpol lutar mot oss. Att modellen lägger den i
början av september och sydpolen i början av mars är alltså en kontroll av ett
tal som inte gick att kontrollera på något annat sätt.

**Differentiell rotation, på riktigt.** Solen roterar inte som ett stycke, och
det är den enda kroppen i appen som inte gör det. Takten följer Newton och Nunns
mätning på solfläckar från 1951:

    ω(φ) = 14,38 − 2,96 · sin²φ  grader per dygn

Vad det ger, mätt i modellen:

| bredd | varv | följd |
|---|---|---|
| 0° | 25,03 dygn | |
| 30° | 26,39 dygn | 0,74°/dygn långsammare än ekvatorn |
| 35° | 26,85 dygn | |

Två grupper som ligger på 8 och 22 graders bredd glider isär med 0,36 grader om
dygnet. På ett år har den nedre dragit ifrån den övre med 131 grader, alltså en
tredjedels varv. Det går att se i appen genom att zooma in på solen och stega
fram månad för månad, och det är hela poängen med etappen: **en fast kropp kan
inte göra så.** Att solen roterar olika fort på olika breddgrader är beviset för
att den är gas rakt igenom.

**Val och förbehåll:**

- **Rotationstiden är solfläckarnas, inte plasmans.** De två skiljer sig: mätt
  med dopplerskift går ekvatorn på 24,5 dygn, mätt på solfläckar 25,0. Eftersom
  det är fläckar som ritas är det fläckarnas takt som gäller. Det är också den
  som ger de 25 dygn punkten ovan efterfrågar; Carringtons klassiska 25,38 är ett
  tredje tal och gäller vid 26 graders bredd.
- **Lagen gäller bara där fläckar finns**, alltså inom ±35 graders bredd.
  Sträckt till polen ger den 31,5 dygn mot uppmätta 34. Det spelar ingen roll
  här eftersom det bara är fläckar som ritas, men det vore fel att dölja.
- **Vridningen tas per yta och inte per hörn.** En solfläcksgrupp följer med som
  en klump, vilket är vad verkliga grupper gör. Läts varje hörn gå i sin egen
  takt skulle en grupp som spänner över några breddgrader dras ut till en smet
  på ett par år – och verkliga grupper hinner aldrig dit, de dör inom veckor.
- **Fläckarna ligger stilla och finns för alltid.** I verkligheten lever en grupp
  några veckor, antalet följer elvaårscykeln, och deras breddgrader vandrar mot
  ekvatorn under cykelns gång – ritat i ett diagram blir mönstret det berömda
  fjärilsparet. Kartan visar alltså hur solen ser ut ett år nära maximum.
- **De är ritade något större än verkliga.** Sammanlagt täcker de synliga
  fläckarna knappt en procent av skivan, mot ungefär en halv procent vid ett
  starkt maximum. Mindre än så syns de inte: vid tröskeln på 30 bildpunkter är
  den största gruppen knappt två pixlar bred.
- **Fackelfälten är utelämnade.** De ljusa slöjorna kring fläckarna syns nästan
  bara nära randen, där man ser snett genom gasen. En platt färgyta kan inte
  uttrycka det.
- **Nollmeridianen är en ren överenskommelse.** Solen har inga bestående drag att
  räkna från – ingen krater, ingen kust, bara gas som byts ut. Carringtons
  nollmeridian är inlagd för formens skull.
- **Randmörkningen fanns redan.** Solskivan har alltid ritats med en gradient
  från vitt i mitten till orange vid kanten, och det visar sig vara fysik och
  inte utsmyckning: vid randen ser man snett in i gasen och når bara de övre,
  svalare lagren.

---

## Etapp 13 – Publik release

Två delar som hör ihop utan att bero på varandra: att appen går att visa på fler
språk än svenska, och att koden går att lägga ut öppet. De kan tas i vilken
ordning som helst.

### 13.1 – Språkstöd

Appen ska kunna visas på svenska och engelska, med språken i egna filer så
att fler språk (t.ex. tyska) bara blir en fil till – ingen kodändring.

- [ ] Lägg alla texter i .resx-resursfiler (standardmekanismen i .NET):
      `Resources/Strings/AppStrings.resx` (engelska som grundspråk) och
      `AppStrings.sv.resx` (svenska). Ett nytt språk = en ny
      `AppStrings.xx.resx`.
- [ ] Flytta ut och översätt:
      - [ ] Menyer och reglage (Pausa/Starta, Hastighet, Visa banor,
            Verklig storlek, Stjärnbilder, Stjärnnamn, Stjärnor, Fokus,
            Återställ vy, Få/Normalt/Många)
      - [ ] Klock- och infotexter (Förflutet, dygn/sek, hjälpraden, fönstertitel)
      - [ ] Himlakroppsnamn (Solen/The Sun, Jorden/Earth, Månen/The Moon ...) –
            datat i `SolarSystemData` får språkneutrala nycklar, namnen slås
            upp i resurserna
      - [ ] Stjärnbildsnamn (Karlavagnen - Stora björn / Big Dipper - Ursa
            Major, Lilla björn / Little Bear ...). Stjärnornas egennamn
            (Betelgeuse, Sirius, Polaris/Polstjärnan) är internationella och
            behöver bara översättas i undantagsfall.
- [ ] Språkval: följ operativsystemets språk som standard, plus en väljare i
      kontrollpanelen så att läraren kan byta språk direkt på lektionen.
- [ ] Datum- och talformat följer valt språk (i dag hårdkodat sv-SE).
- [ ] README på båda språken (eller engelsk README med svensk sektion).

**Verifiera:** Byt språk i väljaren: alla texter, planetnamn och
stjärnbildsnamn byter språk direkt, datumet formateras rätt ("fredag 5
september" / "Friday, September 5") och inga texter blir avklippta i
kontrollpanelen. Svenska ska se exakt ut som i dag.

---

### 13.2 – Öppen källkod på GitHub

Genomgången nedan är gjord mot det här repot och inte skriven ur en allmän
checklista. Sju saker hittades, och den andra bör avgöras innan något alls
publiceras – den går inte att ta tillbaka.

- [ ] **Det finns ingen licensfil.** Utan en sådan gäller full upphovsrätt som
      standard: koden går att läsa på GitHub men ingen får lagligt använda,
      ändra eller sprida den, vilket knappast är avsikten med att lägga upp den.
      För en undervisningsapp ligger MIT eller Apache 2.0 nära till hands; MIT är
      kortare, Apache 2.0 ger uttryckligt patentskydd.

- [ ] **Arbetsmejlen ligger i varje commit.** Hela historiken är signerad
      `krister.hellsing@swedavia.se`, alltså en arbetsgivaradress, och den följer
      med när repot publiceras. Det är ett val att göra medvetet och inte att
      upptäcka efteråt: går det att byta i efterhand krävs en omskrivning av hela
      historien, vilket ändrar alla commit-id. Enklast är att bestämma sig först.
      Värt att fundera på i samma veva är om något i arbetsgivarens regler rör
      kod skriven på fritiden.

- [ ] **Mallrester från projektmallen.** `ApplicationId` står kvar på
      `com.companyname.solarsystem`. `Resources/Images/dotnet_bot.png` ligger
      kvar men används ingenstans i koden – det är Microsofts maskot och behöver
      inte följa med. Detsamma gäller `Resources/Raw/AboutAssets.txt`, som bara
      är mallens egen instruktionstext. Appikonen och startskärmen är fortfarande
      .NET-mallens lila.

- [ ] **Attribution för det som lånats.** Open Sans används på riktigt, genom
      `Styles.xaml`, och dess Apache 2.0-licens kräver att licenstexten följer
      med. Datan bör också få sina källor utskrivna: banelementen kommer från
      NASA/JPL, polriktningarna från IAU:s arbetsgrupp för kartografiska
      koordinater, och stjärnkatalogens ursprung bör redas ut och anges.

- [ ] **README behöver bli ett repo-README.** Den nuvarande är en utmärkt
      funktionsbeskrivning men saknar det en besökare först vill ha: en mening om
      vad det är, en skärmbild, hur man bygger det (.NET 10 SDK, MAUI-
      arbetsbelastningen, Windows) och vilken licens som gäller.

- [ ] **Plattformsmapparna lovar mer än appen håller.** `Platforms/Android`,
      `iOS` och `MacCatalyst` finns kvar från mallen medan `TargetFrameworks`
      bara innehåller Windows. Antingen bort, eller en rad i README om att bara
      Windows är byggbart i dag.

- [ ] **Översätt alla kodkommentarer till engelska.** De är svenska rakt igenom
      i dag, vilket stänger ute alla som inte läser svenska – och det är de flesta
      som hittar ett repo på GitHub.

      Det är den största enskilda posten i hela etappen. Räknat i koden: 1 562
      rader XML-dokumentation och 494 vanliga kommentarrader, tillsammans 2 056
      av 7 329 rader, alltså 28 procent. Tyngst ligger `SurfaceMap.cs` med 339
      rader, `SolarSystemDrawable.cs` med 279, `SolarSystemData.cs` med 263 och
      `Mission.cs` med 202.

      Det är heller ingen mekanisk översättning. Kommentarerna bär projektets
      egentliga dokumentation – varför noderna låg 180 grader fel, varför
      konjunktioner räknas från jorden och inte från solen, varför Venus har
      strimmor trots att den är slät. Den sortens resonemang måste bära lika bra
      på engelska, annars är det bättre att låta bli.

      Skriv enkel engelska, inte elegant. Den som forkar har sannolikt varken
      svenska eller engelska som modersmål utan engelska som andraspråk, och då
      bär korta meningar och vanliga ord längre än idiom och ordvitsar. Flera av
      de svenska kommentarerna leker med språket – det får gärna gå förlorat.

      Kvar att bestämma: om `TODO.md` och `README.md` ska följa med över till
      engelska (se punkten om TODO nedan). Att appen i övrigt är gjord för
      svenska elever är däremot avgjort och kräver ingenting – se beslutet under
      Anteckningar.

- [ ] **Beslut: ska TODO.md följa med?** Den innehåller interna anteckningar och
      provlistor, men också den enda samlade förklaringen till varför saker ser
      ut som de gör – vilka fel som hittats, vilka förenklingar som gjorts och
      varför. För någon som vill förstå koden är den förmodligen mer värd än
      README.

**Verifiera:** Klona repot till en tom katalog på en annan maskin och bygg det
med enbart instruktionerna i README. Går det inte, är README inte färdig.

---


## Etapp 14 – Dela logik och gränssnitt

Målet är att samma solsystem ska kunna drivas av mer än ett gränssnitt, så att
appen på sikt kan köras i en webbläsare vid sidan av skrivbordsappen.

**Utgångsläget är bättre än det ser ut.** En mätning av kodens beroenden:

| mapp | rader | beroenden till gränssnittet |
|---|---|---|
| `Simulation/` | 3 100 | enbart `Color`, 209 användningar |
| `Rendering/` | 1 500 | `Microsoft.Maui.Graphics`: ICanvas, PathF, RectF, PointF |
| `MainPage.xaml(.cs)` | 1 350 | hela MAUI Controls, 41 kontroller, 30 händelsemetoder |

Simulationen importerar inte MAUI någonstans – enda `using` i hela mappen är
`System.Numerics`. Det är inte en gissning: provprogrammet som verifierat varje
etapp sedan etapp 9 kompilerar hela `Simulation/` med en tolv rader lång
ersättning för `Color` och ingenting annat. Logiken är alltså redan fri, den bara
bor i samma projekt som gränssnittet.

### 14.1 – Bryt ut kärnan till ett eget projekt

- [ ] Flytta `Simulation/` till `Solarsystem.Core`, ett bibliotek utan beroende
      till MAUI.
- [ ] Bestäm vad som ska hända med `Color`. Två vägar: en egen liten typ, eller
      ett beroende till paketet `Microsoft.Maui.Graphics`. Det senare låter
      mindre oskyldigt än det är – paketet är fristående från MAUI Controls och
      innehåller bara rit- och färgprimitiver. Men en egen typ gör kärnan helt
      beroendefri, och 209 användningar är ingen stor omskrivning.
- [ ] Låt provprogrammet referera projektet i stället för att kopiera filerna
      och shimma `Color`. Det har fungerat länge men är en kopia som kan hamna ur
      fas med originalet.

**Verifiera:** `Solarsystem.Core` ska bygga utan en enda referens till MAUI, och
provprogrammets alla kontroller från etapp 9 till 12 ska gå igenom mot det
byggda biblioteket i stället för mot kopierade filer.

### 14.2 – Ritlagret som ett eget projekt

- [ ] Flytta `Rendering/` till `Solarsystem.Rendering`, som får bero på
      `Microsoft.Maui.Graphics` men inte på MAUI Controls.
- [ ] Utred den fråga som avgör hela etapp 14:s värde – **går `ICanvas` att
      genomföra på webben?** `SolarSystemDrawable` är 1 282 rader och ritar allt
      genom det gränssnittet. Finns det en canvas-implementation för webben går
      hela ritlagret att återanvända oförändrat. Finns det inte måste det skrivas
      om, och då är det den största posten i hela projektet.

**Verifiera:** Skrivbordsappen ska se likadan ut efter flytten. Ritkoden rörs
inte, bara var den bor.

### 14.3 – Tillståndet ur MainPage

Det här är den egentliga svårigheten. `MainPage.xaml.cs` är 1 080 rader och
innehåller två sorters kod blandat: det som pratar med MAUI:s kontroller, och det
som vet vad appen gör. Det senare måste ett andra gränssnitt kunna använda.

- [ ] Bryt ut tillståndet till en klass utan gränssnittsberoende: klockan och
      hastigheten, vald kropp i fokusväljaren, pågående rymdfärd, vilka sonder
      som visas, kameran.
- [ ] Låt `MainPage` bli tunn – ta emot klick, skicka vidare, visa det som
      kommer tillbaka.

**Verifiera:** Räkna raderna i `MainPage.xaml.cs` efteråt. Blir den inte
väsentligt kortare har tillståndet inte flyttat, bara fått sällskap.

### 14.4 – Välj webbteknik

Utredning, inte bygge. Frågan är inte avgjord och bör inte avgöras förrän 14.1
till 14.3 visat hur mycket som faktiskt går att återanvända.

- [ ] Jämför alternativen. Blazor WebAssembly ligger nära till hands eftersom
      allt redan är C# och kärnan då kan följa med oförändrad. Men det finns
      andra vägar: Blazor Server, kärnan kompilerad till WebAssembly med ett
      gränssnitt i något annat, eller ett gränssnitt skrivet direkt mot en
      webbcanvas.
- [ ] Avgör ritfrågan från 14.2 först. Den väger tyngre än valet av ramverk:
      går ritlagret att återanvända är resten mest arbete med knappar, medan ett
      omskrivet ritlager är ett halvt nytt projekt.
- [ ] Väg in att appen är beräkningstung på klienten – banor, ytor och
      stjärnhimmel räknas om varje bildruta. Det talar för att räkningen ska ske
      i webbläsaren och inte på en server.

**Verifiera:** Ett beslut med skäl nedskrivna, inte ett ramverk valt på känsla.

---

## Anteckningar och beslut

- **Skala:** månar följer Månens princip – i förstorat läge komprimerat
  avstånd till moderplaneten (annars hamnar de orimligt långt bort), i läget
  "Verklig storlek" äkta geometri.
- **Synlighet:** månar och ringar ritas först vid inzoomning (samma tröskel
  som jordens måne) så att översiktsvyn hålls ren.
- **Prestanda:** bälten görs med samma positions-cache som stjärnhimlen;
  inga gradienter i stora mängder (läxan från Vintergatan).
- **Månantal:** vi ritar bara de stora/pedagogiska månarna. Att Jupiter har
  ~95 och Saturnus ~274 kända månar kan i stället nämnas i en infotext.
- **Nya texter fram till språkstödet:** skrivs tills vidare på svenska som i
  dag, men samlas gärna på få ställen i koden så att etapp 13.1 (språkstöd)
  blir enkel att genomföra.
- **Appen får förbli gjord för svenska elever.** Den frågan kom upp inför
  etapp 13.2: ska det svenska anslaget tonas ned när koden läggs ut öppet?
  Nej. Öppen källkod löser det på sitt eget sätt – den som vill ha en variant
  för sitt land forkar och vrider till den. Det gäller ändå bara ett par saker,
  eftersom solsystemet ser likadant ut överallt: gränssnittsspråket, som 13.1
  gör valbart, och stjärnbildernas svenska namn som Karlavagnen. Kodkommentarerna
  är en egen fråga och översätts till engelska i 13.2, så att en fork alls ska
  vara möjlig att göra.

---

## Restpunkter

Sådant som är känt, medvetet lämnat och inte brådskande, men som någon gång bör
tas om hand. Alla projekt har lite teknisk skuld – poängen med listan är att den
är skriven i stället för bortglömd.

### R1 – Prova gränssnittet på riktigt

Etapp 9 och 10 är verifierade med siffror: banor, farter, restider, milstolpar,
träffar på planeterna och indexlogiken i sondväljaren är alla kontrollerade i
ett separat program utanför appen. Ritningen och gränssnittet är däremot bara
provkörda så långt att de inte kraschar – ingen har sett efter hur det faktiskt
ser ut. Det som står här är alltså inte känt fel, bara oprövat.

Rymdfärderna (9.4 och 9.5):

- [x] Månfärdens bana syns när man skjutit upp och zoomat till jorden, och
      farkosten möter månen vid ankomsten.
- [x] Färdpanelen uppe till vänster dyker upp vid uppskjutningen, försvinner när
      färden avbryts och byter text vid framkomsten.
- [x] Kameran hakar på farkosten vid ankomsten och zoomar in till målet, en gång.

Sonderna (10.2 till 10.4):

- [x] Sondernas prickar och spår ser rimliga ut, och färgerna går att skilja åt.
- [x] Milstolpsringarna hamnar rätt längs spåren.
- [x] Årtalen vid ringarna blir läsbara och inte en enda gröt när flera passager
      trängs ihop – överhoppningen vid krock är den mekanism som ska hindra det,
      och den är helt oprövad i praktiken.
- [x] Den valda sondens milstolpar får fullständig text (planet, månad, farthopp)
      utan att skriva över annat.
- [x] Sondpanelen visar rätt när sonden ännu inte skjutits upp, under färden och
      efter sista passagen.
- [x] **Att välja en sond i fokusväljaren.** Bocka först fram en sond i rutan
      "Rymdsonder" – annars står den inte i fokusväljaren. Välj den sedan där.
      Kameran ska hoppa ut och lägga sonden mitt i bild, och solen ska synas som
      en prick i bilden med hela planetsystemet hopkrympt omkring sig. Poängen
      är avståndet: Voyager 1 ligger 169 AU ut, så kameran ställer sig 406 AU
      från solen. Vrid kameran runt och luta den – solen ska stanna i bild hela
      varvet.

De kretsande sonderna (10.5):

- [x] Cassinis ellips syns när man ställer datumet mellan 2004 och 2017 och
      väljer Saturnus, och ligger i samma storleksordning som Titans bana.
- [x] Junos ellips syns vid Jupiter mellan 2016 och 2025, och det går att zooma
      ut tillräckligt för att se hela det vida varvet.
- [x] Ingen av dem ritas utanför sin uppdragstid.

Sondväljaren (10.6):

- [x] Rutan fälls ut och går att klicka i. Den ligger ovanpå vyn och inte i
      kontrollpanelen, så det här är värt att prova först av allt i listan.
- [x] "Alla" och "Inga" gör vad de ska, och räknaren i knapptexten följer med.
- [x] **Att släcka den sond man följer.** Följ en sond enligt punkten ovan, och
      släck den sedan i rutan "Rymdsonder" medan kameran följer den. Fokus ska
      falla tillbaka till Solen, och kameran ska hoppa **in** till översiktsvyns
      15 AU. Utan det sista skulle den bli stående 300–400 AU ute och titta mot
      en prick, eftersom det bara är målpunkten som byts och inte avståndet.
      (Provlistan sade tidigare att vyn "zoomar ut" här. Det var fel skrivet: det
      är en inzoomning, från hundratals AU till femton.)

Etapp 11:

- [x] Jordgloben ser ut och snurrar precis som före ombyggnaden i 11.1.
- [x] Mars ytkarta ser ut som Mars: rätt rostton, dragen känns igen och
      polarisen ligger som en vit kalott. Se dock resten av noten nedan.
- [x] Jupiters band ser ut som band och inte som randiga skarvar. Överlappet på
      2 % räckte – inga fogar syns i bild.
- [x] Jupiters polarkalotter fyller åt rätt håll, trots att de ligger mycket
      längre från polen än jordens isar.
- [x] Jupiters dämpade palett, sedd i bild och gjord om två gånger (se 11.3).
- [x] **De stora månarna gick i praktiken inte att se.** Kameran kunde bara
      centreras på planeter, och en måne ritas som glob först vid 14 pixlars
      radie: Ganymedes är 3,8 procent av Jupiters radie, så Jupiter måste vara
      372 pixlar – och då ligger Io 1 100 pixlar från bildmitten, utanför rutan.
      Åtgärdat genom att lägga månarna i fokusväljaren, se noten efter 11.7.
- [x] Månen och Pluto i appen. Månen ritas som glob först när den blir stor nog
      i bild, och den tröskeln är inte prövad för en måne – bara för planeter.
- [ ] Kontrollpanelen går att fälla ihop. Knappen "Dölj kontroller" i panelens
      överkant, eller tangenten M, ska lämna kvar bara en 24 pixlar hög list och
      låta vyn växa. Knapptexten ska växla till "Visa kontroller".
- [ ] Månbanan och noderna i bild. Kryssa i "Månbanan", välj jorden och zooma in:
      månbanan ska luta mot den gula ekliptikaringen och nodlinjen gå genom
      jorden med båda noderna namngivna. Färger och storlekar är gissade.
- [ ] Förmörkelsevyn. Välj "Solförmörkelse" och tryck "Gå till nästa": månbanan
      och månarna ska slås på av sig själva, kameran ställa sig vid jorden, och
      solen stå längs nodlinjen just den dagen.
- [ ] Heliopausen i bild. Välj Voyager 1 i fokusväljaren så kameran hamnar på
      406 AU; sfären ska då synas som en svag cirkel med solsystemet inuti, och
      Voyager 1 ska ligga utanför den medan New Horizons ligger innanför.
- [ ] Halleys komet i bild. Kryssa i "Halleys komet", skriv 1986-02-09 i
      datumrutan och välj kometen i fokusväljaren. Svansarna ska peka bort från
      solen – den blå raka och den gula kortare och böjd – och när tiden går
      framåt ska de svänga runt så att kometen på väg ut går med svansen före.
      Prova också att stega några år framåt: svansen ska försvinna helt när
      kometen kommit ut förbi asteroidbältet. Banans form, färg och svansarnas
      bredd är gissade och sedda bara i kod.
- [ ] Solfläckarna i bild. Zooma in på solen tills skivan är några centimeter
      bred – fläckarna dyker upp vid 30 bildpunkters radie. De ska ligga i två
      bälten på ömse sidor om ekvatorn, ha en mörk kärna i en ljusare gård, och
      vandra över skivan när tiden går. Stega sedan fram ett år i taget: de
      nedre grupperna ska dra ifrån de övre. Färgerna på kärna och gård är
      gissade, liksom fläckarnas storlek, och sedda bara i en kontrollritning
      utanför appen.
- [ ] "Halley i perihelium" i mötesväljaren. Med kometen släckt: välj den och
      tryck "Gå till nästa". Datumet ska bli 2061-07-28, kryssrutan ska tändas av
      sig själv, kameran ska stå kvar där den stod, och texten ska säga 0,48 AU
      från jorden och 20 grader från solen.
- [x] Merkurius kratrar i appen. I den externa kontrollen ser de ut som
      kratrar, men de ligger tätt och kan bli prickiga vid stark inzoomning.
- [x] Saturnus, Uranus och Neptunus i appen. De är sedda i den externa
      kontrollen men inte med ringarna på plats – särskilt Saturnus ska ses
      tillsammans med sina ringar, och Uranus sydpolskalott ligger nära den kant
      där ringarna korsar.
- [x] **Mars mörka fält hade synligt raka kanter.** De är ritade med fem till
      nio hörn, och `Densify` delar upp långa kanter utan att runda dem – den
      lägger bara ut fler punkter längs samma räta linje. Åtgärdat med Chaikins
      hörnkapning, två varv, som ett val per karta: Mars rundas, jorden inte.
      Kustlinjer ÄR kantiga, och Jupiters band måste dessutom behålla raka kanter
      för att inte glipa. Se noten i 11.2.
- [x] Saturnus och Uranus lutar åt rätt håll efter nodrättningen – enklast att se
      på ringarna vid ett par årtal långt isär.

Kontrollpanelen:

- [x] Raden "Rymdfärd:" med de tre knapparna får plats även i ett smalt fönster.
      Det var just den raden som svämmade över kanten innan knapparna flyttades
      till en egen rad i 10.4.

### R2 – Bekräfta Junos slutdatum ✔

Juno ritades fram till 30 september 2025, vilket var det förlängda uppdragets
planerade slut. Om det förlängdes ytterligare visade appen inte en sond som
faktiskt kretsar kring Jupiter. Valet var medvetet – hellre missa en sond som
flyger än visa en som inte finns – men det byggde på en uppgift som inte gick
att kontrollera när koden skrevs.

- [x] Ta reda på om Juno fortsatte efter september 2025 och rätta slutdatumet i
      `ProbeData`. Det är en rad.

**Den flög vidare.** Juno passerade det planerade slutet och skickade data hela
våren 2026; den 1 maj 2026 tog den närbilder av den lilla månen Thebe med sin
stjärnkamera. Därefter finns inget bekräftat. Att uppgifterna var svåra att få
fram hösten 2025 hade en tråkig förklaring: den amerikanska statsapparaten var
avstängd just när uppdraget skulle ha avslutats, så ingen kunde säga något om
sondens öde.

Slutdatumet är nu 1 maj 2026, och texten säger "Senast bekräftade kontakten" i
stället för att påstå ett uppdragsslut. Risken framåt är budgetär snarare än
teknisk: sonden fungerar, men fanns med bland de uppdrag som föreslogs strykas i
budgetförslaget för 2026. Ett förslag är inte en lag, men läget är oavgjort.

Två saker till kom fram och står nu i kodkommentaren, eftersom de rättar en
rimlig men felaktig gissning:

- **Juno kommer inte att styras ned i Jupiter**, till skillnad från Cassini vid
  Saturnus. Det var planen från början, av samma skäl – en sond som slår ned på
  Europa skulle kunna föra med sig jordbakterier till havet under isen – men
  under åren i omloppsbana böjde månarnas dragningskraft banan så mycket att Juno
  till slut inte passerade i närheten av Europa alls. Då fanns inget kvar att
  skydda mot, och den styrda nedstörtningen ströks.
- **Modellens varvräkning stämmer inte med verklighetens.** Appen ritar ett
  representativt varv på 53 dygn, medan riktiga Juno kortade sin omloppstid flera
  gånger efter passagerna av Ganymedes och Io. De 213 extra dygn sonden nu syns
  motsvarar fyra varv i modellens takt, men fler i verkligheten.

**Verifiera:** Juno ska ritas den 1 oktober 2025, som den inte gjorde förut, och
inte längre efter 1 maj 2026.

Kontrollerat utanför appen, 12 kontroller utan fel: Juno ritas från ankomsten
5 juli 2016 till och med 1 maj 2026 och inte dagen efter, banan är oförändrad
(53,42 dygn, e = 0,9815, 57,7 km/s vid perijovium) och Cassini är orörd.

### R3 – Precisionen i den ritade banan ✔

Farkostens bana byggdes Lambert → `Vector3` i enkel precision → `Conic.FromState`,
där energin blir en liten skillnad mellan två stora tal och precisionen tunnas ut.
Månfärden går i stället via `Conic.FromPeriapsis`, som räknar analytiskt i dubbel
precision och slutar några centimeter från månen.

- [x] Lämna över tillståndet från Lambert i dubbel precision, eller låt lösaren
      returnera kägelsnittet direkt.

**Rättat i punkten ovan: det var inte Mars-banan.** Här stod att den slutar
omkring 850 km från Mars. Mätt slutar den *exakt* på Mars – ingen av fyrtio
provade banor avviker mer än flyttalen kan skilja på. Var siffran kom ifrån vet
jag inte; troligen mättes den före den Lambert-baserade omskrivningen av
`Mission.Plan` i etapp 10.

Felet fanns däremot, och på ett annat ställe: **rymdsondernas ben**. Skarven där
två ben möts är samma punkt i rymden sedd från två håll och borde vara noll. Den
var upp till 40 000 km. Skälet är att Mars-banor är beskedliga ellipser medan
sondbenen är nästan paraboliska, och då blir 2/r − v²/µ en skillnad mellan två
nästan lika stora tal. Varje siffra som saknas i indata förstoras hundrafalt i
svaret. Dessutom returnerade `Vector3.LengthSquared()` sitt värde i enkel
precision, så farten tappade siffror redan innan subtraktionen.

Åtgärdat med en `Vec3` i dubbel precision, som bara banmatematiken använder:
`PositionAuAt` på kropparna, Lambert in och ut, `Conic.FromState`, `Waypoint` och
`StarCatalog.EquatorialToWorldAu`. Ritningen är oförändrad – där räcker enkel
precision gott, en pixel är många tusen kilometer.

**Verifiera:** Sondbenens skarvar ska ligga nere vid flyttalens upplösning, och
allt som verifierades i etapp 10 ska stå kvar.

Kontrollerat utanför appen, 8 kontroller utan fel, med koden före ändringen
utcheckad ur git och körd genom samma mätning:

| passage | före | efter |
|---|---|---|
| Voyager 1 vid Jupiter | 610 km | 36 km |
| Voyager 2 vid Jupiter | **20 589 km** | 51 km |
| Voyager 2 vid Saturnus | 802 km | 18 km |
| Pioneer 10 vid Jupiter | 1 084 km | 71 km |
| Pioneer 11 vid Saturnus | **39 984 km** | 37 km |
| New Horizons vid Jupiter | 1 042 km | 71 km |

Sämsta avvikelsen går från 39 984 km till 404 km, och de två grova utstickarna är
borta. Det som är kvar är inte längre modellens fel utan mätningens: skarvarna
ligger på en halv till knappt fyra flyttalssteg, och ett flyttalssteg är 36 km
vid Jupiter och 285 km ute vid Pluto. Under den gränsen finns ingen skillnad att
mäta, och det är också den upplösning appen ritar med.

Allt annat är oförändrat: Voyager 1:s fart i dag 16,66 km/s, slungan vid Jupiter
+10,8 km/s, sexton ben, Mars-fönstren på samma datum till samma kostnad
(3,12 / 3,09 / 3,00 / 2,93 / 3,30 km/s) och månfärden fortfarande exakt.

**Två anteckningar från etapp 10 stämmer inte.** Där står att Mars-fönstren
kostar "2,90–3,20 km/s"; fönstret 2035 kostar 3,30, både före och efter den här
ändringen. Där står också att alla elva passager träffar inom "74–602 km". Det
går inte att återskapa: med koden som den såg ut före ändringen mäter jag
610 till 39 984 km vid passagedatumet, och 610 till 34 214 km om man i stället
tar närmaste punkt längs banan. Vilken metod som gav de gamla talen vet jag inte,
så de får stå som oreproducerade snarare än rättade till något jag gissat.

### R4 – "Verklig storlek" går inte att zooma in i ✔

Kameran kom inte närmare än 1,5 enheter, och `SuggestedFocusDistance` hade
dessutom ett golv på 8. I verklig skala är jordens radie 0,0026 enheter, så när
man valde en planet i fokusväljaren eller följde med farkosten ner vid ankomsten
hamnade man alldeles för långt bort för att se något. Lägena var riktiga – det
var bara kameran som inte räckte till.

- [x] Låt både golvet och minimiavståndet bero på den valda kroppens visuella
      radie i stället för att vara fasta tal.

`OrbitCamera.MinDistance` är inte längre en konstant utan en egenskap som sätts
varje bildruta ur den kropp kameran tittar på: dess ritade radie gånger 1,15, så
att man kommer nära utan att hamna inuti. Avståndet kläms om direkt när gränsen
ändras, så ett byte mellan lägena aldrig lämnar kameran inne i en planet. Golvet
i `SuggestedFocusDistance` följer också kroppen. En farkost eller sond är en
punkt utan utsträckning och får komma hur nära som helst.

Kvar finns bara ett absolut golv på 0,001 enheter, och det är inte satt av någon
kropp utan av flyttalen: världskoordinaterna är enkel precision och når 2 400
enheter ute vid Neptunus, där steget mellan två närliggande tal är ett par
tiotusendels enhet.

**Verifiera:** I "Verklig storlek", välj en planet och zooma in – ytan ska gå att
se. Väx mellan lägena medan man är inzoomad och se att kameran inte hamnar inuti.

Kontrollerat utanför appen, 33 kontroller utan fel:

- **Verklig skala fungerar nu.** Väljer man Merkurius går den från 0,1 pixlar
  till 80. Jorden ger 5,9 pixlar vid valet – vyn ramar in månbanan, som den ska –
  och går att zooma till 839. Förut var 2 pixlar det närmaste man kom, oavsett
  hur mycket man skrollade.
- **Alla kroppar med ytkarta går att zooma till en glob i båda lägena**, mot
  tröskeln på 14 pixlar. Vid minsta avstånd fyller klotet 839 pixlar oberoende av
  både läge och kropp, eftersom gränsen är proportionell mot radien.
- **Förstorat läge är oförändrat** för åtta av nio planeter. Pluto kommer
  närmare, 57 pixlar blir 80: den var den enda planet som golvet på åtta enheter
  faktiskt höll tillbaka.
- **En bugg till föll ut på köpet.** Det gamla golvet på 1,5 enheter låg *innanför*
  flera kroppar i förstorat läge – jorden ritas med radien 2,6, Jupiter 28,0 och
  solen 8,4 – så man kunde zooma in i dem och se dem inifrån. Nu ligger gränsen
  alltid utanför ytan.

De minsta kropparna i verklig skala – Pluto, månen, Io – möter det absoluta
golvet innan de möter sitt eget och stannar på 460 till 705 pixlar i stället för
839. Långt över vad som behövs.

### R5 – Ett olösbart sondben hoppas över under tystnad ✔

`Probe.Build` hoppade över ett ben som Lambert inte klarar och byggde sonden av
resten. Alla ben går att lösa i dag, och det kontrolleras utanför appen, men lade
någon in ett omöjligt datumpar skulle sonden tyst få en lucka i banan i stället
för att felet syntes.

- [x] Låt ett olösbart ben märkas – returnera null från `Build`, eller åtminstone
      skriv till samma logg som ritfel går till.

Valet blev det senare, av två skäl. `Build` anropas ur statiska fält i
`ProbeData`, så null hade tvingat null-kontroller genom hela kedjan – och ett
undantag där hade fällt hela appen vid start för ett fel i data. Dessutom är en
sond med en lucka mer användbar än ingen sond alls. Men felet passerar inte
längre obemärkt: överhoppade ben skrivs till loggen och samlas i
`Probe.SkippedLegs`, så att provprogrammen utanför appen kan kräva att listan är
tom. Meddelandet säger vilken sond, vilka två passager, hur många dygn och varför.

Loggsökvägen låg inbakad i ritkoden. Den ligger nu i `Simulation/Diagnostics`,
som båda använder – det var förutsättningen för att alls kunna skriva "till samma
logg".

**Rättat i texten ovan:** här stod "alla tretton ben". Det är fel, och det syntes
när provet räknade dem: sonderna har **sexton** ben. Voyager 1 har tre, Voyager 2
fem, Pioneer 10 två, Pioneer 11 tre och New Horizons tre. Tretton var antagligen
en räkning som missade de avslutande benen ut mot dagens läge.

**Verifiera:** Riktig sonddata ska ge en tom lista och ingen logg alls.

Kontrollerat utanför appen, 12 kontroller utan fel:

- **Riktig data går ren.** Fem sonder, sexton ben, tom lista, ingen loggfil ens
  skapad. Voyager 1:s fart i dag är oförändrad 16,66 km/s och de elva
  planetpassagerna finns kvar.
- **Båda felgrenarna är faktiskt utlösta**, inte bara lästa. Passager i fel
  ordning ger "hoppades över (−365 dygn) – passagerna kommer inte i tidsordning",
  och sonden får noll ben och existerar därmed inte. Två punkter rakt emot
  varandra sett från solen – där banplanet är obestämt och varje plan duger lika
  bra – får Lambert att gå bet på riktigt, och det märks likadant.
- **Raderna hamnar i loggen**, i samma fil som ritfel.

**Vad vakten inte fångar.** Ett ben som är orimligt men matematiskt lösbart går
igenom: Jupiter till Saturnus på ett dygn ger en bana i 9 147 km/s, och Lambert
löser den utan att klaga. Vakten letar efter olösliga ben, inte efter orimliga.
Att sålla på rimlighet vore ett annat prov – en övre gräns för farten skulle
duga – men det är inte det den här punkten handlade om, och en gräns satt på
måfå riskerar att sålla bort riktig data.
