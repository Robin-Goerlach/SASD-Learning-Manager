using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Application.Resources;

/// <summary>Input model shared by resource create and update operations.</summary>
public sealed record ResourceEditModel(
    string Title,
    ResourceType Type,
    Guid? ProviderId,
    string? Url,
    string? LocalPath,
    string? Description,
    string? WhySaved,
    string? Creator,
    string? LanguageCode,
    string? VersionText,
    int? EstimatedMinutes,
    ResourceDifficulty Difficulty,
    ResourcePriority Priority,
    ResourceStatus Status,
    int? ProgressPercent,
    IReadOnlyCollection<string> Tags);

/// <summary>
/// Minimal input used by Quick Capture. Deliberately keeps the capture surface small so a useful
/// resource can be preserved immediately and classified later in the Inbox.
/// </summary>
public sealed record QuickCaptureModel(
    string Url,
    string? Title,
    string? Note);

/// <summary>Optimized projection used by the resource grid.</summary>
public sealed record ResourceListItemDto(
    Guid Id,
    string Title,
    string? ProviderName,
    ResourceType Type,
    ResourceStatus Status,
    int? ProgressPercent,
    ResourcePriority Priority,
    ResourceDifficulty Difficulty,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Compact projection for the Inbox. It includes capture time and note because these fields help
/// the user remember why an otherwise unclassified URL was saved.
/// </summary>
public sealed record InboxListItemDto(
    Guid Id,
    string Title,
    string? Url,
    string? ProviderName,
    ResourceType Type,
    string? Note,
    DateTimeOffset CapturedAtUtc);

/// <summary>Compact resource option used by learning-path node editors.</summary>
public sealed record ResourceLookupDto(Guid Id, string Title, ResourceType Type, ResourceStatus Status, string? ProviderName);

/// <summary>Full read model used by the resource edit form.</summary>
public sealed record ResourceDetailDto(
    Guid Id,
    string Title,
    ResourceType Type,
    Guid? ProviderId,
    string? ProviderName,
    string? Url,
    string? LocalPath,
    string? Description,
    string? WhySaved,
    string? Creator,
    string? LanguageCode,
    string? VersionText,
    int? EstimatedMinutes,
    ResourceDifficulty Difficulty,
    ResourcePriority Priority,
    ResourceStatus Status,
    int? ProgressPercent,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ArchivedAtUtc,
    IReadOnlyList<string> Tags);

/// <summary>Filter and paging options for the resource library.</summary>
public sealed record ResourceSearchCriteria(
    string? SearchText,
    Guid? ProviderId,
    ResourceType? Type,
    ResourceStatus? Status,
    ResourcePriority? Priority,
    bool IncludeArchived,
    int PageNumber = 1,
    int PageSize = 100);

/// <summary>Filter and paging options dedicated to the Inbox.</summary>
public sealed record InboxSearchCriteria(
    string? SearchText,
    int PageNumber = 1,
    int PageSize = 100);
