using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.Application.Tests;

/// <summary>Verifies the conservative local communication-integration workflow.</summary>
public sealed class CommunicationImportServiceTests
{
    [Fact]
    public async Task RecruiterMail_MatchesContactAndUniqueContext_AndCreatesActivity()
    {
        var now = new DateTimeOffset(2026, 8, 27, 7, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var organization = new Organization { Id = Guid.NewGuid(), Name = "Example Recruiting GmbH" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            FullName = "Erika Beispiel",
            Email = "erika@example.invalid",
        };
        var opportunity = Opportunity(now, organization.Id);
        var application = Application(now, opportunity.Id);
        store.Organizations.Add(organization);
        store.Contacts.Add(contact);
        store.Opportunities.Add(opportunity);
        store.Applications.Add(application);
        store.Activities.Add(new SASD.Bewerbungsmanager.Domain.Entities.Activity
        {
            Id = Guid.NewGuid(),
            ContactId = contact.Id,
            OpportunityId = opportunity.Id,
            ApplicationId = application.Id,
            Kind = ActivityKind.Email,
            Status = ActivityStatus.Recorded,
            Subject = "Earlier contact",
            OccurredAtUtc = now.AddDays(-3),
            CreatedAtUtc = now.AddDays(-3),
            UpdatedAtUtc = now.AddDays(-3),
        });

        var service = CreateService(store, now);
        var result = await service.ImportAsync(Input(
            now,
            from: "erika@example.invalid",
            subject: "Einladung zum Interview",
            body: "Wir möchten Sie gern kennenlernen."));

        Assert.False(result.WasDuplicate);
        Assert.True(result.ContactMatchedAutomatically);
        Assert.True(result.ContextMatchedAutomatically);
        Assert.True(result.ActivityCreatedAutomatically);
        Assert.Equal(CommunicationKind.ApplicationResponse, result.Message.Kind);
        Assert.Equal(contact.Id, result.Message.ContactId);
        Assert.Equal(application.Id, result.Message.ApplicationId);
        Assert.Equal(opportunity.Id, result.Message.OpportunityId);
        Assert.NotNull(result.Message.ActivityId);
        Assert.Equal(2, store.Activities.Count);
    }

    [Fact]
    public async Task ReimportSameExternalMessage_IsIdempotent()
    {
        var now = new DateTimeOffset(2026, 8, 27, 7, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var service = CreateService(store, now);
        var input = Input(now, "jobs@example.invalid", "Job Alert", "Neue Jobs: https://example.invalid/jobs/42") with
        {
            ExternalMessageId = "message-42",
            Kind = CommunicationKind.JobAlert,
        };

        var first = await service.ImportAsync(input);
        var second = await service.ImportAsync(input);

        Assert.False(first.WasDuplicate);
        Assert.True(second.WasDuplicate);
        Assert.Single(store.CommunicationMessages);
        Assert.Empty(store.Activities);
    }

    [Fact]
    public async Task JobAlert_ExtractsLinks_AndCanCreateOpportunitySnapshot()
    {
        var now = new DateTimeOffset(2026, 8, 27, 7, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var service = CreateService(store, now);
        var result = await service.ImportAsync(Input(
            now,
            "alerts@example.invalid",
            "Neue Stellen für Linux",
            "Senior Linux Engineer\nhttps://example.invalid/jobs/linux-1") with { Kind = CommunicationKind.JobAlert });

        Assert.Single(result.DetectedUrls);
        Assert.Equal("https://example.invalid/jobs/linux-1", result.DetectedUrls[0]);
        Assert.False(result.ActivityCreatedAutomatically);

        var opportunity = await service.CreateOpportunityFromMessageAsync(
            result.Message.Id,
            "Senior Linux Engineer",
            result.DetectedUrls[0]);

        Assert.Equal("Senior Linux Engineer", opportunity.Title);
        Assert.Contains("Senior Linux Engineer", opportunity.DescriptionSnapshot, StringComparison.Ordinal);
        Assert.Single(store.SourceLinks);
        Assert.Equal(opportunity.Id, store.CommunicationMessages[0].OpportunityId);
    }

    private static CommunicationImportService CreateService(MemoryTrackerDataStore store, DateTimeOffset now)
    {
        var clock = new FixedClock(now);
        var activity = new ActivityService(store, clock);
        var workItems = new WorkItemService(store, clock);
        var opportunities = new OpportunityService(store, clock);
        return new CommunicationImportService(store, clock, new EmptyHandoffReader(), activity, workItems, opportunities);
    }

    private static CommunicationImportInput Input(DateTimeOffset now, string from, string subject, string body)
        => new(
            SourceSystem: "SASD Mail Workbench",
            ExternalMessageId: null,
            Direction: CommunicationDirection.Incoming,
            Kind: CommunicationKind.Unclassified,
            FromName: "Erika Beispiel",
            FromAddress: from,
            ToAddresses: "user@example.invalid",
            Subject: subject,
            BodyText: body,
            MessageAtUtc: now,
            SourceReference: "Inbox/Test");

    private static Opportunity Opportunity(DateTimeOffset now, Guid organizationId)
        => new()
        {
            Id = Guid.NewGuid(),
            IntermediaryOrganizationId = organizationId,
            Title = "Linux Platform Engineer",
            DescriptionSnapshot = "Synthetic role description.",
            Status = OpportunityStatus.Contacted,
            FoundAtUtc = now.AddDays(-7),
            CreatedAtUtc = now.AddDays(-7),
            UpdatedAtUtc = now.AddDays(-7),
        };

    private static JobApplication Application(DateTimeOffset now, Guid opportunityId)
    {
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunityId,
            StartedAtUtc = now.AddDays(-5),
            Channel = ApplicationChannel.Email,
            CreatedAtUtc = now.AddDays(-5),
            UpdatedAtUtc = now.AddDays(-5),
        };
        application.InitializeStage(ApplicationStage.Screening, now.AddDays(-5));
        return application;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class EmptyHandoffReader : ICommunicationHandoffReader
    {
        public Task<CommunicationHandoffBatch> ReadAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(new CommunicationHandoffBatch(1, "Test", []));
    }
}
