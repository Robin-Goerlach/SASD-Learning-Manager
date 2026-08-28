using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;

namespace SASD.Bewerbungsmanager.Application.Tests;

public sealed class OperationalServicesTests
{
    [Fact]
    public async Task TodayOverview_SeparatesActionsWaitingAppointmentsAndSearchChecks()
    {
        var now = new DateTimeOffset(2026, 8, 26, 10, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        store.Tasks.AddRange(
        [
            TaskItem(WorkItemKind.Action, "Überfällige Antwort", now.AddDays(-2)),
            TaskItem(WorkItemKind.Action, "Lebenslauf senden", now),
            TaskItem(WorkItemKind.WaitingFor, "Rückmeldung Recruiter", now.AddDays(2)),
        ]);
        store.Activities.Add(new TrackerActivity
        {
            Id = Guid.NewGuid(),
            Kind = ActivityKind.Interview,
            Status = ActivityStatus.Planned,
            Subject = "Technisches Interview",
            ScheduledAtUtc = now.AddDays(1),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });
        store.SearchProfiles.Add(new SearchProfile
        {
            Id = Guid.NewGuid(),
            Name = "Linux Jobs",
            Source = "Example Portal",
            Url = "https://example.invalid/jobs",
            CheckIntervalDays = 1,
            NextCheckAtUtc = now.AddHours(-1),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });

        var service = new TodayService(store, new FixedClock(now));
        var overview = await service.GetOverviewAsync();

        Assert.Single(overview.OverdueActions);
        Assert.Single(overview.DueActions);
        Assert.Single(overview.WaitingFor);
        Assert.Single(overview.UpcomingAppointments);
        Assert.Single(overview.DueSearchProfiles);
    }

    [Fact]
    public async Task ApplicationContext_BuildsOperationalSectionsFromStoredData()
    {
        var now = new DateTimeOffset(2026, 8, 26, 10, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var employer = new Organization { Id = Guid.NewGuid(), Name = "Example Systems GmbH" };
        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            EmployerOrganizationId = employer.Id,
            Title = "Linux Platform Engineer",
            DescriptionSnapshot = "Synthetische Rollenbeschreibung.",
            Status = OpportunityStatus.Applied,
            FoundAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunity.Id,
            StartedAtUtc = now,
            Channel = ApplicationChannel.Email,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        application.InitializeStage(ApplicationStage.Submitted, now);
        store.Organizations.Add(employer);
        store.Opportunities.Add(opportunity);
        store.Applications.Add(application);
        store.Tasks.Add(new TrackerTask
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            Kind = WorkItemKind.WaitingFor,
            Status = WorkItemStatus.Open,
            Title = "Feedback nach Interview",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });

        var text = await new ApplicationContextService(store, new FixedClock(now)).BuildAsync(application.Id);

        Assert.Contains("Position:", text, StringComparison.Ordinal);
        Assert.Contains("Linux Platform Engineer", text, StringComparison.Ordinal);
        Assert.Contains("Warten auf:", text, StringComparison.Ordinal);
        Assert.Contains("Feedback nach Interview", text, StringComparison.Ordinal);
        Assert.Contains("Rollenbeschreibung:", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WorkItemService_RejectsApplicationFromDifferentOpportunity()
    {
        var now = new DateTimeOffset(2026, 8, 26, 10, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var expectedOpportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            Title = "Expected role",
            DescriptionSnapshot = "Synthetic role.",
        };
        var otherOpportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            Title = "Other role",
            DescriptionSnapshot = "Synthetic other role.",
        };
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OpportunityId = otherOpportunity.Id,
            StartedAtUtc = now,
            Channel = ApplicationChannel.Email,
        };
        application.InitializeStage(ApplicationStage.Draft, now);
        store.Opportunities.AddRange([expectedOpportunity, otherOpportunity]);
        store.Applications.Add(application);

        var service = new WorkItemService(store, new FixedClock(now));
        var input = new WorkItemInput(
            expectedOpportunity.Id,
            application.Id,
            null,
            null,
            WorkItemKind.Action,
            "Synthetic task",
            null,
            null);

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(input));
    }

    private static TrackerTask TaskItem(WorkItemKind kind, string title, DateTimeOffset dueAtUtc)
        => new()
        {
            Id = Guid.NewGuid(),
            Kind = kind,
            Status = WorkItemStatus.Open,
            Title = title,
            DueAtUtc = dueAtUtc,
            CreatedAtUtc = dueAtUtc.AddDays(-1),
            UpdatedAtUtc = dueAtUtc.AddDays(-1),
        };

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
