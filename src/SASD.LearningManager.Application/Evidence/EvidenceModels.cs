using SASD.LearningManager.Domain.Evidence;

namespace SASD.LearningManager.Application.Evidence;

/// <summary>
/// Editable input for Evidence. Assignments are explicit because Evidence may support several
/// skills, resources and goals without changing mastery automatically.
/// </summary>
public sealed record EvidenceEditModel(
    string Title,
    string? Description,
    EvidenceType Type,
    DateTimeOffset OccurredAtUtc,
    string? Url,
    string? LocalPath,
    string? Evaluation,
    IReadOnlyCollection<Guid> SkillIds,
    IReadOnlyCollection<Guid> ResourceIds,
    IReadOnlyCollection<Guid> GoalIds);

/// <summary>Compact Evidence projection used by search/list workspaces.</summary>
public sealed record EvidenceListItemDto(
    Guid Id,
    string Title,
    EvidenceType Type,
    DateTimeOffset OccurredAtUtc,
    EvidenceStatus Status,
    int SkillCount);

/// <summary>Complete Evidence projection including all current assignments.</summary>
public sealed record EvidenceDetailDto(
    Guid Id,
    string Title,
    string? Description,
    EvidenceType Type,
    DateTimeOffset OccurredAtUtc,
    string? Url,
    string? LocalPath,
    string? Evaluation,
    EvidenceStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<Guid> SkillIds,
    IReadOnlyList<Guid> ResourceIds,
    IReadOnlyList<Guid> GoalIds);

/// <summary>Search and paging criteria for the Evidence workspace.</summary>
public sealed record EvidenceSearchCriteria(
    string? SearchText,
    Guid? SkillId,
    bool IncludeArchived,
    int PageNumber = 1,
    int PageSize = 100);
