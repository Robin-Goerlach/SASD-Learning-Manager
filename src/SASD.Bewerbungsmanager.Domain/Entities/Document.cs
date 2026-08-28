using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents one known document version. The document catalog points to the user's original file
/// and stores its SHA-256 fingerprint so later changes can be detected reliably.
/// </summary>
public sealed class Document
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the functional document type.</summary>
    public DocumentType Type { get; set; }

    /// <summary>Gets or sets a short descriptive label, for example "Linux System Engineer".</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the user-visible version label, for example "2026-08".</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the document language, normally a short code such as DE or EN.</summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>Gets or sets optional comma-separated tags used for quick human filtering.</summary>
    public string? Tags { get; set; }

    /// <summary>Gets or sets the original file path selected by the user.</summary>
    public string OriginalPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the uppercase hexadecimal SHA-256 hash captured at registration time.</summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>Gets or sets the file size captured at registration time.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Gets or sets whether this catalog entry is hidden from normal active lists.</summary>
    public bool IsArchived { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
