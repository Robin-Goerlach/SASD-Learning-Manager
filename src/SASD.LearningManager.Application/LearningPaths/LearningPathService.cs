using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.LearningPaths;

/// <summary>
/// Coordinates the Milestone-4 learning-path use cases. It is the single application boundary for
/// tree-cycle protection, assignment validation, ordering and graph-like node relationships.
/// </summary>
public sealed class LearningPathService
{
    private readonly ILearningPathRepository _repository;
    private readonly ISkillRepository _skillRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IClock _clock;

    public LearningPathService(
        ILearningPathRepository repository,
        ISkillRepository skillRepository,
        IResourceRepository resourceRepository,
        IGoalRepository goalRepository,
        IClock clock)
    {
        _repository = repository;
        _skillRepository = skillRepository;
        _resourceRepository = resourceRepository;
        _goalRepository = goalRepository;
        _clock = clock;
    }

    public Task<PagedResult<LearningPathListItemDto>> SearchAsync(LearningPathSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (criteria.PageNumber < 1) throw new ArgumentOutOfRangeException(nameof(criteria));
        if (criteria.PageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(criteria));
        return _repository.SearchAsync(criteria, cancellationToken);
    }

    public Task<LearningPathDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetDetailAsync(id, cancellationToken);

    public async Task<IReadOnlyList<LearningPathNodeListItemDto>> ListNodesAsync(Guid learningPathId, bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        await EnsurePathExistsAsync(learningPathId, cancellationToken).ConfigureAwait(false);
        var nodes = await _repository.ListNodesAsync(learningPathId, includeArchived, cancellationToken).ConfigureAwait(false);
        return nodes.Select(static node => new LearningPathNodeListItemDto(
            node.Id, node.LearningPathId, node.ParentNodeId, node.Title, node.Type, node.SortOrder, node.IsRequired, node.Status)).ToArray();
    }

    public Task<LearningPathNodeDetailDto?> GetNodeDetailAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetNodeDetailAsync(id, cancellationToken);

    public Task<IReadOnlyList<LearningPathNodeRelationDto>> ListRelationsAsync(Guid learningPathId, CancellationToken cancellationToken = default)
        => _repository.ListRelationsAsync(learningPathId, cancellationToken);

    /// <summary>Creates a path and validates optional Goal relationships without changing those Goals.</summary>
    public async Task<Guid> CreateAsync(LearningPathEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureEditableStatus(model.Status);
        var goalIds = await ValidateGoalsAsync(model.GoalIds, cancellationToken).ConfigureAwait(false);
        var path = LearningPath.Create(model.Title, model.Description, model.Status, model.Priority,
            model.PlannedStartDate, model.TargetDate, model.NextActionText, model.NextActionDueDate, _clock.UtcNow);
        await _repository.InsertAsync(path, goalIds, cancellationToken).ConfigureAwait(false);
        return path.Id;
    }

    public async Task UpdateAsync(Guid id, LearningPathEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        EnsureEditableStatus(model.Status);
        var path = await GetRequiredPathAsync(id, cancellationToken).ConfigureAwait(false);
        if (path.Status == LearningPathStatus.Archived)
        {
            throw new InvalidOperationException("Restore the learning path before editing it.");
        }

        var existing = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        var goalIds = await ValidateGoalsAsync(model.GoalIds, cancellationToken, existing?.GoalIds ?? []).ConfigureAwait(false);
        path.Update(model.Title, model.Description, model.Priority, model.PlannedStartDate, model.TargetDate,
            model.NextActionText, model.NextActionDueDate, _clock.UtcNow);
        if (path.Status != model.Status)
        {
            path.ChangeStatus(model.Status, _clock.UtcNow);
        }

        await _repository.UpdateAsync(path, goalIds, cancellationToken).ConfigureAwait(false);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var path = await GetRequiredPathAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        path.Archive(_clock.UtcNow);
        await _repository.UpdateAsync(path, detail?.GoalIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var path = await GetRequiredPathAsync(id, cancellationToken).ConfigureAwait(false);
        var detail = await _repository.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        path.Restore(_clock.UtcNow);
        await _repository.UpdateAsync(path, detail?.GoalIds ?? [], cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Creates a node at the end of the selected sibling group.</summary>
    public async Task<Guid> CreateNodeAsync(Guid learningPathId, LearningPathNodeEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var path = await GetRequiredPathAsync(learningPathId, cancellationToken).ConfigureAwait(false);
        EnsurePathEditable(path);
        var nodes = await _repository.ListNodesAsync(learningPathId, includeArchived: true, cancellationToken).ConfigureAwait(false);
        ValidateParent(learningPathId, model.ParentNodeId, nodes, null, cancellationToken);
        var skillIds = await ValidateSkillsAsync(model.SkillIds, [], cancellationToken).ConfigureAwait(false);
        var resourceIds = await ValidateResourcesAsync(model.ResourceIds, [], cancellationToken).ConfigureAwait(false);
        var nextSort = nodes.Where(node => node.Status != LearningPathNodeStatus.Archived && node.ParentNodeId == model.ParentNodeId)
            .Select(static node => node.SortOrder).DefaultIfEmpty(-1).Max() + 1;

        var node = LearningPathNode.Create(learningPathId, model.ParentNodeId, model.Title, model.Description,
            model.Type, nextSort, model.IsRequired, _clock.UtcNow);
        if (model.Status != LearningPathNodeStatus.Planned)
        {
            node.Update(model.Title, model.Description, model.Type, model.IsRequired, model.Status, _clock.UtcNow);
        }

        await _repository.InsertNodeAsync(node, skillIds, resourceIds, cancellationToken).ConfigureAwait(false);
        return node.Id;
    }

    /// <summary>
    /// Updates node metadata and, when the parent changes, appends it to the target sibling group.
    /// The application layer prevents moving a node below one of its own descendants.
    /// </summary>
    public async Task UpdateNodeAsync(Guid nodeId, LearningPathNodeEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (model.Status == LearningPathNodeStatus.Archived)
        {
            throw new ArgumentException("Use the archive operation instead of saving a node directly as archived.", nameof(model));
        }

        var node = await GetRequiredNodeAsync(nodeId, cancellationToken).ConfigureAwait(false);
        var path = await GetRequiredPathAsync(node.LearningPathId, cancellationToken).ConfigureAwait(false);
        EnsurePathEditable(path);
        if (node.Status == LearningPathNodeStatus.Archived)
        {
            throw new InvalidOperationException("Restore the node before editing it.");
        }

        var detail = await _repository.GetNodeDetailAsync(nodeId, cancellationToken).ConfigureAwait(false);
        var nodes = await _repository.ListNodesAsync(node.LearningPathId, includeArchived: true, cancellationToken).ConfigureAwait(false);
        ValidateParent(node.LearningPathId, model.ParentNodeId, nodes, nodeId, cancellationToken);
        var skillIds = await ValidateSkillsAsync(model.SkillIds, detail?.SkillIds ?? [], cancellationToken).ConfigureAwait(false);
        var resourceIds = await ValidateResourcesAsync(model.ResourceIds, detail?.ResourceIds ?? [], cancellationToken).ConfigureAwait(false);

        if (node.ParentNodeId != model.ParentNodeId)
        {
            var sort = nodes.Where(candidate => candidate.Id != node.Id && candidate.Status != LearningPathNodeStatus.Archived && candidate.ParentNodeId == model.ParentNodeId)
                .Select(static candidate => candidate.SortOrder).DefaultIfEmpty(-1).Max() + 1;
            node.MoveTo(model.ParentNodeId, sort, _clock.UtcNow);
        }

        node.Update(model.Title, model.Description, model.Type, model.IsRequired, model.Status, _clock.UtcNow);
        await _repository.UpdateNodeAsync(node, skillIds, resourceIds, cancellationToken).ConfigureAwait(false);
        await NormalizeSiblingOrderAsync(node.LearningPathId, node.ParentNodeId, cancellationToken).ConfigureAwait(false);
    }

    public Task MoveNodeUpAsync(Guid nodeId, CancellationToken cancellationToken = default)
        => MoveWithinSiblingsAsync(nodeId, -1, cancellationToken);

    public Task MoveNodeDownAsync(Guid nodeId, CancellationToken cancellationToken = default)
        => MoveWithinSiblingsAsync(nodeId, 1, cancellationToken);

    /// <summary>Archives the selected node and every descendant, preserving all links for history.</summary>
    public async Task ArchiveNodeSubtreeAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        var node = await GetRequiredNodeAsync(nodeId, cancellationToken).ConfigureAwait(false);
        var path = await GetRequiredPathAsync(node.LearningPathId, cancellationToken).ConfigureAwait(false);
        EnsurePathEditable(path);
        var nodes = await _repository.ListNodesAsync(node.LearningPathId, includeArchived: true, cancellationToken).ConfigureAwait(false);
        var ids = CollectSubtreeIds(nodeId, nodes);
        await _repository.ArchiveNodesAsync(ids, _clock.UtcNow, cancellationToken).ConfigureAwait(false);
        await NormalizeSiblingOrderAsync(node.LearningPathId, node.ParentNodeId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Restores one node. Its parent must already be active so the tree cannot contain a visible orphan.</summary>
    public async Task RestoreNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        var node = await GetRequiredNodeAsync(nodeId, cancellationToken).ConfigureAwait(false);
        if (node.Status != LearningPathNodeStatus.Archived)
        {
            return;
        }

        var path = await GetRequiredPathAsync(node.LearningPathId, cancellationToken).ConfigureAwait(false);
        EnsurePathEditable(path);
        if (node.ParentNodeId is not null)
        {
            var parent = await GetRequiredNodeAsync(node.ParentNodeId.Value, cancellationToken).ConfigureAwait(false);
            if (parent.Status == LearningPathNodeStatus.Archived)
            {
                throw new InvalidOperationException("Restore the parent node before restoring this child.");
            }
        }

        var detail = await _repository.GetNodeDetailAsync(nodeId, cancellationToken).ConfigureAwait(false);
        node.Restore(_clock.UtcNow);
        await _repository.UpdateNodeAsync(node, detail?.SkillIds ?? [], detail?.ResourceIds ?? [], cancellationToken).ConfigureAwait(false);
        await NormalizeSiblingOrderAsync(node.LearningPathId, node.ParentNodeId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Creates a validated dependency/alternative relation between nodes of the same path.</summary>
    public async Task<Guid> AddRelationAsync(LearningPathNodeRelationModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var source = await GetRequiredNodeAsync(model.SourceNodeId, cancellationToken).ConfigureAwait(false);
        var target = await GetRequiredNodeAsync(model.TargetNodeId, cancellationToken).ConfigureAwait(false);
        if (source.LearningPathId != target.LearningPathId)
        {
            throw new InvalidOperationException("Learning-path node relations must stay inside one learning path.");
        }
        if (source.Status == LearningPathNodeStatus.Archived || target.Status == LearningPathNodeStatus.Archived)
        {
            throw new InvalidOperationException("Archived nodes cannot receive new relations.");
        }

        var (normalizedSource, normalizedTarget) = NormalizeRelationEndpoints(source.Id, target.Id, model.Type);
        if (await _repository.RelationExistsAsync(normalizedSource, normalizedTarget, model.Type, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The selected node relation already exists.");
        }

        var relation = LearningPathNodeRelation.Create(normalizedSource, normalizedTarget, model.Type, model.Note, _clock.UtcNow);
        await _repository.InsertRelationAsync(relation, cancellationToken).ConfigureAwait(false);
        return relation.Id;
    }

    public Task DeleteRelationAsync(Guid relationId, CancellationToken cancellationToken = default)
        => _repository.DeleteRelationAsync(relationId, cancellationToken);

    private async Task MoveWithinSiblingsAsync(Guid nodeId, int delta, CancellationToken cancellationToken)
    {
        var node = await GetRequiredNodeAsync(nodeId, cancellationToken).ConfigureAwait(false);
        var path = await GetRequiredPathAsync(node.LearningPathId, cancellationToken).ConfigureAwait(false);
        EnsurePathEditable(path);
        if (node.Status == LearningPathNodeStatus.Archived)
        {
            throw new InvalidOperationException("Archived nodes cannot be reordered.");
        }

        var nodes = await _repository.ListNodesAsync(node.LearningPathId, includeArchived: false, cancellationToken).ConfigureAwait(false);
        var siblings = nodes.Where(candidate => candidate.ParentNodeId == node.ParentNodeId)
            .OrderBy(static candidate => candidate.SortOrder).ThenBy(static candidate => candidate.Title, StringComparer.OrdinalIgnoreCase).ToList();
        var current = siblings.FindIndex(candidate => candidate.Id == node.Id);
        if (current < 0)
        {
            throw new InvalidOperationException("The node is not present in its sibling group.");
        }
        var target = current + delta;
        if (target < 0 || target >= siblings.Count)
        {
            return;
        }

        (siblings[current], siblings[target]) = (siblings[target], siblings[current]);
        var updates = siblings.Select((candidate, index) => new LearningPathNodeOrderUpdate(candidate.Id, candidate.ParentNodeId, index, _clock.UtcNow)).ToArray();
        await _repository.UpdateNodeOrdersAsync(updates, cancellationToken).ConfigureAwait(false);
    }

    private async Task NormalizeSiblingOrderAsync(Guid learningPathId, Guid? parentNodeId, CancellationToken cancellationToken)
    {
        var nodes = await _repository.ListNodesAsync(learningPathId, includeArchived: false, cancellationToken).ConfigureAwait(false);
        var updates = nodes.Where(node => node.ParentNodeId == parentNodeId)
            .OrderBy(static node => node.SortOrder).ThenBy(static node => node.Title, StringComparer.OrdinalIgnoreCase)
            .Select((node, index) => new LearningPathNodeOrderUpdate(node.Id, node.ParentNodeId, index, _clock.UtcNow)).ToArray();
        if (updates.Length > 0)
        {
            await _repository.UpdateNodeOrdersAsync(updates, cancellationToken).ConfigureAwait(false);
        }
    }

    private static IReadOnlyCollection<Guid> CollectSubtreeIds(Guid rootId, IReadOnlyCollection<LearningPathNode> nodes)
    {
        var result = new HashSet<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(rootId);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!result.Add(current))
            {
                continue;
            }

            foreach (var child in nodes.Where(node => node.ParentNodeId == current))
            {
                stack.Push(child.Id);
            }
        }
        return result.ToArray();
    }

    private static void ValidateParent(Guid learningPathId, Guid? parentId, IReadOnlyCollection<LearningPathNode> nodes,
        Guid? movingNodeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (parentId is null)
        {
            return;
        }

        var parent = nodes.FirstOrDefault(node => node.Id == parentId.Value)
            ?? throw new InvalidOperationException("The selected parent node no longer exists.");
        if (parent.LearningPathId != learningPathId || parent.Status == LearningPathNodeStatus.Archived)
        {
            throw new InvalidOperationException("The selected parent node is not available in this learning path.");
        }
        if (movingNodeId is null)
        {
            return;
        }
        if (parent.Id == movingNodeId.Value)
        {
            throw new InvalidOperationException("A node cannot be its own parent.");
        }

        var cursor = parent;
        while (cursor.ParentNodeId is not null)
        {
            if (cursor.ParentNodeId.Value == movingNodeId.Value)
            {
                throw new InvalidOperationException("A node cannot be moved below one of its own descendants.");
            }
            cursor = nodes.FirstOrDefault(node => node.Id == cursor.ParentNodeId.Value)
                ?? throw new InvalidOperationException("The learning-path hierarchy contains a missing parent node.");
        }
    }

    private async Task<IReadOnlyCollection<Guid>> ValidateSkillsAsync(IEnumerable<Guid> ids, IReadOnlyCollection<Guid> allowedArchived,
        CancellationToken cancellationToken)
    {
        var allowed = allowedArchived.ToHashSet();
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

    private async Task<IReadOnlyCollection<Guid>> ValidateResourcesAsync(IEnumerable<Guid> ids, IReadOnlyCollection<Guid> allowedArchived,
        CancellationToken cancellationToken)
    {
        var allowed = allowedArchived.ToHashSet();
        var result = ids.Where(static id => id != Guid.Empty).Distinct().ToArray();
        foreach (var id in result)
        {
            var resource = await _resourceRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Resource '{id}' does not exist.");
            if (resource.Status == ResourceStatus.Archived && !allowed.Contains(id))
            {
                throw new InvalidOperationException($"Resource '{resource.Title}' is archived and cannot be newly assigned.");
            }
        }
        return result;
    }

    private async Task<IReadOnlyCollection<Guid>> ValidateGoalsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? allowedArchived = null)
    {
        var allowed = allowedArchived is null ? new HashSet<Guid>() : allowedArchived.ToHashSet();
        var result = ids.Where(static id => id != Guid.Empty).Distinct().ToArray();
        foreach (var id in result)
        {
            var goal = await _goalRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Goal '{id}' does not exist.");
            if (goal.Status == GoalStatus.Archived && !allowed.Contains(id))
            {
                throw new InvalidOperationException($"Goal '{goal.Title}' is archived and cannot be newly assigned.");
            }
        }
        return result;
    }

    private async Task<LearningPath> GetRequiredPathAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Learning path '{id}' was not found.");

    private async Task EnsurePathExistsAsync(Guid id, CancellationToken cancellationToken)
        => _ = await GetRequiredPathAsync(id, cancellationToken).ConfigureAwait(false);

    private async Task<LearningPathNode> GetRequiredNodeAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetNodeByIdAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Learning path node '{id}' was not found.");

    private static void EnsurePathEditable(LearningPath path)
    {
        if (path.Status == LearningPathStatus.Archived)
        {
            throw new InvalidOperationException("Restore the learning path before modifying its nodes.");
        }
    }

    private static void EnsureEditableStatus(LearningPathStatus status)
    {
        if (status == LearningPathStatus.Archived)
        {
            throw new ArgumentException("Use the archive operation instead of saving a path directly as archived.");
        }
    }

    private static (Guid Source, Guid Target) NormalizeRelationEndpoints(Guid source, Guid target, LearningPathNodeRelationType type)
    {
        if (type is not (LearningPathNodeRelationType.AlternativeTo or LearningPathNodeRelationType.Related))
        {
            return (source, target);
        }

        return string.CompareOrdinal(source.ToString("D"), target.ToString("D")) <= 0 ? (source, target) : (target, source);
    }
}
