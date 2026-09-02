using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;

namespace SASD.LearningManager.Application.Knowledge;

public sealed class KnowledgeArtifactService
{
    private readonly IKnowledgeArtifactRepository _repository; private readonly IClock _clock;
    public KnowledgeArtifactService(IKnowledgeArtifactRepository repository, IClock clock) => (_repository, _clock) = (repository, clock);
    public Task<PagedResult<KnowledgeArtifactListItemDto>> SearchAsync(KnowledgeSearchCriteria c, CancellationToken ct = default)
    { ValidatePage(c.PageNumber, c.PageSize); return _repository.SearchAsync(c, ct); }
    public Task<KnowledgeArtifactDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default) => _repository.GetDetailAsync(id, ct);
    public async Task<Guid> CreateAsync(KnowledgeArtifactEditModel model, CancellationToken ct = default)
    { var item = Domain.Knowledge.KnowledgeArtifact.Create(model.Title, model.Markdown, model.Type, _clock.UtcNow); await _repository.SaveAsync(item, Normalize(model), true, ct); return item.Id; }
    public async Task UpdateAsync(Guid id, KnowledgeArtifactEditModel model, CancellationToken ct = default)
    { var item = await Required(id, ct); item.Update(model.Title, model.Markdown, model.Type, _clock.UtcNow); await _repository.SaveAsync(item, Normalize(model), false, ct); }
    public async Task ArchiveAsync(Guid id, CancellationToken ct = default) { var item = await Required(id, ct); var detail = await _repository.GetDetailAsync(id, ct) ?? throw new KeyNotFoundException(); item.Archive(_clock.UtcNow); await _repository.SaveAsync(item, From(detail), false, ct); }
    public async Task RestoreAsync(Guid id, CancellationToken ct = default) { var item = await Required(id, ct); var detail = await _repository.GetDetailAsync(id, ct) ?? throw new KeyNotFoundException(); item.Restore(_clock.UtcNow); await _repository.SaveAsync(item, From(detail), false, ct); }
    private async Task<Domain.Knowledge.KnowledgeArtifact> Required(Guid id, CancellationToken ct) => await _repository.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException($"Knowledge artifact '{id}' was not found.");
    private static KnowledgeArtifactEditModel Normalize(KnowledgeArtifactEditModel m) => m with { ResourceIds = Ids(m.ResourceIds), SkillIds = Ids(m.SkillIds), TopicIds = Ids(m.TopicIds), GoalIds = Ids(m.GoalIds), LearningPathIds = Ids(m.LearningPathIds) };
    private static KnowledgeArtifactEditModel From(KnowledgeArtifactDetailDto d) => new(d.Title, d.Markdown, d.Type, d.ResourceIds, d.SkillIds, d.TopicIds, d.GoalIds, d.LearningPathIds);
    private static Guid[] Ids(IEnumerable<Guid> ids) => ids.Where(x => x != Guid.Empty).Distinct().ToArray();
    private static void ValidatePage(int n, int s) { if (n < 1 || s is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(n)); }
}
