using SASD.LearningManager.Domain.Knowledge;

namespace SASD.LearningManager.Application.Knowledge;

/// <summary>
/// Editable input for one Knowledge Artifact. Relationship collections are treated as complete
/// replacement sets by the application service and repository.
/// </summary>
public sealed record KnowledgeArtifactEditModel(
    string Title,
    string Markdown,
    KnowledgeArtifactType Type,
    IReadOnlyCollection<Guid> ResourceIds,
    IReadOnlyCollection<Guid> SkillIds,
    IReadOnlyCollection<Guid> TopicIds,
    IReadOnlyCollection<Guid> GoalIds,
    IReadOnlyCollection<Guid> LearningPathIds);

/// <summary>Compact Knowledge Artifact projection used by list and search views.</summary>
public sealed record KnowledgeArtifactListItemDto(
    Guid Id,
    string Title,
    KnowledgeArtifactType Type,
    KnowledgeArtifactStatus Status,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Complete Knowledge Artifact projection including all currently assigned resources, skills,
/// topics, goals and learning paths.
/// </summary>
public sealed record KnowledgeArtifactDetailDto(
    Guid Id,
    string Title,
    string Markdown,
    KnowledgeArtifactType Type,
    KnowledgeArtifactStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<Guid> ResourceIds,
    IReadOnlyList<Guid> SkillIds,
    IReadOnlyList<Guid> TopicIds,
    IReadOnlyList<Guid> GoalIds,
    IReadOnlyList<Guid> LearningPathIds);

/// <summary>Search and paging criteria for the Knowledge workspace.</summary>
public sealed record KnowledgeSearchCriteria(
    string? SearchText,
    bool IncludeArchived,
    int PageNumber = 1,
    int PageSize = 100);
