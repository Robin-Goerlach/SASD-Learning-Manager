using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Skills;

/// <summary>
/// Coordinates skill CRUD, taxonomy assignments and append-only assessments. Resource completion
/// is intentionally absent from this service: mastery can change only through an explicit assessment.
/// </summary>
public sealed class SkillService
{
    private readonly ISkillRepository _repository;
    private readonly ICompetencyCatalogRepository _catalogRepository;
    private readonly IClock _clock;

    public SkillService(ISkillRepository repository, ICompetencyCatalogRepository catalogRepository, IClock clock)
    {
        _repository = repository;
        _catalogRepository = catalogRepository;
        _clock = clock;
    }

    public Task<PagedResult<SkillListItemDto>> SearchAsync(SkillSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (criteria.PageNumber < 1) throw new ArgumentOutOfRangeException(nameof(criteria));
        if (criteria.PageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(criteria));
        return _repository.SearchAsync(criteria, cancellationToken);
    }

    public Task<SkillDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetDetailAsync(id, cancellationToken);

    public Task<IReadOnlyList<SkillLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => _repository.ListLookupAsync(includeArchived, cancellationToken);

    public Task<IReadOnlyList<SkillAssessmentListItemDto>> ListAssessmentsAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.ListAssessmentsAsync(id, cancellationToken);

    public async Task<Guid> CreateAsync(SkillEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureEditableStatus(model.Status);
        await ValidateAssignmentsAsync(model.CompetencyAreaIds, model.TopicIds, cancellationToken).ConfigureAwait(false);
        if (await _repository.NameExistsAsync(model.Name, null, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A skill named '{model.Name.Trim()}' already exists.");
        }

        var skill = Skill.Create(model.Name, model.Description, model.TargetLevel, _clock.UtcNow);
        if (model.Status == SkillStatus.Inactive)
        {
            skill.Update(skill.Name, skill.Description, skill.TargetLevel, SkillStatus.Inactive, _clock.UtcNow);
        }

        await _repository.InsertAsync(skill, DistinctIds(model.CompetencyAreaIds), DistinctIds(model.TopicIds), cancellationToken).ConfigureAwait(false);
        return skill.Id;
    }

    public async Task UpdateAsync(Guid id, SkillEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureEditableStatus(model.Status);
        var skill = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (skill.Status == SkillStatus.Archived)
        {
            throw new InvalidOperationException("Restore the skill before editing it.");
        }

        var existingDetail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        await ValidateAssignmentsAsync(model.CompetencyAreaIds, model.TopicIds, cancellationToken,
            existingDetail?.CompetencyAreaIds ?? [], existingDetail?.TopicIds ?? []).ConfigureAwait(false);
        if (await _repository.NameExistsAsync(model.Name, id, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"A skill named '{model.Name.Trim()}' already exists.");
        }

        skill.Update(model.Name, model.Description, model.TargetLevel, model.Status, _clock.UtcNow);
        await _repository.UpdateAsync(skill, DistinctIds(model.CompetencyAreaIds), DistinctIds(model.TopicIds), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Appends one assessment and atomically advances the current-level snapshot.</summary>
    public async Task<Guid> AssessAsync(Guid skillId, SkillAssessmentModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var skill = await GetRequiredAsync(skillId, cancellationToken).ConfigureAwait(false);
        if (skill.Status == SkillStatus.Archived)
        {
            throw new InvalidOperationException("Restore the skill before assessing it.");
        }

        var now = _clock.UtcNow;
        var assessment = SkillAssessment.Create(skillId, model.Level, model.Type, model.Reason, now, now);
        skill.ApplyAssessment(model.Level, now);
        await _repository.AddAssessmentAsync(skill, assessment, cancellationToken).ConfigureAwait(false);
        return assessment.Id;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        skill.Archive(_clock.UtcNow);
        await _repository.UpdateAsync(skill, detail?.CompetencyAreaIds ?? [], detail?.TopicIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        skill.Restore(_clock.UtcNow);
        await _repository.UpdateAsync(skill, detail?.CompetencyAreaIds ?? [], detail?.TopicIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    private async Task ValidateAssignmentsAsync(IEnumerable<Guid> areaIds, IEnumerable<Guid> topicIds, CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? allowedArchivedAreaIds = null, IReadOnlyCollection<Guid>? allowedArchivedTopicIds = null)
    {
        var allowedAreas = allowedArchivedAreaIds is null ? new HashSet<Guid>() : allowedArchivedAreaIds.ToHashSet();
        var allowedTopics = allowedArchivedTopicIds is null ? new HashSet<Guid>() : allowedArchivedTopicIds.ToHashSet();
        foreach (var areaId in DistinctIds(areaIds))
        {
            var area = await _catalogRepository.GetAreaByIdAsync(areaId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Competency area '{areaId}' does not exist.");
            if (area.Status == CatalogStatus.Archived && !allowedAreas.Contains(areaId))
            {
                throw new InvalidOperationException($"Competency area '{area.Name}' is archived.");
            }
        }

        foreach (var topicId in DistinctIds(topicIds))
        {
            var topic = await _catalogRepository.GetTopicByIdAsync(topicId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Topic '{topicId}' does not exist.");
            if (topic.Status == CatalogStatus.Archived && !allowedTopics.Contains(topicId))
            {
                throw new InvalidOperationException($"Topic '{topic.Name}' is archived.");
            }
        }
    }

    private async Task<Skill> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Skill '{id}' was not found.");

    private static IReadOnlyCollection<Guid> DistinctIds(IEnumerable<Guid> ids)
        => ids.Where(static id => id != Guid.Empty).Distinct().ToArray();

    private static void EnsureEditableStatus(SkillStatus status)
    {
        if (status == SkillStatus.Archived)
        {
            throw new ArgumentException("Use the archive operation instead of saving a skill directly as archived.");
        }
    }
}
