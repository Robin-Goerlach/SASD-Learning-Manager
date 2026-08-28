namespace SASD.LearningManager.WinForms.Presentation;

/// <summary>Centralizes human-readable labels for the deliberately qualitative five-level skill scale.</summary>
internal static class SkillLevelPresentation
{
    public static string Format(int? level) => level switch
    {
        null => "Nicht bewertet",
        1 => "1 – Grundverständnis",
        2 => "2 – mit Unterstützung",
        3 => "3 – selbstständig",
        4 => "4 – sicher und vertieft",
        5 => "5 – Experten-/Erklärniveau",
        _ => $"{level} – unbekannt"
    };

    public static IReadOnlyList<SkillLevelOption> Options(bool includeEmpty)
    {
        var options = new List<SkillLevelOption>();
        if (includeEmpty) options.Add(new SkillLevelOption(null, "(kein Ziel-Level)"));
        for (var level = 1; level <= 5; level++) options.Add(new SkillLevelOption(level, Format(level)));
        return options;
    }
}

/// <summary>Combobox option carrying the numeric level and its explanatory label.</summary>
internal sealed record SkillLevelOption(int? Level, string Text);
