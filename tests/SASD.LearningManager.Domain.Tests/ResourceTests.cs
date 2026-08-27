using SASD.LearningManager.Domain.Common;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Domain.Tests;

public sealed class ResourceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_CompletedResource_SetsCompletionAndProgress()
    {
        var resource = CreateResource(ResourceStatus.Completed);

        Assert.Equal(100, resource.ProgressPercent);
        Assert.Equal(Now, resource.StartedAtUtc);
        Assert.Equal(Now, resource.CompletedAtUtc);
    }

    [Fact]
    public void SetProgress_RejectsValuesAboveOneHundred()
    {
        var resource = CreateResource(ResourceStatus.Planned);

        Assert.Throws<DomainValidationException>(() => resource.SetProgress(101, Now));
    }

    [Fact]
    public void Archive_PreservesIdentityAndSetsArchiveTimestamp()
    {
        var resource = CreateResource(ResourceStatus.Started);
        var id = resource.Id;

        resource.Archive(Now.AddHours(1));

        Assert.Equal(id, resource.Id);
        Assert.Equal(ResourceStatus.Archived, resource.Status);
        Assert.NotNull(resource.ArchivedAtUtc);
    }

    [Fact]
    public void Restore_ArchivedResource_ReturnsToPlannedWithoutChangingProgress()
    {
        var resource = CreateResource(ResourceStatus.Started);
        resource.SetProgress(42, Now);
        resource.Archive(Now.AddHours(1));

        resource.Restore(Now.AddHours(2));

        Assert.Equal(ResourceStatus.Planned, resource.Status);
        Assert.Equal(42, resource.ProgressPercent);
        Assert.Null(resource.ArchivedAtUtc);
    }

    [Fact]
    public void Create_RejectsJavascriptUrl()
    {
        Assert.Throws<DomainValidationException>(() => Resource.Create(
            "Unsafe",
            ResourceType.Article,
            null,
            "javascript:alert(1)",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            ResourceDifficulty.Unknown,
            ResourcePriority.Normal,
            ResourceStatus.Planned,
            Now));
    }

    private static Resource CreateResource(ResourceStatus status)
        => Resource.Create(
            "Docker Networking",
            ResourceType.Course,
            null,
            "https://example.test/course",
            "https://example.test/course",
            null,
            "Test resource",
            "Learning path",
            "Trainer",
            "de",
            "1",
            120,
            ResourceDifficulty.Intermediate,
            ResourcePriority.High,
            status,
            Now);
}
