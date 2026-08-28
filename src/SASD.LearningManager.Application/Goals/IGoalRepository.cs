using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Domain.Goals;

namespace SASD.LearningManager.Application.Goals;

/// <summary>Persistence/query port for goals and their skill relationships.</summary>
public interface IGoalRepository
{
    Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GoalDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<GoalListItemDto>> SearchAsync(GoalSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoalLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task InsertAsync(Goal goal, IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default);
    Task UpdateAsync(Goal goal, IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default);
}
