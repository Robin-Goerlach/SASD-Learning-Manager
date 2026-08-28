using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents one operational next step. The <see cref="Kind"/> explicitly distinguishes between
/// work the user must perform and a response the user is waiting for from somebody else.
/// </summary>
public sealed class TrackerTask
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the related opportunity, when applicable.</summary>
    public Guid? OpportunityId { get; set; }

    /// <summary>Gets or sets the related application, when applicable.</summary>
    public Guid? ApplicationId { get; set; }

    /// <summary>Gets or sets the contact who is relevant to the next step.</summary>
    public Guid? ContactId { get; set; }

    /// <summary>Gets or sets the organization that is relevant to the next step.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets whether this is an ACTION or WAITING_FOR item.</summary>
    public WorkItemKind Kind { get; set; }

    /// <summary>Gets or sets the current lifecycle state.</summary>
    public WorkItemStatus Status { get; set; }

    /// <summary>Gets or sets the concise action or expectation.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets optional context needed to understand the item later.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets when the item should be reviewed or completed.</summary>
    public DateTimeOffset? DueAtUtc { get; set; }

    /// <summary>Gets or sets when the work item was completed.</summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>Marks this work item completed.</summary>
    /// <param name="completedAtUtc">Completion timestamp.</param>
    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (Status != WorkItemStatus.Open)
        {
            throw new InvalidOperationException("Nur offene Aufgaben können abgeschlossen werden.");
        }

        Status = WorkItemStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }

    /// <summary>Cancels this work item without marking it completed.</summary>
    /// <param name="cancelledAtUtc">Cancellation timestamp.</param>
    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        if (Status != WorkItemStatus.Open)
        {
            throw new InvalidOperationException("Nur offene Aufgaben können abgebrochen werden.");
        }

        Status = WorkItemStatus.Cancelled;
        UpdatedAtUtc = cancelledAtUtc;
    }
}
