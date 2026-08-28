namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Stores a source reference for an opportunity, for example a LinkedIn or company career-page URL.
/// The role description itself remains stored as a snapshot on the opportunity.
/// </summary>
public sealed class SourceLink
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the opportunity this source belongs to.</summary>
    public Guid OpportunityId { get; set; }

    /// <summary>Gets or sets the human-readable source name, for example LinkedIn.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the source URL.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional source-system identifier.</summary>
    public string? ExternalId { get; set; }

    /// <summary>Gets or sets when this source reference was captured.</summary>
    public DateTimeOffset CapturedAtUtc { get; set; }
}
