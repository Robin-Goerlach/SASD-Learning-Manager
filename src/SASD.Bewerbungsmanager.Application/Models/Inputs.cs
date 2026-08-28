using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Models;

/// <summary>Input used to create or update an organization.</summary>
public sealed record OrganizationInput(string Name, OrganizationType Type, string? Website, string? Notes);

/// <summary>Input used to create or update a professional contact.</summary>
public sealed record ContactInput(Guid? OrganizationId, string FullName, string? Role, string? Email, string? Phone, string? LinkedInUrl, string? Notes);

/// <summary>Input used to create or update an opportunity.</summary>
public sealed record OpportunityInput(
    Guid? EmployerOrganizationId,
    Guid? IntermediaryOrganizationId,
    string Title,
    string DescriptionSnapshot,
    string? Location,
    string? RemoteText,
    string? SalaryText,
    OpportunityStatus Status,
    DateTimeOffset FoundAtUtc,
    DateTimeOffset? PublishedAtUtc,
    DateTimeOffset? DeadlineAtUtc);

/// <summary>Input used to attach an external source to an opportunity.</summary>
public sealed record SourceLinkInput(string Source, string Url, string? ExternalId);

/// <summary>Input used to create a concrete application.</summary>
public sealed record ApplicationInput(
    Guid OpportunityId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    ApplicationStage Stage,
    ApplicationChannel Channel,
    string? SalaryExpectation);

/// <summary>Input used to correct or add the actual submission metadata of an application.</summary>
public sealed record ApplicationSubmissionInput(DateTimeOffset? SubmittedAtUtc, ApplicationChannel Channel);

/// <summary>Input used to create an activity or appointment.</summary>
public sealed record ActivityInput(
    Guid? OpportunityId,
    Guid? ApplicationId,
    Guid? ContactId,
    Guid? OrganizationId,
    ActivityKind Kind,
    ActivityStatus Status,
    string Subject,
    string? Notes,
    DateTimeOffset? OccurredAtUtc,
    DateTimeOffset? ScheduledAtUtc);

/// <summary>Input used to create an ACTION or WAITING_FOR item.</summary>
public sealed record WorkItemInput(
    Guid? OpportunityId,
    Guid? ApplicationId,
    Guid? ContactId,
    Guid? OrganizationId,
    WorkItemKind Kind,
    string Title,
    string? Notes,
    DateTimeOffset? DueAtUtc);

/// <summary>Input used to create or update a manually checked job-search source.</summary>
public sealed record SearchProfileInput(
    string Name,
    string Source,
    string Url,
    int CheckIntervalDays,
    DateTimeOffset NextCheckAtUtc,
    bool IsActive,
    string? Notes);

/// <summary>Input used to register an existing file as a known document version.</summary>
public sealed record DocumentInput(
    DocumentType Type,
    string Label,
    string Version,
    string Language,
    string? Tags,
    string OriginalPath);

/// <summary>Read model for the small status summary shown above the operational cockpit.</summary>
public sealed record DashboardSummary(int ActiveOpportunities, int Applications, int Interviews, int Offers);

/// <summary>
/// Operational read model for the Today page. Lists remain separated by meaning so the UI can
/// immediately answer what is overdue, what the user must do, who is expected to respond, what
/// appointments are coming up, and which searches should be checked.
/// </summary>
public sealed record TodayOverview(
    IReadOnlyList<TrackerTask> OverdueActions,
    IReadOnlyList<TrackerTask> DueActions,
    IReadOnlyList<TrackerTask> WaitingFor,
    IReadOnlyList<Activity> UpcomingAppointments,
    IReadOnlyList<SearchProfile> DueSearchProfiles);
