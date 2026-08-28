using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Domain.Competencies;

namespace SASD.LearningManager.Application.Competencies;

/// <summary>
/// Coordinates the competency taxonomy. Topics may belong to multiple competency areas, while
/// neither concept carries mastery itself; mastery belongs exclusively to skills.
/// </summary>
public sealed class CompetencyCatalogService
{
    private readonly ICompetencyCatalogRepository _repository;
    private readonly IClock _clock;

    public CompetencyCatalogService(ICompetencyCatalogRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task<IReadOnlyList<CompetencyAreaListItemDto>> ListAreasAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => _repository.ListAreasAsync(includeArchived, cancellationToken);

    public Task<IReadOnlyList<TopicListItemDto>> ListTopicsAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => _repository.ListTopicsAsync(includeArchived, cancellationToken);

    public Task<TopicDetailDto?> GetTopicDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetTopicDetailAsync(id, cancellationToken);

    public async Task<Guid> CreateAreaAsync(CompetencyAreaEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureCatalogStatusEditable(model.Status);
        if (await _repository.AreaNameExistsAsync(model.Name, null, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A competency area named '{model.Name.Trim()}' already exists.");
        }

        var area = CompetencyArea.Create(model.Name, model.Description, _clock.UtcNow);
        if (model.Status == CatalogStatus.Inactive)
        {
            area.SetActive(false, _clock.UtcNow);
        }

        await _repository.InsertAreaAsync(area, cancellationToken).ConfigureAwait(false);
        return area.Id;
    }

    public async Task UpdateAreaAsync(Guid id, CompetencyAreaEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureCatalogStatusEditable(model.Status);
        var area = await _repository.GetAreaByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Competency area '{id}' was not found.");
        if (area.Status == CatalogStatus.Archived)
        {
            throw new InvalidOperationException("Restore the competency area before editing it.");
        }

        if (await _repository.AreaNameExistsAsync(model.Name, id, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A competency area named '{model.Name.Trim()}' already exists.");
        }

        area.Update(model.Name, model.Description, _clock.UtcNow);
        area.SetActive(model.Status == CatalogStatus.Active, _clock.UtcNow);
        await _repository.UpdateAreaAsync(area, cancellationToken).ConfigureAwait(false);
    }

    public async Task ArchiveAreaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var area = await GetAreaRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        area.Archive(_clock.UtcNow);
        await _repository.UpdateAreaAsync(area, cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAreaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var area = await GetAreaRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        area.Restore(_clock.UtcNow);
        await _repository.UpdateAreaAsync(area, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Guid> CreateTopicAsync(TopicEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureCatalogStatusEditable(model.Status);
        await ValidateAreaIdsAsync(model.CompetencyAreaIds, cancellationToken).ConfigureAwait(false);
        if (await _repository.TopicNameExistsAsync(model.Name, null, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A topic named '{model.Name.Trim()}' already exists.");
        }

        var topic = Topic.Create(model.Name, model.Description, _clock.UtcNow);
        if (model.Status == CatalogStatus.Inactive)
        {
            topic.SetActive(false, _clock.UtcNow);
        }

        await _repository.InsertTopicAsync(topic, DistinctIds(model.CompetencyAreaIds), cancellationToken).ConfigureAwait(false);
        return topic.Id;
    }

    public async Task UpdateTopicAsync(Guid id, TopicEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureCatalogStatusEditable(model.Status);
        var topic = await _repository.GetTopicByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Topic '{id}' was not found.");
        if (topic.Status == CatalogStatus.Archived)
        {
            throw new InvalidOperationException("Restore the topic before editing it.");
        }

        var existingDetail = await _repository.GetTopicDetailAsync(id, cancellationToken).ConfigureAwait(false);
        await ValidateAreaIdsAsync(model.CompetencyAreaIds, cancellationToken, existingDetail?.CompetencyAreaIds ?? []).ConfigureAwait(false);
        if (await _repository.TopicNameExistsAsync(model.Name, id, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A topic named '{model.Name.Trim()}' already exists.");
        }

        topic.Update(model.Name, model.Description, _clock.UtcNow);
        topic.SetActive(model.Status == CatalogStatus.Active, _clock.UtcNow);
        await _repository.UpdateTopicAsync(topic, DistinctIds(model.CompetencyAreaIds), cancellationToken).ConfigureAwait(false);
    }

    public async Task ArchiveTopicAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var topic = await GetTopicRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        topic.Archive(_clock.UtcNow);
        var detail = await _repository.GetTopicDetailAsync(id, cancellationToken).ConfigureAwait(false);
        await _repository.UpdateTopicAsync(topic, detail?.CompetencyAreaIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreTopicAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var topic = await GetTopicRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        topic.Restore(_clock.UtcNow);
        var detail = await _repository.GetTopicDetailAsync(id, cancellationToken).ConfigureAwait(false);
        await _repository.UpdateTopicAsync(topic, detail?.CompetencyAreaIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    private async Task ValidateAreaIdsAsync(IEnumerable<Guid> areaIds, CancellationToken cancellationToken, IReadOnlyCollection<Guid>? allowedArchivedIds = null)
    {
        var allowed = allowedArchivedIds is null ? new HashSet<Guid>() : allowedArchivedIds.ToHashSet();
        foreach (var id in DistinctIds(areaIds))
        {
            var area = await _repository.GetAreaByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Competency area '{id}' does not exist.");
            if (area.Status == CatalogStatus.Archived && !allowed.Contains(id))
            {
                throw new InvalidOperationException($"Competency area '{area.Name}' is archived and cannot be newly assigned.");
            }
        }
    }

    private async Task<CompetencyArea> GetAreaRequiredAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetAreaByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Competency area '{id}' was not found.");

    private async Task<Topic> GetTopicRequiredAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetTopicByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Topic '{id}' was not found.");

    private static IReadOnlyCollection<Guid> DistinctIds(IEnumerable<Guid> ids)
        => ids.Where(static id => id != Guid.Empty).Distinct().ToArray();

    private static void EnsureCatalogStatusEditable(CatalogStatus status)
    {
        if (status == CatalogStatus.Archived)
        {
            throw new ArgumentException("Use the archive operation instead of saving an item directly as archived.");
        }
    }
}
