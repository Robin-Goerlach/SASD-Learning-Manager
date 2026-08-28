using System.IO.Compression;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>
/// Creates, validates and stages complete local tracker backups. SQLite is copied with its online
/// backup API instead of copying the database file directly, so WAL mode cannot produce a torn backup.
/// </summary>
public sealed class TrackerBackupService(
    IDbContextFactory<ApplicationTrackerDbContext> contextFactory,
    TrackerStoragePaths storagePaths,
    ILogger<TrackerBackupService> logger)
{
    private const int CurrentSchemaVersion = 1;
    private const string ManifestEntryName = "manifest.json";
    private const string DatabaseEntryName = "database/application-tracker.db";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>Creates a complete ZIP backup at the requested location.</summary>
    public async Task<BackupCreationResult> CreateBackupAsync(string targetPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            throw new ArgumentException("Ein Zielpfad für die Sicherung ist erforderlich.", nameof(targetPath));
        }

        var fullTargetPath = Path.GetFullPath(targetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)!);

        var workDirectory = Path.Combine(storagePaths.ApplicationDirectory, "BackupWork", Guid.NewGuid().ToString("N"));
        var archiveTempPath = fullTargetPath + ".tmp";
        Directory.CreateDirectory(workDirectory);

        try
        {
            var databaseCopyPath = BackupFileUtility.ResolveSafeTarget(workDirectory, DatabaseEntryName);
            Directory.CreateDirectory(Path.GetDirectoryName(databaseCopyPath)!);
            await CreateConsistentDatabaseCopyAsync(databaseCopyPath, cancellationToken).ConfigureAwait(false);

            var files = new List<BackupManifestFile>();
            await AddManifestFileAsync(workDirectory, DatabaseEntryName, files, cancellationToken).ConfigureAwait(false);

            if (Directory.Exists(storagePaths.DocumentsDirectory))
            {
                foreach (var sourcePath in Directory.EnumerateFiles(storagePaths.DocumentsDirectory, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var relative = Path.GetRelativePath(storagePaths.DocumentsDirectory, sourcePath).Replace('\\', '/');
                    var archivePath = BackupFileUtility.NormalizeArchivePath($"documents/{relative}");
                    var target = BackupFileUtility.ResolveSafeTarget(workDirectory, archivePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    File.Copy(sourcePath, target, overwrite: false);
                    await AddManifestFileAsync(workDirectory, archivePath, files, cancellationToken).ConfigureAwait(false);
                }
            }

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var appliedMigrations = (await context.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
            var createdAtUtc = DateTimeOffset.UtcNow;
            var manifest = new BackupManifest(CurrentSchemaVersion, createdAtUtc, appliedMigrations, files);
            await File.WriteAllTextAsync(
                Path.Combine(workDirectory, ManifestEntryName),
                JsonSerializer.Serialize(manifest, JsonOptions),
                cancellationToken).ConfigureAwait(false);

            if (File.Exists(archiveTempPath))
            {
                File.Delete(archiveTempPath);
            }

            ZipFile.CreateFromDirectory(workDirectory, archiveTempPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            File.Move(archiveTempPath, fullTargetPath, overwrite: true);

            var totalBytes = files.Sum(item => item.SizeBytes);
            logger.LogInformation("Created tracker backup with {FileCount} authoritative files.", files.Count);
            return new BackupCreationResult(fullTargetPath, createdAtUtc, files.Count, totalBytes);
        }
        finally
        {
            if (File.Exists(archiveTempPath))
            {
                File.Delete(archiveTempPath);
            }

            if (Directory.Exists(workDirectory))
            {
                Directory.Delete(workDirectory, recursive: true);
            }
        }
    }

    /// <summary>Checks structural safety, hashes, sizes and migration compatibility of a backup.</summary>
    public async Task<BackupValidationResult> ValidateBackupAsync(string archivePath, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        BackupManifest? manifest = null;

        if (string.IsNullOrWhiteSpace(archivePath) || !File.Exists(archivePath))
        {
            return new BackupValidationResult(false, "Die Sicherungsdatei wurde nicht gefunden.", null, ["Datei fehlt."]);
        }

        try
        {
            using var archive = ZipFile.OpenRead(archivePath);
            var entries = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in archive.Entries.Where(item => !string.IsNullOrEmpty(item.Name)))
            {
                var normalized = BackupFileUtility.NormalizeArchivePath(entry.FullName);
                if (!entries.TryAdd(normalized, entry))
                {
                    errors.Add($"Doppelter ZIP-Pfad: {normalized}");
                }
            }

            if (!entries.TryGetValue(ManifestEntryName, out var manifestEntry))
            {
                errors.Add("manifest.json fehlt.");
            }
            else
            {
                await using var manifestStream = manifestEntry.Open();
                manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(manifestStream, JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
                if (manifest is null)
                {
                    errors.Add("manifest.json konnte nicht gelesen werden.");
                }
            }

            if (manifest is not null)
            {
                if (manifest.SchemaVersion != CurrentSchemaVersion)
                {
                    errors.Add($"Nicht unterstützte Backup-Schemaversion {manifest.SchemaVersion}.");
                }

                var manifestPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var file in manifest.Files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var normalized = BackupFileUtility.NormalizeArchivePath(file.Path);
                    if (!manifestPaths.Add(normalized))
                    {
                        errors.Add($"Doppelter Manifest-Pfad: {normalized}");
                        continue;
                    }

                    if (!entries.TryGetValue(normalized, out var entry))
                    {
                        errors.Add($"Datei fehlt im ZIP: {normalized}");
                        continue;
                    }

                    if (entry.Length != file.SizeBytes)
                    {
                        errors.Add($"Dateigröße stimmt nicht: {normalized}");
                        continue;
                    }

                    await using var stream = entry.Open();
                    using var sha256 = System.Security.Cryptography.SHA256.Create();
                    var hash = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
                    var actualHash = Convert.ToHexString(hash);
                    if (!string.Equals(actualHash, file.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"SHA-256 stimmt nicht: {normalized}");
                    }
                }

                if (!manifestPaths.Contains(DatabaseEntryName))
                {
                    errors.Add("Die SQLite-Datenbank fehlt im Manifest.");
                }

                var allowedEntries = new HashSet<string>(manifestPaths, StringComparer.OrdinalIgnoreCase) { ManifestEntryName };
                foreach (var entryPath in entries.Keys)
                {
                    if (!allowedEntries.Contains(entryPath))
                    {
                        errors.Add($"Unerwartete Datei im Backup: {entryPath}");
                    }
                }

                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                var knownMigrations = context.Database.GetMigrations().ToHashSet(StringComparer.Ordinal);
                foreach (var migration in manifest.AppliedMigrations)
                {
                    if (!knownMigrations.Contains(migration))
                    {
                        errors.Add($"Backup benötigt eine unbekannte neuere Migration: {migration}");
                    }
                }
            }
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or JsonException or UnauthorizedAccessException)
        {
            errors.Add(exception.Message);
        }

        var valid = errors.Count == 0 && manifest is not null;
        return new BackupValidationResult(
            valid,
            valid ? "Sicherung ist vollständig und mit dieser Programmversion kompatibel." : "Sicherung ist nicht verwendbar.",
            manifest,
            errors);
    }

    /// <summary>
    /// Validates and extracts a backup into private staging. No live database file is replaced here;
    /// the actual switch is intentionally deferred to the next process start.
    /// </summary>
    public async Task<RestoreStageResult> StageRestoreAsync(string archivePath, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateBackupAsync(archivePath, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid || validation.Manifest is null)
        {
            throw new InvalidDataException(string.Join(Environment.NewLine, validation.Errors));
        }

        if (File.Exists(storagePaths.PendingRestorePath))
        {
            throw new InvalidOperationException("Es ist bereits eine Wiederherstellung für den nächsten Start vorgemerkt.");
        }

        Directory.CreateDirectory(storagePaths.RestoreStagingDirectory);
        var stagingDirectory = Path.Combine(storagePaths.RestoreStagingDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingDirectory);

        try
        {
            using var archive = ZipFile.OpenRead(archivePath);
            foreach (var entry in archive.Entries.Where(item => !string.IsNullOrEmpty(item.Name)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var target = BackupFileUtility.ResolveSafeTarget(stagingDirectory, entry.FullName);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await using var source = entry.Open();
                await using var destination = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
            }

            // Re-check extracted bytes as a second boundary. This also protects against accidental
            // staging-directory modifications before the pending marker becomes authoritative.
            await ValidateExtractedManifestAsync(stagingDirectory, validation.Manifest, cancellationToken).ConfigureAwait(false);

            var descriptor = new PendingRestoreDescriptor(stagingDirectory, Path.GetFullPath(archivePath), DateTimeOffset.UtcNow);
            var temporaryMarker = storagePaths.PendingRestorePath + ".tmp";
            await File.WriteAllTextAsync(temporaryMarker, JsonSerializer.Serialize(descriptor, JsonOptions), cancellationToken).ConfigureAwait(false);
            File.Move(temporaryMarker, storagePaths.PendingRestorePath, overwrite: true);
            return new RestoreStageResult(descriptor.SourceArchive, descriptor.StagedAtUtc, validation.Manifest.Files.Count);
        }
        catch
        {
            if (Directory.Exists(stagingDirectory))
            {
                Directory.Delete(stagingDirectory, recursive: true);
            }

            throw;
        }
    }

    internal static async Task ValidateExtractedManifestAsync(
        string stagingDirectory,
        BackupManifest manifest,
        CancellationToken cancellationToken)
    {
        foreach (var file in manifest.Files)
        {
            var path = BackupFileUtility.ResolveSafeTarget(stagingDirectory, file.Path);
            if (!File.Exists(path))
            {
                throw new InvalidDataException($"Gestagte Datei fehlt: {file.Path}");
            }

            var info = new FileInfo(path);
            if (info.Length != file.SizeBytes)
            {
                throw new InvalidDataException($"Gestagte Dateigröße stimmt nicht: {file.Path}");
            }

            var hash = await BackupFileUtility.ComputeSha256Async(path, cancellationToken).ConfigureAwait(false);
            if (!string.Equals(hash, file.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Gestagter SHA-256 stimmt nicht: {file.Path}");
            }
        }
    }

    private async Task CreateConsistentDatabaseCopyAsync(string targetPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(storagePaths.DatabasePath))
        {
            // A fresh application can be backed up only after initialization; returning a targeted
            // message is clearer than creating an empty SQLite file that looks authoritative.
            throw new FileNotFoundException("Die lokale Bewerbungsdatenbank wurde noch nicht angelegt.", storagePaths.DatabasePath);
        }

        var sourceConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = storagePaths.DatabasePath,
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();
        var targetConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = targetPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();

        await using var source = new SqliteConnection(sourceConnectionString);
        await using var target = new SqliteConnection(targetConnectionString);
        await source.OpenAsync(cancellationToken).ConfigureAwait(false);
        await target.OpenAsync(cancellationToken).ConfigureAwait(false);
        source.BackupDatabase(target);
    }

    private static async Task AddManifestFileAsync(
        string rootDirectory,
        string archivePath,
        ICollection<BackupManifestFile> files,
        CancellationToken cancellationToken)
    {
        var path = BackupFileUtility.ResolveSafeTarget(rootDirectory, archivePath);
        var info = new FileInfo(path);
        var hash = await BackupFileUtility.ComputeSha256Async(path, cancellationToken).ConfigureAwait(false);
        files.Add(new BackupManifestFile(BackupFileUtility.NormalizeArchivePath(archivePath), info.Length, hash));
    }
}
