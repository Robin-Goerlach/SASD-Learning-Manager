using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Domain.LearningPaths;

namespace SASD.LearningManager.Application.LearningPaths;

/// <summary>Persistence and query port for learning paths, tree nodes and node relationships.</summary>
public interface ILearningPathRepository
{
    Task<LearningPath?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LearningPathDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<LearningPathListItemDto>> SearchAsync(LearningPathSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task InsertAsync(LearningPath path, IReadOnlyCollection<Guid> goalIds, CancellationToken cancellationToken = default);
    Task UpdateAsync(LearningPath path, IReadOnlyCollection<Guid> goalIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LearningPathNode>> ListNodesAsync(Guid learningPathId, bool includeArchived, CancellationToken cancellationToken = default);
    Task<LearningPathNode?> GetNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LearningPathNodeDetailDto?> GetNodeDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task InsertNodeAsync(LearningPathNode node, IReadOnlyCollection<Guid> skillIds, IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken = default);
    Task UpdateNodeAsync(LearningPathNode node, IReadOnlyCollection<Guid> skillIds, IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken = default);
    Task UpdateNodeOrdersAsync(IReadOnlyCollection<LearningPathNodeOrderUpdate> updates, CancellationToken cancellationToken = default);
    Task ArchiveNodesAsync(IReadOnlyCollection<Guid> nodeIds, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LearningPathNodeRelationDto>> ListRelationsAsync(Guid learningPathId, CancellationToken cancellationToken = default);
    Task<bool> RelationExistsAsync(Guid sourceNodeId, Guid targetNodeId, LearningPathNodeRelationType type, CancellationToken cancellationToken = default);
    Task InsertRelationAsync(LearningPathNodeRelation relation, CancellationToken cancellationToken = default);
    Task DeleteRelationAsync(Guid relationId, CancellationToken cancellationToken = default);
}
