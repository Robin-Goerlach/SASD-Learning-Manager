using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;

namespace SASD.LearningManager.Application.Evidence;

public sealed class EvidenceService
{
    private readonly IEvidenceRepository _repository; private readonly IClock _clock;
    public EvidenceService(IEvidenceRepository repository, IClock clock) => (_repository, _clock) = (repository, clock);
    public Task<PagedResult<EvidenceListItemDto>> SearchAsync(EvidenceSearchCriteria c, CancellationToken ct = default)
    { if (c.PageNumber < 1 || c.PageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(c)); return _repository.SearchAsync(c, ct); }
    public Task<EvidenceDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default) => _repository.GetDetailAsync(id, ct);
    public async Task<Guid> CreateAsync(EvidenceEditModel model, CancellationToken ct = default)
    { var item = Domain.Evidence.EvidenceItem.Create(model.Title, model.Description, model.Type, model.OccurredAtUtc, model.Url, model.LocalPath, model.Evaluation, _clock.UtcNow); await _repository.SaveAsync(item, Normalize(model), true, ct); return item.Id; }
    public async Task UpdateAsync(Guid id, EvidenceEditModel model, CancellationToken ct = default)
    { var item = await Required(id, ct); item.Update(model.Title, model.Description, model.Type, model.OccurredAtUtc, model.Url, model.LocalPath, model.Evaluation, _clock.UtcNow); await _repository.SaveAsync(item, Normalize(model), false, ct); }
    public async Task ArchiveAsync(Guid id, CancellationToken ct = default) { var item = await Required(id, ct); var d = await _repository.GetDetailAsync(id, ct) ?? throw new KeyNotFoundException(); item.Archive(_clock.UtcNow); await _repository.SaveAsync(item, From(d), false, ct); }
    public async Task RestoreAsync(Guid id, CancellationToken ct = default) { var item = await Required(id, ct); var d = await _repository.GetDetailAsync(id, ct) ?? throw new KeyNotFoundException(); item.Restore(_clock.UtcNow); await _repository.SaveAsync(item, From(d), false, ct); }
    private async Task<Domain.Evidence.EvidenceItem> Required(Guid id, CancellationToken ct) => await _repository.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException($"Evidence '{id}' was not found.");
    private static EvidenceEditModel Normalize(EvidenceEditModel m) => m with { SkillIds = Ids(m.SkillIds), ResourceIds = Ids(m.ResourceIds), GoalIds = Ids(m.GoalIds) };
    private static EvidenceEditModel From(EvidenceDetailDto d) => new(d.Title, d.Description, d.Type, d.OccurredAtUtc, d.Url, d.LocalPath, d.Evaluation, d.SkillIds, d.ResourceIds, d.GoalIds);
    private static Guid[] Ids(IEnumerable<Guid> ids) => ids.Where(x => x != Guid.Empty).Distinct().ToArray();
}
