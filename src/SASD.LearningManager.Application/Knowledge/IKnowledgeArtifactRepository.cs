using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Domain.Knowledge;

namespace SASD.LearningManager.Application.Knowledge;

public interface IKnowledgeArtifactRepository
{
    Task<KnowledgeArtifact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeArtifactDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<KnowledgeArtifactListItemDto>> SearchAsync(KnowledgeSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task SaveAsync(KnowledgeArtifact item, KnowledgeArtifactEditModel model, bool insert, CancellationToken cancellationToken = default);
}
