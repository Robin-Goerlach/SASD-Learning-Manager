using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Models;

/// <summary>
/// One actually submitted application inside a period-based application evidence report.
/// </summary>
public sealed record ApplicationEvidenceItem(
    Guid ApplicationId,
    DateTimeOffset SubmittedAtUtc,
    string Position,
    string Employer,
    string? Location,
    ApplicationChannel Channel,
    ApplicationStage Stage,
    string Sources);

/// <summary>
/// Read-only period report used for official or personal proof of submitted applications.
/// </summary>
public sealed record ApplicationEvidenceReport(
    DateOnly FromDate,
    DateOnly ToDate,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<ApplicationEvidenceItem> Items);

/// <summary>Source reference included in an application exchange dossier.</summary>
public sealed record ApplicationDossierSource(string Source, string Url, string? ExternalId);

/// <summary>Professional contact included in an application exchange dossier.</summary>
public sealed record ApplicationDossierContact(string FullName, string? Role, string? Email, string? Phone, string? LinkedInUrl);

/// <summary>Timeline entry included in an application exchange dossier.</summary>
public sealed record ApplicationDossierActivity(
    ActivityKind Kind,
    ActivityStatus Status,
    string Subject,
    string? Notes,
    DateTimeOffset? OccurredAtUtc,
    DateTimeOffset? ScheduledAtUtc);

/// <summary>Operational work item included in an application exchange dossier.</summary>
public sealed record ApplicationDossierTask(
    WorkItemKind Kind,
    WorkItemStatus Status,
    string Title,
    string? Notes,
    DateTimeOffset? DueAtUtc);

/// <summary>
/// Metadata of a document version actually used for an application. Local archive paths are
/// deliberately omitted so a shared dossier does not disclose workstation-specific directories.
/// </summary>
public sealed record ApplicationDossierDocument(
    DocumentType Type,
    string Label,
    string Version,
    string Language,
    string Sha256,
    DateTimeOffset CapturedAtUtc);

/// <summary>
/// Portable, privacy-conscious dossier for exchanging one application with another tool or person.
/// It contains structured business context but intentionally excludes local absolute file paths and
/// document file contents.
/// </summary>
public sealed record ApplicationExchangeDossier(
    int SchemaVersion,
    DateTimeOffset ExportedAtUtc,
    Guid ApplicationId,
    Guid OpportunityId,
    string Position,
    string Employer,
    string Intermediary,
    ApplicationStage Stage,
    ApplicationChannel Channel,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    string? SalaryExpectation,
    string? Location,
    string? RemoteText,
    string? SalaryText,
    string RoleDescriptionSnapshot,
    IReadOnlyList<ApplicationDossierSource> Sources,
    IReadOnlyList<ApplicationDossierContact> Contacts,
    IReadOnlyList<ApplicationDossierActivity> Activities,
    IReadOnlyList<ApplicationDossierTask> Tasks,
    IReadOnlyList<ApplicationDossierDocument> Documents);
