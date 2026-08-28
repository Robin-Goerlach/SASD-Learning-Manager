using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Builds period-based evidence of applications that were actually submitted. Drafts without a
/// submission timestamp are deliberately excluded because a proof should not claim an application
/// that was only prepared.
/// </summary>
public sealed class ApplicationEvidenceService(ITrackerDataStore store, IClock clock)
{
    /// <summary>
    /// Builds an inclusive local-calendar report between <paramref name="fromDate"/> and
    /// <paramref name="toDate"/>. Submission timestamps remain stored as UTC, while the period is
    /// interpreted as the user's local calendar because that is how evidence periods are normally
    /// communicated.
    /// </summary>
    public async Task<ApplicationEvidenceReport> BuildAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate)
        {
            throw new ValidationException("Das Enddatum darf nicht vor dem Startdatum liegen.");
        }

        var applications = await store.ListApplicationsAsync(cancellationToken).ConfigureAwait(false);
        var opportunities = await store.ListOpportunitiesAsync(cancellationToken).ConfigureAwait(false);
        var organizations = await store.ListOrganizationsAsync(includeArchived: true, cancellationToken).ConfigureAwait(false);

        var opportunityById = opportunities.ToDictionary(item => item.Id);
        var organizationById = organizations.ToDictionary(item => item.Id);
        var sourceCache = new Dictionary<Guid, string>();
        var items = new List<ApplicationEvidenceItem>();

        foreach (var application in applications)
        {
            if (application.SubmittedAtUtc is not DateTimeOffset submittedAtUtc)
            {
                continue;
            }

            var localDate = DateOnly.FromDateTime(submittedAtUtc.ToLocalTime().DateTime);
            if (localDate < fromDate || localDate > toDate)
            {
                continue;
            }

            if (!opportunityById.TryGetValue(application.OpportunityId, out var opportunity))
            {
                // A dangling foreign key should not normally be possible with SQLite foreign keys
                // enabled. Ignoring it here keeps an export from fabricating incomplete evidence.
                continue;
            }

            if (!sourceCache.TryGetValue(opportunity.Id, out var sources))
            {
                var links = await store.ListSourceLinksAsync(opportunity.Id, cancellationToken).ConfigureAwait(false);
                sources = string.Join(", ", links
                    .Select(item => item.Source.Trim())
                    .Where(value => value.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase));
                sourceCache[opportunity.Id] = sources;
            }

            var employer = opportunity.EmployerOrganizationId is Guid employerId
                && organizationById.TryGetValue(employerId, out var organization)
                    ? organization.Name
                    : string.Empty;

            items.Add(new ApplicationEvidenceItem(
                application.Id,
                submittedAtUtc,
                opportunity.Title,
                employer,
                opportunity.Location,
                application.Channel,
                application.Stage,
                sources));
        }

        var ordered = items
            .OrderBy(item => item.SubmittedAtUtc)
            .ThenBy(item => item.Employer, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Position, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new ApplicationEvidenceReport(fromDate, toDate, clock.UtcNow, ordered);
    }
}
