using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En kropps rotationsaxel och dess rotation kring den.
///
/// Axeln beskrivs på samma sätt som ett banplan: lutningen mot ekliptikan och
/// den uppstigande nodens longitud. Det är med flit – planetens ekvatorsplan är
/// just det plan som dess regelbundna månar och ringar ligger i, så samma två
/// tal beskriver både var polen pekar och hur månarnas banor lutar.
///
/// <b>Nordpolen räknas efter högerhandsregeln</b>, alltså åt det håll axeln
/// pekar när kroppen snurrar moturs sett uppifrån. För Venus, Uranus och Pluto
/// pekar den söderut om ekliptikan, och det är precis vad det innebär att de
/// roterar baklänges: lutningen blir större än 90 grader. Alternativet vore att
/// alltid låta polen peka norrut och i stället ge dem negativ rotationstid, men
/// då hade månarnas banplan behövt vändas om innan de kunde användas. Nu kan
/// Miranda läsa Uranus axel rakt av.
/// </summary>
/// <param name="InclinationDeg">
/// Ekvatorsplanets lutning mot ekliptikan. Över 90 grader betyder retrograd
/// rotation. Observera att detta inte är samma sak som den axellutning som
/// brukar stå i tabeller – den räknas mot kroppens egen bana. Merkurius står
/// upprätt mot sin bana (0,03°) men lutar 7,0° mot ekliptikan, eftersom banan
/// själv lutar 7,0°.
/// </param>
/// <param name="NodeDeg">
/// Longituden för ekvatorns uppstigande nod på ekliptikan, alltså åt vilket
/// håll kroppen lutar.
/// </param>
/// <param name="RotationDays">
/// Ett varv kring axeln, i dygn. Alltid positivt: riktningen ligger redan i
/// polens läge.
/// </param>
/// <param name="PrimeMeridianDeg">
/// Nollmeridianens vinkel från den uppstigande noden vid epoken J2000, räknad
/// åt samma håll som kroppen snurrar. Det är detta tal som avgör vilken sida
/// som är vänd mot solen ett givet datum.
/// </param>
public sealed record BodyAxis(
    double InclinationDeg,
    double NodeDeg,
    double RotationDays,
    double PrimeMeridianDeg)
{
    /// <summary>
    /// Hur mycket långsammare ytan vrider sig ju längre från ekvatorn man
    /// kommer, i grader per dygn. Noll – standard – betyder att kroppen roterar
    /// som ett stycke, vilket allt fast gör.
    ///
    /// Solen är den enda kropp i appen där talet inte är noll, och det är i sig
    /// en upptäckt värd något: en kropp som roterar olika fort på olika
    /// breddgrader kan omöjligt vara fast. Takten följer <c>ω(φ) = A + B·sin²φ</c>,
    /// där A är ekvatorns takt (360 grader delat med <see cref="RotationDays"/>)
    /// och B är talet här.
    /// </summary>
    public double DifferentialDegPerDay { get; init; }

    readonly double _sinI = Math.Sin(InclinationDeg * Math.PI / 180.0);
    readonly double _cosI = Math.Cos(InclinationDeg * Math.PI / 180.0);
    readonly double _sinNode = Math.Sin(NodeDeg * Math.PI / 180.0);
    readonly double _cosNode = Math.Cos(NodeDeg * Math.PI / 180.0);

    /// <summary>Rotationsaxeln i världskoordinater (Y = norr om ekliptikan).</summary>
    public Vector3 NorthPole => new(
        (float)(_sinI * _sinNode), (float)_cosI, (float)(_sinI * _cosNode));

    /// <summary>
    /// Punkten på ekvatorn vid den uppstigande noden. Ligger per definition i
    /// ekvatorsplanet och vinkelrätt mot axeln, så den duger som nollriktning
    /// när ytan ska ritas – och som första basvektor för ringarna.
    /// </summary>
    public Vector3 NodeAxis => new((float)_cosNode, 0f, (float)(-_sinNode));

    /// <summary>Ekvatorsplanets andra basvektor, ett kvarts varv öster om noden.</summary>
    public Vector3 EastAxis => new(
        (float)(-_cosI * _sinNode), (float)_sinI, (float)(-_cosI * _cosNode));

    /// <summary>
    /// Hur långt nollmeridianen har vridit sig från noden vid given tid, i
    /// radianer. Vinkeln viks inte ned till ett varv: Jupiter hinner nittio
    /// miljoner grader på ett sekel, vilket ett dubbelt flyttal bär med god
    /// marginal.
    /// </summary>
    public double SpinRadians(double daysSinceJ2000)
        => (PrimeMeridianDeg + 360.0 * daysSinceJ2000 / RotationDays) * Math.PI / 180.0;

    /// <summary>
    /// Vridningen på en viss breddgrad. Samma sak som varianten ovan för allt
    /// som roterar som ett stycke; för solen är det den här som gäller.
    /// </summary>
    public double SpinRadians(double daysSinceJ2000, double sinLat)
    {
        if (DifferentialDegPerDay == 0.0)
            return SpinRadians(daysSinceJ2000);

        double rate = 360.0 / RotationDays + DifferentialDegPerDay * sinLat * sinLat;
        return (PrimeMeridianDeg + rate * daysSinceJ2000) * Math.PI / 180.0;
    }

    /// <summary>
    /// Riktningen från kroppens medelpunkt ut till en punkt på ytan, uttryckt i
    /// världskoordinater. Longituden räknas åt det håll ytan vrider sig, vilket
    /// för alla kroppar som roterar rättvänt är detsamma som östlig longitud.
    /// </summary>
    public Vector3 Direction(double sinLat, double cosLat, double lonRad, double spinRad)
    {
        double a = spinRad + lonRad;
        double u = cosLat * Math.Cos(a);   // längs noden
        double v = cosLat * Math.Sin(a);   // österut
        return new Vector3(
            (float)(u * _cosNode - v * _cosI * _sinNode + sinLat * _sinI * _sinNode),
            (float)(v * _sinI + sinLat * _cosI),
            (float)(-u * _sinNode - v * _cosI * _cosNode + sinLat * _sinI * _cosNode));
    }
}
