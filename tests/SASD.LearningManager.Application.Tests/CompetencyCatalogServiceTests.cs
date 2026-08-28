using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Domain.Competencies;

namespace SASD.LearningManager.Application.Tests;

public sealed class CompetencyCatalogServiceTests
{
    [Fact]
    public async Task CreateTopicAsync_LinksMultipleAreas()
    {
        var repo = new FakeCompetencyCatalogRepository();
        var clock = new FakeClock();
        var linux = CompetencyArea.Create("Linux", null, clock.UtcNow);
        var cloud = CompetencyArea.Create("Cloud", null, clock.UtcNow);
        repo.Areas.Add(linux.Id, linux);
        repo.Areas.Add(cloud.Id, cloud);
        var service = new CompetencyCatalogService(repo, clock);

        var id = await service.CreateTopicAsync(new TopicEditModel(
            "Container Networking", null, CatalogStatus.Active, [linux.Id, cloud.Id]),
            TestContext.Current.CancellationToken);

        Assert.Contains(linux.Id, repo.TopicAreas[id]);
        Assert.Contains(cloud.Id, repo.TopicAreas[id]);
    }

    [Fact]
    public async Task CreateAreaAsync_RejectsCaseInsensitiveDuplicateName()
    {
        var repo = new FakeCompetencyCatalogRepository();
        var clock = new FakeClock();
        var service = new CompetencyCatalogService(repo, clock);
        await service.CreateAreaAsync(new CompetencyAreaEditModel("Linux", null, CatalogStatus.Active), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAreaAsync(
            new CompetencyAreaEditModel("linux", null, CatalogStatus.Active), TestContext.Current.CancellationToken));
    }
}
