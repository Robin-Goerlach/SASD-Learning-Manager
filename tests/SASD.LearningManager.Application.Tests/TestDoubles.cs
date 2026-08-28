using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Providers;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Tests;

internal sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
}

internal sealed class FakeLinkLauncher : IExternalLinkLauncher
{
    public Uri? LastOpened { get; private set; }
    public void Open(Uri uri) => LastOpened = uri;
}

internal sealed class FakeProviderRepository : IProviderRepository
{
    public Dictionary<Guid, Provider> Items { get; } = [];

    public Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<IReadOnlyList<ProviderListItemDto>> ListAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ProviderListItemDto>>(Items.Values
            .Where(x => includeArchived || x.Status != ProviderStatus.Archived)
            .Select(x => new ProviderListItemDto(x.Id, x.Name, x.Type, x.Status, x.WebsiteUrl)).ToArray());

    public Task<bool> NameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.Any(x => x.Id != excludingId && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task InsertAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        Items.Add(provider.Id, provider);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        Items[provider.Id] = provider;
        return Task.CompletedTask;
    }
}

internal sealed class FakeResourceRepository : IResourceRepository
{
    public Dictionary<Guid, Resource> Items { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<string>> Tags { get; } = [];

    public Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<ResourceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<ResourceDetailDto?>(null);

    public Task<Resource?> FindByNormalizedUrlAsync(string normalizedUrl, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.FirstOrDefault(x => x.Id != excludingId && string.Equals(x.NormalizedUrl, normalizedUrl, StringComparison.OrdinalIgnoreCase)));

    public Task<PagedResult<ResourceListItemDto>> SearchAsync(ResourceSearchCriteria criteria, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<ResourceListItemDto>([], criteria.PageNumber, criteria.PageSize, 0));

    public Task<PagedResult<InboxListItemDto>> SearchInboxAsync(InboxSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var items = Items.Values
            .Where(static x => x.Status == ResourceStatus.Inbox)
            .OrderByDescending(static x => x.CreatedAtUtc)
            .Select(static x => new InboxListItemDto(x.Id, x.Title, x.Url, null, x.Type, x.WhySaved, x.CreatedAtUtc))
            .ToArray();
        return Task.FromResult(new PagedResult<InboxListItemDto>(items, criteria.PageNumber, criteria.PageSize, items.Length));
    }

    public Task InsertAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default)
    {
        Items.Add(resource.Id, resource);
        Tags[resource.Id] = tags;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Resource resource, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default)
    {
        Items[resource.Id] = resource;
        Tags[resource.Id] = tags;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetTagsAsync(Guid resourceId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(Tags.TryGetValue(resourceId, out var tags) ? tags.ToArray() : []);
}

internal sealed class FakeCompetencyCatalogRepository : ICompetencyCatalogRepository
{
    public Dictionary<Guid, CompetencyArea> Areas { get; } = [];
    public Dictionary<Guid, Topic> Topics { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<Guid>> TopicAreas { get; } = [];

    public Task<IReadOnlyList<CompetencyAreaListItemDto>> ListAreasAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CompetencyAreaListItemDto>>(Areas.Values
            .Where(x => includeArchived || x.Status != CatalogStatus.Archived)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new CompetencyAreaListItemDto(x.Id, x.Name, x.Description, x.Status)).ToArray());

    public Task<CompetencyArea?> GetAreaByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Areas.GetValueOrDefault(id));

    public Task<bool> AreaNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Areas.Values.Any(x => x.Id != excludingId && string.Equals(x.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task InsertAreaAsync(CompetencyArea area, CancellationToken cancellationToken = default)
    {
        Areas.Add(area.Id, area);
        return Task.CompletedTask;
    }

    public Task UpdateAreaAsync(CompetencyArea area, CancellationToken cancellationToken = default)
    {
        Areas[area.Id] = area;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TopicListItemDto>> ListTopicsAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TopicListItemDto>>(Topics.Values
            .Where(x => includeArchived || x.Status != CatalogStatus.Archived)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new TopicListItemDto(x.Id, x.Name, x.Description, string.Empty, x.Status)).ToArray());

    public Task<Topic?> GetTopicByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Topics.GetValueOrDefault(id));

    public Task<TopicDetailDto?> GetTopicDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!Topics.TryGetValue(id, out var topic)) return Task.FromResult<TopicDetailDto?>(null);
        var areaIds = TopicAreas.TryGetValue(id, out var ids) ? ids.ToArray() : [];
        return Task.FromResult<TopicDetailDto?>(new TopicDetailDto(topic.Id, topic.Name, topic.Description, topic.Status, areaIds));
    }

    public Task<bool> TopicNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Topics.Values.Any(x => x.Id != excludingId && string.Equals(x.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task InsertTopicAsync(Topic topic, IReadOnlyCollection<Guid> competencyAreaIds, CancellationToken cancellationToken = default)
    {
        Topics.Add(topic.Id, topic);
        TopicAreas[topic.Id] = competencyAreaIds.ToArray();
        return Task.CompletedTask;
    }

    public Task UpdateTopicAsync(Topic topic, IReadOnlyCollection<Guid> competencyAreaIds, CancellationToken cancellationToken = default)
    {
        Topics[topic.Id] = topic;
        TopicAreas[topic.Id] = competencyAreaIds.ToArray();
        return Task.CompletedTask;
    }
}

internal sealed class FakeSkillRepository : ISkillRepository
{
    public Dictionary<Guid, Skill> Items { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<Guid>> Areas { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<Guid>> Topics { get; } = [];
    public Dictionary<Guid, List<SkillAssessment>> Assessments { get; } = [];

    public Task<Skill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<SkillDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!Items.TryGetValue(id, out var skill)) return Task.FromResult<SkillDetailDto?>(null);
        return Task.FromResult<SkillDetailDto?>(new SkillDetailDto(skill.Id, skill.Name, skill.Description, skill.CurrentLevel,
            skill.TargetLevel, skill.Status, skill.CreatedAtUtc, skill.UpdatedAtUtc, skill.ArchivedAtUtc,
            Areas.TryGetValue(id, out var areas) ? areas.ToArray() : [],
            Topics.TryGetValue(id, out var topics) ? topics.ToArray() : []));
    }

    public Task<bool> NameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Values.Any(x => x.Id != excludingId && string.Equals(x.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task<PagedResult<SkillListItemDto>> SearchAsync(SkillSearchCriteria criteria, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<SkillListItemDto>([], criteria.PageNumber, criteria.PageSize, 0));

    public Task<IReadOnlyList<SkillLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SkillLookupDto>>(Items.Values
            .Where(x => includeArchived || x.Status != SkillStatus.Archived)
            .Select(x => new SkillLookupDto(x.Id, x.Name, x.Status, x.CurrentLevel, x.TargetLevel)).ToArray());

    public Task<IReadOnlyList<SkillAssessmentListItemDto>> ListAssessmentsAsync(Guid skillId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SkillAssessmentListItemDto>>(Assessments.TryGetValue(skillId, out var values)
            ? values.OrderByDescending(x => x.AssessedAtUtc).Select(x => new SkillAssessmentListItemDto(x.Id, x.Level, x.Type, x.Reason, x.AssessedAtUtc)).ToArray()
            : []);

    public Task InsertAsync(Skill skill, IReadOnlyCollection<Guid> competencyAreaIds, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        Items.Add(skill.Id, skill);
        Areas[skill.Id] = competencyAreaIds.ToArray();
        Topics[skill.Id] = topicIds.ToArray();
        Assessments[skill.Id] = [];
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Skill skill, IReadOnlyCollection<Guid> competencyAreaIds, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        Items[skill.Id] = skill;
        Areas[skill.Id] = competencyAreaIds.ToArray();
        Topics[skill.Id] = topicIds.ToArray();
        return Task.CompletedTask;
    }

    public Task AddAssessmentAsync(Skill skill, SkillAssessment assessment, CancellationToken cancellationToken = default)
    {
        Items[skill.Id] = skill;
        if (!Assessments.TryGetValue(skill.Id, out var list))
        {
            list = [];
            Assessments[skill.Id] = list;
        }
        list.Add(assessment);
        return Task.CompletedTask;
    }
}

internal sealed class FakeGoalRepository : IGoalRepository
{
    public Dictionary<Guid, Goal> Items { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<Guid>> SkillLinks { get; } = [];

    public Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<GoalDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!Items.TryGetValue(id, out var goal)) return Task.FromResult<GoalDetailDto?>(null);
        return Task.FromResult<GoalDetailDto?>(new GoalDetailDto(goal.Id, goal.Title, goal.Description, goal.Type, goal.Motivation,
            goal.Priority, goal.Status, goal.TargetDate, goal.NextActionText, goal.NextActionDueDate, goal.CreatedAtUtc,
            goal.UpdatedAtUtc, goal.AchievedAtUtc, goal.ArchivedAtUtc,
            SkillLinks.TryGetValue(id, out var ids) ? ids.ToArray() : []));
    }

    public Task<PagedResult<GoalListItemDto>> SearchAsync(GoalSearchCriteria criteria, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<GoalListItemDto>([], criteria.PageNumber, criteria.PageSize, 0));

    public Task InsertAsync(Goal goal, IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default)
    {
        Items.Add(goal.Id, goal);
        SkillLinks[goal.Id] = skillIds.ToArray();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Goal goal, IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default)
    {
        Items[goal.Id] = goal;
        SkillLinks[goal.Id] = skillIds.ToArray();
        return Task.CompletedTask;
    }
}
