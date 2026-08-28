namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>Describes one authoritative file stored inside a tracker backup.</summary>
public sealed record BackupManifestFile(string Path, long SizeBytes, string Sha256);

/// <summary>
/// Versioned backup contract. Version 1 deliberately stores only technical metadata; personal
/// application data remains in the database/document files and is never duplicated into the manifest.
/// </summary>
public sealed record BackupManifest(
    int SchemaVersion,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<string> AppliedMigrations,
    IReadOnlyList<BackupManifestFile> Files);

/// <summary>Result of a complete backup integrity and compatibility check.</summary>
public sealed record BackupValidationResult(
    bool IsValid,
    string Summary,
    BackupManifest? Manifest,
    IReadOnlyList<string> Errors);

/// <summary>Result returned after a backup was written.</summary>
public sealed record BackupCreationResult(string Path, DateTimeOffset CreatedAtUtc, int FileCount, long TotalBytes);

/// <summary>Result returned after a validated package was staged for the next startup.</summary>
public sealed record RestoreStageResult(string SourcePath, DateTimeOffset StagedAtUtc, int FileCount);

/// <summary>Technical database health report intentionally free of business text.</summary>
public sealed record TrackerDiagnosticReport(
    int SchemaVersion,
    DateTimeOffset GeneratedAtUtc,
    string QuickCheck,
    int ForeignKeyViolationCount,
    IReadOnlyList<string> AppliedMigrations,
    IReadOnlyList<string> PendingMigrations,
    IReadOnlyDictionary<string, long> RecordCounts);

internal sealed record PendingRestoreDescriptor(string StagingDirectory, string SourceArchive, DateTimeOffset StagedAtUtc);

/// <summary>Severity of one local release-readiness gate.</summary>
public enum ReleaseGateSeverity
{
    /// <summary>The gate is satisfied.</summary>
    Passed,

    /// <summary>The gate is usable but deserves review before a public release.</summary>
    Warning,

    /// <summary>The gate blocks release readiness.</summary>
    Failed,
}

/// <summary>One privacy-safe technical release gate.</summary>
public sealed record ReleaseGateResult(
    string Id,
    string Title,
    ReleaseGateSeverity Severity,
    string Detail);

/// <summary>Aggregated local technical release-readiness report.</summary>
public sealed record ReleaseReadinessReport(
    int SchemaVersion,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<ReleaseGateResult> Gates)
{
    /// <summary>Gets whether no blocking local gate failed.</summary>
    public bool IsReadyForRc => Gates.All(item => item.Severity != ReleaseGateSeverity.Failed);

    /// <summary>Formats the report for the WinForms status area without exposing business content.</summary>
    public string ToDisplayText()
    {
        var lines = new List<string>
        {
            IsReadyForRc ? "Lokale RC-Gates: BESTANDEN" : "Lokale RC-Gates: NICHT BESTANDEN",
            $"Zeitpunkt (UTC): {GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}",
            string.Empty,
        };

        foreach (var gate in Gates)
        {
            var marker = gate.Severity switch
            {
                ReleaseGateSeverity.Passed => "OK",
                ReleaseGateSeverity.Warning => "WARN",
                _ => "FAIL",
            };
            lines.Add($"[{marker}] {gate.Id} – {gate.Title}");
            lines.Add($"       {gate.Detail}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
