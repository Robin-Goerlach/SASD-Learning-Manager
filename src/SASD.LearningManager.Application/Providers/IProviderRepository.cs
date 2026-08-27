using SASD.LearningManager.Domain.Providers;

namespace SASD.LearningManager.Application.Providers;

/// <summary>Persistence port for the provider aggregate.</summary>
public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProviderListItemDto>> ListAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task InsertAsync(Provider provider, CancellationToken cancellationToken = default);
    Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default);
}
