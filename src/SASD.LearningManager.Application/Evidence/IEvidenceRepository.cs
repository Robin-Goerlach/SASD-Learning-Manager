using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Domain.Evidence;

namespace SASD.LearningManager.Application.Evidence;

public interface IEvidenceRepository
{
    Task<EvidenceItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EvidenceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<EvidenceListItemDto>> SearchAsync(EvidenceSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task SaveAsync(EvidenceItem item, EvidenceEditModel model, bool insert, CancellationToken cancellationToken = default);
}
