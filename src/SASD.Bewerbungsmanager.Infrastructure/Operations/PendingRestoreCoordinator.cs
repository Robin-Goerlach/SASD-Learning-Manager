using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>
/// Applies a previously staged restore before the application opens operational DbContexts. A recovery
/// copy is retained and used for best-effort rollback if file replacement fails halfway through.
/// </summary>
public sealed class PendingRestoreCoordinator(
    TrackerStoragePaths storagePaths,
    ILogger<PendingRestoreCoordinator> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Applies a pending restore marker when one exists; otherwise this method is a no-op.</summary>
    public async Task ApplyPendingRestoreAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(storagePaths.PendingRestorePath))
        {
            return;
        }

        PendingRestoreDescriptor descriptor;
        try
        {
            var json = await File.ReadAllTextAsync(storagePaths.PendingRestorePath, cancellationToken).ConfigureAwait(false);
            descriptor = JsonSerializer.Deserialize<PendingRestoreDescriptor>(json, JsonOptions)
                ?? throw new InvalidDataException("Die vorgemerkte Wiederherstellung ist unvollständig.");
        }
        catch (Exception exception) when (exception is IOException or JsonException or InvalidDataException)
        {
            throw new InvalidOperationException("Die vorgemerkte Wiederherstellung konnte nicht gelesen werden.", exception);
        }

        var stagingRoot = Path.GetFullPath(storagePaths.RestoreStagingDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var descriptorRoot = Path.GetFullPath(descriptor.StagingDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!descriptorRoot.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Der vorgemerkte Restore verweist nicht auf das private Staging-Verzeichnis.");
        }

        var manifestPath = BackupFileUtility.ResolveSafeTarget(descriptor.StagingDirectory, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            throw new InvalidDataException("Die gestagte Wiederherstellung enthält kein manifest.json.");
        }

        var manifestJson = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
        var manifest = JsonSerializer.Deserialize<BackupManifest>(manifestJson, JsonOptions)
            ?? throw new InvalidDataException("Das gestagte Backup-Manifest ist ungültig.");
        await TrackerBackupService.ValidateExtractedManifestAsync(descriptor.StagingDirectory, manifest, cancellationToken)
            .ConfigureAwait(false);

        var stagedDatabase = BackupFileUtility.ResolveSafeTarget(descriptor.StagingDirectory, "database/application-tracker.db");
        var stagedDocuments = BackupFileUtility.ResolveSafeTarget(descriptor.StagingDirectory, "documents");
        if (!File.Exists(stagedDatabase))
        {
            throw new InvalidDataException("Die gestagte SQLite-Datenbank fehlt.");
        }

        Directory.CreateDirectory(storagePaths.RecoveryDirectory);
        var recoveryRoot = Path.Combine(
            storagePaths.RecoveryDirectory,
            $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(recoveryRoot);

        try
        {
            await CreateRecoveryCopyAsync(recoveryRoot, cancellationToken).ConfigureAwait(false);
            ReplaceLiveData(stagedDatabase, stagedDocuments);

            File.Delete(storagePaths.PendingRestorePath);
            if (Directory.Exists(descriptor.StagingDirectory))
            {
                Directory.Delete(descriptor.StagingDirectory, recursive: true);
            }

            logger.LogInformation("Applied staged tracker restore. Recovery copy retained at {RecoveryRoot}.", recoveryRoot);
        }
        catch
        {
            logger.LogError("Restore failed. Attempting rollback from the pre-restore recovery copy.");
            try
            {
                RestoreRecoveryCopy(recoveryRoot);
            }
            catch (Exception rollbackException)
            {
                logger.LogCritical(rollbackException, "Automatic rollback after restore failure also failed.");
            }

            throw;
        }
    }

    private async Task CreateRecoveryCopyAsync(string recoveryRoot, CancellationToken cancellationToken)
    {
        var recoveryDatabase = Path.Combine(recoveryRoot, "application-tracker.db");
        if (File.Exists(storagePaths.DatabasePath))
        {
            var sourceConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = storagePaths.DatabasePath,
                Mode = SqliteOpenMode.ReadOnly,
            }.ToString();
            var targetConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = recoveryDatabase,
                Mode = SqliteOpenMode.ReadWriteCreate,
            }.ToString();

            await using var source = new SqliteConnection(sourceConnectionString);
            await using var target = new SqliteConnection(targetConnectionString);
            await source.OpenAsync(cancellationToken).ConfigureAwait(false);
            await target.OpenAsync(cancellationToken).ConfigureAwait(false);
            source.BackupDatabase(target);
        }

        BackupFileUtility.CopyDirectory(storagePaths.DocumentsDirectory, Path.Combine(recoveryRoot, "Documents"));
    }

    private void ReplaceLiveData(string stagedDatabase, string stagedDocuments)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(storagePaths.DatabasePath)!);
        DeleteSqliteFiles(storagePaths.DatabasePath);
        File.Copy(stagedDatabase, storagePaths.DatabasePath, overwrite: true);

        if (Directory.Exists(storagePaths.DocumentsDirectory))
        {
            Directory.Delete(storagePaths.DocumentsDirectory, recursive: true);
        }

        BackupFileUtility.CopyDirectory(stagedDocuments, storagePaths.DocumentsDirectory);
    }

    private void RestoreRecoveryCopy(string recoveryRoot)
    {
        var recoveryDatabase = Path.Combine(recoveryRoot, "application-tracker.db");
        DeleteSqliteFiles(storagePaths.DatabasePath);
        if (File.Exists(recoveryDatabase))
        {
            File.Copy(recoveryDatabase, storagePaths.DatabasePath, overwrite: true);
        }

        if (Directory.Exists(storagePaths.DocumentsDirectory))
        {
            Directory.Delete(storagePaths.DocumentsDirectory, recursive: true);
        }

        BackupFileUtility.CopyDirectory(Path.Combine(recoveryRoot, "Documents"), storagePaths.DocumentsDirectory);
    }

    private static void DeleteSqliteFiles(string databasePath)
    {
        foreach (var path in new[] { databasePath, databasePath + "-wal", databasePath + "-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
