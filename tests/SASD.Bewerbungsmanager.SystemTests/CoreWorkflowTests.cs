using Xunit;
using Microsoft.EntityFrameworkCore;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.SystemTests;

public sealed class CoreWorkflowTests
{
    [Fact]
    public async Task OrganizationOpportunityApplicationStatusHistory_WorksAsOneCoreFlow()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sasd-bewerbungsmanager-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new TestDbContextFactory(path);
            await using (var context = await factory.CreateDbContextAsync())
            {
                await context.Database.MigrateAsync();
            }

            var clock = new FixedClock(new DateTimeOffset(2026, 8, 26, 8, 0, 0, TimeSpan.Zero));
            var store = new TrackerDataStore(factory);
            var organizations = new OrganizationService(store, clock);
            var opportunities = new OpportunityService(store, clock);
            var contacts = new ContactService(store, clock);
            var applications = new ApplicationService(store, clock);
            var activities = new ActivityService(store, clock);
            var workItems = new WorkItemService(store, clock);
            var searches = new SearchProfileService(store, clock);

            var employer = await organizations.CreateAsync(new OrganizationInput("Example Health IT GmbH", OrganizationType.Employer, null, null));
            var recruiter = await contacts.CreateAsync(new ContactInput(employer.Id, "Erika Beispiel", "Recruiting", "erika@example.invalid", null, null, null));
            var opportunity = await opportunities.CreateAsync(new OpportunityInput(
                employer.Id,
                null,
                "System Engineer Linux",
                "Verantwortung für eine synthetische Linux-Serverlandschaft.",
                "Beispielstadt",
                "Hybrid",
                null,
                OpportunityStatus.ApplicationPlanned,
                clock.UtcNow,
                null,
                null));
            var application = await applications.CreateAsync(new ApplicationInput(
                opportunity.Id,
                clock.UtcNow,
                null,
                ApplicationStage.Draft,
                ApplicationChannel.Email,
                "65–70 k€"));

            await applications.ChangeStageAsync(application.Id, ApplicationStage.Submitted, "Unterlagen versendet");
            await applications.UpdateSubmissionAsync(
                application.Id,
                new ApplicationSubmissionInput(clock.UtcNow, ApplicationChannel.Email));

            await workItems.CreateAsync(new WorkItemInput(
                opportunity.Id,
                application.Id,
                null,
                employer.Id,
                WorkItemKind.Action,
                "Interview vorbereiten",
                null,
                clock.UtcNow));
            await workItems.CreateAsync(new WorkItemInput(
                opportunity.Id,
                application.Id,
                null,
                employer.Id,
                WorkItemKind.WaitingFor,
                "Rückmeldung zum Termin",
                null,
                clock.UtcNow.AddDays(2)));
            await activities.CreateAsync(new ActivityInput(
                opportunity.Id,
                application.Id,
                null,
                employer.Id,
                ActivityKind.Interview,
                ActivityStatus.Planned,
                "Technisches Interview",
                null,
                null,
                clock.UtcNow.AddDays(1)));
            await searches.CreateAsync(new SearchProfileInput(
                "Linux Jobs",
                "Example Portal",
                "https://example.invalid/jobs",
                1,
                clock.UtcNow,
                true,
                null));

            var communication = new CommunicationImportService(
                store,
                clock,
                new JsonCommunicationHandoffReader(),
                activities,
                workItems,
                opportunities);
            var importedMail = await communication.ImportAsync(new CommunicationImportInput(
                "SASD Mail Workbench",
                "system-test-mail-1",
                CommunicationDirection.Incoming,
                CommunicationKind.ApplicationResponse,
                "Erika Beispiel",
                "erika@example.invalid",
                "user@example.invalid",
                "Einladung zum Gespräch",
                "Wir möchten Sie zu einem weiteren Gespräch einladen.",
                clock.UtcNow,
                "Inbox/SystemTest",
                opportunity.Id,
                application.Id,
                recruiter.Id,
                employer.Id));
            Assert.True(importedMail.ActivityCreatedAutomatically);
            Assert.NotNull(importedMail.Message.ActivityId);
            Assert.Single(await store.ListCommunicationMessagesAsync());

            // Regression for the real WinForms startup path: DashboardService immediately loads
            // and orders opportunities/applications. SQLite cannot order DateTimeOffset in SQL,
            // so this call used to throw as soon as the dashboard became visible.
            var dashboard = new DashboardService(store);
            var summary = await dashboard.GetSummaryAsync();

            Assert.Equal(1, summary.ActiveOpportunities);
            Assert.Equal(1, summary.Applications);

            var today = await new TodayService(store, clock).GetOverviewAsync();
            Assert.Single(today.DueActions);
            Assert.Single(today.WaitingFor);
            Assert.Single(today.UpcomingAppointments);
            Assert.Single(today.DueSearchProfiles);

            // v0.4.0 regression: a normalized discovered job can be persisted through the latest
            // migration without polluting the durable opportunity workflow until the user promotes it.
            var jobLeads = new JobLeadService(store, clock, [], opportunities, searches);
            var jobLeadResult = await jobLeads.ImportClipboardAsync(new JobLeadClipboardInput(
                null,
                "Example Portal",
                "system-job-1",
                "SRE Engineer",
                "Example Infrastructure GmbH",
                "Example City",
                "Remote",
                null,
                "https://jobs.example.invalid/system-job-1",
                "Synthetic job-search result.",
                null));
            Assert.False(jobLeadResult.WasDuplicate);
            Assert.Single(await store.ListJobLeadsAsync());

            var contextText = await new ApplicationContextService(store, clock).BuildAsync(application.Id);
            Assert.Contains("Interview vorbereiten", contextText, StringComparison.Ordinal);
            Assert.Contains("Rückmeldung zum Termin", contextText, StringComparison.Ordinal);

            // v0.5.0 regression: the optional assistant workflow prepares a guarded local prompt and
            // persists a deliberately pasted response without changing the underlying application.
            var assistantContext = new ApplicationContextService(store, clock);
            var assistant = new AssistantWorkspaceService(store, assistantContext, clock);
            var assistantSession = await assistant.PrepareAsync(new AssistantPreparationInput(
                opportunity.Id,
                application.Id,
                AssistantTaskKind.InterviewPreparation,
                "Synthetic additional instruction."));
            Assert.Contains("BEGIN CONTEXT", assistantSession.PromptText, StringComparison.Ordinal);
            await assistant.CompleteAsync(assistantSession.Id, new AssistantCompletionInput(
                "Synthetic assistant response.",
                "Example Assistant"));
            var persistedAssistantSession = Assert.Single(await store.ListAssistantSessionsAsync());
            Assert.Equal(AssistantSessionStatus.Completed, persistedAssistantSession.Status);
            Assert.Equal("Synthetic assistant response.", persistedAssistantSession.ResponseText);

            // v0.2.0 regression: the same real SQLite graph must be usable for evidence and
            // exchange projections without introducing another schema migration.
            var localSubmissionDate = DateOnly.FromDateTime(clock.UtcNow.ToLocalTime().DateTime);
            var evidence = await new ApplicationEvidenceService(store, clock).BuildAsync(localSubmissionDate, localSubmissionDate);
            var evidenceItem = Assert.Single(evidence.Items);
            Assert.Equal("System Engineer Linux", evidenceItem.Position);

            var dossier = await new ApplicationDossierService(store, clock).BuildAsync(application.Id);
            Assert.Equal("System Engineer Linux", dossier.Position);
            Assert.Single(dossier.Tasks, item => item.Status == WorkItemStatus.Open && item.Kind == WorkItemKind.Action);

            var persisted = await store.GetApplicationAsync(application.Id);

            Assert.NotNull(persisted);
            var actual = persisted!;
            Assert.Equal(ApplicationStage.Submitted, actual.Stage);
            Assert.Equal(2, actual.StatusHistory.Count);
        }
        finally
        {
            TryDelete(path);
            TryDelete(path + "-shm");
            TryDelete(path + "-wal");
        }
    }

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class TestDbContextFactory(string databasePath) : IDbContextFactory<ApplicationTrackerDbContext>
    {
        public ApplicationTrackerDbContext CreateDbContext()
        {
            // Microsoft.Data.Sqlite enables connection pooling by default. For this file-based
            // system test we deliberately disable pooling so disposing the DbContext also closes
            // the underlying file handle immediately. Otherwise Windows can still see the
            // temporary database as in use when the test cleanup deletes it.
            var connectionString = $"Data Source={databasePath};Pooling=False;Foreign Keys=True";
            var options = new DbContextOptionsBuilder<ApplicationTrackerDbContext>()
                .UseSqlite(connectionString)
                .Options;

            return new ApplicationTrackerDbContext(options);
        }

        public Task<ApplicationTrackerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }
}
