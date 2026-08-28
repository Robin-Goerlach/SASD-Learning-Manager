using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Tests;

public sealed class SkillServiceTests
{
    [Fact]
    public async Task CreateAsync_AssignsTaxonomyAndKeepsCurrentLevelUnknown()
    {
        var catalog = new FakeCompetencyCatalogRepository();
        var clock = new FakeClock();
        var area = CompetencyArea.Create("Linux", null, clock.UtcNow);
        var topic = Topic.Create("systemd", null, clock.UtcNow);
        catalog.Areas.Add(area.Id, area);
        catalog.Topics.Add(topic.Id, topic);
        var repo = new FakeSkillRepository();
        var service = new SkillService(repo, catalog, clock);

        var id = await service.CreateAsync(
            new SkillEditModel("Services diagnostizieren", null, 4, SkillStatus.Active, [area.Id], [topic.Id]),
            TestContext.Current.CancellationToken);

        Assert.Null(repo.Items[id].CurrentLevel);
        Assert.Equal(4, repo.Items[id].TargetLevel);
        Assert.Contains(area.Id, repo.Areas[id]);
        Assert.Contains(topic.Id, repo.Topics[id]);
    }

    [Fact]
    public async Task AssessAsync_AppendsHistoryAndUpdatesCurrentSnapshot()
    {
        var catalog = new FakeCompetencyCatalogRepository();
        var repo = new FakeSkillRepository();
        var clock = new FakeClock();
        var service = new SkillService(repo, catalog, clock);
        var id = await service.CreateAsync(
            new SkillEditModel("Docker Networking", null, 4, SkillStatus.Active, [], []),
            TestContext.Current.CancellationToken);

        await service.AssessAsync(id,
            new SkillAssessmentModel(2, SkillAssessmentType.PracticalReview, "Compose Lab"),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, repo.Items[id].CurrentLevel);
        Assert.Single(repo.Assessments[id]);
        Assert.Equal("Compose Lab", repo.Assessments[id][0].Reason);
    }

    [Fact]
    public async Task UpdateAsync_AllowsPreviouslyLinkedArchivedAreaButRejectsNewArchivedArea()
    {
        var catalog = new FakeCompetencyCatalogRepository();
        var repo = new FakeSkillRepository();
        var clock = new FakeClock();
        var active = CompetencyArea.Create("Linux", null, clock.UtcNow);
        var archived = CompetencyArea.Create("Legacy", null, clock.UtcNow);
        archived.Archive(clock.UtcNow.AddMinutes(1));
        catalog.Areas.Add(active.Id, active);
        catalog.Areas.Add(archived.Id, archived);
        var service = new SkillService(repo, catalog, clock);
        var id = await service.CreateAsync(
            new SkillEditModel("Skill", null, 3, SkillStatus.Active, [active.Id], []),
            TestContext.Current.CancellationToken);

        repo.Areas[id] = [archived.Id]; // Simulates a historic relation archived later.
        await service.UpdateAsync(id,
            new SkillEditModel("Skill", null, 3, SkillStatus.Active, [archived.Id], []),
            TestContext.Current.CancellationToken);

        var secondId = await service.CreateAsync(
            new SkillEditModel("Second", null, 3, SkillStatus.Active, [], []),
            TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(secondId,
            new SkillEditModel("Second", null, 3, SkillStatus.Active, [archived.Id], []),
            TestContext.Current.CancellationToken));
    }
}
