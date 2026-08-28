using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.Infrastructure.Operations;

/// <summary>
/// Presents one backup API to the UI and transparently handles both legacy plain ZIP backups and
/// password-protected SASD backup envelopes. The inner tracker backup format remains unchanged.
/// </summary>
public sealed class TrackerBackupCoordinator(
    TrackerBackupService backupService,
    PasswordProtectedBackupService passwordProtection,
    TrackerStoragePaths storagePaths)
{
    /// <summary>Gets whether an existing backup requires a password before it can be inspected.</summary>
    public bool RequiresPassword(string path) => passwordProtection.IsEncryptedBackup(path);

    /// <summary>
    /// Creates either a legacy plain ZIP backup when <paramref name="password"/> is null or an
    /// authenticated encrypted SASD backup when a password is supplied.
    /// </summary>
    public async Task<BackupCreationResult> CreateBackupAsync(
        string targetPath,
        string? password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(password))
        {
            return await backupService.CreateBackupAsync(targetPath, cancellationToken).ConfigureAwait(false);
        }

        PasswordProtectedBackupService.ValidateNewPassword(password);
        var temporaryZip = CreateTemporaryZipPath();
        try
        {
            var inner = await backupService.CreateBackupAsync(temporaryZip, cancellationToken).ConfigureAwait(false);
            await passwordProtection.EncryptFileAsync(temporaryZip, targetPath, password, cancellationToken).ConfigureAwait(false);
            return inner with { Path = Path.GetFullPath(targetPath) };
        }
        finally
        {
            DeleteIfExists(temporaryZip);
        }
    }

    /// <summary>Validates a plain or encrypted backup without modifying tracker data.</summary>
    public async Task<BackupValidationResult> ValidateBackupAsync(
        string path,
        string? password,
        CancellationToken cancellationToken = default)
    {
        if (!passwordProtection.IsEncryptedBackup(path))
        {
            return await backupService.ValidateBackupAsync(path, cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrEmpty(password))
        {
            return new BackupValidationResult(
                false,
                "Die Sicherung ist verschlüsselt und benötigt ein Passwort.",
                null,
                ["Passwort fehlt."]);
        }

        var temporaryZip = CreateTemporaryZipPath();
        try
        {
            await passwordProtection.DecryptFileAsync(path, temporaryZip, password, cancellationToken).ConfigureAwait(false);
            return await backupService.ValidateBackupAsync(temporaryZip, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidDataException exception)
        {
            return new BackupValidationResult(false, "Sicherung ist nicht verwendbar.", null, [exception.Message]);
        }
        finally
        {
            DeleteIfExists(temporaryZip);
        }
    }

    /// <summary>
    /// Validates and stages a plain or encrypted backup for the existing restart-bound restore path.
    /// Decrypted temporary ZIP files are deleted immediately after staging is complete.
    /// </summary>
    public async Task<RestoreStageResult> StageRestoreAsync(
        string path,
        string? password,
        CancellationToken cancellationToken = default)
    {
        if (!passwordProtection.IsEncryptedBackup(path))
        {
            return await backupService.StageRestoreAsync(path, cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidDataException("Für diese verschlüsselte Sicherung ist ein Passwort erforderlich.");
        }

        var temporaryZip = CreateTemporaryZipPath();
        try
        {
            await passwordProtection.DecryptFileAsync(path, temporaryZip, password, cancellationToken).ConfigureAwait(false);
            var staged = await backupService.StageRestoreAsync(temporaryZip, cancellationToken).ConfigureAwait(false);
            return staged with { SourcePath = Path.GetFullPath(path) };
        }
        finally
        {
            DeleteIfExists(temporaryZip);
        }
    }

    private string CreateTemporaryZipPath()
    {
        var directory = Path.Combine(storagePaths.ApplicationDirectory, "BackupWork", "EncryptedEnvelope");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"{Guid.NewGuid():N}.zip");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
