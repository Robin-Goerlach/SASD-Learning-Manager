using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.LearningPaths;

/// <summary>Optimized list projection for the Learning Paths workspace.</summary>
public sealed record LearningPathListItemDto(
    Guid Id,
    string Title,
    LearningPathStatus Status,
    LearningPathPriority Priority,
    DateOnly? TargetDate,
    int NodeCount,
    int RequiredCompleted,
    int RequiredTotal,
    decimal? CoreCompletionPercent,
    string? NextActionText,
    DateTimeOffset UpdatedAtUtc);

/// <summary>Full editable path metadata plus its goal relationships and calculated progress.</summary>
public sealed record LearningPathDetailDto(
    Guid Id,
    string Title,
    string? Description,
    LearningPathStatus Status,
    LearningPathPriority Priority,
    DateOnly? PlannedStartDate,
    DateOnly? TargetDate,
    string? NextActionText,
    DateOnly? NextActionDueDate,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? ArchivedAtUtc,
    IReadOnlyList<Guid> GoalIds,
    int RequiredCompleted,
    int RequiredTotal,
    int OptionalCompleted,
    int OptionalTotal,
    decimal? CoreCompletionPercent);

/// <summary>Editable learning-path metadata.</summary>
public sealed record LearningPathEditModel(
    string Title,
    string? Description,
    LearningPathStatus Status,
    LearningPathPriority Priority,
    DateOnly? PlannedStartDate,
    DateOnly? TargetDate,
    string? NextActionText,
    DateOnly? NextActionDueDate,
    IReadOnlyCollection<Guid> GoalIds);

/// <summary>Tree projection used by the WinForms TreeView.</summary>
public sealed record LearningPathNodeListItemDto(
    Guid Id,
    Guid LearningPathId,
    Guid? ParentNodeId,
    string Title,
    LearningPathNodeType Type,
    int SortOrder,
    bool IsRequired,
    LearningPathNodeStatus Status);

/// <summary>Full node read model used by the node editor.</summary>
public sealed record LearningPathNodeDetailDto(
    Guid Id,
    Guid LearningPathId,
    Guid? ParentNodeId,
    string Title,
    string? Description,
    LearningPathNodeType Type,
    int SortOrder,
    bool IsRequired,
    LearningPathNodeStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ArchivedAtUtc,
    IReadOnlyList<Guid> SkillIds,
    IReadOnlyList<Guid> ResourceIds);

/// <summary>
/// Editable node metadata. Sort order is intentionally not exposed: creation appends to a sibling
/// group and explicit move operations own ordering, preventing arbitrary duplicate sort values.
/// </summary>
public sealed record LearningPathNodeEditModel(
    Guid? ParentNodeId,
    string Title,
    string? Description,
    LearningPathNodeType Type,
    bool IsRequired,
    LearningPathNodeStatus Status,
    IReadOnlyCollection<Guid> SkillIds,
    IReadOnlyCollection<Guid> ResourceIds);

/// <summary>Read model for a non-hierarchical node relationship.</summary>
public sealed record LearningPathNodeRelationDto(
    Guid Id,
    Guid SourceNodeId,
    string SourceTitle,
    Guid TargetNodeId,
    string TargetTitle,
    LearningPathNodeRelationType Type,
    string? Note,
    DateTimeOffset CreatedAtUtc);

/// <summary>Input model for creating a node relationship.</summary>
public sealed record LearningPathNodeRelationModel(
    Guid SourceNodeId,
    Guid TargetNodeId,
    LearningPathNodeRelationType Type,
    string? Note);

/// <summary>Filtering and paging options for the learning-path library.</summary>
public sealed record LearningPathSearchCriteria(
    string? SearchText,
    LearningPathStatus? Status,
    bool IncludeArchived,
    int PageNumber = 1,
    int PageSize = 100);

/// <summary>Small provider-independent resource option used by node assignment UI.</summary>
public sealed record PathResourceLookupDto(
    Guid Id,
    string Title,
    ResourceType Type,
    ResourceStatus Status,
    string? ProviderName);

/// <summary>Small skill option used by node assignment UI.</summary>
public sealed record PathSkillLookupDto(
    Guid Id,
    string Name,
    SkillStatus Status,
    int? CurrentLevel,
    int? TargetLevel);

/// <summary>Represents one node-order update inside a sibling group.</summary>
public sealed record LearningPathNodeOrderUpdate(Guid NodeId, Guid? ParentNodeId, int SortOrder, DateTimeOffset UpdatedAtUtc);
