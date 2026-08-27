using SASD.LearningManager.Domain.Providers;

namespace SASD.LearningManager.Domain.Tests;

public sealed class ProviderTests
{
    [Fact]
    public void Restore_ArchivedProvider_ReturnsAsInactive()
    {
        var now = DateTimeOffset.UtcNow;
        var provider = Provider.Create("Example", "https://example.test", null, ProviderType.LearningPlatform, now);
        provider.Archive(now.AddMinutes(1));

        provider.Restore(now.AddMinutes(2));

        Assert.Equal(ProviderStatus.Inactive, provider.Status);
        Assert.Null(provider.ArchivedAtUtc);
    }
    [Fact]
    public void Restore_NonArchivedProvider_DoesNotDemoteIt()
    {
        var now = DateTimeOffset.UtcNow;
        var provider = Provider.Create("Example", "https://example.test", null, ProviderType.LearningPlatform, now);

        provider.Restore(now.AddMinutes(1));

        Assert.Equal(ProviderStatus.Active, provider.Status);
    }

    [Fact]
    public void Update_ArchivedProvider_IsRejected()
    {
        var now = DateTimeOffset.UtcNow;
        var provider = Provider.Create("Example", "https://example.test", null, ProviderType.LearningPlatform, now);
        provider.Archive(now.AddMinutes(1));

        Assert.Throws<SASD.LearningManager.Domain.Common.DomainValidationException>(() =>
            provider.Update("Changed", null, null, ProviderType.Other, now.AddMinutes(2)));
    }

}
