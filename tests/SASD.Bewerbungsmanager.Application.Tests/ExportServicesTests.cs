using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;

namespace SASD.Bewerbungsmanager.Application.Tests;

/// <summary>Tests the semantics of v0.2.0 evidence and exchange projections.</summary>
public sealed class ExportServicesTests
{
    [Fact]
    public async Task ApplicationEvidence_IncludesOnlyActuallySubmittedApplicationsInsideInclusivePeriod()
    {
        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var employer = new Organization { Id = Guid.NewGuid(), Name = "Example Operations GmbH" };
        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            EmployerOrganizationId = employer.Id,
            Title = "Linux Platform Engineer",
            DescriptionSnapshot = "Synthetic role description.",
            Location = "Example City",
        };
        store.Organizations.Add(employer);
        store.Opportunities.Add(opportunity);
        store.SourceLinks.Add(new SourceLink
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunity.Id,
            Source = "Example Portal",
            Url = "https://example.invalid/job/42",
            CapturedAtUtc = now,
        });

        store.Applications.Add(CreateApplication(opportunity.Id, now.AddDays(-2), now.AddDays(-2), ApplicationStage.Screening));
        store.Applications.Add(CreateApplication(opportunity.Id, now.AddDays(-20), now.AddDays(-20), ApplicationStage.Rejected));
        store.Applications.Add(CreateApplication(opportunity.Id, now.AddDays(-1), null, ApplicationStage.Draft));

        var service = new ApplicationEvidenceService(store, new FixedClock(now));
        var report = await service.BuildAsync(new DateOnly(2026, 8, 24), new DateOnly(2026, 8, 28));

        var item = Assert.Single(report.Items);
        Assert.Equal("Linux Platform Engineer", item.Position);
        Assert.Equal("Example Operations GmbH", item.Employer);
        Assert.Equal("Example Portal", item.Sources);
        Assert.Equal(ApplicationChannel.Email, item.Channel);
        Assert.Equal(ApplicationStage.Screening, item.Stage);
    }

    [Fact]
    public async Task ApplicationEvidence_RejectsInvertedPeriod()
    {
        var service = new ApplicationEvidenceService(
            new MemoryTrackerDataStore(),
            new FixedClock(new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero)));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.BuildAsync(new DateOnly(2026, 8, 27), new DateOnly(2026, 8, 1)));
    }

    [Fact]
    public async Task ApplicationService_UpdateSubmission_PersistsFactualDateAndChannel()
    {
        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var application = CreateApplication(Guid.NewGuid(), now.AddDays(-3), null, ApplicationStage.Draft);
        store.Applications.Add(application);
        var service = new ApplicationService(store, new FixedClock(now));

        await service.UpdateSubmissionAsync(
            application.Id,
            new SASD.Bewerbungsmanager.Application.Models.ApplicationSubmissionInput(now.AddDays(-1), ApplicationChannel.Portal));

        Assert.Equal(now.AddDays(-1), application.SubmittedAtUtc);
        Assert.Equal(ApplicationChannel.Portal, application.Channel);
        Assert.Equal(now, application.UpdatedAtUtc);
    }

    [Fact]
    public async Task ApplicationService_UpdateSubmission_RejectsDateBeforeApplicationStarted()
    {
        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var application = CreateApplication(Guid.NewGuid(), now.AddDays(-2), null, ApplicationStage.Draft);
        store.Applications.Add(application);
        var service = new ApplicationService(store, new FixedClock(now));

        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateSubmissionAsync(
            application.Id,
            new SASD.Bewerbungsmanager.Application.Models.ApplicationSubmissionInput(now.AddDays(-3), ApplicationChannel.Email)));
    }

    [Fact]
    public async Task ApplicationDossier_BuildsStructuredContextWithoutLocalDocumentPaths()
    {
        var now = new DateTimeOffset(2026, 8, 27, 8, 0, 0, TimeSpan.Zero);
        var store = new MemoryTrackerDataStore();
        var employer = new Organization { Id = Guid.NewGuid(), Name = "Example Health IT GmbH" };
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            OrganizationId = employer.Id,
            FullName = "Erika Beispiel",
            Role = "Recruiting",
            Email = "erika.beispiel@example.invalid",
        };
        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            EmployerOrganizationId = employer.Id,
            Title = "System Engineer Linux",
            DescriptionSnapshot = "Synthetic role description.",
            Location = "Example City",
            RemoteText = "Hybrid",
        };
        var application = CreateApplication(opportunity.Id, now.AddDays(-5), now.AddDays(-4), ApplicationStage.Interview);
        store.Organizations.Add(employer);
        store.Contacts.Add(contact);
        store.Opportunities.Add(opportunity);
        store.Applications.Add(application);
        store.SourceLinks.Add(new SourceLink
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunity.Id,
            Source = "Company careers",
            Url = "https://example.invalid/careers/17",
            CapturedAtUtc = now.AddDays(-5),
        });
        store.Activities.Add(new TrackerActivity
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunity.Id,
            ApplicationId = application.Id,
            ContactId = contact.Id,
            Kind = ActivityKind.Interview,
            Status = ActivityStatus.Planned,
            Subject = "Technical interview",
            ScheduledAtUtc = now.AddDays(1),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });
        store.Tasks.Add(new TrackerTask
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunity.Id,
            ApplicationId = application.Id,
            ContactId = contact.Id,
            Kind = WorkItemKind.Action,
            Status = WorkItemStatus.Open,
            Title = "Prepare interview",
            DueAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });
        store.ApplicationDocumentSnapshots.Add(new ApplicationDocumentSnapshot
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            DocumentId = Guid.NewGuid(),
            Type = DocumentType.Cv,
            Label = "Linux CV",
            Version = "2026-08",
            Language = "DE",
            Sha256 = new string('A', 64),
            StoredPath = @"C:\Private\Documents\do-not-export.pdf",
            CapturedAtUtc = now,
        });

        var dossier = await new ApplicationDossierService(store, new FixedClock(now)).BuildAsync(application.Id);

        Assert.Equal(1, dossier.SchemaVersion);
        Assert.Equal("System Engineer Linux", dossier.Position);
        Assert.Equal("Example Health IT GmbH", dossier.Employer);
        Assert.Single(dossier.Contacts);
        Assert.Single(dossier.Activities);
        Assert.Single(dossier.Tasks);
        var document = Assert.Single(dossier.Documents);
        Assert.Equal("Linux CV", document.Label);
        Assert.DoesNotContain(
            typeof(SASD.Bewerbungsmanager.Application.Models.ApplicationDossierDocument).GetProperties(),
            property => property.Name.Contains("Path", StringComparison.OrdinalIgnoreCase));
    }

    private static JobApplication CreateApplication(
        Guid opportunityId,
        DateTimeOffset startedAtUtc,
        DateTimeOffset? submittedAtUtc,
        ApplicationStage stage)
    {
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunityId,
            StartedAtUtc = startedAtUtc,
            SubmittedAtUtc = submittedAtUtc,
            Channel = ApplicationChannel.Email,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = startedAtUtc,
        };
        application.InitializeStage(stage, startedAtUtc);
        return application;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
