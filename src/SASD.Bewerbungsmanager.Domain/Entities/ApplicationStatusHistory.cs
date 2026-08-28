using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents one immutable stage transition in the history of a concrete application.
/// </summary>
public sealed class ApplicationStatusHistory
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the application whose state was recorded.</summary>
    public Guid ApplicationId { get; set; }

    /// <summary>Gets or sets the stage that became effective.</summary>
    public ApplicationStage Stage { get; set; }

    /// <summary>Gets or sets when this stage became effective.</summary>
    public DateTimeOffset ChangedAtUtc { get; set; }

    /// <summary>Gets or sets an optional concise explanation for the transition.</summary>
    public string? Note { get; set; }
}
