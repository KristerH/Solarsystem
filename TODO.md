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
- [x] Att kameran kan följa med ner till planeten vid ankomst återstår.

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

- [ ] En väljare där varje sond kan bockas i för sig, i stället för dagens
      kryssruta "Rymdsonder" som tänder och släcker allihop. Man ska kunna visa
      bara Voyager 1, eller Voyager 1 och 2, eller vilken annan kombination som
      helst.
- [ ] Valet ska gälla sondens allt: pricken, spåret och milstolparna.
- [ ] Släcks den sond som är vald i fokusväljaren faller fokus tillbaka till
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

**Verifiera:** Bocka i bara Voyager 1 och kontrollera att spår, prick och
milstolpar för de andra fyra försvinner, medan Voyager 1 ser ut precis som förut.
Bocka i båda Voyagersonderna och luta kameran: nu ska de två motsatta vägarna ut
ur ekliptikan gå att jämföra utan att Pioneersonderna och New Horizons ligger i
vägen. Bocka ur allihop och jämför med hur vyn ser ut när dagens kryssruta
"Rymdsonder" släcks – det ska vara samma bild.

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

- [ ] Bryt ut `DrawEarthGlobe` till något alla kroppar kan använda, och
      `EarthMap` till en ytkarta per kropp.
- [ ] Lägg rotationsdata på `CelestialBody`: rotationsperiod (negativ för
      retrograd) och nollmeridianens läge vid epoken. Polriktningarna finns
      redan inlagda för Mars, Jupiter, Saturnus, Uranus, Neptunus och Pluto
      sedan månetapperna – de ger axelläget gratis.
- [ ] Samma tröskel som i dag: ytan ritas först när klotet är stort nog.

**Verifiera:** Jorden ska se ut och rotera exakt som före ombyggnaden.

### 11.2 – Mars

- [ ] Rödbrun yta med de mörka områdena (Syrtis Major är det tydligaste),
      vita polarisar och gärna Valles Marineris som ett streck.
- [ ] Rotation 24 h 37 min, nästan som jordens.

**Verifiera:** Polarisarna ska ligga still medan ytan snurrar under dem.

### 11.3 – Jupiter

- [ ] Molnband i latitud, ljusa zoner och mörka bälten.
- [ ] Stora röda fläcken som en oval på södra halvklotet.
- [ ] Rotation på bara 9 h 55 min – snabbast i solsystemet trots att den är
      störst. Går utmärkt att se i appen på låg hastighet.

**Verifiera:** Röda fläcken ska försvinna runt kanten och komma tillbaka på
andra sidan efter ungefär fem timmar simulerad tid.

### 11.4 – Saturnus, Uranus och Neptunus

- [ ] Saturnus: svagare band än Jupiters, i gulbeige. Rotation 10 h 39 min.
- [ ] Uranus: nästan enfärgat blågrönt – poängen är att den är så slät.
      Rotation 17 h 14 min, retrograd och på sidan.
- [ ] Neptunus: blå med Stora mörka fläcken. Rotation 16 h 06 min.

**Verifiera:** Uranus ska snurra kring en axel som ligger nästan i banplanet,
så att ytan rullar i stället för att snurra.

### 11.5 – Merkurius och Venus

- [ ] Merkurius: grå och kraterrik, mycket lik månen. Rotation 58,6 dygn –
      exakt tre varv på två av sina år, en 3:2-resonans med solen.
- [ ] Venus: inget av ytan syns, bara ett jämnt gulvitt moltack. Rotation
      243 dygn **baklänges**, alltså längre än dess år på 225 dygn.

**Verifiera:** Venus ska snurra åt motsatt håll mot alla andra planeter, och
så långsamt att man behöver skruva upp hastigheten för att se det.

### 11.6 – Månen och Pluto

- [ ] Månen: grå med de mörka haven (Mare Imbrium, Mare Tranquillitatis där
      Apollo 11 landade) och ljusa kraterstrålar kring Tycho.
- [ ] **Bunden rotation**: månen roterar exakt ett varv per omloppsbana och
      vänder därför alltid samma sida mot jorden. Det är en av de bästa
      poängerna i hela appen att kunna visa.
- [ ] Pluto: Tombaugh Regio, det ljusa hjärtformade området som New Horizons
      fotograferade 2015. Rotation 6,4 dygn, bunden till Charon.

**Verifiera:** Zooma in på jorden och följ månen ett helt varv – samma sida
ska vara vänd mot jorden hela tiden.

---

## Etapp 12 – Språkstöd (inför publik release)

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
  dag, men samlas gärna på få ställen i koden så att etapp 12 (språkstöd)
  blir enkel att genomföra.
