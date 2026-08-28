using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Builds a structured, portable dossier for one application. The dossier is intentionally a
/// read-only projection and does not expose local document paths or document file contents.
/// </summary>
public sealed class ApplicationDossierService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Builds the versioned exchange dossier for one concrete application.</summary>
    public async Task<ApplicationExchangeDossier> BuildAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = await store.GetApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Bewerbung wurde nicht gefunden.");
        var opportunity = await store.GetOpportunityAsync(application.OpportunityId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die zugehörige Stelle wurde nicht gefunden.");

        var organizations = await store.ListOrganizationsAsync(includeArchived: true, cancellationToken).ConfigureAwait(false);
        var contacts = await store.ListContactsAsync(includeArchived: true, cancellationToken).ConfigureAwait(false);
        var activities = await store.ListActivitiesAsync(cancellationToken).ConfigureAwait(false);
        var tasks = await store.ListTasksAsync(cancellationToken).ConfigureAwait(false);
        var sources = await store.ListSourceLinksAsync(opportunity.Id, cancellationToken).ConfigureAwait(false);
        var documents = await store.ListApplicationDocumentSnapshotsAsync(applicationId, cancellationToken).ConfigureAwait(false);

        var relevantActivities = activities
            .Where(item => item.ApplicationId == applicationId || item.OpportunityId == opportunity.Id)
            .OrderBy(item => item.OccurredAtUtc ?? item.ScheduledAtUtc ?? item.CreatedAtUtc)
            .ToList();
        var relevantTasks = tasks
            .Where(item => item.ApplicationId == applicationId || item.OpportunityId == opportunity.Id)
            .OrderBy(item => item.DueAtUtc ?? item.CreatedAtUtc)
            .ToList();

        var contactIds = relevantActivities.Select(item => item.ContactId)
            .Concat(relevantTasks.Select(item => item.ContactId))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToHashSet();

        return new ApplicationExchangeDossier(
            SchemaVersion: 1,
            ExportedAtUtc: clock.UtcNow,
            ApplicationId: application.Id,
            OpportunityId: opportunity.Id,
            Position: opportunity.Title,
            Employer: ResolveOrganization(opportunity.EmployerOrganizationId, organizations),
            Intermediary: ResolveOrganization(opportunity.IntermediaryOrganizationId, organizations),
            Stage: application.Stage,
            Channel: application.Channel,
            StartedAtUtc: application.StartedAtUtc,
            SubmittedAtUtc: application.SubmittedAtUtc,
            SalaryExpectation: application.SalaryExpectation,
            Location: opportunity.Location,
            RemoteText: opportunity.RemoteText,
            SalaryText: opportunity.SalaryText,
            RoleDescriptionSnapshot: opportunity.DescriptionSnapshot,
            Sources: sources
                .OrderBy(item => item.Source, StringComparer.CurrentCultureIgnoreCase)
                .Select(item => new ApplicationDossierSource(item.Source, item.Url, item.ExternalId))
                .ToList(),
            Contacts: contacts
                .Where(item => contactIds.Contains(item.Id))
                .OrderBy(item => item.FullName, StringComparer.CurrentCultureIgnoreCase)
                .Select(item => new ApplicationDossierContact(item.FullName, item.Role, item.Email, item.Phone, item.LinkedInUrl))
                .ToList(),
            Activities: relevantActivities
                .Select(item => new ApplicationDossierActivity(
                    item.Kind,
                    item.Status,
                    item.Subject,
                    item.Notes,
                    item.OccurredAtUtc,
                    item.ScheduledAtUtc))
                .ToList(),
            Tasks: relevantTasks
                .Select(item => new ApplicationDossierTask(item.Kind, item.Status, item.Title, item.Notes, item.DueAtUtc))
                .ToList(),
            Documents: documents
                .OrderBy(item => item.Type)
                .ThenBy(item => item.Label, StringComparer.CurrentCultureIgnoreCase)
                .Select(item => new ApplicationDossierDocument(
                    item.Type,
                    item.Label,
                    item.Version,
                    item.Language,
                    item.Sha256,
                    item.CapturedAtUtc))
                .ToList());
    }

    private static string ResolveOrganization(Guid? id, IReadOnlyList<SASD.Bewerbungsmanager.Domain.Entities.Organization> organizations)
        => id is Guid organizationId
            ? organizations.SingleOrDefault(item => item.Id == organizationId)?.Name ?? string.Empty
            : string.Empty;
}
