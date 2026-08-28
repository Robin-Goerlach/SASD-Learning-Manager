using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents one discovered job-board result before the user decides whether it deserves a durable
/// <see cref="Opportunity"/>. Keeping search results separate prevents every portal hit from polluting
/// the opportunity list while still preserving where and when a result was found.
/// </summary>
public sealed class JobLead
{
    /// <summary>Gets or sets the stable local identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the search profile that produced the result, when known.</summary>
    public Guid? SearchProfileId { get; set; }

    /// <summary>Gets or sets the source/adapter name, for example LinkedIn, Bundesagentur, or Generic CSV.</summary>
    public string SourceSystem { get; set; } = string.Empty;

    /// <summary>Gets or sets the source-specific stable job identifier, when one exists.</summary>
    public string? ExternalJobId { get; set; }

    /// <summary>Gets or sets the deterministic SHA-256 fingerprint used for idempotent imports.</summary>
    public string FingerprintSha256 { get; set; } = string.Empty;

    /// <summary>Gets or sets the position title captured from the source.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the organization/employer text exactly as reported by the source.</summary>
    public string? OrganizationName { get; set; }

    /// <summary>Gets or sets the location text reported by the source.</summary>
    public string? Location { get; set; }

    /// <summary>Gets or sets the source text describing remote or hybrid work.</summary>
    public string? RemoteText { get; set; }

    /// <summary>Gets or sets salary information captured from the source.</summary>
    public string? SalaryText { get; set; }

    /// <summary>Gets or sets the canonical HTTP/HTTPS source URL when available.</summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Gets or sets the job-description text captured by the adapter. It is intentionally stored as
    /// plain text so it can later become the opportunity's durable description snapshot.
    /// </summary>
    public string? DescriptionText { get; set; }

    /// <summary>Gets or sets the source publication timestamp when known.</summary>
    public DateTimeOffset? PublishedAtUtc { get; set; }

    /// <summary>Gets or sets when the result was discovered/imported.</summary>
    public DateTimeOffset FoundAtUtc { get; set; }

    /// <summary>Gets or sets the current review state.</summary>
    public JobLeadStatus Status { get; set; }

    /// <summary>Gets or sets the opportunity created from this result, once promoted.</summary>
    public Guid? OpportunityId { get; set; }

    /// <summary>Gets or sets when this record was created locally.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>Marks this lead reviewed without yet creating an opportunity.</summary>
    /// <param name="changedAtUtc">Time of the user's review action.</param>
    public void MarkReviewed(DateTimeOffset changedAtUtc)
    {
        if (Status == JobLeadStatus.Imported)
        {
            throw new InvalidOperationException("Eine bereits übernommene Stelle kann nicht wieder auf 'geprüft' gesetzt werden.");
        }

        Status = JobLeadStatus.Reviewed;
        UpdatedAtUtc = changedAtUtc;
    }

    /// <summary>Marks this result deliberately ignored.</summary>
    /// <param name="changedAtUtc">Time of the dismissal.</param>
    public void Ignore(DateTimeOffset changedAtUtc)
    {
        if (Status == JobLeadStatus.Imported)
        {
            throw new InvalidOperationException("Eine bereits übernommene Stelle kann nicht ignoriert werden.");
        }

        Status = JobLeadStatus.Ignored;
        UpdatedAtUtc = changedAtUtc;
    }

    /// <summary>Links the discovered result to the durable opportunity created from it.</summary>
    /// <param name="opportunityId">Created opportunity identifier.</param>
    /// <param name="changedAtUtc">Time of the promotion.</param>
    public void LinkOpportunity(Guid opportunityId, DateTimeOffset changedAtUtc)
    {
        if (opportunityId == Guid.Empty)
        {
            throw new ArgumentException("Eine gültige Stellen-ID ist erforderlich.", nameof(opportunityId));
        }

        OpportunityId = opportunityId;
        Status = JobLeadStatus.Imported;
        UpdatedAtUtc = changedAtUtc;
    }
}
