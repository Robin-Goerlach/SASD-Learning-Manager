using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Stores one normalized communication item handed to the application tracker. The entity deliberately
/// contains only the normalized text and addressing data needed for the job-search workflow; mailbox
/// credentials, MIME payloads, attachments, and protocol-specific state remain the responsibility of
/// the SASD Mail Workbench or another source system.
/// </summary>
public sealed class CommunicationMessage
{
    /// <summary>Gets or sets the stable local identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the source system, for example "SASD Mail Workbench" or "Clipboard".</summary>
    public string SourceSystem { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional source-system message identifier used for deterministic deduplication.</summary>
    public string? ExternalMessageId { get; set; }

    /// <summary>Gets or sets the SHA-256 fingerprint used when no stable external message id is available.</summary>
    public string FingerprintSha256 { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the message was incoming or outgoing.</summary>
    public CommunicationDirection Direction { get; set; }

    /// <summary>Gets or sets the job-search classification of the message.</summary>
    public CommunicationKind Kind { get; set; }

    /// <summary>Gets or sets the current processing state inside the application tracker.</summary>
    public CommunicationStatus Status { get; set; }

    /// <summary>Gets or sets the display name of the sender when supplied by the source system.</summary>
    public string? FromName { get; set; }

    /// <summary>Gets or sets the normalized sender address when available.</summary>
    public string? FromAddress { get; set; }

    /// <summary>Gets or sets a compact textual representation of the recipients.</summary>
    public string? ToAddresses { get; set; }

    /// <summary>Gets or sets the message subject.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized plain-text message body. Raw MIME source and attachments are not
    /// duplicated into this database because the Mail Workbench remains the source of truth for them.
    /// </summary>
    public string BodyText { get; set; } = string.Empty;

    /// <summary>Gets or sets when the message itself was sent or received.</summary>
    public DateTimeOffset MessageAtUtc { get; set; }

    /// <summary>Gets or sets an optional human-readable source reference such as a folder/message location.</summary>
    public string? SourceReference { get; set; }

    /// <summary>Gets or sets the related opportunity when a deterministic or user-confirmed match exists.</summary>
    public Guid? OpportunityId { get; set; }

    /// <summary>Gets or sets the related concrete application when a deterministic or user-confirmed match exists.</summary>
    public Guid? ApplicationId { get; set; }

    /// <summary>Gets or sets the matched professional contact when available.</summary>
    public Guid? ContactId { get; set; }

    /// <summary>Gets or sets the matched organization when available.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets the activity created from this message, preventing duplicate timeline entries.</summary>
    public Guid? ActivityId { get; set; }

    /// <summary>Gets or sets when the message was first imported into the tracker.</summary>
    public DateTimeOffset ImportedAtUtc { get; set; }

    /// <summary>Gets or sets when local processing metadata was last changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>Updates the confirmed job-search context of this communication.</summary>
    public void LinkContext(
        Guid? opportunityId,
        Guid? applicationId,
        Guid? contactId,
        Guid? organizationId,
        DateTimeOffset changedAtUtc)
    {
        OpportunityId = opportunityId;
        ApplicationId = applicationId;
        ContactId = contactId;
        OrganizationId = organizationId;
        Status = ActivityId is not null || opportunityId is not null || applicationId is not null || contactId is not null || organizationId is not null
            ? CommunicationStatus.Linked
            : CommunicationStatus.Imported;
        UpdatedAtUtc = changedAtUtc;
    }

    /// <summary>Associates the timeline activity generated from this communication.</summary>
    public void AttachActivity(Guid activityId, DateTimeOffset changedAtUtc)
    {
        ActivityId = activityId;
        Status = CommunicationStatus.Linked;
        UpdatedAtUtc = changedAtUtc;
    }

    /// <summary>Marks the imported message as intentionally irrelevant for the application tracker.</summary>
    public void Ignore(DateTimeOffset changedAtUtc)
    {
        Status = CommunicationStatus.Ignored;
        UpdatedAtUtc = changedAtUtc;
    }
}
