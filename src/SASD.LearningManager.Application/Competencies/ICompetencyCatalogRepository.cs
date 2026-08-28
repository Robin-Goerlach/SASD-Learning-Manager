using SASD.LearningManager.Domain.Competencies;

namespace SASD.LearningManager.Application.Competencies;

/// <summary>Persistence/query port for competency areas and topics.</summary>
public interface ICompetencyCatalogRepository
{
    Task<IReadOnlyList<CompetencyAreaListItemDto>> ListAreasAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<CompetencyArea?> GetAreaByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AreaNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task InsertAreaAsync(CompetencyArea area, CancellationToken cancellationToken = default);
    Task UpdateAreaAsync(CompetencyArea area, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopicListItemDto>> ListTopicsAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<Topic?> GetTopicByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TopicDetailDto?> GetTopicDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> TopicNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task InsertTopicAsync(Topic topic, IReadOnlyCollection<Guid> competencyAreaIds, CancellationToken cancellationToken = default);
    Task UpdateTopicAsync(Topic topic, IReadOnlyCollection<Guid> competencyAreaIds, CancellationToken cancellationToken = default);
}
