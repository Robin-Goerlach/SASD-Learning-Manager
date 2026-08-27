using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Providers;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Application.Tests;

internal sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
}

internal sealed class FakeLinkLauncher : IExternalLinkLauncher
{
    public Uri? LastOpened { get; private set; }
    public void Open(Uri uri) => LastOpened = uri;
}

internal sealed class FakeProviderRepository : IProviderRepository
{
    public Dictionary<Guid, Provider> Items { get; } = [];

    public Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<IReadOnlyList<ProviderListItemDto>> ListAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ProviderListItemDto>>(Items.Values
            .Where(x => includeArchived || x.Status != ProviderStatus.Archived)
            .Select(x => new ProviderListItemDto(x.Id, x.Name, x.Type, x.Status, x.WebsiteUrl)).ToArray());

    public Task<bool> NameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.Any(x => x.Id != excludingId && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task InsertAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        Items.Add(provider.Id, provider);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        Items[provider.Id] = provider;
        return Task.CompletedTask;
    }
}

internal sealed class FakeResourceRepository : IResourceRepository
{
    public Dictionary<Guid, Resource> Items { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<string>> Tags { get; } = [];

    public Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<ResourceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<ResourceDetailDto?>(null);

    public Task<Resource?> FindByNormalizedUrlAsync(string normalizedUrl, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.FirstOrDefault(x => x.Id != excludingId && string.Equals(x.NormalizedUrl, normalizedUrl, StringComparison.OrdinalIgnoreCase)));

    public Task<PagedResult<ResourceListItemDto>> SearchAsync(ResourceSearchCriteria criteria, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<ResourceListItemDto>([], criteria.PageNumber, criteria.PageSize, 0));

    public Task<PagedResult<InboxListItemDto>> SearchInboxAsync(InboxSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var items = Items.Values
            .Where(static x => x.Status == ResourceStatus.Inbox)
            .OrderByDescending(static x => x.CreatedAtUtc)
            .Select(static x => new InboxListItemDto(x.Id, x.Title, x.Url, null, x.Type, x.WhySaved, x.CreatedAtUtc))
            .ToArray();
        return Task.FromResult(new PagedResult<InboxListItemDto>(items, criteria.PageNumber, criteria.PageSize, items.Length));
    }

    public Task InsertAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default)
    {
        Items.Add(resource.Id, resource);
        Tags[resource.Id] = tags;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default)
    {
        Items[resource.Id] = resource;
        Tags[resource.Id] = tags;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetTagsAsync(Guid resourceId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(Tags.TryGetValue(resourceId, out var tags) ? tags.ToArray() : []);
}
