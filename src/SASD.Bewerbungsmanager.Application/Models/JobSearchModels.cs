using SASD.Bewerbungsmanager.Domain.Entities;

namespace SASD.Bewerbungsmanager.Application.Models;

/// <summary>Versioned normalized job-search handoff used by source adapters.</summary>
public sealed record JobSourceBatch(
    int SchemaVersion,
    string SourceSystem,
    Guid? SearchProfileId,
    DateTimeOffset CapturedAtUtc,
    IReadOnlyList<JobSourceItem> Items);

/// <summary>One normalized source result before it is stored as a local job lead.</summary>
public sealed record JobSourceItem(
    string? ExternalJobId,
    string Title,
    string? OrganizationName,
    string? Location,
    string? RemoteText,
    string? SalaryText,
    string? Url,
    string? DescriptionText,
    DateTimeOffset? PublishedAtUtc);

/// <summary>Manual clipboard input for one discovered job when no file adapter is involved.</summary>
public sealed record JobLeadClipboardInput(
    Guid? SearchProfileId,
    string SourceSystem,
    string? ExternalJobId,
    string Title,
    string? OrganizationName,
    string? Location,
    string? RemoteText,
    string? SalaryText,
    string? Url,
    string? DescriptionText,
    DateTimeOffset? PublishedAtUtc);

/// <summary>Result of importing one normalized source item.</summary>
public sealed record JobLeadImportItemResult(JobLead Lead, bool WasDuplicate);

/// <summary>Aggregate result for one source-adapter import operation.</summary>
public sealed record JobLeadBatchImportResult(
    int Imported,
    int Duplicates,
    IReadOnlyList<JobLeadImportItemResult> Results);

/// <summary>Options used when promoting a discovered job into a durable opportunity.</summary>
public sealed record JobLeadOpportunityInput(
    Guid? EmployerOrganizationId,
    Guid? IntermediaryOrganizationId);
