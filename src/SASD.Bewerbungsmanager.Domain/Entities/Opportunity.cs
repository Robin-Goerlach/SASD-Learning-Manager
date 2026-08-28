using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents an interesting professional opportunity. It intentionally exists before and
/// independently from a concrete <see cref="Application"/>.
/// </summary>
public sealed class Opportunity
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the organization that acts as the actual or expected employer.
    /// A recruiting intermediary is modeled separately.
    /// </summary>
    public Guid? EmployerOrganizationId { get; set; }

    /// <summary>Gets or sets the recruiting or staffing intermediary, when one is involved.</summary>
    public Guid? IntermediaryOrganizationId { get; set; }

    /// <summary>Gets or sets the position title as it was understood when captured.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the captured role description. This is a snapshot rather than a live
    /// dependency on a job-board URL so later changes or deletions of the advertisement do not
    /// destroy the historical context.
    /// </summary>
    public string DescriptionSnapshot { get; set; } = string.Empty;

    /// <summary>Gets or sets the location text from the opportunity.</summary>
    public string? Location { get; set; }

    /// <summary>Gets or sets the human-readable remote or hybrid arrangement.</summary>
    public string? RemoteText { get; set; }

    /// <summary>Gets or sets the salary information exactly as it is useful for the process.</summary>
    public string? SalaryText { get; set; }

    /// <summary>Gets or sets the current coarse opportunity status.</summary>
    public OpportunityStatus Status { get; set; }

    /// <summary>Gets or sets when the opportunity was first found.</summary>
    public DateTimeOffset FoundAtUtc { get; set; }

    /// <summary>Gets or sets the publication time if it is known.</summary>
    public DateTimeOffset? PublishedAtUtc { get; set; }

    /// <summary>Gets or sets an application deadline if one is known.</summary>
    public DateTimeOffset? DeadlineAtUtc { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
