using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Application.Resources;

/// <summary>Persistence/query port for canonical learning resources.</summary>
public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResourceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds another resource with the same normalized URL. <paramref name="excludingId"/> is used
    /// during edits so the resource being edited never identifies itself as a duplicate.
    /// </summary>
    Task<Resource?> FindByNormalizedUrlAsync(string normalizedUrl, Guid? excludingId = null, CancellationToken cancellationToken = default);

    Task<PagedResult<ResourceListItemDto>> SearchAsync(ResourceSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<PagedResult<InboxListItemDto>> SearchInboxAsync(InboxSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task InsertAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default);
    Task UpdateAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetTagsAsync(Guid resourceId, CancellationToken cancellationToken = default);
}
