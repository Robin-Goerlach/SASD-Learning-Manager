using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Domain.Providers;

namespace SASD.LearningManager.Application.Providers;

/// <summary>
/// Implements provider-related use cases. The service deliberately keeps provider management
/// separate from concrete learning-platform integrations.
/// </summary>
public sealed class ProviderService
{
    private readonly IProviderRepository _repository;
    private readonly IClock _clock;

    public ProviderService(IProviderRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task<IReadOnlyList<ProviderListItemDto>> ListAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
        => _repository.ListAsync(includeArchived, cancellationToken);

    public Task<Provider?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    /// <summary>Creates a provider after enforcing a user-friendly unique-name rule.</summary>
    public async Task<Guid> CreateAsync(ProviderEditModel model, CancellationToken cancellationToken = default)
    {
        var normalizedName = model.Name.Trim();
        if (await _repository.NameExistsAsync(normalizedName, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A provider named '{normalizedName}' already exists.");
        }

        var provider = Provider.Create(model.Name, model.WebsiteUrl, model.Description, model.Type, _clock.UtcNow);
        await _repository.InsertAsync(provider, cancellationToken).ConfigureAwait(false);
        return provider.Id;
    }

    /// <summary>Updates an existing provider.</summary>
    public async Task UpdateAsync(Guid id, ProviderEditModel model, CancellationToken cancellationToken = default)
    {
        var provider = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var normalizedName = model.Name.Trim();
        if (await _repository.NameExistsAsync(normalizedName, id, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A provider named '{normalizedName}' already exists.");
        }

        provider.Update(model.Name, model.WebsiteUrl, model.Description, model.Type, _clock.UtcNow);
        await _repository.UpdateAsync(provider, cancellationToken).ConfigureAwait(false);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        provider.Archive(_clock.UtcNow);
        await _repository.UpdateAsync(provider, cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        provider.Restore(_clock.UtcNow);
        await _repository.UpdateAsync(provider, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Provider> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Provider '{id}' was not found.");
    }
}
