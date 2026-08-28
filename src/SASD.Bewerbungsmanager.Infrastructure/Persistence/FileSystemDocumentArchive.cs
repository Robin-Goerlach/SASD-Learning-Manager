using System.Security.Cryptography;
using SASD.Bewerbungsmanager.Application.Abstractions;

namespace SASD.Bewerbungsmanager.Infrastructure.Persistence;

/// <summary>
/// File-system implementation for document inspection and immutable application snapshots.
/// Snapshots live beside the configured tracker data root and therefore never belong in Git.
/// </summary>
public sealed class FileSystemDocumentArchive(TrackerStoragePaths storagePaths) : IDocumentArchive
{
    /// <inheritdoc />
    public async Task<DocumentInspection> InspectAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = NormalizeExistingPath(path);
        var file = new FileInfo(fullPath);
        var hash = await ComputeSha256Async(fullPath, cancellationToken).ConfigureAwait(false);
        return new DocumentInspection(fullPath, hash, file.Length);
    }

    /// <inheritdoc />
    public async Task<string> CreateApplicationSnapshotAsync(
        Guid applicationId,
        string sourcePath,
        string expectedSha256,
        CancellationToken cancellationToken = default)
    {
        var fullPath = NormalizeExistingPath(sourcePath);
        var actualHash = await ComputeSha256Async(fullPath, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(actualHash, expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Die Quelldatei wurde seit ihrer Registrierung verändert. Bitte diese Version als neues Dokument registrieren.");
        }

        var targetDirectory = Path.Combine(storagePaths.DocumentsDirectory, applicationId.ToString("N"));
        Directory.CreateDirectory(targetDirectory);
        var extension = Path.GetExtension(fullPath);
        var targetPath = Path.Combine(targetDirectory, $"{actualHash}{extension}");
        if (File.Exists(targetPath))
        {
            return targetPath;
        }

        // Copy to a temporary file first. A cancellation or interrupted write can therefore never
        // leave a path that looks like a valid immutable snapshot but contains only partial data.
        var temporaryPath = targetPath + ".tmp";
        try
        {
            await using (var source = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var destination = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            File.Move(temporaryPath, targetPath, overwrite: false);
            return targetPath;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static string NormalizeExistingPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Ein Dateipfad ist erforderlich.", nameof(path));
        }

        var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Die ausgewählte Datei wurde nicht gefunden.", fullPath);
        }

        return fullPath;
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }
}
