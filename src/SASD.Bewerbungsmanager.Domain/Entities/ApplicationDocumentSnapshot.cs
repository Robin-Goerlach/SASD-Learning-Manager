using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Immutable evidence of the exact document version assigned to a concrete application. Besides
/// the original path and hash it stores the path of the private local snapshot copied when used.
/// </summary>
public sealed class ApplicationDocumentSnapshot
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the application this document snapshot belongs to.</summary>
    public Guid ApplicationId { get; set; }

    /// <summary>Gets or sets the source catalog document, when it still exists.</summary>
    public Guid? DocumentId { get; set; }

    /// <summary>Gets or sets the type captured from the catalog entry.</summary>
    public DocumentType Type { get; set; }

    /// <summary>Gets or sets the label captured at assignment time.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the version captured at assignment time.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the language captured at assignment time.</summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>Gets or sets the original source path captured at assignment time.</summary>
    public string OriginalPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the private snapshot path inside LocalApplicationData.</summary>
    public string StoredPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the SHA-256 fingerprint of the exact copied file.</summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>Gets or sets when this immutable assignment snapshot was created.</summary>
    public DateTimeOffset CapturedAtUtc { get; set; }
}
