using SASD.LearningManager.Domain.Evidence;

namespace SASD.LearningManager.Application.Evidence;

public sealed record EvidenceEditModel(string Title, string? Description, EvidenceType Type, DateTimeOffset OccurredAtUtc,
    string? Url, string? LocalPath, string? Evaluation, IReadOnlyCollection<Guid> SkillIds,
    IReadOnlyCollection<Guid> ResourceIds, IReadOnlyCollection<Guid> GoalIds);
public sealed record EvidenceListItemDto(Guid Id, string Title, EvidenceType Type, DateTimeOffset OccurredAtUtc, EvidenceStatus Status, int SkillCount);
public sealed record EvidenceDetailDto(Guid Id, string Title, string? Description, EvidenceType Type, DateTimeOffset OccurredAtUtc,
    string? Url, string? LocalPath, string? Evaluation, EvidenceStatus Status, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<Guid> SkillIds, IReadOnlyList<Guid> ResourceIds, IReadOnlyList<Guid> GoalIds);
public sealed record EvidenceSearchCriteria(string? SearchText, Guid? SkillId, bool IncludeArchived, int PageNumber = 1, int PageSize = 100);
