using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.Application.Tests;

public sealed class DocumentServiceTests
{
    [Fact]
    public async Task AttachToApplicationAsync_CreatesImmutableMetadataSnapshot()
    {
        var now = new DateTimeOffset(2026, 8, 26, 10, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OpportunityId = Guid.NewGuid(),
            StartedAtUtc = now,
            Channel = ApplicationChannel.Email,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        application.InitializeStage(ApplicationStage.Draft, now);
        store.Applications.Add(application);
        var archive = new FakeArchive();
        var service = new DocumentService(store, archive, new FixedClock(now));

        var document = await service.RegisterAsync(new DocumentInput(
            DocumentType.Cv,
            "Linux CV",
            "2026-08",
            "DE",
            "Linux, DevOps",
            "C:/synthetic/cv.pdf"));
        var snapshot = await service.AttachToApplicationAsync(application.Id, document.Id);

        Assert.Equal(document.Sha256, snapshot.Sha256);
        Assert.Equal("/private/snapshot.pdf", snapshot.StoredPath);
        Assert.Single(store.ApplicationDocumentSnapshots);
    }

    private sealed class FakeArchive : IDocumentArchive
    {
        public Task<DocumentInspection> InspectAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(new DocumentInspection("C:/synthetic/cv.pdf", new string('A', 64), 1234));

        public Task<string> CreateApplicationSnapshotAsync(Guid applicationId, string sourcePath, string expectedSha256, CancellationToken cancellationToken = default)
            => Task.FromResult("/private/snapshot.pdf");
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
