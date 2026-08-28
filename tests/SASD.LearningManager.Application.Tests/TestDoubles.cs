using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Providers;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.LearningPaths;
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

    public Task<IReadOnlyList<ResourceLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ResourceLookupDto>>(Items.Values
            .Where(x => includeArchived || x.Status != ResourceStatus.Archived)
            .Select(x => new ResourceLookupDto(x.Id, x.Title, x.Type, x.Status, null)).ToArray());

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

    public Task<IReadOnlyList<GoalLookupDto>> ListLookupAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<GoalLookupDto>>(Items.Values
            .Where(x => includeArchived || x.Status != GoalStatus.Archived)
            .Select(x => new GoalLookupDto(x.Id, x.Title, x.Status)).ToArray());

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


internal sealed class FakeLearningPathRepository : ILearningPathRepository
{
    public Dictionary<Guid, LearningPath> Items { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<Guid>> GoalLinks { get; } = [];
    public Dictionary<Guid, LearningPathNode> Nodes { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<Guid>> NodeSkills { get; } = [];
    public Dictionary<Guid, IReadOnlyCollection<Guid>> NodeResources { get; } = [];
    public Dictionary<Guid, LearningPathNodeRelation> Relations { get; } = [];

    public Task<LearningPath?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.GetValueOrDefault(id));

    public Task<LearningPathDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!Items.TryGetValue(id, out var path)) return Task.FromResult<LearningPathDetailDto?>(null);
        var nodes = Nodes.Values.Where(node => node.LearningPathId == id && node.Status != LearningPathNodeStatus.Archived).ToArray();
        var progress = LearningPathProgress.Calculate(nodes);
        return Task.FromResult<LearningPathDetailDto?>(new LearningPathDetailDto(path.Id, path.Title, path.Description, path.Status,
            path.Priority, path.PlannedStartDate, path.TargetDate, path.NextActionText, path.NextActionDueDate, path.CreatedAtUtc,
            path.UpdatedAtUtc, path.StartedAtUtc, path.CompletedAtUtc, path.ArchivedAtUtc,
            GoalLinks.TryGetValue(id, out var goals) ? goals.ToArray() : [], progress.RequiredCompleted, progress.RequiredTotal,
            progress.OptionalCompleted, progress.OptionalTotal, progress.CoreCompletionPercent));
    }

    public Task<PagedResult<LearningPathListItemDto>> SearchAsync(LearningPathSearchCriteria criteria, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResult<LearningPathListItemDto>([], criteria.PageNumber, criteria.PageSize, 0));

    public Task InsertAsync(LearningPath path, IReadOnlyCollection<Guid> goalIds, CancellationToken cancellationToken = default)
    {
        Items.Add(path.Id, path);
        GoalLinks[path.Id] = goalIds.ToArray();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LearningPath path, IReadOnlyCollection<Guid> goalIds, CancellationToken cancellationToken = default)
    {
        Items[path.Id] = path;
        GoalLinks[path.Id] = goalIds.ToArray();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LearningPathNode>> ListNodesAsync(Guid learningPathId, bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<LearningPathNode>>(Nodes.Values
            .Where(node => node.LearningPathId == learningPathId && (includeArchived || node.Status != LearningPathNodeStatus.Archived))
            .OrderBy(node => node.ParentNodeId).ThenBy(node => node.SortOrder).ToArray());

    public Task<LearningPathNode?> GetNodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Nodes.GetValueOrDefault(id));

    public Task<LearningPathNodeDetailDto?> GetNodeDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!Nodes.TryGetValue(id, out var node)) return Task.FromResult<LearningPathNodeDetailDto?>(null);
        return Task.FromResult<LearningPathNodeDetailDto?>(new LearningPathNodeDetailDto(node.Id, node.LearningPathId, node.ParentNodeId,
            node.Title, node.Description, node.Type, node.SortOrder, node.IsRequired, node.Status, node.CreatedAtUtc, node.UpdatedAtUtc,
            node.ArchivedAtUtc, NodeSkills.TryGetValue(id, out var skills) ? skills.ToArray() : [],
            NodeResources.TryGetValue(id, out var resources) ? resources.ToArray() : []));
    }

    public Task InsertNodeAsync(LearningPathNode node, IReadOnlyCollection<Guid> skillIds, IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken = default)
    {
        Nodes.Add(node.Id, node);
        NodeSkills[node.Id] = skillIds.ToArray();
        NodeResources[node.Id] = resourceIds.ToArray();
        return Task.CompletedTask;
    }

    public Task UpdateNodeAsync(LearningPathNode node, IReadOnlyCollection<Guid> skillIds, IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken = default)
    {
        Nodes[node.Id] = node;
        NodeSkills[node.Id] = skillIds.ToArray();
        NodeResources[node.Id] = resourceIds.ToArray();
        return Task.CompletedTask;
    }

    public Task UpdateNodeOrdersAsync(IReadOnlyCollection<LearningPathNodeOrderUpdate> updates, CancellationToken cancellationToken = default)
    {
        foreach (var update in updates)
        {
            Nodes[update.NodeId].MoveTo(update.ParentNodeId, update.SortOrder, update.UpdatedAtUtc);
        }
        return Task.CompletedTask;
    }

    public Task ArchiveNodesAsync(IReadOnlyCollection<Guid> nodeIds, DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        foreach (var id in nodeIds) Nodes[id].Archive(nowUtc);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LearningPathNodeRelationDto>> ListRelationsAsync(Guid learningPathId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<LearningPathNodeRelationDto>>(Relations.Values
            .Where(relation => Nodes[relation.SourceNodeId].LearningPathId == learningPathId)
            .Select(relation => new LearningPathNodeRelationDto(relation.Id, relation.SourceNodeId, Nodes[relation.SourceNodeId].Title,
                relation.TargetNodeId, Nodes[relation.TargetNodeId].Title, relation.Type, relation.Note, relation.CreatedAtUtc)).ToArray());

    public Task<bool> RelationExistsAsync(Guid sourceNodeId, Guid targetNodeId, LearningPathNodeRelationType type, CancellationToken cancellationToken = default)
        => Task.FromResult(Relations.Values.Any(relation => relation.SourceNodeId == sourceNodeId && relation.TargetNodeId == targetNodeId && relation.Type == type));

    public Task InsertRelationAsync(LearningPathNodeRelation relation, CancellationToken cancellationToken = default)
    {
        Relations.Add(relation.Id, relation);
        return Task.CompletedTask;
    }

    public Task DeleteRelationAsync(Guid relationId, CancellationToken cancellationToken = default)
    {
        Relations.Remove(relationId);
        return Task.CompletedTask;
    }
}
