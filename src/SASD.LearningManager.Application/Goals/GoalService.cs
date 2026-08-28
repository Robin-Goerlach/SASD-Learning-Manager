using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Goals;

/// <summary>
/// Coordinates goal lifecycle and Goal-to-Skill relationships. Linking a skill to a goal expresses
/// required competence only; it never changes current mastery or assessment history.
/// </summary>
public sealed class GoalService
{
    private readonly IGoalRepository _repository;
    private readonly ISkillRepository _skillRepository;
    private readonly IClock _clock;

    public GoalService(IGoalRepository repository, ISkillRepository skillRepository, IClock clock)
    {
        _repository = repository;
        _skillRepository = skillRepository;
        _clock = clock;
    }

    public Task<PagedResult<GoalListItemDto>> SearchAsync(GoalSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (criteria.PageNumber < 1) throw new ArgumentOutOfRangeException(nameof(criteria));
        if (criteria.PageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(criteria));
        return _repository.SearchAsync(criteria, cancellationToken);
    }

    public Task<GoalDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetDetailAsync(id, cancellationToken);

    /// <summary>Lists goals for relationship editors without exposing repository details to WinForms.</summary>
    public Task<IReadOnlyList<GoalLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => _repository.ListLookupAsync(includeArchived, cancellationToken);

    public async Task<Guid> CreateAsync(GoalEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureEditableStatus(model.Status);
        var skillIds = await ValidateSkillsAsync(model.SkillIds, cancellationToken).ConfigureAwait(false);
        var goal = Goal.Create(model.Title, model.Description, model.Type, model.Motivation, model.Priority,
            model.Status, model.TargetDate, model.NextActionText, model.NextActionDueDate, _clock.UtcNow);
        await _repository.InsertAsync(goal, skillIds, cancellationToken).ConfigureAwait(false);
        return goal.Id;
    }

    public async Task UpdateAsync(Guid id, GoalEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureEditableStatus(model.Status);
        var goal = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (goal.Status == GoalStatus.Archived)
        {
            throw new InvalidOperationException("Restore the goal before editing it.");
        }

        var existingDetail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        var skillIds = await ValidateSkillsAsync(model.SkillIds, cancellationToken, existingDetail?.SkillIds ?? []).ConfigureAwait(false);
        goal.Update(model.Title, model.Description, model.Type, model.Motivation, model.Priority,
            model.TargetDate, model.NextActionText, model.NextActionDueDate, _clock.UtcNow);
        if (goal.Status != model.Status)
        {
            goal.ChangeStatus(model.Status, _clock.UtcNow);
        }

        await _repository.UpdateAsync(goal, skillIds, cancellationToken).ConfigureAwait(false);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        goal.Archive(_clock.UtcNow);
        await _repository.UpdateAsync(goal, detail?.SkillIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        goal.Restore(_clock.UtcNow);
        await _repository.UpdateAsync(goal, detail?.SkillIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyCollection<Guid>> ValidateSkillsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? allowedArchivedSkillIds = null)
    {
        var allowed = allowedArchivedSkillIds is null ? new HashSet<Guid>() : allowedArchivedSkillIds.ToHashSet();
        var result = ids.Where(static id => id != Guid.Empty).Distinct().ToArray();
        foreach (var id in result)
        {
            var skill = await _skillRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Skill '{id}' does not exist.");
            if (skill.Status == SkillStatus.Archived && !allowed.Contains(id))
            {
                throw new InvalidOperationException($"Skill '{skill.Name}' is archived and cannot be newly assigned.");
            }
        }

        return result;
    }

    private async Task<Goal> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Goal '{id}' was not found.");

    private static void EnsureEditableStatus(GoalStatus status)
    {
        if (status == GoalStatus.Archived)
        {
            throw new ArgumentException("Use the archive operation instead of saving a goal directly as archived.");
        }
    }
}
