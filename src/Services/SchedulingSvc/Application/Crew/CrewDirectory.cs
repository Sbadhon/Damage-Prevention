using System.Linq;

namespace SchedulingSvc.Application.Crew;

public static class CrewDirectory
{
    private static readonly CrewInfo[] Crews =
    {
        // Gas
        new("CREW-GAS-ALPHA",      "Gas Line Crew Alpha",        "gas"),
        new("CREW-GAS-BETA",       "Gas Line Crew Beta",         "gas"),
        new("CREW-GAS-GAMMA",      "Gas Line Crew Gamma",        "gas"),

        // Electric
        new("CREW-ELECTRIC-ALPHA", "Electric Line Crew Alpha",   "electric"),
        new("CREW-ELECTRIC-BETA",  "Electric Line Crew Beta",    "electric"),
        new("CREW-ELECTRIC-GAMMA", "Electric Line Crew Gamma",   "electric"),

        // Communications (Fiber/Copper/Telecom)
        new("CREW-COMM-ALPHA",     "Communications Crew Alpha",  "communications"),
        new("CREW-COMM-BETA",      "Communications Crew Beta",   "communications"),
        new("CREW-COMM-GAMMA",     "Communications Crew Gamma",  "communications"),

        // Water
        new("CREW-WATER-ALPHA",    "Water Main Crew Alpha",      "water"),
        new("CREW-WATER-BETA",     "Water Main Crew Beta",       "water"),

        // Sewer
        new("CREW-SEWER-ALPHA",    "Sewer Crew Alpha",           "sewer"),
        new("CREW-SEWER-BETA",     "Sewer Crew Beta",            "sewer"),

        // Storm Drain
        new("CREW-STORM-ALPHA",    "Storm Drain Crew Alpha",     "storm"),
        new("CREW-STORM-BETA",     "Storm Drain Crew Beta",      "storm"),

        // Reclaimed / Irrigation
        new("CREW-RECLAIM-ALPHA",  "Reclaimed Water Crew Alpha", "reclaimed"),
        new("CREW-RECLAIM-BETA",   "Reclaimed Water Crew Beta",  "reclaimed"),

        // General / Locate
        new("CREW-GENERAL-ALPHA",  "General Locate Crew Alpha",  "general"),
        new("CREW-GENERAL-BETA",   "General Locate Crew Beta",   "general"),
        new("CREW-GENERAL-GAMMA",  "General Locate Crew Gamma",  "general")
    };

    /// <summary>
    /// Picks a crew based on workType string.
    /// Falls back to a general crew if no specific match.
    /// </summary>
    public static CrewInfo PickForWorkType(string workType)
    {
        if (string.IsNullOrWhiteSpace(workType))
            return DefaultCrew();

        var normalized = workType.Trim().ToLowerInvariant();

        // Simple keyword routing – refine as needed.

        // Gas
        if (normalized.Contains("gas") || normalized.Contains("pipeline") || normalized.Contains("odor"))
            return FirstBySpecialty("gas");

        // Electric
        if (normalized.Contains("electric") || normalized.Contains("power") || normalized.Contains("feeder"))
            return FirstBySpecialty("electric");

        // Fiber / Telecom / Communications
        if (normalized.Contains("fiber") || normalized.Contains("telecom") ||
            normalized.Contains("optic") || normalized.Contains("comm"))
            return FirstBySpecialty("communications");

        // Water
        if (normalized.Contains("water") || normalized.Contains("potable") || normalized.Contains("hydrant"))
            return FirstBySpecialty("water");

        // Sewer
        if (normalized.Contains("sewer") || normalized.Contains("sanitary"))
            return FirstBySpecialty("sewer");

        // Storm
        if (normalized.Contains("storm") || normalized.Contains("drain"))
            return FirstBySpecialty("storm");

        // Reclaimed / Irrigation
        if (normalized.Contains("reclaim") || normalized.Contains("irrigation"))
            return FirstBySpecialty("reclaimed");

        // Fallback -> General locate crew
        return DefaultCrew();
    }

    public static CrewInfo[] GetAll() => Crews;

    private static CrewInfo FirstBySpecialty(string specialty) =>
        Crews.FirstOrDefault(c => c.Specialty.Equals(specialty, StringComparison.OrdinalIgnoreCase))
        ?? DefaultCrew();

    private static CrewInfo DefaultCrew() =>
        Crews.First(c => c.Specialty == "general");
}

public sealed record CrewInfo(
    string CrewId,
    string CrewName,
    string Specialty
);
