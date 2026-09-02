using SASD.LearningManager.Domain.Knowledge;

namespace SASD.LearningManager.Application.Knowledge;

public sealed record KnowledgeArtifactEditModel(string Title, string Markdown, KnowledgeArtifactType Type,
    IReadOnlyCollection<Guid> ResourceIds, IReadOnlyCollection<Guid> SkillIds, IReadOnlyCollection<Guid> TopicIds,
    IReadOnlyCollection<Guid> GoalIds, IReadOnlyCollection<Guid> LearningPathIds);
public sealed record KnowledgeArtifactListItemDto(Guid Id, string Title, KnowledgeArtifactType Type, KnowledgeArtifactStatus Status, DateTimeOffset UpdatedAtUtc);
public sealed record KnowledgeArtifactDetailDto(Guid Id, string Title, string Markdown, KnowledgeArtifactType Type, KnowledgeArtifactStatus Status,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc, IReadOnlyList<Guid> ResourceIds, IReadOnlyList<Guid> SkillIds,
    IReadOnlyList<Guid> TopicIds, IReadOnlyList<Guid> GoalIds, IReadOnlyList<Guid> LearningPathIds);
public sealed record KnowledgeSearchCriteria(string? SearchText, bool IncludeArchived, int PageNumber = 1, int PageSize = 100);
