using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Coordinates the document catalog and immutable document snapshots used by concrete applications.
/// File I/O is delegated to <see cref="IDocumentArchive"/> so this layer remains platform-testable.
/// </summary>
public sealed class DocumentService(ITrackerDataStore store, IDocumentArchive archive, IClock clock)
{
    /// <summary>Returns active catalog documents by default.</summary>
    public Task<IReadOnlyList<Document>> ListAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
        => store.ListDocumentsAsync(includeArchived, cancellationToken);

    /// <summary>
    /// Registers the current version of an existing file and captures its SHA-256 fingerprint.
    /// The original file is not copied until it is actually assigned to an application.
    /// </summary>
    public async Task<Document> RegisterAsync(DocumentInput input, CancellationToken cancellationToken = default)
    {
        var path = Validation.Required(input.OriginalPath, "Datei", 4096);
        var inspection = await archive.InspectAsync(path, cancellationToken).ConfigureAwait(false);
        var now = clock.UtcNow;
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Type = input.Type,
            Label = Validation.Required(input.Label, "Bezeichnung", 200),
            Version = Validation.Required(input.Version, "Version", 100),
            Language = Validation.Required(input.Language, "Sprache", 20).ToUpperInvariant(),
            Tags = Validation.Optional(input.Tags, "Tags", 1000),
            OriginalPath = inspection.FullPath,
            Sha256 = inspection.Sha256,
            SizeBytes = inspection.SizeBytes,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddDocumentAsync(document, cancellationToken).ConfigureAwait(false);
        return document;
    }

    /// <summary>Returns immutable document snapshots already associated with an application.</summary>
    public Task<IReadOnlyList<ApplicationDocumentSnapshot>> ListApplicationSnapshotsAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
        => store.ListApplicationDocumentSnapshotsAsync(applicationId, cancellationToken);

    /// <summary>
    /// Assigns a catalog document to an application. The source file is re-verified against its
    /// registered hash and copied to the private local archive before the immutable database record
    /// is written.
    /// </summary>
    public async Task<ApplicationDocumentSnapshot> AttachToApplicationAsync(
        Guid applicationId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        _ = await store.GetApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Bewerbung wurde nicht gefunden.");
        var document = await store.GetDocumentAsync(documentId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Das Dokument wurde nicht gefunden.");

        var storedPath = await archive.CreateApplicationSnapshotAsync(
            applicationId,
            document.OriginalPath,
            document.Sha256,
            cancellationToken).ConfigureAwait(false);

        var snapshot = new ApplicationDocumentSnapshot
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            DocumentId = document.Id,
            Type = document.Type,
            Label = document.Label,
            Version = document.Version,
            Language = document.Language,
            OriginalPath = document.OriginalPath,
            StoredPath = storedPath,
            Sha256 = document.Sha256,
            CapturedAtUtc = clock.UtcNow,
        };

        await store.AddApplicationDocumentSnapshotAsync(snapshot, cancellationToken).ConfigureAwait(false);
        return snapshot;
    }
}
