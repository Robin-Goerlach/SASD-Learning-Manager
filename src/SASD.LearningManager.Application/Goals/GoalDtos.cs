using SASD.LearningManager.Domain.Goals;

namespace SASD.LearningManager.Application.Goals;

/// <summary>Optimized projection for the Goals workspace.</summary>
public sealed record GoalListItemDto(
    Guid Id,
    string Title,
    GoalType Type,
    GoalStatus Status,
    GoalPriority Priority,
    DateOnly? TargetDate,
    int SkillCount,
    string? NextActionText,
    DateOnly? NextActionDueDate,
    DateTimeOffset UpdatedAtUtc);

/// <summary>Full goal editor read model.</summary>
public sealed record GoalDetailDto(
    Guid Id,
    string Title,
    string? Description,
    GoalType Type,
    string? Motivation,
    GoalPriority Priority,
    GoalStatus Status,
    DateOnly? TargetDate,
    string? NextActionText,
    DateOnly? NextActionDueDate,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? AchievedAtUtc,
    DateTimeOffset? ArchivedAtUtc,
    IReadOnlyList<Guid> SkillIds);

/// <summary>Editable goal input including skill relationships.</summary>
public sealed record GoalEditModel(
    string Title,
    string? Description,
    GoalType Type,
    string? Motivation,
    GoalPriority Priority,
    GoalStatus Status,
    DateOnly? TargetDate,
    string? NextActionText,
    DateOnly? NextActionDueDate,
    IReadOnlyCollection<Guid> SkillIds);

/// <summary>Filter and paging options for Goals.</summary>
public sealed record GoalSearchCriteria(
    string? SearchText,
    GoalStatus? Status,
    bool IncludeArchived,
    int PageNumber = 1,
    int PageSize = 100);
