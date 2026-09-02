using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;

namespace SASD.LearningManager.Application.Knowledge;

/// <summary>
/// Coordinates Knowledge Artifact use cases. The service keeps relationship normalization and
/// lifecycle handling outside WinForms while leaving persistence details behind the repository port.
/// </summary>
public sealed class KnowledgeArtifactService
{
    private readonly IKnowledgeArtifactRepository _repository;
    private readonly IClock _clock;

    /// <summary>Creates a Knowledge Artifact application service.</summary>
    public KnowledgeArtifactService(IKnowledgeArtifactRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    /// <summary>Searches Knowledge Artifacts with validated paging.</summary>
    public Task<PagedResult<KnowledgeArtifactListItemDto>> SearchAsync(
        KnowledgeSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(criteria.PageNumber, criteria.PageSize);
        return _repository.SearchAsync(criteria, cancellationToken);
    }

    /// <summary>Loads the complete Knowledge Artifact projection including all assignments.</summary>
    public Task<KnowledgeArtifactDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetDetailAsync(id, cancellationToken);

    /// <summary>Creates one Markdown Knowledge Artifact and its selected relationships.</summary>
    public async Task<Guid> CreateAsync(
        KnowledgeArtifactEditModel model,
        CancellationToken cancellationToken = default)
    {
        var artifact = Domain.Knowledge.KnowledgeArtifact.Create(
            model.Title,
            model.Markdown,
            model.Type,
            _clock.UtcNow);

        await _repository.SaveAsync(artifact, Normalize(model), isNew: true, cancellationToken).ConfigureAwait(false);
        return artifact.Id;
    }

    /// <summary>Updates content, type and relationships of an existing active artifact.</summary>
    public async Task UpdateAsync(
        Guid id,
        KnowledgeArtifactEditModel model,
        CancellationToken cancellationToken = default)
    {
        var artifact = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        artifact.Update(model.Title, model.Markdown, model.Type, _clock.UtcNow);
        await _repository.SaveAsync(artifact, Normalize(model), isNew: false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Archives an artifact without discarding its historical relationships.</summary>
    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var artifact = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await GetRequiredDetailAsync(id, cancellationToken).ConfigureAwait(false);
        artifact.Archive(_clock.UtcNow);
        await _repository.SaveAsync(artifact, From(detail), isNew: false, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Restores an archived artifact and retains its previous relationships.</summary>
    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var artifact = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await GetRequiredDetailAsync(id, cancellationToken).ConfigureAwait(false);
        artifact.Restore(_clock.UtcNow);
        await _repository.SaveAsync(artifact, From(detail), isNew: false, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Domain.Knowledge.KnowledgeArtifact> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Knowledge artifact '{id}' was not found.");

    private async Task<KnowledgeArtifactDetailDto> GetRequiredDetailAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Knowledge artifact '{id}' detail was not found.");

    private static KnowledgeArtifactEditModel Normalize(KnowledgeArtifactEditModel model)
        => model with
        {
            ResourceIds = NormalizeIds(model.ResourceIds),
            SkillIds = NormalizeIds(model.SkillIds),
            TopicIds = NormalizeIds(model.TopicIds),
            GoalIds = NormalizeIds(model.GoalIds),
            LearningPathIds = NormalizeIds(model.LearningPathIds)
        };

    private static KnowledgeArtifactEditModel From(KnowledgeArtifactDetailDto detail)
        => new(
            detail.Title,
            detail.Markdown,
            detail.Type,
            detail.ResourceIds,
            detail.SkillIds,
            detail.TopicIds,
            detail.GoalIds,
            detail.LearningPathIds);

    private static Guid[] NormalizeIds(IEnumerable<Guid> ids)
        => ids.Where(static id => id != Guid.Empty).Distinct().ToArray();

    private static void ValidatePage(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least one.");
        }

        if (pageSize is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 500.");
        }
    }
}
