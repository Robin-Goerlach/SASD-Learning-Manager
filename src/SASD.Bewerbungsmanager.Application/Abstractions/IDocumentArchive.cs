namespace SASD.Bewerbungsmanager.Application.Abstractions;

/// <summary>
/// Abstracts file inspection and immutable local snapshots so the application layer does not
/// depend on concrete file-system APIs.
/// </summary>
public interface IDocumentArchive
{
    /// <summary>Reads stable metadata and the SHA-256 fingerprint of an existing file.</summary>
    /// <param name="path">User-selected source path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File metadata captured during inspection.</returns>
    Task<DocumentInspection> InspectAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies the exact file version into the application's private document archive and verifies
    /// that it still matches the expected fingerprint.
    /// </summary>
    /// <param name="applicationId">Application receiving the immutable snapshot.</param>
    /// <param name="sourcePath">Original source file.</param>
    /// <param name="expectedSha256">Previously captured SHA-256 fingerprint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full path of the private snapshot.</returns>
    Task<string> CreateApplicationSnapshotAsync(
        Guid applicationId,
        string sourcePath,
        string expectedSha256,
        CancellationToken cancellationToken = default);
}

/// <summary>Stable metadata collected while registering a document file.</summary>
/// <param name="FullPath">Normalized full source path.</param>
/// <param name="Sha256">Uppercase hexadecimal SHA-256 fingerprint.</param>
/// <param name="SizeBytes">File size in bytes.</param>
public sealed record DocumentInspection(string FullPath, string Sha256, long SizeBytes);
