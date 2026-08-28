using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Skills;

/// <summary>Persistence/query port for skill definitions and append-only assessment history.</summary>
public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SkillDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<SkillListItemDto>> SearchAsync(SkillSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SkillLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SkillAssessmentListItemDto>> ListAssessmentsAsync(Guid skillId, CancellationToken cancellationToken = default);
    Task InsertAsync(Skill skill, IReadOnlyCollection<Guid> competencyAreaIds, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default);
    Task UpdateAsync(Skill skill, IReadOnlyCollection<Guid> competencyAreaIds, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default);
    Task AddAssessmentAsync(Skill skill, SkillAssessment assessment, CancellationToken cancellationToken = default);
}
