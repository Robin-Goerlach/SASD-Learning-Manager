using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Models;

/// <summary>Normalized input for one communication message.</summary>
public sealed record CommunicationImportInput(
    string SourceSystem,
    string? ExternalMessageId,
    CommunicationDirection Direction,
    CommunicationKind Kind,
    string? FromName,
    string? FromAddress,
    string? ToAddresses,
    string Subject,
    string BodyText,
    DateTimeOffset MessageAtUtc,
    string? SourceReference,
    Guid? OpportunityId = null,
    Guid? ApplicationId = null,
    Guid? ContactId = null,
    Guid? OrganizationId = null);

/// <summary>User-confirmed relation changes for a previously imported communication.</summary>
public sealed record CommunicationLinkInput(
    Guid? OpportunityId,
    Guid? ApplicationId,
    Guid? ContactId,
    Guid? OrganizationId);

/// <summary>
/// Versioned local interchange envelope used between the SASD Mail Workbench and the application
/// tracker. Version 1 intentionally transfers normalized plain text only and no attachments or raw MIME.
/// </summary>
public sealed record CommunicationHandoffBatch(
    int SchemaVersion,
    string SourceSystem,
    IReadOnlyList<CommunicationHandoffMessage> Messages);

/// <summary>One normalized message inside a versioned communication handoff batch.</summary>
public sealed record CommunicationHandoffMessage(
    string? ExternalMessageId,
    CommunicationDirection Direction,
    CommunicationKind Kind,
    string? FromName,
    string? FromAddress,
    string? ToAddresses,
    string Subject,
    string BodyText,
    DateTimeOffset MessageAtUtc,
    string? SourceReference,
    Guid? OpportunityId = null,
    Guid? ApplicationId = null,
    Guid? ContactId = null,
    Guid? OrganizationId = null);

/// <summary>Result of importing one message, including conservative automatic matching information.</summary>
public sealed record CommunicationImportResult(
    CommunicationMessage Message,
    bool WasDuplicate,
    bool ContactMatchedAutomatically,
    bool ContextMatchedAutomatically,
    bool ActivityCreatedAutomatically,
    IReadOnlyList<string> DetectedUrls);

/// <summary>Batch-level import result for a Mail Workbench handoff file.</summary>
public sealed record CommunicationBatchImportResult(
    int Imported,
    int Duplicates,
    int ActivitiesCreated,
    IReadOnlyList<CommunicationImportResult> Results);

/// <summary>Lightweight deterministic text analysis used for job-alert and clipboard workflows.</summary>
public sealed record CommunicationTextAnalysis(
    CommunicationKind SuggestedKind,
    string SuggestedTitle,
    IReadOnlyList<string> Urls);
