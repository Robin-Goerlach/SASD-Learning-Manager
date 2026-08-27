using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Domain.Providers;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Application.Resources;

/// <summary>
/// Coordinates resource-library use cases. It protects the canonical-resource rule, validates
/// provider references and keeps URL duplicate detection outside the WinForms layer.
/// </summary>
public sealed class ResourceService
{
    private readonly IResourceRepository _repository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUrlNormalizer _urlNormalizer;
    private readonly IExternalLinkLauncher _externalLinkLauncher;
    private readonly IClock _clock;

    public ResourceService(
        IResourceRepository repository,
        IProviderRepository providerRepository,
        IUrlNormalizer urlNormalizer,
        IExternalLinkLauncher externalLinkLauncher,
        IClock clock)
    {
        _repository = repository;
        _providerRepository = providerRepository;
        _urlNormalizer = urlNormalizer;
        _externalLinkLauncher = externalLinkLauncher;
        _clock = clock;
    }

    public Task<PagedResult<ResourceListItemDto>> SearchAsync(ResourceSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (criteria.PageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria), "Page number must be at least one.");
        }

        if (criteria.PageSize is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria), "Page size must be between 1 and 500.");
        }

        return _repository.SearchAsync(criteria, cancellationToken);
    }

    public Task<ResourceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetDetailAsync(id, cancellationToken);

    /// <summary>
    /// Captures a URL with the smallest useful data set and places it in the Inbox. Full
    /// classification is intentionally deferred so capture remains fast and interruption-free.
    /// </summary>
    public async Task<Guid> QuickCaptureAsync(QuickCaptureModel model, bool allowDuplicateUrl = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var normalizedUrl = _urlNormalizer.Normalize(model.Url)
            ?? throw new ArgumentException("Quick Capture requires a valid HTTP/HTTPS URL.", nameof(model));

        if (!allowDuplicateUrl)
        {
            await EnsureUrlIsUniqueAsync(normalizedUrl, null, cancellationToken).ConfigureAwait(false);
        }

        var title = string.IsNullOrWhiteSpace(model.Title)
            ? "(Titel noch nicht ermittelt)"
            : model.Title.Trim();

        var resource = Resource.Create(
            title,
            ResourceType.Other,
            null,
            model.Url,
            normalizedUrl,
            null,
            null,
            string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
            null,
            null,
            null,
            null,
            ResourceDifficulty.Unknown,
            ResourcePriority.Normal,
            ResourceStatus.Inbox,
            _clock.UtcNow);

        await _repository.InsertAsync(resource, [], cancellationToken).ConfigureAwait(false);
        return resource.Id;
    }

    /// <summary>Returns the dedicated Inbox projection used by the classification workspace.</summary>
    public Task<PagedResult<InboxListItemDto>> SearchInboxAsync(InboxSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (criteria.PageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria), "Page number must be at least one.");
        }

        if (criteria.PageSize is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria), "Page size must be between 1 and 500.");
        }

        return _repository.SearchInboxAsync(criteria, cancellationToken);
    }

    /// <summary>Creates one canonical resource and its normalized tag assignments.</summary>
    public async Task<Guid> CreateAsync(ResourceEditModel model, bool allowDuplicateUrl = false, CancellationToken cancellationToken = default)
    {
        EnsureEditableStatus(model.Status);
        await ValidateProviderAsync(model.ProviderId, cancellationToken).ConfigureAwait(false);
        var normalizedUrl = _urlNormalizer.Normalize(model.Url);
        if (!allowDuplicateUrl)
        {
            await EnsureUrlIsUniqueAsync(normalizedUrl, null, cancellationToken).ConfigureAwait(false);
        }

        var resource = Resource.Create(
            model.Title,
            model.Type,
            model.ProviderId,
            model.Url,
            normalizedUrl,
            model.LocalPath,
            model.Description,
            model.WhySaved,
            model.Creator,
            model.LanguageCode,
            model.VersionText,
            model.EstimatedMinutes,
            model.Difficulty,
            model.Priority,
            model.Status,
            _clock.UtcNow);

        if (model.ProgressPercent is not null && model.Status != ResourceStatus.Completed)
        {
            resource.SetProgress(model.ProgressPercent, _clock.UtcNow);
        }

        await _repository.InsertAsync(resource, NormalizeTags(model.Tags), cancellationToken).ConfigureAwait(false);
        return resource.Id;
    }

    /// <summary>Updates resource metadata, progress and lifecycle in one persistence operation.</summary>
    public async Task UpdateAsync(Guid id, ResourceEditModel model, bool allowDuplicateUrl = false, CancellationToken cancellationToken = default)
    {
        EnsureEditableStatus(model.Status);
        var resource = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Resource '{id}' was not found.");

        if (resource.Status == ResourceStatus.Archived)
        {
            throw new InvalidOperationException("Restore the resource before editing it.");
        }

        await ValidateProviderAsync(model.ProviderId, cancellationToken).ConfigureAwait(false);
        var normalizedUrl = _urlNormalizer.Normalize(model.Url);
        if (!allowDuplicateUrl)
        {
            await EnsureUrlIsUniqueAsync(normalizedUrl, id, cancellationToken).ConfigureAwait(false);
        }

        resource.UpdateMetadata(
            model.Title,
            model.Type,
            model.ProviderId,
            model.Url,
            normalizedUrl,
            model.LocalPath,
            model.Description,
            model.WhySaved,
            model.Creator,
            model.LanguageCode,
            model.VersionText,
            model.EstimatedMinutes,
            model.Difficulty,
            model.Priority,
            _clock.UtcNow);

        if (resource.Status != model.Status)
        {
            resource.ChangeStatus(model.Status, _clock.UtcNow);
        }

        if (model.Status != ResourceStatus.Completed)
        {
            resource.SetProgress(model.ProgressPercent, _clock.UtcNow);
        }

        await _repository.UpdateAsync(resource, NormalizeTags(model.Tags), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Completes the Inbox classification workflow. The operation requires an Inbox resource and
    /// a non-Inbox target status so a successful classification cannot silently remain unfinished.
    /// </summary>
    public async Task ClassifyInboxAsync(Guid id, ResourceEditModel model, bool allowDuplicateUrl = false, CancellationToken cancellationToken = default)
    {
        var resource = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (resource.Status != ResourceStatus.Inbox)
        {
            throw new InvalidOperationException("Only Inbox resources can be classified through this use case.");
        }

        if (model.Status == ResourceStatus.Inbox)
        {
            throw new InvalidOperationException("Choose a learning status other than Inbox to finish classification.");
        }

        await UpdateAsync(id, model, allowDuplicateUrl, cancellationToken).ConfigureAwait(false);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        resource.Archive(_clock.UtcNow);
        var tags = await _repository.GetTagsAsync(id, cancellationToken).ConfigureAwait(false);
        await _repository.UpdateAsync(resource, tags, cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        resource.Restore(_clock.UtcNow);
        var tags = await _repository.GetTagsAsync(id, cancellationToken).ConfigureAwait(false);
        await _repository.UpdateAsync(resource, tags, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Opens the resource URL after re-validating the scheme at the application boundary.</summary>
    public async Task OpenUrlAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(resource.Url) ||
            !Uri.TryCreate(resource.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("The resource does not contain a valid HTTP/HTTPS URL.");
        }

        _externalLinkLauncher.Open(uri);
    }

    private async Task<Resource> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Resource '{id}' was not found.");

    private async Task ValidateProviderAsync(Guid? providerId, CancellationToken cancellationToken)
    {
        if (providerId is null)
        {
            return;
        }

        var provider = await _providerRepository.GetByIdAsync(providerId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The selected provider no longer exists.");

        if (provider.Status == ProviderStatus.Archived)
        {
            throw new InvalidOperationException("An archived provider cannot be assigned to an active resource.");
        }
    }

    private async Task EnsureUrlIsUniqueAsync(string? normalizedUrl, Guid? currentId, CancellationToken cancellationToken)
    {
        if (normalizedUrl is null)
        {
            return;
        }

        var existing = await _repository.FindByNormalizedUrlAsync(normalizedUrl, currentId, cancellationToken).ConfigureAwait(false);
        if (existing is not null && existing.Id != currentId)
        {
            throw new DuplicateResourceException(existing.Id, existing.Title);
        }
    }

    private static void EnsureEditableStatus(ResourceStatus status)
    {
        if (status == ResourceStatus.Archived)
        {
            throw new InvalidOperationException("Archive resources through the dedicated archive operation.");
        }
    }

    private static IReadOnlyCollection<string> NormalizeTags(IEnumerable<string> tags)
    {
        return tags
            .Select(static tag => tag.Trim())
            .Where(static tag => tag.Length > 0)
            .Select(static tag => tag.Length <= 100 ? tag : throw new ArgumentException("Tags must not exceed 100 characters."))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static tag => tag, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
