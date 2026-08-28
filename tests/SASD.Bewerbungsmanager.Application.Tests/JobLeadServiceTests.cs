using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Tests;

public sealed class JobLeadServiceTests
{
    [Fact]
    public async Task ImportBatch_DeduplicatesCanonicalUrl_AndMarksSearchProfileChecked()
    {
        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var profile = new SearchProfile
        {
            Id = Guid.NewGuid(),
            Name = "Linux Jobs",
            Source = "Example Portal",
            Url = "https://example.invalid/jobs",
            CheckIntervalDays = 2,
            NextCheckAtUtc = now,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        store.SearchProfiles.Add(profile);
        var service = CreateService(store, now);

        var batch = new JobSourceBatch(
            1,
            "Example Portal",
            profile.Id,
            now,
            [
                new JobSourceItem("job-42", "Linux Engineer", "Example GmbH", "Darmstadt", "Hybrid", null,
                    "https://jobs.example.invalid/42?utm_source=alert", "Synthetic role", null),
                new JobSourceItem(null, "Linux Engineer", "Example GmbH", "Darmstadt", "Hybrid", null,
                    "https://jobs.example.invalid/42", "Synthetic role duplicate", null),
            ]);

        var result = await service.ImportBatchAsync(batch);

        Assert.Equal(1, result.Imported);
        Assert.Equal(1, result.Duplicates);
        Assert.Single(store.JobLeads);
        Assert.Equal(now, profile.LastCheckedAtUtc);
        Assert.Equal(now.AddDays(2), profile.NextCheckAtUtc);
    }

    [Fact]
    public async Task Promote_CreatesOpportunityAndSourceLink_AndLinksLead()
    {
        var now = new DateTimeOffset(2026, 8, 27, 9, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var employer = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Example Systems GmbH",
            Type = OrganizationType.Employer,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        store.Organizations.Add(employer);
        var service = CreateService(store, now);
        var imported = await service.ImportClipboardAsync(new JobLeadClipboardInput(
            null,
            "Example Portal",
            "job-7",
            "Platform Engineer",
            "Example Systems GmbH",
            "Example City",
            "Remote möglich",
            "70 k€",
            "https://jobs.example.invalid/7",
            "Synthetische Stellenbeschreibung.",
            null));

        var opportunity = await service.PromoteAsync(imported.Lead.Id, new JobLeadOpportunityInput(employer.Id, null));

        Assert.Equal("Platform Engineer", opportunity.Title);
        Assert.Equal(employer.Id, opportunity.EmployerOrganizationId);
        Assert.Equal(OpportunityStatus.Identified, opportunity.Status);
        Assert.Single(store.SourceLinks, item => item.OpportunityId == opportunity.Id);
        Assert.Equal(JobLeadStatus.Imported, imported.Lead.Status);
        Assert.Equal(opportunity.Id, imported.Lead.OpportunityId);
    }

    [Fact]
    public async Task Ignore_RemovesLeadFromDefaultInboxButKeepsHistory()
    {
        var now = new DateTimeOffset(2026, 8, 27, 10, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var service = CreateService(store, now);
        var imported = await service.ImportClipboardAsync(new JobLeadClipboardInput(
            null,
            "Clipboard",
            null,
            "Synthetic Admin",
            null,
            null,
            null,
            null,
            null,
            "Synthetic description",
            null));

        await service.IgnoreAsync(imported.Lead.Id);

        Assert.Empty(await service.ListAsync());
        Assert.Single(await service.ListAsync(includeIgnored: true));
        Assert.Equal(JobLeadStatus.Ignored, imported.Lead.Status);
    }

    private static JobLeadService CreateService(MemoryTrackerDataStore store, DateTimeOffset now)
    {
        var clock = new FixedClock(now);
        var opportunityService = new OpportunityService(store, clock);
        var searchProfileService = new SearchProfileService(store, clock);
        return new JobLeadService(store, clock, [], opportunityService, searchProfileService);
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
