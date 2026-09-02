using Microsoft.Extensions.Logging.Abstractions;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.Domain.Skills;
using SASD.LearningManager.Infrastructure.Persistence;
using SASD.LearningManager.Infrastructure.Persistence.Repositories;

namespace SASD.LearningManager.Infrastructure.Tests;

public sealed class SqliteIntegrationTests : IAsyncLifetime
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SASD-LearningManager-Tests", Guid.NewGuid().ToString("N"));
    private SqliteConnectionFactory _connectionFactory = null!;
    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(_directory);
        _connectionFactory = new SqliteConnectionFactory(Path.Combine(_directory, "test.db"));
        var initializer = new DatabaseInitializer(_connectionFactory, NullLogger<DatabaseInitializer>.Instance);
        await initializer.InitializeAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // SQLite pooling can briefly retain a handle on Windows. Test cleanup must not hide test results.
        }
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Migration_0005_creates_knowledge_and_evidence_schema()
    {
        await using var connection = await _connectionFactory.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name IN ('KnowledgeArtifacts','Evidence','EvidenceSkills','KnowledgeArtifactResources');";

        Assert.Equal(4L, (long)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!);
    }

    [Fact]
    public async Task Migrations_CreateSeedProviders()
    {
        var repository = new ProviderRepository(_connectionFactory);

        var providers = await repository.ListAsync(includeArchived: false, TestContext.Current.CancellationToken);

        Assert.Contains(providers, p => p.Name == "O'Reilly");
        Assert.Contains(providers, p => p.Name == "LinkedIn Learning");
        Assert.Contains(providers, p => p.Name == "Eigene Quelle");
    }

    [Fact]
    public async Task ResourceRepository_RoundTripsResourceAndTags()
    {
        var repository = new ResourceRepository(_connectionFactory);
        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var resource = Resource.Create(
            "Linux Performance",
            ResourceType.Course,
            null,
            "https://example.test/perf",
            "https://example.test/perf",
            null,
            "Diagnostics",
            "Refresh",
            "Trainer",
            "en",
            "2026",
            90,
            ResourceDifficulty.Advanced,
            ResourcePriority.High,
            ResourceStatus.Started,
            now);
        resource.SetProgress(35, now);

        await repository.InsertAsync(resource, ["linux", "performance"], TestContext.Current.CancellationToken);
        var detail = await repository.GetDetailAsync(resource.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(detail);
        Assert.Equal("Linux Performance", detail.Title);
        Assert.Equal(35, detail.ProgressPercent);
        Assert.Equal(
            new[] { "linux", "performance" },
            detail.Tags.Select(static tag => tag.ToLowerInvariant()).ToArray());
    }

    [Fact]
    public async Task Search_FiltersTextAndPaginates()
    {
        var repository = new ResourceRepository(_connectionFactory);
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 12; i++)
        {
            var resource = Resource.Create(
                $"Docker Course {i:00}", ResourceType.Course, null,
                $"https://example.test/docker/{i}", $"https://example.test/docker/{i}", null,
                null, null, null, "en", null, null,
                ResourceDifficulty.Beginner, ResourcePriority.Normal, ResourceStatus.Planned, now.AddMinutes(i));
            await repository.InsertAsync(resource, [], TestContext.Current.CancellationToken);
        }

        var page = await repository.SearchAsync(
            new ResourceSearchCriteria("Docker", null, null, null, null, false, 2, 5),
            TestContext.Current.CancellationToken);

        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(5, page.Items.Count);
    }

    [Fact]
    public async Task SearchInboxAsync_ReturnsOnlyInboxResourcesNewestFirst()
    {
        var repository = new ResourceRepository(_connectionFactory);
        var start = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var first = Resource.Create(
            "First Inbox", ResourceType.Other, null,
            "https://example.test/first", "https://example.test/first", null,
            null, "first note", null, null, null, null,
            ResourceDifficulty.Unknown, ResourcePriority.Normal, ResourceStatus.Inbox, start);
        var second = Resource.Create(
            "Second Inbox", ResourceType.Other, null,
            "https://example.test/second", "https://example.test/second", null,
            null, "second note", null, null, null, null,
            ResourceDifficulty.Unknown, ResourcePriority.Normal, ResourceStatus.Inbox, start.AddMinutes(1));
        var planned = Resource.Create(
            "Planned", ResourceType.Course, null,
            "https://example.test/planned", "https://example.test/planned", null,
            null, null, null, null, null, null,
            ResourceDifficulty.Beginner, ResourcePriority.Normal, ResourceStatus.Planned, start.AddMinutes(2));

        await repository.InsertAsync(first, [], TestContext.Current.CancellationToken);
        await repository.InsertAsync(second, [], TestContext.Current.CancellationToken);
        await repository.InsertAsync(planned, [], TestContext.Current.CancellationToken);

        var page = await repository.SearchInboxAsync(new InboxSearchCriteria(null, 1, 10), TestContext.Current.CancellationToken);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(new[] { "Second Inbox", "First Inbox" }, page.Items.Select(static x => x.Title).ToArray());
        Assert.Equal("second note", page.Items[0].Note);
    }

    [Fact]
    public async Task FindByNormalizedUrlAsync_ExcludesEditedResourceButFindsAnotherDuplicate()
    {
        var repository = new ResourceRepository(_connectionFactory);
        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var first = Resource.Create(
            "First", ResourceType.Course, null,
            "https://example.test/same", "https://example.test/same", null,
            null, null, null, null, null, null,
            ResourceDifficulty.Unknown, ResourcePriority.Normal, ResourceStatus.Planned, now);
        var second = Resource.Create(
            "Second", ResourceType.Course, null,
            "https://example.test/same", "https://example.test/same", null,
            null, null, null, null, null, null,
            ResourceDifficulty.Unknown, ResourcePriority.Normal, ResourceStatus.Planned, now.AddMinutes(1));
        await repository.InsertAsync(first, [], TestContext.Current.CancellationToken);
        await repository.InsertAsync(second, [], TestContext.Current.CancellationToken);

        var duplicate = await repository.FindByNormalizedUrlAsync("https://example.test/same", second.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(duplicate);
        Assert.Equal(first.Id, duplicate.Id);
    }

    [Fact]
    public async Task M3Repositories_RoundTripTaxonomySkillAssessmentAndGoalLinks()
    {
        var catalog = new CompetencyCatalogRepository(_connectionFactory);
        var skills = new SkillRepository(_connectionFactory);
        var goals = new GoalRepository(_connectionFactory);
        var now = new DateTimeOffset(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);

        var area = CompetencyArea.Create("Linux", "Operating systems", now);
        await catalog.InsertAreaAsync(area, TestContext.Current.CancellationToken);
        var topic = Topic.Create("systemd", "Service management", now);
        await catalog.InsertTopicAsync(topic, [area.Id], TestContext.Current.CancellationToken);

        var skill = Skill.Create("Services diagnostizieren", "systemctl und journalctl", 4, now);
        await skills.InsertAsync(skill, [area.Id], [topic.Id], TestContext.Current.CancellationToken);
        var assessment = SkillAssessment.Create(skill.Id, 2, SkillAssessmentType.PracticalReview, "Lab", now, now);
        skill.ApplyAssessment(2, now);
        await skills.AddAssessmentAsync(skill, assessment, TestContext.Current.CancellationToken);

        var goal = Goal.Create("Linux vertiefen", null, GoalType.Learning, "Jobprofil", GoalPriority.High,
            GoalStatus.Active, new DateOnly(2026, 12, 31), "systemd Lab", null, now);
        await goals.InsertAsync(goal, [skill.Id], TestContext.Current.CancellationToken);

        var skillDetail = await skills.GetDetailAsync(skill.Id, TestContext.Current.CancellationToken);
        var history = await skills.ListAssessmentsAsync(skill.Id, TestContext.Current.CancellationToken);
        var goalDetail = await goals.GetDetailAsync(goal.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(skillDetail);
        Assert.Equal(2, skillDetail.CurrentLevel);
        Assert.Contains(area.Id, skillDetail.CompetencyAreaIds);
        Assert.Contains(topic.Id, skillDetail.TopicIds);
        Assert.Single(history);
        Assert.NotNull(goalDetail);
        Assert.Contains(skill.Id, goalDetail.SkillIds);
    }

    [Fact]
    public async Task M3SkillSearch_ComputesGapAndTaxonomyLabels()
    {
        var catalog = new CompetencyCatalogRepository(_connectionFactory);
        var skills = new SkillRepository(_connectionFactory);
        var now = new DateTimeOffset(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);
        var area = CompetencyArea.Create("Cloud", null, now);
        await catalog.InsertAreaAsync(area, TestContext.Current.CancellationToken);
        var topic = Topic.Create("Docker Networking", null, now);
        await catalog.InsertTopicAsync(topic, [area.Id], TestContext.Current.CancellationToken);
        var skill = Skill.Create("Bridge-Netzwerke konfigurieren", null, 4, now);
        await skills.InsertAsync(skill, [area.Id], [topic.Id], TestContext.Current.CancellationToken);
        var assessment = SkillAssessment.Create(skill.Id, 2, SkillAssessmentType.SelfAssessment, null, now, now);
        skill.ApplyAssessment(2, now);
        await skills.AddAssessmentAsync(skill, assessment, TestContext.Current.CancellationToken);

        var result = await skills.SearchAsync(new SkillSearchCriteria("Bridge", null, false, 1, 100), TestContext.Current.CancellationToken);

        var item = Assert.Single(result.Items);
        Assert.Equal(2, item.Gap);
        Assert.Contains("Cloud", item.CompetencyAreas);
        Assert.Contains("Docker Networking", item.Topics);
    }

    [Fact]
    public async Task M3Database_HasNoForeignKeyViolations()
    {
        await using var connection = await _connectionFactory.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key_check;";
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        Assert.False(await reader.ReadAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task M4Repository_RoundTripsPathNodesAssignmentsAndRelations()
    {
        var paths = new LearningPathRepository(_connectionFactory);
        var skills = new SkillRepository(_connectionFactory);
        var resources = new ResourceRepository(_connectionFactory);
        var now = new DateTimeOffset(2026, 8, 28, 8, 0, 0, TimeSpan.Zero);
        var skill = Skill.Create("Docker Networking", null, 4, now);
        await skills.InsertAsync(skill, [], [], TestContext.Current.CancellationToken);
        var resource = Resource.Create("Docker Course", ResourceType.Course, null, "https://example.test/docker-path",
            "https://example.test/docker-path", null, null, null, null, null, null, null,
            ResourceDifficulty.Beginner, ResourcePriority.High, ResourceStatus.Planned, now);
        await resources.InsertAsync(resource, [], TestContext.Current.CancellationToken);
        var path = LearningPath.Create("Docker Refresher", null, LearningPathStatus.Active, LearningPathPriority.High,
            null, null, "Networking", null, now);
        await paths.InsertAsync(path, [], TestContext.Current.CancellationToken);
        var module = LearningPathNode.Create(path.Id, null, "Networking", null, LearningPathNodeType.Module, 0, true, now);
        await paths.InsertNodeAsync(module, [skill.Id], [resource.Id], TestContext.Current.CancellationToken);
        var lab = LearningPathNode.Create(path.Id, module.Id, "Lab", null, LearningPathNodeType.Activity, 0, false, now);
        await paths.InsertNodeAsync(lab, [], [], TestContext.Current.CancellationToken);
        var relation = LearningPathNodeRelation.Create(module.Id, lab.Id, LearningPathNodeRelationType.RecommendedBefore, "Course before lab", now);
        await paths.InsertRelationAsync(relation, TestContext.Current.CancellationToken);

        var detail = await paths.GetNodeDetailAsync(module.Id, TestContext.Current.CancellationToken);
        var nodes = await paths.ListNodesAsync(path.Id, includeArchived: false, TestContext.Current.CancellationToken);
        var relations = await paths.ListRelationsAsync(path.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(detail);
        Assert.Contains(skill.Id, detail.SkillIds);
        Assert.Contains(resource.Id, detail.ResourceIds);
        Assert.Equal(2, nodes.Count);
        var savedRelation = Assert.Single(relations);
        Assert.Equal(LearningPathNodeRelationType.RecommendedBefore, savedRelation.Type);
    }

    [Fact]
    public async Task M4Search_ComputesCoreProgressFromRequiredNodes()
    {
        var paths = new LearningPathRepository(_connectionFactory);
        var now = new DateTimeOffset(2026, 8, 28, 8, 0, 0, TimeSpan.Zero);
        var path = LearningPath.Create("Performance", null, LearningPathStatus.Active, LearningPathPriority.High,
            null, null, null, null, now);
        await paths.InsertAsync(path, [], TestContext.Current.CancellationToken);
        var done = LearningPathNode.Create(path.Id, null, "CPU", null, LearningPathNodeType.Topic, 0, true, now);
        done.Update("CPU", null, LearningPathNodeType.Topic, true, LearningPathNodeStatus.Completed, now);
        var open = LearningPathNode.Create(path.Id, null, "Memory", null, LearningPathNodeType.Topic, 1, true, now);
        var optional = LearningPathNode.Create(path.Id, null, "NUMA deep dive", null, LearningPathNodeType.Topic, 2, false, now);
        optional.Update("NUMA deep dive", null, LearningPathNodeType.Topic, false, LearningPathNodeStatus.Completed, now);
        await paths.InsertNodeAsync(done, [], [], TestContext.Current.CancellationToken);
        await paths.InsertNodeAsync(open, [], [], TestContext.Current.CancellationToken);
        await paths.InsertNodeAsync(optional, [], [], TestContext.Current.CancellationToken);

        var result = await paths.SearchAsync(new LearningPathSearchCriteria("Performance", null, false, 1, 100), TestContext.Current.CancellationToken);

        var item = Assert.Single(result.Items);
        Assert.Equal(1, item.RequiredCompleted);
        Assert.Equal(2, item.RequiredTotal);
        Assert.Equal(50m, item.CoreCompletionPercent);
    }

    [Fact]
    public async Task M4Migration_CreatesExpectedLearningPathTables()
    {
        await using var connection = await _connectionFactory.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE 'LearningPath%' ORDER BY name;";
        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            names.Add(reader.GetString(0));
        }

        Assert.Contains("LearningPaths", names);
        Assert.Contains("LearningPathNodes", names);
        Assert.Contains("LearningPathNodeSkills", names);
        Assert.Contains("LearningPathNodeResources", names);
        Assert.Contains("LearningPathNodeRelations", names);
    }

}
