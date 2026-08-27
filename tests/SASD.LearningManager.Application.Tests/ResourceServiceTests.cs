using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Application.Tests;

public sealed class ResourceServiceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesTagsAndStoresOneCanonicalResource()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);

        var id = await service.CreateAsync(
            Model("https://EXAMPLE.test/course#part", [" docker ", "Docker", "linux"]),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(resources.Items);
        Assert.Equal(id, resources.Items.Single().Key);
        Assert.Equal(
            new[] { "docker", "linux" },
            resources.Tags[id].Select(static tag => tag.ToLowerInvariant()).ToArray());
        Assert.Equal("https://example.test/course", resources.Items[id].NormalizedUrl);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUrl_ThrowsInsteadOfCreatingCopy()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);
        await service.CreateAsync(Model("https://example.test/course", []), cancellationToken: TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DuplicateResourceException>(() => service.CreateAsync(
            Model("HTTPS://EXAMPLE.TEST:443/course#x", []),
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Single(resources.Items);
    }

    [Fact]
    public async Task CreateAsync_ExplicitDuplicateOverride_AllowsDocumentedExceptionCase()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);
        await service.CreateAsync(Model("https://example.test/course", []), cancellationToken: TestContext.Current.CancellationToken);

        await service.CreateAsync(
            Model("https://example.test/course#other-context", []),
            allowDuplicateUrl: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, resources.Items.Count);
    }

    [Fact]
    public async Task ArchiveAndRestore_PreservesTheSameResourceIdentity()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);
        var id = await service.CreateAsync(Model("https://example.test/a", ["one"]), cancellationToken: TestContext.Current.CancellationToken);

        await service.ArchiveAsync(id, TestContext.Current.CancellationToken);
        Assert.Equal(ResourceStatus.Archived, resources.Items[id].Status);

        await service.RestoreAsync(id, TestContext.Current.CancellationToken);
        Assert.Equal(ResourceStatus.Planned, resources.Items[id].Status);
        Assert.Single(resources.Items);
    }

    [Fact]
    public async Task OpenUrlAsync_DelegatesOnlyValidatedHttpUrl()
    {
        var resources = new FakeResourceRepository();
        var launcher = new FakeLinkLauncher();
        var service = CreateService(resources, launcher);
        var id = await service.CreateAsync(Model("https://example.test/learn", []), cancellationToken: TestContext.Current.CancellationToken);

        await service.OpenUrlAsync(id, TestContext.Current.CancellationToken);

        Assert.Equal(new Uri("https://example.test/learn"), launcher.LastOpened);
    }

    [Fact]
    public async Task CreateAsync_ArchivedStatus_IsRejectedBecauseArchiveHasDedicatedUseCase()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);
        var model = Model("https://example.test/archive", []) with { Status = ResourceStatus.Archived };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(model, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(resources.Items);
    }

    [Fact]
    public async Task QuickCaptureAsync_UrlOnly_CreatesMinimalInboxResource()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);

        var id = await service.QuickCaptureAsync(
            new QuickCaptureModel("https://example.test/article#section", null, "Später für Docker Networking prüfen."),
            cancellationToken: TestContext.Current.CancellationToken);

        var resource = resources.Items[id];
        Assert.Equal("(Titel noch nicht ermittelt)", resource.Title);
        Assert.Equal(ResourceStatus.Inbox, resource.Status);
        Assert.Equal(ResourceType.Other, resource.Type);
        Assert.Equal(ResourceDifficulty.Unknown, resource.Difficulty);
        Assert.Equal(ResourcePriority.Normal, resource.Priority);
        Assert.Equal("https://example.test/article", resource.NormalizedUrl);
        Assert.Equal("Später für Docker Networking prüfen.", resource.WhySaved);
        Assert.Null(resource.ProviderId);
        Assert.Null(resource.ProgressPercent);
    }

    [Fact]
    public async Task QuickCaptureAsync_DuplicateUrl_UsesCanonicalDuplicateProtection()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);
        await service.QuickCaptureAsync(
            new QuickCaptureModel("https://example.test/item", "Erster", null),
            cancellationToken: TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DuplicateResourceException>(() => service.QuickCaptureAsync(
            new QuickCaptureModel("HTTPS://EXAMPLE.TEST:443/item#x", "Zweiter", null),
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Single(resources.Items);
    }

    [Fact]
    public async Task ClassifyInboxAsync_ChangesInboxResourceToPlannedAndKeepsIdentity()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);
        var id = await service.QuickCaptureAsync(
            new QuickCaptureModel("https://example.test/course", "Course", "Capture note"),
            cancellationToken: TestContext.Current.CancellationToken);

        var model = Model("https://example.test/course", ["docker"]) with
        {
            Title = "Docker Networking Course",
            Status = ResourceStatus.Planned
        };

        await service.ClassifyInboxAsync(id, model, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(resources.Items);
        Assert.Equal(ResourceStatus.Planned, resources.Items[id].Status);
        Assert.Equal("Docker Networking Course", resources.Items[id].Title);
        Assert.Contains("docker", resources.Tags[id], StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ClassifyInboxAsync_InboxTargetStatus_IsRejected()
    {
        var resources = new FakeResourceRepository();
        var service = CreateService(resources);
        var id = await service.QuickCaptureAsync(
            new QuickCaptureModel("https://example.test/course", "Course", null),
            cancellationToken: TestContext.Current.CancellationToken);
        var model = Model("https://example.test/course", []) with { Status = ResourceStatus.Inbox };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClassifyInboxAsync(
            id,
            model,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(ResourceStatus.Inbox, resources.Items[id].Status);
    }

    private static ResourceService CreateService(FakeResourceRepository resources, FakeLinkLauncher? launcher = null)
        => new(resources, new FakeProviderRepository(), new UrlNormalizer(), launcher ?? new FakeLinkLauncher(), new FakeClock());

    private static ResourceEditModel Model(string url, IReadOnlyCollection<string> tags)
        => new(
            "Test Course",
            ResourceType.Course,
            null,
            url,
            null,
            "Description",
            "Reason",
            "Trainer",
            "de",
            "1",
            60,
            ResourceDifficulty.Intermediate,
            ResourcePriority.Normal,
            ResourceStatus.Planned,
            0,
            tags);
}
