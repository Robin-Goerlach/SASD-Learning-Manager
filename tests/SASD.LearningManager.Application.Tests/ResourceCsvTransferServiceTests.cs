using SASD.LearningManager.Application.Abstractions;
using SASD.LearningManager.Application.ImportExport;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Resources;

namespace SASD.LearningManager.Application.Tests;

/// <summary>Application-level tests for portable resource CSV import behavior.</summary>
public sealed class ResourceCsvTransferServiceTests
{
    private const string Header = "Title,Type,Provider,Url,LocalPath,Description,WhySaved,Creator,LanguageCode,VersionText,EstimatedMinutes,Difficulty,Priority,Status,ProgressPercent,Tags";

    [Fact]
    public async Task ImportAsync_CreatesMissingProviderAndResourceWithTags()
    {
        var providers = new FakeProviderRepository();
        var resources = new FakeResourceRepository();
        var service = CreateService(providers, resources);
        var path = CreateTemporaryCsv(
            Header + Environment.NewLine +
            "Docker Compose Lab,Lab,Example Provider,https://example.test/docker-compose,,,,,de,,,Intermediate,High,Planned,,docker;compose" + Environment.NewLine);

        try
        {
            var report = await service.ImportAsync(path, TestContext.Current.CancellationToken);

            Assert.Equal(1, report.TotalRows);
            Assert.Equal(1, report.Created);
            Assert.Equal(0, report.SkippedDuplicates);
            Assert.Empty(report.Errors);
            Assert.Single(providers.Items);
            var resource = Assert.Single(resources.Items.Values);
            Assert.Equal("Docker Compose Lab", resource.Title);
            Assert.Equal(ResourceType.Lab, resource.Type);
            Assert.Contains("docker", resources.Tags[resource.Id], StringComparer.OrdinalIgnoreCase);
            Assert.Contains("compose", resources.Tags[resource.Id], StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ImportAsync_SkipsExistingCanonicalUrl()
    {
        var providers = new FakeProviderRepository();
        var resources = new FakeResourceRepository();
        var service = CreateService(providers, resources);
        var path = CreateTemporaryCsv(
            Header + Environment.NewLine +
            "First,Course,,https://example.test/course,,,,,en,,,Intermediate,Normal,Planned,,first" + Environment.NewLine +
            "Duplicate,Course,,https://example.test/course/,,,,,en,,,Intermediate,Normal,Planned,,duplicate" + Environment.NewLine);

        try
        {
            var report = await service.ImportAsync(path, TestContext.Current.CancellationToken);

            Assert.Equal(2, report.TotalRows);
            Assert.Equal(1, report.Created);
            Assert.Equal(1, report.SkippedDuplicates);
            Assert.Single(resources.Items);
            Assert.Single(report.Errors);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ImportAsync_ReportsInvalidRowAndContinuesWithFollowingRow()
    {
        var providers = new FakeProviderRepository();
        var resources = new FakeResourceRepository();
        var service = CreateService(providers, resources);
        var path = CreateTemporaryCsv(
            Header + Environment.NewLine +
            "Bad Type,NotAType,,,,,,,en,,,Intermediate,Normal,Planned,,bad" + Environment.NewLine +
            "Good Course,Course,,,,,,,en,,,Beginner,Normal,Planned,,good" + Environment.NewLine);

        try
        {
            var report = await service.ImportAsync(path, TestContext.Current.CancellationToken);

            Assert.Equal(2, report.TotalRows);
            Assert.Equal(1, report.Created);
            Assert.Equal(0, report.SkippedDuplicates);
            var error = Assert.Single(report.Errors);
            Assert.Equal(2, error.RowNumber);
            Assert.Contains("Type", error.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Single(resources.Items);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ImportAsync_ShippedChatRecommendationFixture_IsSchemaCompatible()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "resources-chat-recommendations.csv");
        Assert.True(File.Exists(path), $"The shipped CSV fixture was not copied to the test output: {path}");

        var providers = new FakeProviderRepository();
        var resources = new FakeResourceRepository();
        var service = CreateService(providers, resources);

        var report = await service.ImportAsync(path, TestContext.Current.CancellationToken);

        // The fixture is intentionally substantial: this protects the actual hand-off data against
        // column drift, invalid enum values, malformed quoting and accidental duplicate URLs.
        Assert.True(report.TotalRows >= 25, $"Expected a substantial recommendation fixture, got {report.TotalRows} rows.");
        Assert.Equal(report.TotalRows, report.Created);
        Assert.Equal(0, report.SkippedDuplicates);
        Assert.Empty(report.Errors);
        Assert.Equal(report.TotalRows, resources.Items.Count);
        Assert.NotEmpty(providers.Items);
    }

    private static ResourceCsvTransferService CreateService(
        FakeProviderRepository providers,
        FakeResourceRepository resources)
    {
        var clock = new FakeClock();
        var providerService = new ProviderService(providers, clock);
        var resourceService = new ResourceService(
            resources,
            providers,
            new TestUrlNormalizer(),
            new FakeLinkLauncher(),
            clock);
        return new ResourceCsvTransferService(resourceService, providerService);
    }

    private static string CreateTemporaryCsv(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"sasd-learning-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Minimal normalizer for Application tests. It deliberately mirrors the production duplicate
    /// behavior relevant to these tests without introducing an Infrastructure project reference.
    /// </summary>
    private sealed class TestUrlNormalizer : IUrlNormalizer
    {
        public string? Normalize(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var uri = new Uri(url.Trim(), UriKind.Absolute);
            var builder = new UriBuilder(uri)
            {
                Fragment = string.Empty,
                Host = uri.Host.ToLowerInvariant(),
                Scheme = uri.Scheme.ToLowerInvariant()
            };

            var normalized = builder.Uri.AbsoluteUri;
            return normalized.EndsWith("/", StringComparison.Ordinal)
                ? normalized[..^1]
                : normalized;
        }
    }
}
