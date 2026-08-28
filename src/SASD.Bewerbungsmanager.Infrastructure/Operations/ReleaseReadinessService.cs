using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>
/// Evaluates local technical gates that can be proven from a running tracker installation. CI,
/// code signing, licensing and installer publication remain external release gates by design.
/// </summary>
public sealed class ReleaseReadinessService(
    TrackerDiagnosticsService diagnosticsService,
    TrackerStoragePaths storagePaths)
{
    /// <summary>Creates a privacy-safe local RC readiness report.</summary>
    public async Task<ReleaseReadinessReport> CreateReportAsync(CancellationToken cancellationToken = default)
    {
        var diagnostics = await diagnosticsService.CreateReportAsync(cancellationToken).ConfigureAwait(false);
        var gates = new List<ReleaseGateResult>
        {
            CreateQuickCheckGate(diagnostics),
            CreateForeignKeyGate(diagnostics),
            CreateMigrationGate(diagnostics),
            CreatePendingRestoreGate(),
            CreateDatabaseGate(),
            await CreateWritableDataDirectoryGateAsync(cancellationToken).ConfigureAwait(false),
            CreateRecoveryGate(),
        };

        return new ReleaseReadinessReport(1, DateTimeOffset.UtcNow, gates);
    }

    private static ReleaseGateResult CreateQuickCheckGate(TrackerDiagnosticReport diagnostics)
    {
        var passed = string.Equals(diagnostics.QuickCheck, "ok", StringComparison.OrdinalIgnoreCase);
        return new ReleaseGateResult(
            "DB-001",
            "SQLite quick_check",
            passed ? ReleaseGateSeverity.Passed : ReleaseGateSeverity.Failed,
            passed ? "SQLite meldet ok." : $"SQLite meldet: {diagnostics.QuickCheck}");
    }

    private static ReleaseGateResult CreateForeignKeyGate(TrackerDiagnosticReport diagnostics)
        => new(
            "DB-002",
            "Foreign-Key-Integrität",
            diagnostics.ForeignKeyViolationCount == 0 ? ReleaseGateSeverity.Passed : ReleaseGateSeverity.Failed,
            diagnostics.ForeignKeyViolationCount == 0
                ? "Keine Foreign-Key-Verletzungen gefunden."
                : $"{diagnostics.ForeignKeyViolationCount} Foreign-Key-Verletzung(en) gefunden.");

    private static ReleaseGateResult CreateMigrationGate(TrackerDiagnosticReport diagnostics)
        => new(
            "DB-003",
            "Datenbankschema aktuell",
            diagnostics.PendingMigrations.Count == 0 ? ReleaseGateSeverity.Passed : ReleaseGateSeverity.Failed,
            diagnostics.PendingMigrations.Count == 0
                ? $"{diagnostics.AppliedMigrations.Count} Migration(en) angewendet; keine ausstehend."
                : $"{diagnostics.PendingMigrations.Count} Migration(en) sind noch ausstehend.");

    private ReleaseGateResult CreatePendingRestoreGate()
        => new(
            "RST-001",
            "Kein Restore vorgemerkt",
            File.Exists(storagePaths.PendingRestorePath) ? ReleaseGateSeverity.Failed : ReleaseGateSeverity.Passed,
            File.Exists(storagePaths.PendingRestorePath)
                ? "Ein Restore ist für den nächsten Start vorgemerkt."
                : "Kein ausstehender Restore-Marker vorhanden.");

    private ReleaseGateResult CreateDatabaseGate()
    {
        if (!File.Exists(storagePaths.DatabasePath))
        {
            return new ReleaseGateResult("DB-004", "Datenbankdatei vorhanden", ReleaseGateSeverity.Failed, "Die SQLite-Datenbank fehlt.");
        }

        var bytes = new FileInfo(storagePaths.DatabasePath).Length;
        return new ReleaseGateResult(
            "DB-004",
            "Datenbankdatei vorhanden",
            bytes > 0 ? ReleaseGateSeverity.Passed : ReleaseGateSeverity.Failed,
            bytes > 0 ? $"SQLite-Datei vorhanden ({bytes:N0} Bytes)." : "Die SQLite-Datei ist leer.");
    }

    private async Task<ReleaseGateResult> CreateWritableDataDirectoryGateAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(storagePaths.ApplicationDirectory);
        var probePath = Path.Combine(storagePaths.ApplicationDirectory, $".write-probe-{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllTextAsync(probePath, "ok", cancellationToken).ConfigureAwait(false);
            return new ReleaseGateResult("IO-001", "Lokales Datenverzeichnis schreibbar", ReleaseGateSeverity.Passed, "Schreibprobe erfolgreich.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new ReleaseGateResult(
                "IO-001",
                "Lokales Datenverzeichnis schreibbar",
                ReleaseGateSeverity.Failed,
                $"Schreibprobe fehlgeschlagen ({exception.GetType().Name}).");
        }
        finally
        {
            if (File.Exists(probePath))
            {
                File.Delete(probePath);
            }
        }
    }

    private ReleaseGateResult CreateRecoveryGate()
    {
        if (!Directory.Exists(storagePaths.RecoveryDirectory))
        {
            return new ReleaseGateResult(
                "RST-002",
                "Recovery-Kopien",
                ReleaseGateSeverity.Failed,
                "Noch keine pre-restore Recovery-Kopie vorhanden; der praktische Restore-Nachweis steht aus.");
        }

        var count = Directory.EnumerateDirectories(storagePaths.RecoveryDirectory).Count();
        return new ReleaseGateResult(
            "RST-002",
            "Recovery-Kopien",
            count > 0 ? ReleaseGateSeverity.Passed : ReleaseGateSeverity.Failed,
            count > 0
                ? $"{count} Recovery-Kopie(n) vorhanden."
                : "Recovery-Verzeichnis vorhanden, aber noch ohne Kopie.");
    }
}
