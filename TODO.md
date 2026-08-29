# Att göra: månar, ringar och bälten

Planen är att lägga till en eller några punkter i taget, så att varje steg
hinner verifieras innan nästa påbörjas. Bocka av med `[x]` när en etapp är
klar och godkänd.

**Klart hittills:** Jordens måne (etapp 0) är redan inlagd och verifierad,
liksom Saturnus ringar. De fungerar som mall för resten.

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

- [ ] Ett diskret band av små prickar mellan Mars (1,52 AU) och Jupiter
      (5,20 AU), tätast kring 2,2–3,3 AU. Slumpade banor med spridning i
      lutning (± ett par grader) och excentricitet, som roterar med
      keplerska hastigheter (inre varvet snabbare än yttre).
- [ ] Kryssruta "Visa asteroidbältet" (av som standard, så att vyn inte blir
      plottrig).
- [ ] Eventuellt: dvärgplaneten Ceres som namngiven prick i bältet.

**Verifiera:** Bältet ska se glest ut även i appen – en pedagogisk poäng är
att asteroidbältet i verkligheten mest är tomrum (rymdsonder flyger igenom
utan problem). Prestanda: ingen märkbar försämring vid rotation/zoom
(punkterna cachas som stjärnhimlen).

---

## Etapp 8 – Kuiperbältet (runt solen, bortom Neptunus)

- [ ] Glest band av isprickar ca 30–50 AU, med större spridning i lutning
      än asteroidbältet. Pluto ligger mitt i det – bra att kunna visa.
- [ ] Samma kryssruta som asteroidbältet eller en egen.

**Verifiera:** Zooma ut och luta kameran: Kuiperbältet ska vara tjockare/
"luddigare" i höjdled än asteroidbältet, och Plutos lutande bana ska ligga
inom dess svärm.

---

## Etapp 9 – Språkstöd (inför publik release)

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

## Etapp 10 – Rymdfärd till Mars eller månen

En egen liten rymdfärd: eleverna väljer mål, skjuter upp en farkost och följer
den hela vägen fram. Här möts allt appen redan kan – banor, tid och skala –
i något eleverna själva styr.

- [ ] **Ställ om vilket datum man befinner sig på**. Detta behövs först, och
      är användbart även utan rymdfärder: man ska kunna hoppa till vilket datum
      som helst, både bakåt och framåt, i stället för att som i dag alltid
      starta på dagens datum och bara kunna gå framåt.
      - [ ] Ett datumfält där man skriver in år, månad och dag, och vyn hoppar dit.
      - [ ] Knappar för att stega ± dag, ± månad och ± år, så att man kan bläddra
            sig fram utan att skriva.
      - [ ] Knappen "Idag" som återställer till nuet.
      - [ ] Låt hastighetsreglaget kunna gå bakåt, så att tiden kan spelas baklänges.
      Kepler-matematiken klarar redan negativ tid, så arbetet ligger i
      gränssnittet och i klockan (`_startDate` plus `_simDays` i `MainPage`).
      Detta är också grunden för "Hoppa till nästa startfönster" nedan, och
      behövs i etapp 11 för att spola tillbaka till Voyagers uppskjutning 1977.
      Rolig bieffekt: eleverna kan slå upp sin egen födelsedag och se var
      planeterna stod då.

- [ ] **Farkosten som himlakropp**: en liten prick med namn och ett spår efter
      sig (de senaste par hundra positionerna), som följer en Kepler-bana precis
      som planeterna. Ingen ny banmatematik behövs – samma `CelestialBody`.
- [ ] **Hohmann-bana till Mars**: den energisnålaste vägen är en halv ellips
      med perihelium vid jordens bana (1,00 AU) och aphelium vid Mars
      (1,52 AU). Halva storaxeln blir då 1,26 AU, vilket ger en restid på
      ungefär 259 dygn – hälften av den banans omloppstid.
- [ ] **Startfönster**: farkosten måste skjutas upp när Mars ligger 44,3°
      framför jorden. Under de 259 dygnen hinner Mars nämligen bara 135,7° av
      sitt varv, medan farkosten går 180° – och 44,3 + 135,7 = 180. Ligger Mars
      fel vid uppskjutningen anländer farkosten till tom rymd: 20° fel motsvarar
      80 miljoner km. Läget upprepas var 780:e dygn (25,6 månader), vilket är
      varför verkliga Mars-uppdrag alltid skjuts upp i klungor – sommaren 2020
      skickade USA, Kina och Förenade arabemiraten var sin sond inom två veckor,
      och sedan hände ingenting på två år. Knappen "Skjut upp" bör vara inaktiv
      däremellan, med "Hoppa till nästa startfönster" bredvid.
- [ ] **Färd till månen**: samma sak fast kring jorden – en ellips från låg
      omloppsbana ut till månens avstånd, restid ca 3 dygn. Kräver att
      farkosten kan kretsa kring en planet i stället för kring solen, ungefär
      som månarna gör i dag. Bra kontrast till Mars: månen är tillbaka på samma
      ställe var 27:e dygn, så dit kan man åka i stort sett när som helst.
- [ ] **Panel under färden**: förfluten restid, återstående tid, avstånd kvar
      till målet och farkostens fart.
- [ ] **Ankomst**: farkosten möter målet, färden markeras som avslutad och
      kameran kan följa med ner till planeten.

**Verifiera:** Restiden till Mars ska bli ungefär 259 dygn, och farkosten ska
faktiskt möta Mars – inte komma fram till den punkt där Mars *var* vid
uppskjutningen. Testa gärna att skjuta upp vid fel tidpunkt om det går: då ska
farkosten anlända till tom rymd, vilket är hela poängen med startfönster.
Månfärden ska ta ca 3 dygn.

---

## Etapp 11 – Voyager och de andra rymdsonderna

De farkoster mänskligheten faktiskt har skickat ut. Etapp 10 handlar om en
påhittad resa som eleven själv styr; den här handlar om de verkliga färderna,
med riktiga datum. Voyager 1 är det avlägsnaste föremål människan har byggt.

- [ ] **Hyperboliska banor i Kepler-koden**. Sonderna har fått så hög fart att
      de aldrig kommer tillbaka: deras banor har excentricitet större än 1 och
      är alltså hyperbler, inte ellipser. `SolveKepler` löser i dag bara
      `E - e*sin E = M`, som gäller för ellipser. För hyperbler behövs
      `e*sinh H - H = M` och en egen positionsformel. Detta är etappens enda
      riktiga matematikarbete och bör göras först.
- [ ] **Voyager 1 och Voyager 2** (uppskjutna 1977) med verkliga riktningar och
      farter. Voyager 2 är den enda farkost som besökt alla fyra
      jätteplaneterna – möjligt tack vare en planetuppställning som bara
      återkommer vart 176:e år.
- [ ] **Pioneer 10 och 11 samt New Horizons** (Pluto 2015). Fem farkoster är
      på väg ut ur solsystemet.
- [ ] **Planetpassagerna som milstolpar** med datum, t.ex. Voyager 1 vid
      Jupiter i mars 1979 och vid Saturnus i november 1980, Voyager 2 vid
      Neptunus i augusti 1989. Kan visas som markerade punkter längs banan.
- [ ] **Gravitationsslunga**: sonderna fick fart genom att svänga förbi
      planeterna. Visa farten i en panel så att hoppen vid varje passage syns –
      det är själva förklaringen till hur de kunde nå så långt.
- [ ] **Ut ur ekliptikan**: efter Saturnus böjde Voyager 1 av brant uppåt
      (ca 35°) och Voyager 2 nedåt (ca 48°). Bra tillfälle att visa att
      solsystemet är en skiva som sonderna nu lämnat.
- [ ] **Kretsande sonder** som Cassini vid Saturnus (1997–2017) och Juno vid
      Jupiter – enklare fall, vanliga ellipser kring en planet.
- [ ] **Skalan**: sonderna är i dag över 100 AU bort, tre gånger längre än
      Neptunus. Kameran måste kunna zooma ut så långt, och då krymper hela
      planetsystemet till en prick – vilket i sig är poängen.

**Verifiera:** Spola tiden till mars 1979 – Voyager 1 ska då vara vid Jupiter,
inte någon annanstans. Samma sak för Voyager 2 vid Neptunus i augusti 1989.
Kontrollera dagens avstånd mot NASA:s siffror (Voyager 1 ligger kring 167 AU
och Voyager 2 kring 140 AU år 2026). Luta kameran och se att de två Voyager-
sonderna lämnat ekliptikan åt var sitt håll.

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
- **Nya texter under etapp 2–8:** skrivs tills vidare på svenska som i dag,
  men samlas gärna på få ställen i koden så att etapp 9 (språkstöd) blir
  enkel att genomföra.
