using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents one chronological interaction, note, or appointment in the job-search process.
/// Activities can either document something that already happened or represent a planned event.
/// </summary>
public sealed class Activity
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the related opportunity, when the activity belongs to a specific role.</summary>
    public Guid? OpportunityId { get; set; }

    /// <summary>Gets or sets the related concrete application, when applicable.</summary>
    public Guid? ApplicationId { get; set; }

    /// <summary>Gets or sets the related professional contact, when applicable.</summary>
    public Guid? ContactId { get; set; }

    /// <summary>Gets or sets the related organization, when applicable.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets the type of interaction or appointment.</summary>
    public ActivityKind Kind { get; set; }

    /// <summary>Gets or sets the current lifecycle state of the activity.</summary>
    public ActivityStatus Status { get; set; }

    /// <summary>Gets or sets a concise title suitable for timeline and today views.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Gets or sets optional free-form notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets when a historical activity occurred.</summary>
    public DateTimeOffset? OccurredAtUtc { get; set; }

    /// <summary>Gets or sets when a planned activity is scheduled.</summary>
    public DateTimeOffset? ScheduledAtUtc { get; set; }

    /// <summary>Gets or sets when a planned activity was marked completed.</summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Marks a planned activity as completed. Historical records are already complete by nature,
    /// therefore calling this method for them is rejected instead of silently changing semantics.
    /// </summary>
    /// <param name="completedAtUtc">Time at which the planned activity was completed.</param>
    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (Status != ActivityStatus.Planned)
        {
            throw new InvalidOperationException("Nur geplante Aktivitäten können abgeschlossen werden.");
        }

        Status = ActivityStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        OccurredAtUtc ??= ScheduledAtUtc ?? completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }

    /// <summary>Marks a planned activity as cancelled.</summary>
    /// <param name="cancelledAtUtc">Time at which the appointment was cancelled.</param>
    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        if (Status != ActivityStatus.Planned)
        {
            throw new InvalidOperationException("Nur geplante Aktivitäten können abgesagt werden.");
        }

        Status = ActivityStatus.Cancelled;
        UpdatedAtUtc = cancelledAtUtc;
    }
}
