using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Skills;

/// <summary>Optimized projection for the skill library.</summary>
public sealed record SkillListItemDto(
    Guid Id,
    string Name,
    int? CurrentLevel,
    int? TargetLevel,
    int? Gap,
    string CompetencyAreas,
    string Topics,
    SkillStatus Status,
    DateTimeOffset UpdatedAtUtc);

/// <summary>Compact skill option used by goal and relationship editors.</summary>
public sealed record SkillLookupDto(Guid Id, string Name, SkillStatus Status, int? CurrentLevel, int? TargetLevel);

/// <summary>Full read model for the skill editor.</summary>
public sealed record SkillDetailDto(
    Guid Id,
    string Name,
    string? Description,
    int? CurrentLevel,
    int? TargetLevel,
    SkillStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ArchivedAtUtc,
    IReadOnlyList<Guid> CompetencyAreaIds,
    IReadOnlyList<Guid> TopicIds);

/// <summary>Editable skill metadata and taxonomy relations.</summary>
public sealed record SkillEditModel(
    string Name,
    string? Description,
    int? TargetLevel,
    SkillStatus Status,
    IReadOnlyCollection<Guid> CompetencyAreaIds,
    IReadOnlyCollection<Guid> TopicIds);

/// <summary>Point-in-time skill assessment input.</summary>
public sealed record SkillAssessmentModel(int Level, SkillAssessmentType Type, string? Reason);

/// <summary>Assessment-history projection displayed in skill detail/review flows.</summary>
public sealed record SkillAssessmentListItemDto(
    Guid Id,
    int Level,
    SkillAssessmentType Type,
    string? Reason,
    DateTimeOffset AssessedAtUtc);

/// <summary>Filter and paging options for the skill library.</summary>
public sealed record SkillSearchCriteria(
    string? SearchText,
    SkillStatus? Status,
    bool IncludeArchived,
    int PageNumber = 1,
    int PageSize = 100);
