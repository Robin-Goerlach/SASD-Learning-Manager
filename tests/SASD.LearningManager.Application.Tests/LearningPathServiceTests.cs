using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Tests;

public sealed class LearningPathServiceTests
{
    [Fact]
    public async Task CreateAsync_LinksGoalWithoutChangingGoalState()
    {
        var clock = new FakeClock();
        var goals = new FakeGoalRepository();
        var goal = Goal.Create("Cloud Engineer", null, GoalType.Career, null, GoalPriority.High,
            GoalStatus.Active, null, null, null, clock.UtcNow);
        goals.Items.Add(goal.Id, goal);
        goals.SkillLinks[goal.Id] = [];
        var paths = new FakeLearningPathRepository();
        var service = CreateService(paths, goals, new FakeSkillRepository(), new FakeResourceRepository(), clock);

        var id = await service.CreateAsync(new LearningPathEditModel("Cloud Path", null, LearningPathStatus.Active,
            LearningPathPriority.High, null, null, null, null, [goal.Id]), TestContext.Current.CancellationToken);

        Assert.Contains(goal.Id, paths.GoalLinks[id]);
        Assert.Equal(GoalStatus.Active, goals.Items[goal.Id].Status);
    }

    [Fact]
    public async Task CreateNodeAsync_AssignsSkillAndResource()
    {
        var clock = new FakeClock();
        var paths = new FakeLearningPathRepository();
        var path = LearningPath.Create("Docker", null, LearningPathStatus.Active, LearningPathPriority.High,
            null, null, null, null, clock.UtcNow);
        paths.Items.Add(path.Id, path);
        paths.GoalLinks[path.Id] = [];
        var skills = new FakeSkillRepository();
        var skill = Skill.Create("Docker Networking", null, 4, clock.UtcNow);
        skills.Items.Add(skill.Id, skill);
        skills.Areas[skill.Id] = [];
        skills.Topics[skill.Id] = [];
        skills.Assessments[skill.Id] = [];
        var resources = new FakeResourceRepository();
        var resource = Resource.Create("Docker Course", ResourceType.Course, null, "https://example.test/docker",
            "https://example.test/docker", null, null, null, null, null, null, null,
            ResourceDifficulty.Beginner, ResourcePriority.High, ResourceStatus.Planned, clock.UtcNow);
        resources.Items.Add(resource.Id, resource);
        resources.Tags[resource.Id] = [];
        var service = CreateService(paths, new FakeGoalRepository(), skills, resources, clock);

        var nodeId = await service.CreateNodeAsync(path.Id, new LearningPathNodeEditModel(null, "Networking", null,
            LearningPathNodeType.Topic, true, LearningPathNodeStatus.Planned, [skill.Id], [resource.Id]),
            TestContext.Current.CancellationToken);

        Assert.Contains(skill.Id, paths.NodeSkills[nodeId]);
        Assert.Contains(resource.Id, paths.NodeResources[nodeId]);
    }

    [Fact]
    public async Task UpdateNodeAsync_RejectsMoveBelowOwnDescendant()
    {
        var clock = new FakeClock();
        var paths = new FakeLearningPathRepository();
        var path = LearningPath.Create("Path", null, LearningPathStatus.Active, LearningPathPriority.Normal,
            null, null, null, null, clock.UtcNow);
        paths.Items.Add(path.Id, path);
        paths.GoalLinks[path.Id] = [];
        var parent = LearningPathNode.Create(path.Id, null, "Parent", null, LearningPathNodeType.Module, 0, true, clock.UtcNow);
        var child = LearningPathNode.Create(path.Id, parent.Id, "Child", null, LearningPathNodeType.Topic, 0, true, clock.UtcNow);
        paths.Nodes[parent.Id] = parent;
        paths.Nodes[child.Id] = child;
        paths.NodeSkills[parent.Id] = [];
        paths.NodeSkills[child.Id] = [];
        paths.NodeResources[parent.Id] = [];
        paths.NodeResources[child.Id] = [];
        var service = CreateService(paths, new FakeGoalRepository(), new FakeSkillRepository(), new FakeResourceRepository(), clock);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateNodeAsync(parent.Id,
            new LearningPathNodeEditModel(child.Id, "Parent", null, LearningPathNodeType.Module, true,
                LearningPathNodeStatus.Planned, [], []), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ArchiveNodeSubtreeAsync_ArchivesDescendantsButNotSibling()
    {
        var clock = new FakeClock();
        var paths = new FakeLearningPathRepository();
        var path = LearningPath.Create("Path", null, LearningPathStatus.Active, LearningPathPriority.Normal,
            null, null, null, null, clock.UtcNow);
        paths.Items.Add(path.Id, path);
        paths.GoalLinks[path.Id] = [];
        var root = LearningPathNode.Create(path.Id, null, "Root", null, LearningPathNodeType.Module, 0, true, clock.UtcNow);
        var child = LearningPathNode.Create(path.Id, root.Id, "Child", null, LearningPathNodeType.Topic, 0, true, clock.UtcNow);
        var sibling = LearningPathNode.Create(path.Id, null, "Sibling", null, LearningPathNodeType.Module, 1, true, clock.UtcNow);
        foreach (var node in new[] { root, child, sibling })
        {
            paths.Nodes[node.Id] = node;
            paths.NodeSkills[node.Id] = [];
            paths.NodeResources[node.Id] = [];
        }
        var service = CreateService(paths, new FakeGoalRepository(), new FakeSkillRepository(), new FakeResourceRepository(), clock);

        await service.ArchiveNodeSubtreeAsync(root.Id, TestContext.Current.CancellationToken);

        Assert.Equal(LearningPathNodeStatus.Archived, paths.Nodes[root.Id].Status);
        Assert.Equal(LearningPathNodeStatus.Archived, paths.Nodes[child.Id].Status);
        Assert.Equal(LearningPathNodeStatus.Planned, paths.Nodes[sibling.Id].Status);
    }

    [Fact]
    public async Task AddRelationAsync_TreatsAlternativeAsSymmetricForDuplicateDetection()
    {
        var clock = new FakeClock();
        var paths = new FakeLearningPathRepository();
        var path = LearningPath.Create("Path", null, LearningPathStatus.Active, LearningPathPriority.Normal,
            null, null, null, null, clock.UtcNow);
        paths.Items.Add(path.Id, path);
        paths.GoalLinks[path.Id] = [];
        var first = LearningPathNode.Create(path.Id, null, "A", null, LearningPathNodeType.Topic, 0, true, clock.UtcNow);
        var second = LearningPathNode.Create(path.Id, null, "B", null, LearningPathNodeType.Topic, 1, true, clock.UtcNow);
        foreach (var node in new[] { first, second })
        {
            paths.Nodes[node.Id] = node;
            paths.NodeSkills[node.Id] = [];
            paths.NodeResources[node.Id] = [];
        }
        var service = CreateService(paths, new FakeGoalRepository(), new FakeSkillRepository(), new FakeResourceRepository(), clock);

        await service.AddRelationAsync(new LearningPathNodeRelationModel(first.Id, second.Id,
            LearningPathNodeRelationType.AlternativeTo, null), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddRelationAsync(
            new LearningPathNodeRelationModel(second.Id, first.Id, LearningPathNodeRelationType.AlternativeTo, null),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task MoveNodeDownAsync_ReordersOnlySiblings()
    {
        var clock = new FakeClock();
        var paths = new FakeLearningPathRepository();
        var path = LearningPath.Create("Path", null, LearningPathStatus.Active, LearningPathPriority.Normal,
            null, null, null, null, clock.UtcNow);
        paths.Items.Add(path.Id, path);
        paths.GoalLinks[path.Id] = [];
        var first = LearningPathNode.Create(path.Id, null, "First", null, LearningPathNodeType.Topic, 0, true, clock.UtcNow);
        var second = LearningPathNode.Create(path.Id, null, "Second", null, LearningPathNodeType.Topic, 1, true, clock.UtcNow);
        paths.Nodes[first.Id] = first;
        paths.Nodes[second.Id] = second;
        paths.NodeSkills[first.Id] = [];
        paths.NodeSkills[second.Id] = [];
        paths.NodeResources[first.Id] = [];
        paths.NodeResources[second.Id] = [];
        var service = CreateService(paths, new FakeGoalRepository(), new FakeSkillRepository(), new FakeResourceRepository(), clock);

        await service.MoveNodeDownAsync(first.Id, TestContext.Current.CancellationToken);

        Assert.Equal(1, paths.Nodes[first.Id].SortOrder);
        Assert.Equal(0, paths.Nodes[second.Id].SortOrder);
    }

    [Fact]
    public async Task CreateNodeAsync_RejectsArchivedResourceAsNewAssignment()
    {
        var clock = new FakeClock();
        var paths = new FakeLearningPathRepository();
        var path = LearningPath.Create("Path", null, LearningPathStatus.Active, LearningPathPriority.Normal,
            null, null, null, null, clock.UtcNow);
        paths.Items.Add(path.Id, path);
        paths.GoalLinks[path.Id] = [];
        var resources = new FakeResourceRepository();
        var resource = Resource.Create("Old", ResourceType.Course, null, null, null, null, null, null, null, null,
            null, null, ResourceDifficulty.Unknown, ResourcePriority.Normal, ResourceStatus.Planned, clock.UtcNow);
        resource.Archive(clock.UtcNow);
        resources.Items[resource.Id] = resource;
        resources.Tags[resource.Id] = [];
        var service = CreateService(paths, new FakeGoalRepository(), new FakeSkillRepository(), resources, clock);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateNodeAsync(path.Id,
            new LearningPathNodeEditModel(null, "Node", null, LearningPathNodeType.Topic, true,
                LearningPathNodeStatus.Planned, [], [resource.Id]), TestContext.Current.CancellationToken));
    }

    private static LearningPathService CreateService(FakeLearningPathRepository paths, FakeGoalRepository goals,
        FakeSkillRepository skills, FakeResourceRepository resources, FakeClock clock)
        => new(paths, skills, resources, goals, clock);
}
