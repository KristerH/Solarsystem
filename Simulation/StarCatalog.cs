using System.Numerics;

namespace Solarsystem.Simulation;

/// <summary>
/// En stjärna med verkliga koordinater (rektascension/deklination, epok J2000),
/// skenbar magnitud och färgindex B-V.
/// </summary>
public sealed record Star(
    string Id,
    string? ProperName,
    double RaHours,
    double DecDeg,
    double Magnitude,
    double ColorIndex)
{
    /// <summary>Riktning i världskoordinater, samma system som planeternas banor.</summary>
    public Vector3 Direction { get; } = StarCatalog.EquatorialToWorld(RaHours, DecDeg);
}

/// <summary>En stjärnbild: svenskt namn och de linjer som binder ihop figuren.</summary>
public sealed record Constellation(string Name, (string A, string B)[] Lines);

/// <summary>
/// Ljusstarka stjärnor ur Yale Bright Star-katalogen samt figurlinjer för de
/// stjärnbilder som är lättast att känna igen. Positionerna är verkliga, så
/// stjärnhimlen stämmer med den man ser ute – och eftersom stjärnorna räknas om
/// från ekvatorial- till ekliptikakoordinater hamnar de rätt i förhållande till
/// planeternas banplan (planeterna rör sig genom zodiakens stjärnbilder).
/// </summary>
public static class StarCatalog
{
    /// <summary>Ekliptikans lutning vid J2000.</summary>
    const double ObliquityDeg = 23.4392911;

    /// <summary>Galaktiska nordpolen (J2000) – Vintergatans plan står vinkelrätt mot den.</summary>
    public static readonly Vector3 GalacticNorthPole = EquatorialToWorld(192.85948 / 15.0, 27.12825);

    /// <summary>Galaktiska centrum i Skytten – Vintergatan är som tätast åt det hållet.</summary>
    public static readonly Vector3 GalacticCenter = EquatorialToWorld(266.40510 / 15.0, -28.93617);

    /// <summary>
    /// Ekvatorialkoordinater (J2000) till samma världssystem som planeterna använder:
    /// först rotation till ekliptiska koordinater, sedan Y = norr om ekliptikan.
    /// </summary>
    public static Vector3 EquatorialToWorld(double raHours, double decDeg)
    {
        double ra = raHours * 15.0 * Math.PI / 180.0;
        double dec = decDeg * Math.PI / 180.0;
        double eps = ObliquityDeg * Math.PI / 180.0;

        // Enhetsvektor i ekvatorialsystemet.
        double xq = Math.Cos(dec) * Math.Cos(ra);
        double yq = Math.Cos(dec) * Math.Sin(ra);
        double zq = Math.Sin(dec);

        // Rotation kring vårdagjämningsaxeln -> ekliptiska koordinater.
        double xe = xq;
        double ye = yq * Math.Cos(eps) + zq * Math.Sin(eps);
        double ze = -yq * Math.Sin(eps) + zq * Math.Cos(eps);

        // Samma avbildning som planeternas banor: ekliptikan horisontell, norr uppåt.
        return Vector3.Normalize(new Vector3((float)xe, (float)ze, (float)-ye));
    }

    public static readonly Star[] Stars =
    [
        // --- Orion
        new("Betelgeuse", "Betelgeuse", 5.9195, 7.407, 0.50, 1.85),
        new("Rigel", "Rigel", 5.2423, -8.202, 0.13, -0.03),
        new("Bellatrix", "Bellatrix", 5.4188, 6.350, 1.64, -0.22),
        new("Mintaka", "Mintaka", 5.5334, -0.299, 2.23, -0.18),
        new("Alnilam", "Alnilam", 5.6036, -1.202, 1.69, -0.18),
        new("Alnitak", "Alnitak", 5.6793, -1.943, 1.77, -0.20),
        new("Saiph", "Saiph", 5.7959, -9.670, 2.06, -0.17),
        new("Meissa", null, 5.5855, 9.934, 3.39, -0.16),

        // --- Stora hunden / Lilla hunden
        new("Sirius", "Sirius", 6.7525, -16.716, -1.46, 0.00),
        new("Mirzam", "Mirzam", 6.3783, -17.956, 1.98, -0.24),
        new("Adhara", "Adhara", 6.9770, -28.972, 1.50, -0.21),
        new("Wezen", "Wezen", 7.1399, -26.393, 1.83, 0.67),
        new("Aludra", null, 7.4014, -29.303, 2.45, -0.08),
        new("IotaCMa", null, 6.9337, -17.054, 4.37, -0.06),
        new("Procyon", "Procyon", 7.6551, 5.225, 0.34, 0.42),
        new("Gomeisa", null, 7.4527, 8.289, 2.89, -0.09),

        // --- Oxen
        new("Aldebaran", "Aldebaran", 4.5987, 16.509, 0.85, 1.54),
        new("Elnath", "Elnath", 5.4381, 28.608, 1.65, -0.13),
        new("Alcyone", "Plejaderna", 3.7914, 24.105, 2.87, -0.09),
        new("ZetaTau", null, 5.6274, 21.143, 3.00, -0.15),
        new("GammaTau", null, 4.3299, 15.628, 3.65, 0.99),
        new("DeltaTau", null, 4.3820, 17.542, 3.76, 0.98),
        new("EpsilonTau", null, 4.4776, 19.180, 3.53, 1.01),
        new("Theta2Tau", null, 4.4784, 15.871, 3.40, 0.18),

        // --- Tvillingarna
        new("Castor", "Castor", 7.5766, 31.888, 1.58, 0.03),
        new("Pollux", "Pollux", 7.7553, 28.026, 1.14, 1.00),
        new("Alhena", "Alhena", 6.6285, 16.399, 1.93, 0.00),
        new("DeltaGem", null, 7.3353, 21.982, 3.53, 0.37),
        new("Mebsuta", null, 6.7320, 25.131, 2.98, 1.40),
        new("Mekbuda", null, 7.0685, 20.570, 3.79, 0.79),
        new("Tejat", null, 6.3826, 22.514, 2.88, 1.64),
        new("Propus", null, 6.2489, 22.507, 3.28, 1.60),
        new("UpsilonGem", null, 7.5960, 26.896, 4.06, 1.54),
        new("TauGem", null, 7.1861, 30.245, 4.41, 1.26),
        new("LambdaGem", null, 7.4287, 16.540, 3.58, 0.11),

        // --- Kusken
        new("Capella", "Capella", 5.2782, 45.998, 0.08, 0.80),
        new("Menkalinan", null, 5.9922, 44.947, 1.90, 0.08),
        new("ThetaAur", null, 5.9953, 37.213, 2.62, -0.08),
        new("IotaAur", null, 4.9497, 33.166, 2.69, 1.53),
        new("EpsilonAur", null, 5.0328, 43.823, 2.99, 0.54),

        // --- Lejonet
        new("Regulus", "Regulus", 10.1395, 11.967, 1.35, -0.11),
        new("Denebola", "Denebola", 11.8177, 14.572, 2.14, 0.09),
        new("Algieba", "Algieba", 10.3329, 19.841, 2.08, 1.13),
        new("Zosma", null, 11.2351, 20.524, 2.56, 0.13),
        new("Chort", null, 11.2373, 15.430, 3.32, 0.00),
        new("EtaLeo", null, 10.1222, 16.763, 3.48, -0.03),
        new("Adhafera", null, 10.2785, 23.417, 3.44, 0.31),
        new("Rasalas", null, 9.8792, 26.007, 3.88, 1.22),
        new("Algenubi", null, 9.7642, 23.774, 2.98, 0.80),

        // --- Stora björnen (Karlavagnen)
        new("Dubhe", "Dubhe", 11.0622, 61.751, 1.79, 1.07),
        new("Merak", "Merak", 11.0307, 56.383, 2.37, 0.03),
        new("Phecda", null, 11.8972, 53.695, 2.44, 0.04),
        new("Megrez", null, 12.2571, 57.033, 3.31, 0.08),
        new("Alioth", "Alioth", 12.9005, 55.960, 1.77, -0.02),
        new("Mizar", "Mizar", 13.3988, 54.925, 2.27, 0.02),
        new("Alkaid", "Alkaid", 13.7923, 49.313, 1.86, -0.19),

        // --- Lilla björnen
        new("Polaris", "Polstjärnan", 2.5303, 89.264, 1.98, 0.60),
        new("Kochab", "Kochab", 14.8451, 74.156, 2.08, 1.47),
        new("Pherkad", null, 15.3455, 71.834, 3.05, 0.05),
        new("Yildun", null, 17.5369, 86.586, 4.35, 0.02),
        new("EpsilonUMi", null, 16.7661, 82.037, 4.21, 0.89),
        new("ZetaUMi", null, 15.7343, 77.794, 4.29, 0.04),
        new("EtaUMi", null, 16.2919, 75.755, 4.95, 0.36),

        // --- Cassiopeia
        new("Schedar", "Schedar", 0.6751, 56.537, 2.24, 1.17),
        new("Caph", "Caph", 0.1530, 59.150, 2.28, 0.34),
        new("GammaCas", null, 0.9451, 60.717, 2.47, -0.15),
        new("Ruchbah", null, 1.4303, 60.235, 2.68, 0.13),
        new("Segin", null, 1.9066, 63.670, 3.38, -0.15),

        // --- Cepheus
        new("Alderamin", null, 21.3097, 62.586, 2.44, 0.22),
        new("Alfirk", null, 21.4776, 70.561, 3.23, -0.22),
        new("GammaCep", null, 23.6558, 77.632, 3.21, 1.03),
        new("DeltaCep", null, 22.4767, 58.415, 4.07, 0.66),
        new("ZetaCep", null, 22.1811, 58.201, 3.35, 1.56),
        new("IotaCep", null, 22.8281, 66.201, 3.52, 1.05),

        // --- Draken
        new("Eltanin", "Eltanin", 17.9434, 51.489, 2.23, 1.52),
        new("Rastaban", null, 17.5072, 52.301, 2.79, 0.95),
        new("XiDra", null, 17.8926, 56.873, 3.75, 1.18),
        new("DeltaDra", null, 19.2093, 67.661, 3.07, 1.00),
        new("ZetaDra", null, 17.1465, 65.715, 3.17, -0.12),
        new("EtaDra", null, 16.3999, 61.514, 2.73, 0.91),
        new("IotaDra", null, 15.4155, 58.966, 3.29, 1.16),

        // --- Svanen
        new("Deneb", "Deneb", 20.6906, 45.280, 1.25, 0.09),
        new("Albireo", "Albireo", 19.5121, 27.960, 3.08, 1.09),
        new("Sadr", "Sadr", 20.3705, 40.257, 2.23, 0.68),
        new("GienahCyg", null, 20.7702, 33.970, 2.46, 1.02),
        new("DeltaCyg", null, 19.7495, 45.131, 2.87, -0.03),
        new("EtaCyg", null, 19.9484, 35.083, 3.89, 1.02),

        // --- Lyran
        new("Vega", "Vega", 18.6156, 38.784, 0.03, 0.00),
        new("Sheliak", null, 18.8347, 33.363, 3.52, 0.00),
        new("Sulafat", null, 18.9824, 32.690, 3.24, -0.05),
        new("ZetaLyr", null, 18.7461, 37.605, 4.36, 0.19),
        new("Delta2Lyr", null, 18.9111, 36.899, 4.30, 1.68),

        // --- Örnen
        new("Altair", "Altair", 19.8464, 8.868, 0.77, 0.22),
        new("Tarazed", null, 19.7709, 10.613, 2.72, 1.52),
        new("Alshain", null, 19.9214, 6.407, 3.71, 0.86),
        new("DeltaAql", null, 19.4247, 3.115, 3.36, 0.32),
        new("ZetaAql", null, 19.0904, 13.863, 2.99, 0.01),
        new("ThetaAql", null, 20.1882, -0.821, 3.23, -0.07),
        new("LambdaAql", null, 19.1041, -4.882, 3.43, -0.09),
        new("EtaAql", null, 19.8735, 1.006, 3.90, 0.79),

        // --- Herkules
        new("Rasalgethi", null, 17.2443, 14.390, 3.06, 1.44),
        new("Kornephoros", null, 16.5036, 21.490, 2.77, 0.94),
        new("ZetaHer", null, 16.6881, 31.603, 2.81, 0.65),
        new("PiHer", null, 17.2506, 36.809, 3.16, 1.44),
        new("EtaHer", null, 16.7147, 38.922, 3.53, 0.92),
        new("EpsilonHer", null, 17.0048, 30.926, 3.92, 0.00),
        new("DeltaHer", null, 17.2504, 24.839, 3.12, 0.08),

        // --- Björnvaktaren / Norra kronan
        new("Arcturus", "Arcturus", 14.2610, 19.182, -0.05, 1.23),
        new("Izar", null, 14.7498, 27.074, 2.37, 0.97),
        new("Muphrid", null, 13.9114, 18.398, 2.68, 0.58),
        new("Seginus", null, 14.5340, 38.308, 3.03, 0.19),
        new("Nekkar", null, 15.0322, 40.390, 3.49, 0.97),
        new("DeltaBoo", null, 15.2582, 33.315, 3.47, 0.95),
        new("RhoBoo", null, 14.5307, 30.371, 3.58, 1.30),
        new("Alphecca", "Alphecca", 15.5781, 26.715, 2.23, -0.02),
        new("BetaCrB", null, 15.4637, 29.106, 3.66, 0.28),
        new("GammaCrB", null, 15.7113, 26.296, 3.81, 0.03),
        new("DeltaCrB", null, 15.8258, 26.068, 4.57, 0.80),
        new("EpsilonCrB", null, 15.9599, 26.878, 4.13, 1.23),
        new("ThetaCrB", null, 15.5488, 31.359, 4.14, -0.13),
        new("IotaCrB", null, 16.0244, 29.851, 4.96, 0.05),

        // --- Jungfrun
        new("Spica", "Spica", 13.4199, -11.161, 0.98, -0.24),
        new("Porrima", null, 12.6944, -1.449, 2.74, 0.36),
        new("Vindemiatrix", null, 13.0362, 10.959, 2.83, 0.94),
        new("Zavijava", null, 11.8446, 1.765, 3.59, 0.52),
        new("Auva", null, 12.9266, 3.398, 3.38, 1.58),
        new("Heze", null, 13.5786, -0.596, 3.37, 0.11),
        new("EtaVir", null, 12.3325, -0.667, 3.89, 0.02),

        // --- Skorpionen
        new("Antares", "Antares", 16.4901, -26.432, 1.09, 1.83),
        new("Shaula", "Shaula", 17.5601, -37.104, 1.62, -0.23),
        new("Sargas", "Sargas", 17.6220, -42.998, 1.86, 0.40),
        new("Dschubba", null, 16.0056, -22.622, 2.29, -0.12),
        new("Acrab", null, 16.0906, -19.805, 2.62, -0.07),
        new("Larawag", null, 16.8360, -34.293, 2.29, 1.15),
        new("Girtab", null, 17.7081, -39.030, 2.39, -0.20),
        new("Lesath", null, 17.5121, -37.296, 2.69, -0.22),
        new("SigmaSco", null, 16.3536, -25.593, 2.89, 0.13),
        new("PiSco", null, 15.9810, -26.114, 2.89, -0.19),
        new("TauSco", null, 16.5988, -28.216, 2.82, -0.25),
        new("MuSco", null, 16.8641, -38.048, 3.00, -0.20),
        new("Zeta2Sco", null, 16.9098, -42.361, 3.62, 1.40),
        new("EtaSco", null, 17.2032, -43.239, 3.32, 0.41),
        new("Iota1Sco", null, 17.7932, -40.127, 2.99, 0.51),

        // --- Skytten
        new("KausAustralis", "Kaus Australis", 18.4029, -34.385, 1.85, -0.03),
        new("KausMedia", null, 18.3499, -29.828, 2.70, 1.38),
        new("KausBorealis", null, 18.4661, -25.422, 2.81, 1.02),
        new("Nunki", "Nunki", 18.9211, -26.297, 2.05, -0.13),
        new("Ascella", null, 19.0436, -29.880, 2.60, 0.08),
        new("PhiSgr", null, 18.7460, -26.991, 3.17, -0.11),
        new("TauSgr", null, 19.1156, -27.670, 3.32, 1.19),
        new("Gamma2Sgr", null, 18.0966, -30.424, 2.99, 1.00),

        // --- Kentauren och Södra korset
        new("RigilKentaurus", "Rigil Kentaurus", 14.6600, -60.835, -0.27, 0.71),
        new("Hadar", "Hadar", 14.0637, -60.373, 0.61, -0.23),
        new("Menkent", null, 14.1114, -36.370, 2.06, 1.01),
        new("EpsilonCen", null, 13.6647, -53.466, 2.30, -0.22),
        new("ZetaCen", null, 13.9257, -47.288, 2.55, -0.22),
        new("GammaCen", null, 12.6919, -48.958, 2.17, -0.01),
        new("Acrux", "Acrux", 12.4433, -63.099, 0.77, -0.24),
        new("Mimosa", "Mimosa", 12.7953, -59.689, 1.25, -0.24),
        new("Gacrux", "Gacrux", 12.5194, -57.113, 1.63, 1.60),
        new("DeltaCru", null, 12.2525, -58.749, 2.79, -0.19),

        // --- Perseus
        new("Mirfak", "Mirfak", 3.4054, 49.861, 1.79, 0.48),
        new("Algol", "Algol", 3.1361, 40.956, 2.09, -0.05),
        new("ZetaPer", null, 3.9020, 31.884, 2.85, 0.26),
        new("EpsilonPer", null, 3.9646, 40.010, 2.89, -0.20),
        new("GammaPer", null, 3.0797, 53.506, 2.93, 0.70),
        new("DeltaPer", null, 3.7154, 47.788, 3.01, -0.13),
        new("EtaPer", null, 2.8450, 55.895, 3.76, 1.69),

        // --- Andromeda och Pegasus
        new("Alpheratz", "Alpheratz", 0.1398, 29.091, 2.06, -0.11),
        new("Mirach", "Mirach", 1.1622, 35.621, 2.06, 1.58),
        new("Almach", "Almach", 2.0650, 42.330, 2.10, 1.37),
        new("DeltaAnd", null, 0.6555, 30.861, 3.27, 1.28),
        new("Markab", "Markab", 23.0793, 15.205, 2.48, -0.04),
        new("Scheat", "Scheat", 23.0629, 28.083, 2.42, 1.67),
        new("Algenib", null, 0.2206, 15.184, 2.83, -0.19),
        new("Enif", "Enif", 21.7364, 9.875, 2.39, 1.53),
        new("EtaPeg", null, 22.7169, 30.221, 2.94, 0.86),
        new("ZetaPeg", null, 22.6912, 10.831, 3.40, -0.09),
        new("ThetaPeg", null, 22.1699, 6.198, 3.53, 0.09),

        // --- Väduren
        new("Hamal", "Hamal", 2.1195, 23.462, 2.00, 1.15),
        new("Sheratan", null, 1.9105, 20.808, 2.64, 0.13),
        new("Mesarthim", null, 1.8925, 19.294, 3.88, 0.01),
        new("Ari41", null, 2.8330, 27.261, 3.61, -0.10),

        // --- Övriga ljusstarka stjärnor
        new("Canopus", "Canopus", 6.3992, -52.696, -0.72, 0.15),
        new("Achernar", "Achernar", 1.6286, -57.237, 0.46, -0.16),
        new("Fomalhaut", "Fomalhaut", 22.9608, -29.622, 1.16, 0.09),
        new("Miaplacidus", "Miaplacidus", 9.2200, -69.717, 1.68, 0.07),
        new("Avior", null, 8.3752, -59.510, 1.86, 1.19),
        new("Aspidiske", null, 9.2850, -59.275, 2.21, 0.18),
        new("ThetaCar", null, 10.7150, -64.394, 2.76, -0.22),
        new("GammaVel", null, 8.1584, -47.337, 1.83, -0.25),
        new("KappaVel", null, 9.3689, -55.011, 2.50, -0.14),
        new("MuVel", null, 10.7772, -49.420, 2.69, 0.90),
        new("Suhail", null, 9.1332, -43.433, 2.23, 1.67),
        new("DeltaVel", null, 8.7450, -54.709, 1.96, 0.04),
        new("Naos", null, 8.0597, -40.003, 2.25, -0.27),
        new("Alnair", "Alnair", 22.1372, -46.961, 1.74, -0.13),
        new("BetaGru", null, 22.7113, -46.885, 2.15, 1.60),
        new("GammaGru", null, 21.8987, -37.365, 3.00, -0.12),
        new("Peacock", "Peacock", 20.4275, -56.735, 1.94, -0.12),
        new("Atria", null, 16.8110, -69.028, 1.91, 1.44),
        new("Alphard", "Alphard", 9.4597, -8.659, 1.98, 1.44),
        new("Diphda", null, 0.7265, -17.987, 2.04, 1.02),
        new("Menkar", null, 3.0380, 4.090, 2.53, 1.63),
        new("Ankaa", null, 0.4381, -42.306, 2.39, 1.09),
        new("Rasalhague", "Rasalhague", 17.5822, 12.560, 2.08, 0.15),
        new("Sabik", null, 17.1729, -15.725, 2.43, 0.06),
        new("ZetaOph", null, 16.6194, -10.567, 2.56, 0.02),
        new("YedPrior", null, 16.2394, -3.694, 2.73, 1.58),
        new("Cebalrai", null, 17.7243, 4.567, 2.76, 1.16),
        new("Unukalhai", null, 15.7378, 6.426, 2.65, 1.17),
        new("Zubeneschamali", null, 15.2831, -9.383, 2.61, -0.11),
        new("Zubenelgenubi", null, 14.8479, -16.042, 2.75, 0.15),
        new("AlphaLup", null, 14.6989, -47.388, 2.30, -0.20),
        new("BetaLup", null, 14.9758, -43.134, 2.68, -0.16),
        new("GammaLup", null, 15.5852, -41.167, 2.78, -0.19),
        new("AlphaAra", null, 17.5307, -49.876, 2.84, -0.17),
        new("BetaAra", null, 17.4212, -55.530, 2.84, 1.46),
        new("Sadalsuud", null, 21.5257, -5.571, 2.90, 0.83),
        new("Sadalmelik", null, 22.0964, -0.320, 2.95, 0.98),
        new("DenebAlgedi", null, 21.7840, -16.127, 2.85, 0.29),
        new("Dabih", null, 20.3502, -14.781, 3.05, 0.79),
        new("Algedi", null, 20.3003, -12.545, 3.57, 0.94),
        new("Nashira", null, 21.6683, -16.662, 3.68, 0.32),
        new("Mira", null, 2.3224, -2.978, 3.04, 1.42),
    ];

    public static readonly Constellation[] Constellations =
    [
        new("Orion",
        [
            ("Betelgeuse", "Bellatrix"), ("Bellatrix", "Mintaka"), ("Betelgeuse", "Alnitak"),
            ("Mintaka", "Alnilam"), ("Alnilam", "Alnitak"), ("Alnitak", "Saiph"),
            ("Mintaka", "Rigel"), ("Meissa", "Betelgeuse"), ("Meissa", "Bellatrix"),
        ]),
        new("Karlavagnen - Stora björn",
        [
            ("Dubhe", "Merak"), ("Merak", "Phecda"), ("Phecda", "Megrez"), ("Megrez", "Dubhe"),
            ("Megrez", "Alioth"), ("Alioth", "Mizar"), ("Mizar", "Alkaid"),
        ]),
        new("Lilla björn",
        [
            ("Polaris", "Yildun"), ("Yildun", "EpsilonUMi"), ("EpsilonUMi", "ZetaUMi"),
            ("ZetaUMi", "Kochab"), ("Kochab", "Pherkad"), ("Pherkad", "EtaUMi"), ("EtaUMi", "ZetaUMi"),
        ]),
        new("Cassiopeia",
        [
            ("Caph", "Schedar"), ("Schedar", "GammaCas"), ("GammaCas", "Ruchbah"), ("Ruchbah", "Segin"),
        ]),
        new("Svanen",
        [
            ("Deneb", "Sadr"), ("Sadr", "EtaCyg"), ("EtaCyg", "Albireo"),
            ("DeltaCyg", "Sadr"), ("Sadr", "GienahCyg"),
        ]),
        new("Lyran",
        [
            ("Vega", "ZetaLyr"), ("ZetaLyr", "Sheliak"), ("Sheliak", "Sulafat"),
            ("Sulafat", "Delta2Lyr"), ("Delta2Lyr", "ZetaLyr"),
        ]),
        new("Örnen",
        [
            ("ZetaAql", "Tarazed"), ("Tarazed", "Altair"), ("Altair", "Alshain"),
            ("Tarazed", "DeltaAql"), ("DeltaAql", "LambdaAql"), ("DeltaAql", "EtaAql"),
            ("EtaAql", "ThetaAql"),
        ]),
        new("Herkules",
        [
            ("Rasalgethi", "Kornephoros"), ("Kornephoros", "ZetaHer"), ("ZetaHer", "EpsilonHer"),
            ("EpsilonHer", "PiHer"), ("PiHer", "EtaHer"), ("EtaHer", "ZetaHer"),
            ("Rasalgethi", "DeltaHer"), ("DeltaHer", "EpsilonHer"),
        ]),
        new("Björnvaktaren",
        [
            ("Arcturus", "Izar"), ("Izar", "DeltaBoo"), ("DeltaBoo", "Nekkar"),
            ("Nekkar", "Seginus"), ("Seginus", "RhoBoo"), ("RhoBoo", "Arcturus"),
            ("Arcturus", "Muphrid"),
        ]),
        new("Norra kronan",
        [
            ("ThetaCrB", "BetaCrB"), ("BetaCrB", "Alphecca"), ("Alphecca", "GammaCrB"),
            ("GammaCrB", "DeltaCrB"), ("DeltaCrB", "EpsilonCrB"), ("EpsilonCrB", "IotaCrB"),
        ]),
        new("Lejonet",
        [
            ("Algenubi", "Rasalas"), ("Rasalas", "Adhafera"), ("Adhafera", "Algieba"),
            ("Algieba", "EtaLeo"), ("EtaLeo", "Regulus"), ("Regulus", "Chort"),
            ("Chort", "Denebola"), ("Denebola", "Zosma"), ("Zosma", "Algieba"),
        ]),
        new("Jungfrun",
        [
            ("Spica", "Heze"), ("Heze", "Porrima"), ("Porrima", "EtaVir"), ("EtaVir", "Zavijava"),
            ("Porrima", "Auva"), ("Auva", "Vindemiatrix"),
        ]),
        new("Oxen",
        [
            ("GammaTau", "DeltaTau"), ("DeltaTau", "EpsilonTau"), ("EpsilonTau", "Elnath"),
            ("GammaTau", "Theta2Tau"), ("Theta2Tau", "Aldebaran"), ("Aldebaran", "ZetaTau"),
        ]),
        new("Tvillingarna",
        [
            ("Castor", "TauGem"), ("TauGem", "Mebsuta"), ("Mebsuta", "Tejat"), ("Tejat", "Propus"),
            ("Pollux", "UpsilonGem"), ("UpsilonGem", "DeltaGem"), ("DeltaGem", "Mekbuda"),
            ("Mekbuda", "Alhena"), ("DeltaGem", "LambdaGem"),
        ]),
        new("Kusken",
        [
            ("Capella", "Menkalinan"), ("Menkalinan", "ThetaAur"), ("ThetaAur", "Elnath"),
            ("Elnath", "IotaAur"), ("IotaAur", "EpsilonAur"), ("EpsilonAur", "Capella"),
        ]),
        new("Stora hunden",
        [
            ("Mirzam", "Sirius"), ("Sirius", "IotaCMa"), ("IotaCMa", "Wezen"),
            ("Wezen", "Adhara"), ("Wezen", "Aludra"),
        ]),
        new("Lilla hunden", [("Procyon", "Gomeisa")]),
        new("Skorpionen",
        [
            ("Acrab", "Dschubba"), ("Dschubba", "PiSco"), ("PiSco", "SigmaSco"),
            ("SigmaSco", "Antares"), ("Antares", "TauSco"), ("TauSco", "Larawag"),
            ("Larawag", "MuSco"), ("MuSco", "Zeta2Sco"), ("Zeta2Sco", "EtaSco"),
            ("EtaSco", "Sargas"), ("Sargas", "Iota1Sco"), ("Iota1Sco", "Girtab"),
            ("Girtab", "Shaula"), ("Shaula", "Lesath"),
        ]),
        new("Skytten",
        [
            ("Gamma2Sgr", "KausMedia"), ("KausMedia", "KausAustralis"), ("KausAustralis", "Ascella"),
            ("Ascella", "TauSgr"), ("TauSgr", "Nunki"), ("Nunki", "PhiSgr"),
            ("PhiSgr", "KausMedia"), ("PhiSgr", "KausBorealis"), ("KausBorealis", "KausMedia"),
        ]),
        new("Södra korset", [("Acrux", "Gacrux"), ("Mimosa", "DeltaCru")]),
        new("Kentauren",
        [
            ("RigilKentaurus", "Hadar"), ("Hadar", "EpsilonCen"), ("EpsilonCen", "ZetaCen"),
            ("ZetaCen", "GammaCen"), ("ZetaCen", "Menkent"),
        ]),
        new("Perseus",
        [
            ("EtaPer", "GammaPer"), ("GammaPer", "Mirfak"), ("Mirfak", "DeltaPer"),
            ("DeltaPer", "EpsilonPer"), ("EpsilonPer", "ZetaPer"), ("Mirfak", "Algol"),
        ]),
        new("Andromeda",
        [
            ("Alpheratz", "DeltaAnd"), ("DeltaAnd", "Mirach"), ("Mirach", "Almach"),
        ]),
        new("Pegasus",
        [
            ("Alpheratz", "Scheat"), ("Scheat", "Markab"), ("Markab", "Algenib"),
            ("Algenib", "Alpheratz"), ("Markab", "ThetaPeg"), ("ThetaPeg", "Enif"),
            ("Scheat", "EtaPeg"), ("Markab", "ZetaPeg"),
        ]),
        new("Cepheus",
        [
            ("Alderamin", "Alfirk"), ("Alfirk", "GammaCep"), ("GammaCep", "IotaCep"),
            ("IotaCep", "ZetaCep"), ("ZetaCep", "Alderamin"), ("ZetaCep", "DeltaCep"),
        ]),
        new("Draken",
        [
            ("Eltanin", "Rastaban"), ("Rastaban", "XiDra"), ("XiDra", "DeltaDra"),
            ("DeltaDra", "ZetaDra"), ("ZetaDra", "EtaDra"), ("EtaDra", "IotaDra"),
        ]),
        new("Väduren",
        [
            ("Mesarthim", "Sheratan"), ("Sheratan", "Hamal"), ("Hamal", "Ari41"),
        ]),
    ];
}
