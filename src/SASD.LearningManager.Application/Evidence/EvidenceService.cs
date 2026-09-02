using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;

namespace SASD.LearningManager.Application.Evidence;

/// <summary>
/// Coordinates Evidence use cases. Evidence can support skills, resources and goals, but the
/// service deliberately does not alter Skill mastery; mastery remains an explicit assessment act.
/// </summary>
public sealed class EvidenceService
{
    private readonly IEvidenceRepository _repository;
    private readonly IClock _clock;

    /// <summary>Creates an Evidence application service.</summary>
    public EvidenceService(IEvidenceRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    /// <summary>Searches Evidence records with validated paging.</summary>
    public Task<PagedResult<EvidenceListItemDto>> SearchAsync(
        EvidenceSearchCriteria criteria,
        CancellationToken cancellationToken = default)
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

    /// <summary>Loads the full Evidence projection including all current assignments.</summary>
    public Task<EvidenceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetDetailAsync(id, cancellationToken);

    /// <summary>Creates Evidence and its Skill, Resource and Goal relationships.</summary>
    public async Task<Guid> CreateAsync(EvidenceEditModel model, CancellationToken cancellationToken = default)
    {
        var evidence = Domain.Evidence.EvidenceItem.Create(
            model.Title,
            model.Description,
            model.Type,
            model.OccurredAtUtc,
            model.Url,
            model.LocalPath,
            model.Evaluation,
            _clock.UtcNow);

        await _repository.SaveAsync(evidence, Normalize(model), isNew: true, cancellationToken).ConfigureAwait(false);
        return evidence.Id;
    }

    /// <summary>Updates Evidence metadata and relationships without changing Skill assessments.</summary>
    public async Task UpdateAsync(Guid id, EvidenceEditModel model, CancellationToken cancellationToken = default)
    {
        var evidence = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        evidence.Update(
            model.Title,
            model.Description,
            model.Type,
            model.OccurredAtUtc,
            model.Url,
            model.LocalPath,
            model.Evaluation,
            _clock.UtcNow);

        await _repository.SaveAsync(evidence, Normalize(model), isNew: false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Archives Evidence while retaining its historical assignments.</summary>
    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evidence = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await GetRequiredDetailAsync(id, cancellationToken).ConfigureAwait(false);
        evidence.Archive(_clock.UtcNow);
        await _repository.SaveAsync(evidence, From(detail), isNew: false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Restores archived Evidence and preserves its previous assignments.</summary>
    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evidence = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await GetRequiredDetailAsync(id, cancellationToken).ConfigureAwait(false);
        evidence.Restore(_clock.UtcNow);
        await _repository.SaveAsync(evidence, From(detail), isNew: false, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Domain.Evidence.EvidenceItem> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Evidence '{id}' was not found.");

    private async Task<EvidenceDetailDto> GetRequiredDetailAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Evidence '{id}' detail was not found.");

    private static EvidenceEditModel Normalize(EvidenceEditModel model)
        => model with
        {
            SkillIds = NormalizeIds(model.SkillIds),
            ResourceIds = NormalizeIds(model.ResourceIds),
            GoalIds = NormalizeIds(model.GoalIds)
        };

    private static EvidenceEditModel From(EvidenceDetailDto detail)
        => new(
            detail.Title,
            detail.Description,
            detail.Type,
            detail.OccurredAtUtc,
            detail.Url,
            detail.LocalPath,
            detail.Evaluation,
            detail.SkillIds,
            detail.ResourceIds,
            detail.GoalIds);

    private static Guid[] NormalizeIds(IEnumerable<Guid> ids)
        => ids.Where(static id => id != Guid.Empty).Distinct().ToArray();
}
